using System.Text;
using XVNML.Core.Dialogue;
using XVNML.Core.Lexer;
using XVNML.Core.Tags;

namespace XVNML.Core.Parser
{
    //Parser State
    public enum ParserEvaluationState
    {
        //Normal Parsing of XVNML tag
        Tag,

        //Set conditions before following through with the parsing
        Preprocessing,

        //References, anything starting with a dollarsign $
        Referencing,

        //If a Dialogue Tag is defined
        //Parse the contents inside
        Dialogue,

        //After a tag being open
        //it'll evaluate anything between open and close tags
        TagValue,

        //If using the Script tag, parse the language
        //target. (not supported at this time)
        Script
    }

    //Parse Resource State
    public enum ParseResourceState
    {
        Internal,

        //If a .xvnml is definied as a src for things
        //like scenes, casts, or audio,
        //It'll evaluate the contents of that files
        //and put information in the main .xvnml object
        External
    }

    internal class Parser
    {
        #region Fields and Properties
        protected static int Position = -1;

        protected static string? FileTarget;

        private static bool _Conflict;

        static Tokenizer? Tokenizer = null;

        private static ParserEvaluationState EvaluationState = ParserEvaluationState.Tag;
        private static ParseResourceState EvaluationResourceState = ParseResourceState.Internal;

        static Stack<TagBase> TagStackFrame = new Stack<TagBase>();
        static int TagLevel => TagStackFrame.Count - 1;

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
        private static StringBuilder _TagValueStringBuilder = new StringBuilder();
        #endregion

        public static void Parse()
        {
            if (FileTarget == null)
                throw new NullReferenceException("FileTarget cannot be null when parsing.");

            Tokenizer ??= new Tokenizer(FileTarget, TokenizerReadState.IO, out _Conflict);


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
                    var resultTest = _TagValueStringBuilder.ToString();
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
                                    var newTag = (TagBase?)TagConverter.Convert(Current.Text!);
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

                                    Console.WriteLine($"New tag created named: {newTag.tagTypeName}{(newTag.parentTag != null ? $": Parent => {newTag.parentTag.tagTypeName}" : $": No Parent")}");
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
                                Console.WriteLine($"Expected Identifier at Line {Current?.Line} Position {Current?.Position}");
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

                                TagParameter? newParameter = new TagParameter()
                                {
                                    name = Current?.Value?.ToString()
                                };

                                //Cache string name
                                cachedTagName = newParameter?.name;

                                cachedTagParameterInfo.paramters.Add(cachedTagName!, newParameter!);
                            }
                            continue;

                        case TokenType.EOF:
                            Console.WriteLine("Parsing of XVNML Document now complete.");
                            return;

                        case TokenType.DoubleColon:
                            int length = cachedTagParameterInfo!.totalParameters;
                            var parameterName = cachedTagParameterInfo.paramters[cachedTagName!].name;

                            //Go next, and grab the value for the TagParameter
                            Next();

                            //Make sure it's valid input
                            //Expecting the following:
                            if (Current?.Type == TokenType.Char ||
                                Current?.Type == TokenType.String ||
                                Current?.Type == TokenType.Number ||
                                Current?.Type == TokenType.EmptyString ||
                                Current?.Type == TokenType.Identifier)
                            {
                                cachedTagParameterInfo.paramters[cachedTagName!].value = Current.Value;
                                continue;
                            }

                            //Find a reference
                            if (Current?.Type == TokenType.DollarSign)
                            {
                                Next();
                                //Expect Identifer
                                if (Current?.Type != TokenType.Identifier)
                                {
                                    Console.WriteLine($"Reference Error at Line {Current?.Line} Position {Current?.Position}: Expected Identifier");
                                    return;
                                }
                                cachedTagParameterInfo.paramters[cachedTagName!].value = Current.Value;
                                continue;
                            }

                            Console.WriteLine($"Invalid assignment to parameter: {parameterName} at Line {Current?.Line} Position {Current?.Position}");
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

            TopOfStack.OnResolve();

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