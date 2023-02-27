using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XVNML.Core.Lexer;
using XVNML.Core.TagParser.Enums;
using XVNML.Core.Tags;

namespace XVNML.Core.TagParser
{
    internal class Parser
    {
        #region Fields and Properties
        protected int _position = -1;

        internal string? fileTarget;

        private bool _conflict;

        Tokenizer? _tokenizer = null;

        private ParserEvaluationState _evaluationState = ParserEvaluationState.Tag;
        private ParseResourceState? _evaluationResourceState = ParseResourceState.Internal;

        readonly Stack<TagBase> _tagStackFrame = new Stack<TagBase>();
        int TagLevel => _tagStackFrame.Count - 1;

        TagBase? _topOfStack
        {
            get
            {
                if (_tagStackFrame.Count == 0) return null;
                return _tagStackFrame.Peek();
            }
        }

        internal TagBase? _rootTag;

        //Temporary Cache
        TagParameterInfo? _cachedTagParameterInfo;
        string? _cachedTagName;

        public void SetTarget(string fileTarget) => this.fileTarget = fileTarget.ToString();

        private SyntaxToken? _Current => Peek(0, true);

        //This is important for anything in between tags (for example) <title>Hi</title>
        private StringBuilder _tagValueStringBuilder = new StringBuilder();
        private readonly Queue<Action> _solvingQueue = new Queue<Action>();
        #endregion

        public void Parse()
        {
            _position = -1;
            _conflict = false;
            _evaluationResourceState = ParseResourceState.Internal;
            _evaluationState = ParserEvaluationState.Tag;
            _rootTag = null;
            _cachedTagName = null;
            _cachedTagParameterInfo = null;
            _tagValueStringBuilder = new StringBuilder();
            if (fileTarget == null)
                throw new NullReferenceException("FileTarget cannot be null when parsing.");

            _tokenizer = new Tokenizer(fileTarget, TokenizerReadState.IO, out _conflict);


            if (_conflict)
            {
                Console.WriteLine($"There was conflict with tokenizer. Failed to parse xvnml file: Located in {fileTarget}");
                return;
            }

            //Evaluate
            AnalyzeTokens();
        }

        private SyntaxToken? Peek(int offset, bool includeSpaces = false)
        {
            if (_tokenizer == null) return null;
            try
            {
                var token = _tokenizer[_position + offset];

                while (true)
                {
                    token = _tokenizer[_position + offset] ?? default;

                    if ((token!.Type == TokenType.WhiteSpace && includeSpaces == false) ||
                        token.Type == TokenType.SingleLineComment ||
                        token.Type == TokenType.MultilineComment)
                    {
                        _position++;
                        continue;
                    }
                    return token;
                }
            }
            catch
            {
                return _tokenizer[_tokenizer.Length];
            }
        }

        private SyntaxToken? Next()
        {
            _position++;
            return _Current;
        }

        private void AnalyzeTokens()
        {
            //Find starting of tags. With each tag found,
            //evaluate the Parameters/Flags, if it's self closing or not
            //it's children elements, and its values
            if (_tokenizer == null) return;

            for (int i = 0; i < _tokenizer.Length; i++)
            {
                //If there was a conflict in resolving types, stop parsing;
                if (_conflict) return;

                Next();

                SyntaxToken? token = _Current;

                if (_evaluationState == ParserEvaluationState.TagValue || _evaluationState == ParserEvaluationState.Dialogue)
                {

                    if (_Current?.Type == TokenType.OpenBracket && Peek(1, true)?.Type == TokenType.ForwardSlash)
                    {
                        ChangeEvaluationState(ParserEvaluationState.Tag);
                        _position--;
                        continue;
                    }

                    _tagValueStringBuilder.Append(_Current?.Type == TokenType.String ?
                        $"\"{_Current?.Text}\"" :
                        _Current?.Text);
                }

                if (_evaluationState == ParserEvaluationState.Tag)
                {
                    #region Tag Evaluation
                    switch (token?.Type)
                    {
                        case TokenType.Invalid:
                            Console.WriteLine($"Invalid token: \"{token.Type}\" at Line {token.Line} Position {token.Position}");
                            break;

                        case TokenType.OpenBracket:
                            //Check for <any_string_of_characters)
                            while (true)
                            {
                                Next();

                                if (_Current == null || _Current.Type == TokenType.EOF) break;

                                //Check if foward/slash
                                if (_Current.Type == TokenType.ForwardSlash)
                                {
                                    //This means the tag is about to close
                                    Next();

                                    //Expect name
                                    if (_Current.Type != TokenType.Identifier)
                                    {
                                        //TODO: Create ExpectedIdentifierException
                                        Console.WriteLine($"Expected Identifier at Line {_Current.Line} Position {_Current.Position}");
                                        return;
                                    }

                                    //Expect matching name
                                    if (_Current.Text != _topOfStack?.tagTypeName)
                                    {
                                        Console.WriteLine($"Tag Leveling for {_topOfStack?.tagTypeName} does not match with closing tag " +
                                            $"{_Current.Text}");
                                        return;
                                    }

                                    Next();

                                    //Close Bracket is now expected
                                    if (_Current.Type != TokenType.CloseBracket)
                                    {
                                        //TODO: Create ExpectedIdentifierException
                                        Console.WriteLine($"Expected CloseBracket Line {_Current.Line} Position {_Current.Position}");
                                        return;
                                    }

                                    //Now the object can be closed
                                    CloseCurrentTag();
                                    continue;
                                }

                                //Check if what comes after < is an identifier
                                //and a white space.
                                //If it is, we'll property gather information
                                //regarding the tag.
                                if (_Current.Type == TokenType.Identifier)
                                {
                                    //This means that we are creating a tag
                                    var newTag = TagConverter.Convert(_Current.Text!);
                                    newTag!.tagTypeName = _Current.Text;

                                    //If top is still open, it means we're nesting
                                    if (_topOfStack != null &&
                                        _topOfStack.tagState == TagEvaluationState.Open)
                                    {
                                        _topOfStack.elements = _topOfStack.elements ?? new List<TagBase>();
                                        _topOfStack.elements.Add(newTag);
                                        newTag.parentTag = _topOfStack;
                                    }

                                    _tagStackFrame.Push(newTag);
                                    break;
                                }
                            }
                            continue;

                        case TokenType.CloseBracket:
                            //Change the stat of the tag if there is any
                            if (_topOfStack!.isSelfClosing)
                            {
                                CloseCurrentTag();
                                continue;
                            }

                            _topOfStack.tagState = TagEvaluationState.Open;
                            _topOfStack.parameterInfo = _cachedTagParameterInfo;
                            _cachedTagParameterInfo = null;

                            if (Peek(1)?.Type == TokenType.OpenBracket) continue;

                            ChangeEvaluationState(ParserEvaluationState.TagValue);
                            continue;

                        case TokenType.ForwardSlash:
                            //Check if a CloseBracket exceeds that
                            //If it is, mark as SelfClosing
                            if (Peek(1)?.Type == TokenType.CloseBracket)
                            {
                                //The tag is self-closing
                                _topOfStack!.isSelfClosing = true;
                            }
                            continue;


                        case TokenType.Pound:
                            //There will have to be a unique feature
                            //where flags are set prior to calculations.
                            if (_topOfStack!.tagState != TagEvaluationState.OnParameters) continue;

                            // This means that we are about to send in a flag
                            // Expect a identifier, but the identifier should not 
                            // expect a ::, since it's just a flag
                            _topOfStack.isSettingFlag = true;
                            continue;

                        case TokenType.Identifier:
                            //After a tag has been set, check it's status
                            if (_topOfStack!.tagState == TagEvaluationState.OnParameters)
                            {
                                if (_cachedTagParameterInfo == null)
                                    _cachedTagParameterInfo = new TagParameterInfo();

                                //Check if flag
                                if (_topOfStack!.isSettingFlag == true && Peek(1)?.Type != TokenType.DoubleColon)
                                {
                                    //This means this is a Flag for the tag
                                    _cachedTagParameterInfo.flagParameters.Add(_Current?.Text!);
                                    _topOfStack!.isSettingFlag = false;
                                    continue;
                                }

                                if(_topOfStack!.isSettingFlag)
                                {
                                    Abort($"A flag should not expect the token {Peek(1)?.Text}");
                                    return;
                                }

                                TagParameter newParameter = new TagParameter()
                                {
                                    name = _Current?.Value?.ToString()
                                };

                                //Cache string name
                                _cachedTagName = newParameter?.name;

                                _cachedTagParameterInfo.paramters.Add(_cachedTagName!, newParameter!);
                            }
                            continue;

                        case TokenType.EOF:
                            Console.WriteLine($"Parsing of XVNML Document now complete.: {fileTarget}");
                            RunReferenceSolveProcedure();
                            return;

                        case TokenType.DoubleColon:
                            {
                                int length = _cachedTagParameterInfo!.totalParameters;
                                var parameterName = _cachedTagParameterInfo.paramters[_cachedTagName!].name;

                                var expected = Peek(1);

                                //Go next, and grab the value for the TagParameter

                                //Make sure it's valid input
                                //Expecting the following:
                                if (expected?.Type == TokenType.Char ||
                                    expected?.Type == TokenType.String ||
                                    expected?.Type == TokenType.Number ||
                                    expected?.Type == TokenType.EmptyString ||
                                    expected?.Type == TokenType.Identifier)
                                {
                                    Next();
                                    _cachedTagParameterInfo.paramters[_cachedTagName!].value = _Current?.Value;
                                    continue;
                                }

                                //Find a reference
                                if (expected?.Type == TokenType.DollarSign)
                                {
                                    Next();

                                    //Expect Identifer
                                    if (Peek(1)?.Type != TokenType.Identifier)
                                    {
                                        Console.WriteLine($"Reference Error at Line {_Current?.Line} Position {_Current?.Position}: Expected Identifier: {fileTarget}");
                                        return;
                                    }
                                    Next();
                                    _cachedTagParameterInfo.paramters[_cachedTagName!].value = _Current?.Value;
                                    _cachedTagParameterInfo.paramters[_cachedTagName!].isReferencing = true;

                                    continue;
                                }

                                Console.WriteLine($"Invalid assignment to parameter: {parameterName} at Line {_Current?.Line} Position {_Current?.Position}: {fileTarget}");
                            }
                            return;

                        case TokenType.String:
                            if (_topOfStack?.tagState == TagEvaluationState.Open)
                            {
                                //Add as tag value
                                _topOfStack.value = _Current?.Value;
                            }
                            continue;

                        case TokenType.At:
                            if (_evaluationState == ParserEvaluationState.TagValue) ChangeEvaluationState(ParserEvaluationState.Dialogue);
                            continue;
                        default:
                            break;
                    }
                }
                #endregion
            }
        }

        private void RunReferenceSolveProcedure()
        {
            while (_solvingQueue.Count != 0)
            {
                var nextSolvee = _solvingQueue.Dequeue();

                nextSolvee();
            }
        }

        internal void QueueForReferenceSolve(Action method)
        {
            _solvingQueue.Enqueue(method);
        }

        public static void Abort(string? reason)
        {
            throw new Exception($"Parser has aborted. Reason: {reason ?? "Undefined"}");
        }

        private void CloseCurrentTag()
        {
            //Now the object can be closed
            _topOfStack!.tagState = TagEvaluationState.Close;
            _topOfStack.parameterInfo = _topOfStack.parameterInfo ?? _cachedTagParameterInfo;

            _cachedTagParameterInfo = null;
            _cachedTagName = string.Empty;

            if (_tagValueStringBuilder.Length > 0)
            {
                _topOfStack.value = _tagValueStringBuilder.ToString().Trim('\n');
                _tagValueStringBuilder.Clear();
            }

            var dirInfo = new DirectoryInfo(fileTarget!);
            var fileOrigin = dirInfo.Parent?.ToString();
            _topOfStack.parserRef = this;
            _topOfStack.OnResolve(fileOrigin);

            _rootTag = _tagStackFrame.Pop();
        }

        private void ChangeEvaluationState(ParserEvaluationState state)
        {
            _evaluationState = state;
        }
    }
}