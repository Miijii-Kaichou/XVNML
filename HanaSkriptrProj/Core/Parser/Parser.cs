using System.Text;
using XVNML.Core.Lexer;
using XVNML.Core.Parser.Enums;
using XVNML.Core.Tags;

namespace XVNML.Core.Parser
{
    internal class Parser
    {
        #region Fields and Properties
        protected static int Position = -1;

        internal static string? FileTarget;

        private static bool _Conflict;

        static Tokenizer? Tokenizer = null;

        private static ParserEvaluationState EvaluationState = ParserEvaluationState.Tag;
        private static ParseResourceState EvaluationResourceState = ParseResourceState.Internal;

        static readonly Stack<TagBase> TagStackFrame = new();
        static int TagLevel => TagStackFrame.Count - 1;
        public static Action Completed;

        static TagBase? TopOfStack
        {
            get
            {
                if (TagStackFrame.Count == 0) return null;
                return TagStackFrame.Peek();
            }
        }

        internal static TagBase? RootTag;

        //Temporary Cache
        static TagParameterInfo? cachedTagParameterInfo;
        static string? cachedTagName;

        public static void SetTarget(ReadOnlySpan<char> fileTarget) => FileTarget = fileTarget.ToString();

        private static SyntaxToken? Current => Peek(0, true);

        public static bool ExpectingMoreTagParameters { get; private set; }

        //This is important for anything in between tags (for example) <title>Hi</title>
        private static StringBuilder _TagValueStringBuilder = new();
        private static readonly Queue<Action> _SolvingQueue = new();

        public event ReferenceLinkerHandler onParserComplete;
        #endregion

        public static void Parse()
        {
            Position = -1;
            _Conflict = false;
            EvaluationResourceState = ParseResourceState.Internal;
            EvaluationState = ParserEvaluationState.Tag;
            RootTag = null;
            cachedTagName = null;
            cachedTagParameterInfo = null;
            ExpectingMoreTagParameters = false;
            _TagValueStringBuilder = new StringBuilder();
            if (FileTarget == null)
                throw new NullReferenceException("FileTarget cannot be null when parsing.");

            Tokenizer = new Tokenizer(FileTarget, TokenizerReadState.IO, out _Conflict);


            if (_Conflict)
            {
                Console.WriteLine("There was conflict with tokenizer. Failed to parse xvnml file");
                return;
            }

            //Evaluate
            AnalyzeTokens();
        }

        private static SyntaxToken? Peek(int offset, bool includeSpaces = false)
        {
            if (Tokenizer == null) return null;
            try
            {
                var token = Tokenizer[Position + offset];

                while (true)
                {
                    token = Tokenizer[Position + offset] ?? default!;

                    if ((token.Type == TokenType.WhiteSpace && includeSpaces == false) ||
                        token.Type == TokenType.SingleLineComment ||
                        token.Type == TokenType.MultilineComment)
                    {
                        Position++;
                        continue;
                    }
                    return token;
                }
            }
            catch
            {
                return Tokenizer[Tokenizer.Length];
            }
        }

        private static SyntaxToken? Next()
        {
            Position++;
            return Current;
        }

        private static void AnalyzeTokens()
        {
            //Find starting of tags. With each tag found,
            //evaluate the Parameters/Flags, if it's self closing or not
            //it's children elements, and its values
            if (Tokenizer == null) return;

            for (int i = 0; i < Tokenizer.Length; i++)
            {
                //If there was a conflict in resolving types, stop parsing;
                if (_Conflict) return;

                Next();

                SyntaxToken? token = Current;

                if (EvaluationState == ParserEvaluationState.TagValue || EvaluationState == ParserEvaluationState.Dialogue)
                {

                    if (Current?.Type == TokenType.OpenBracket && Peek(1, true)?.Type == TokenType.ForwardSlash)
                    {
                        ChangeEvaluationState(ParserEvaluationState.Tag);
                        Position--;
                        continue;
                    }

                    _TagValueStringBuilder.Append(Current?.Type == TokenType.String ?
                        $"\"{Current?.Text}\"" :
                        Current?.Text);
                }

                if (EvaluationState == ParserEvaluationState.Tag)
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

                                if (Current == null || Current.Type == TokenType.EOF) break;

                                //Check if foward/slash
                                if (Current.Type == TokenType.ForwardSlash)
                                {
                                    //This means the tag is about to close
                                    Next();

                                    //Expect name
                                    if (Current.Type != TokenType.Identifier)
                                    {
                                        //TODO: Create ExpectedIdentifierException
                                        Console.WriteLine($"Expected Identifier at Line {Current.Line} Position {Current.Position}");
                                        return;
                                    }

                                    //Expect matching name
                                    if (Current.Text != TopOfStack?.tagTypeName)
                                    {
                                        Console.WriteLine($"Tag Leveling for {TopOfStack?.tagTypeName} does not match with closing tag " +
                                            $"{Current.Text}");
                                        return;
                                    }

                                    Next();

                                    //Close Bracket is now expected
                                    if (Current.Type != TokenType.CloseBracket)
                                    {
                                        //TODO: Create ExpectedIdentifierException
                                        Console.WriteLine($"Expected CloseBracket Line {Current.Line} Position {Current.Position}");
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
                                if (Current.Type == TokenType.Identifier)
                                {

                                    if (Current.Text == "date")
                                    {
                                        _ = Current.Text;
                                    }

                                    //This means that we are creating a tag
                                    var newTag = TagConverter.Convert(Current.Text!);
                                    newTag!.tagTypeName = Current.Text;

                                    //If top is still open, it means we're nesting
                                    if (TopOfStack != null &&
                                        TopOfStack.tagState == TagEvaluationState.Open)
                                    {
                                        TopOfStack.elements ??= new List<TagBase>();
                                        TopOfStack.elements.Add(newTag);
                                        newTag.parentTag = TopOfStack;
                                    }

                                    TagStackFrame.Push(newTag);
                                    break;
                                }
                            }
                            continue;

                        case TokenType.CloseBracket:
                            //Change the stat of the tag if there is any
                            if (TopOfStack!.isSelfClosing)
                            {
                                CloseCurrentTag();
                                continue;
                            }

                            TopOfStack.tagState = TagEvaluationState.Open;
                            TopOfStack.parameterInfo = cachedTagParameterInfo;
                            cachedTagParameterInfo = null;

                            if (Peek(1)?.Type == TokenType.OpenBracket) continue;

                            ChangeEvaluationState(ParserEvaluationState.TagValue);
                            continue;

                        case TokenType.Comma:
                            continue;

                        case TokenType.Line:
                            //We expect an identifier
                            if (Peek(1)?.Type != TokenType.Identifier)
                            {
                                Console.WriteLine($"Expected Identifier at Line {Current?.Line} Position {Current?.Position}: {FileTarget}");
                                return;
                            }
                            ExpectingMoreTagParameters = true;
                            continue;

                        case TokenType.ForwardSlash:
                            //Check if a CloseBracket exceeds that
                            //If it is, mark as SelfClosing
                            if (Peek(1)?.Type == TokenType.CloseBracket)
                            {
                                //The tag is self-closing
                                TopOfStack!.isSelfClosing = true;
                            }
                            continue;


                        case TokenType.Pound:
                            //There will have to be a unique feature
                            //where flags are set prior to calculations.

                            continue;

                        case TokenType.Identifier:
                            //After a tag has been set, check it's status
                            if (TopOfStack!.tagState == TagEvaluationState.OnParameters)
                            {

                                //Set off ExpectingMoreParameters
                                ExpectingMoreTagParameters = false;

                                if (cachedTagParameterInfo == null)
                                    cachedTagParameterInfo = new TagParameterInfo();

                                //Check if flag
                                if (Peek(1)?.Type != TokenType.DoubleColon)
                                {
                                    //This means this is a Flag for the tag
                                    cachedTagParameterInfo.flagParameters.Add(Current?.Text!);
                                    continue;
                                }

                                TagParameter? newParameter = new()
                                {
                                    name = Current?.Value?.ToString()
                                };

                                //Cache string name
                                cachedTagName = newParameter?.name;

                                cachedTagParameterInfo.paramters.Add(cachedTagName!, newParameter!);
                            }
                            continue;

                        case TokenType.EOF:
                            Console.WriteLine($"Parsing of XVNML Document now complete.: {FileTarget}");
                            RunReferenceSolveProcedure();
                            return;

                        case TokenType.DoubleColon:
                            {
                                int length = cachedTagParameterInfo!.totalParameters;
                                var parameterName = cachedTagParameterInfo.paramters[cachedTagName!].name;


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
                                    cachedTagParameterInfo.paramters[cachedTagName!].value = Current.Value;
                                    continue;
                                }

                                //Find a reference
                                if (expected?.Type == TokenType.DollarSign)
                                {
                                    Next();

                                    //Expect Identifer
                                    if (Peek(1).Type != TokenType.Identifier)
                                    {
                                        Console.WriteLine($"Reference Error at Line {Current?.Line} Position {Current?.Position}: Expected Identifier: {FileTarget}");
                                        return;
                                    }
                                    cachedTagParameterInfo.paramters[cachedTagName!].value = Current.Value;
                                    cachedTagParameterInfo.paramters[cachedTagName!].isReferencing = true;

                                    continue;
                                }

                                Console.WriteLine($"Invalid assignment to parameter: {parameterName} at Line {Current?.Line} Position {Current?.Position}: {FileTarget}");
                            }
                            return;

                        case TokenType.String:
                            if (TopOfStack?.tagState == TagEvaluationState.Open)
                            {
                                //Add as tag value
                                TopOfStack.value = Current?.Value;
                            }
                            continue;

                        case TokenType.At:
                            if (EvaluationState == ParserEvaluationState.TagValue) ChangeEvaluationState(ParserEvaluationState.Dialogue);
                            continue;
                        default:
                            break;
                    }
                }
                #endregion
            }
        }

        private static void RunReferenceSolveProcedure()
        {
            while (_SolvingQueue.Count != 0)
            {
                var nextSolvee = _SolvingQueue.Dequeue();

                nextSolvee();
            }
        }

        internal static void QueueForReferenceSolve(Action method)
        {
            _SolvingQueue.Enqueue(method);
        }

        public static void Abort()
        {
            throw new Exception("Parser has aborted.");
        }

        private static void CloseCurrentTag()
        {
            //Now the object can be closed
            TopOfStack!.tagState = TagEvaluationState.Close;
            TopOfStack!.parameterInfo ??= cachedTagParameterInfo;

            cachedTagParameterInfo = null;
            cachedTagName = string.Empty;

            if (_TagValueStringBuilder.Length > 0)
            {
                TopOfStack.value = _TagValueStringBuilder.ToString().Trim('\n');
                _TagValueStringBuilder.Clear();
            }

            var dirInfo = new DirectoryInfo(FileTarget?.ToString());
            var fileOrigin = dirInfo.Parent?.ToString();
            TopOfStack.OnResolve(fileOrigin);

            RootTag = TagStackFrame.Pop();
        }

        private static void ChangeEvaluationState(ParserEvaluationState state)
        {
            EvaluationState = state;
        }

        private static void ChangeResourceState(ParseResourceState state)
        {
            EvaluationResourceState = state;
        }
    }
}