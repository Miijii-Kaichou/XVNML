using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XVNML.Core.Lexer;
using XVNML.Core.Parser.Enums;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

namespace XVNML.Core.Parser
{
    public sealed class TagParser
    {
        #region Fields and Properties
        protected int _position = -1;

        internal string? fileTarget;

        private bool _conflict;

        Tokenizer? _tokenizer = null;

        private ParserEvaluationState _evaluationState = ParserEvaluationState.Tag;

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

        internal TagBase? root;

        //Temporary Cache
        TagParameterInfo? _cachedTagParameterInfo;
        string? _cachedTagName;

        public void SetTarget(string fileTarget) => this.fileTarget = fileTarget.ToString();

        private SyntaxToken? _Current => Peek(0, true);

        //This is important for anything in between tags (for example) <title>Hi</title>
        private StringBuilder _tagValueStringBuilder = new StringBuilder();
        private readonly Queue<Action> _solvingQueue = new Queue<Action>();

        internal Action? _onParserCompleted;
        #endregion

        public void Parse(Action? onComplete)
        {
            _onParserCompleted = onComplete;
            _position = -1;
            _conflict = false;
            _evaluationState = ParserEvaluationState.Tag;
            root = null;
            _cachedTagName = null;
            _cachedTagParameterInfo = null;
            _tagValueStringBuilder = new StringBuilder(1024);
            
            if (fileTarget == null)
            {
                Abort("FileTarget cannot be null when parsing.");
                return;
            }

            _tokenizer = new Tokenizer(fileTarget, TokenizerReadState.IO);
            _conflict = _tokenizer.GetConflictState();

            if (_conflict)
            {
                Abort($"There was conflict with tokenizer. Failed to parse xvnml file: Located in {fileTarget}");
                return;
            }

            //Evaluate
            AnalyzeTokens();
        }

        private SyntaxToken? Peek(int offset, bool includeSpaces = false)
        {
            if (_tokenizer == null)
                return null;

            var length = _tokenizer.Length;
            var index = _position + offset;

            while (index < length)
            {
                var token = _tokenizer[index];

                if (token == null)
                {
                    Abort("Token was null");
                    return token;
                }

                if ((token.Type == TokenType.WhiteSpace && !includeSpaces) ||
                    token.Type == TokenType.SingleLineComment ||
                    token.Type == TokenType.MultilineComment)
                {
                    index++;
                    continue;
                }

                return token;
            }

            return _tokenizer[length];
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
                    HandleTagValue();
                    continue;
                }

                if (_evaluationState == ParserEvaluationState.Tag)
                {
                    EvaluateTags(ref token, out bool complete);
                    if (complete) return;
                    continue;
                }
            }
        }

        private void EvaluateTags(ref SyntaxToken? token, out bool complete)
        {
            complete = false;
            TokenType? tokenType = token?.Type;

            switch (tokenType)
            {
                case TokenType.Invalid:
                    Abort($"Invalid token: \"{token.Type}\" at Line {token.Line} Position {token.Position}");
                    complete = !complete;
                    return;

                case TokenType.OpenBracket:
                    HandleOpenBracket();
                    return;

                case TokenType.CloseBracket:
                    HandleCloseBracket();
                    return;

                case TokenType.ForwardSlash:
                    HandleForwardSlash();
                    return;

                case TokenType.Pound:
                    HandlePound();
                    return;

                case TokenType.Identifier:
                    HandleIdentifier();
                    return;

                case TokenType.EOF:
                    XVNMLLogger.Log($"Parsing of XVNML Document now complete.: {fileTarget}", this);
                    RunReferenceSolveProcedure();
                    complete = !complete;
                    return;

                case TokenType.DoubleColon:
                    HandleDoubleColon();
                    return;

                case TokenType.String:
                    HandleString();
                    return;

                case TokenType.At:
                    if (_evaluationState == ParserEvaluationState.TagValue)
                        ChangeEvaluationState(ParserEvaluationState.Dialogue);
                    return;

                default:
                    break;
            }
        }

        private void RunReferenceSolveProcedure()
        {
            while (_solvingQueue.Count != 0)
            {
                var nextSolvee = _solvingQueue.Dequeue();

                nextSolvee();
            }

            _onParserCompleted?.Invoke();
        }

        internal void QueueForReferenceSolve(Action method)
        {
            _solvingQueue.Enqueue(method);
        }

        public static void Abort(string? reason)
        {
            XVNMLLogger.LogError(reason!, null, null);
            throw new Exception($"Parser has aborted. Reason: {reason ?? "Undefined"}");
        }

        private void CloseCurrentTag()
        {
            var top = _topOfStack;

            if (top == null) return;

            //Now the object can be closed
            top.tagState = TagEvaluationState.Close;
            top._parameterInfo ??= _cachedTagParameterInfo;

            _cachedTagParameterInfo = null;
            _cachedTagName = string.Empty;

            if (_tagValueStringBuilder.Length > 0)
            {
                top.value = _tagValueStringBuilder.ToString().Trim('\n');
                _tagValueStringBuilder.Clear();
            }

            var dirInfo = new DirectoryInfo(fileTarget!);
            var fileOrigin = dirInfo.Parent?.ToString();
            top.ParserRef = this;
            top.OnResolve(fileOrigin);

            root = _tagStackFrame.Pop();
        }

        private void ChangeEvaluationState(ParserEvaluationState state)
        {
            _evaluationState = state;
        }

        #region Handler Methods
        private void HandleOpenBracket()
        {
            //Check for <any_string_of_characters)
            while (_Current != null && _Current.Type != TokenType.EOF)
            {
                Next();

                //Check if foward/slash
                if (_Current.Type == TokenType.ForwardSlash)
                {
                    //This means the tag is about to close
                    Next();

                    //Expect name
                    if (_Current.Type != TokenType.Identifier)
                    {
                        //TODO: Create ExpectedIdentifierException
                        Abort($"Expected Identifier at Line {_Current.Line} Position {_Current.Position}");
                        return;
                    }

                    //Expect matching name
                    if (_Current.Text != _topOfStack?.tagTypeName)
                    {
                        Abort($"Tag Leveling for {_topOfStack?.tagTypeName} does not match with closing tag " +
                            $"{_Current.Text}");
                        return;
                    }

                    Next();

                    //Close Bracket is now expected
                    if (_Current.Type != TokenType.CloseBracket)
                    {
                        //TODO: Create ExpectedIdentifierException
                        Abort($"Expected CloseBracket Line {_Current.Line} Position {_Current.Position}");
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
        }
        
        private void HandleCloseBracket()
        {
            if (_topOfStack!.isSelfClosing)
            {
                CloseCurrentTag();
                return;
            }

            _topOfStack.tagState = TagEvaluationState.Open;
            _topOfStack._parameterInfo = _cachedTagParameterInfo;
            _cachedTagParameterInfo = null;

            if (Peek(1)?.Type == TokenType.OpenBracket) return;

            ChangeEvaluationState(ParserEvaluationState.TagValue);
        }

        private void HandleForwardSlash()
        {
            if (Peek(1)?.Type != TokenType.CloseBracket) return;
            _topOfStack!.isSelfClosing = true;
        }

        private void HandlePound()
        {
            if (_topOfStack!.tagState != TagEvaluationState.OnParameters) return;
            _topOfStack.isSettingFlag = true;
        }

        private void HandleIdentifier()
        {
            if (_topOfStack!.tagState != TagEvaluationState.OnParameters) return;

            _cachedTagParameterInfo ??= new TagParameterInfo();

            if (_topOfStack!.isSettingFlag == true)
            {
                if (Peek(1, true)?.Type != TokenType.DoubleColon)
                {
                    //This means this is a Flag for the tag
                    _cachedTagParameterInfo.flagParameters.Add(_Current?.Text!);
                    _topOfStack!.isSettingFlag = false;
                    return;
                }

                Abort($"A flag should not expect the token {Peek(1)?.Text}");
                return;
            }

            TagParameter newParameter = new TagParameter()
            {
                name = _Current?.Value?.ToString()
            };

            _cachedTagName = newParameter?.name;

            _cachedTagParameterInfo.paramters.Add(_cachedTagName!, newParameter!);
        }

        private void HandleDoubleColon()
        {
            var parameterName = _cachedTagParameterInfo!.paramters[_cachedTagName!].name;
            var expected = Peek(1);

            if (expected?.Type! == TokenType.Char ||
                expected?.Type! == TokenType.String ||
                expected?.Type! == TokenType.Number ||
                expected?.Type! == TokenType.EmptyString ||
                expected?.Type! == TokenType.Identifier)
            {
                Next();
                _cachedTagParameterInfo.paramters[_cachedTagName!].value = _Current?.Value;
                return;
            }

            if (expected?.Type == TokenType.DollarSign)
            {
                Next();

                expected = Peek(1);

                if (expected?.Type != TokenType.Identifier)
                {
                    Abort($"Reference Error at Line {_Current?.Line} Position {_Current?.Position}: Expected Identifier: {fileTarget}");
                    return;
                }

                Next();

                _cachedTagParameterInfo.paramters[_cachedTagName!].value = _Current?.Value;
                _cachedTagParameterInfo.paramters[_cachedTagName!].isReferencing = true;

                return;
            }

            Abort($"Invalid assignment to parameter: {parameterName} at Line {_Current?.Line} Position {_Current?.Position}: {fileTarget}");
        }

        private void HandleString()
        {
            if (_topOfStack?.tagState != TagEvaluationState.Open) return;
            _topOfStack.value = _Current?.Value;
        }

        private void HandleTagValue()
        {
            if (_Current?.Type == TokenType.OpenBracket && Peek(1, true)?.Type == TokenType.ForwardSlash)
            {
                ChangeEvaluationState(ParserEvaluationState.Tag);
                _position--;
                return;
            }

            _tagValueStringBuilder.Append(_Current?.Type == TokenType.String ?
                $"\"{_Current?.Text}\"" :
                _Current?.Text);
        }
        #endregion
    }
}