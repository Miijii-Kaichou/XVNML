using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Lexer;
using XVNML.Core.Tags;

internal delegate void ReferenceLinkerHandler(TagBase? sender, TagBase? referencingTag, Type type);
namespace XVNML.Core.Dialogue
{
    internal sealed class DialogueParser
    {
        public string Source { get; }

        private DialogueParserMode CurrentMode;

        //For any persistence
        private static (string? Character, string? Expression, string? Voice)? _PreviousCast;
        private readonly (string? Character, string? Expression, string? Voice) _DefaultCast = (string.Empty, null, null);

        //private CommandState? CommandState;

        private bool? IsReadingLineContent = false;
        private bool? IsCreatingPromptChoices = false;

        private static Tokenizer? Tokenizer;
        private static bool _Conflict;
        private static int _position = -1;

        private static SyntaxToken? Current => Peek(0, true);

        public bool ReadyToBuild { get; private set; }
        public bool FindFirstLine { get; private set; }

        private bool _evaluatingCast = false;

        public DialogueParser(string dialogueSource, out DialogueScript output)
        {
            _position = -1;
            _Conflict = false;
            ReadyToBuild = false;
            FindFirstLine = false;
            _evaluatingCast = false;
            CurrentMode = DialogueParserMode.Dialogue;
            Source = dialogueSource;

            output = ParseDialogue() ?? default!;
        }

        private DialogueScript? ParseDialogue()
        {
            Tokenizer = new Tokenizer(Source, TokenizerReadState.Local, out _Conflict);
            return CreateDialogueOutput();
        }

        private DialogueScript? CreateDialogueOutput()
        {
            if (Tokenizer == null) return null;

            DialogueScript output = new DialogueScript();
            DialogueLine line = new DialogueLine();
            int linesCollected = -1;
            Stack<((DialogueLine, int), Stack<string>)>? promptCacheStack = new Stack<((DialogueLine, int), Stack<string>)>();
            // Used to define a Cast Signature (based on documentation)
            var castSignatureCollection = new List<SyntaxToken>();
            var castSignatureString = new StringBuilder();

            for (int i = 0; i < Tokenizer.Length; i++)
            {
                if (_Conflict) return null;

                Next();

                SyntaxToken? token = Current;

                if (IsReadingLineContent!.Value && CurrentMode == DialogueParserMode.Dialogue)
                {

                    ReadyToBuild = false;
                    line?.AppendContent(token?.Type == TokenType.String ? $"\"{token?.Text!}\"" : token?.Text!);
                    IsReadingLineContent = token?.Type != TokenType.DoubleOpenBracket &&
                    token?.Type != TokenType.OpenBracket;
                    ReadyToBuild = !IsReadingLineContent.Value;
                    if (IsReadingLineContent.Value)
                        continue;

                }

                if (IsReadingLineContent.Value && CurrentMode == DialogueParserMode.Prompt)
                {

                    ReadyToBuild = false;
                    line?.AppendContent(token?.Type == TokenType.String ? $"\"{token?.Text!}\"" : token?.Text!);
                    IsReadingLineContent = token?.Type != TokenType.DoubleCloseBracket;
                    ReadyToBuild = !IsReadingLineContent.Value;
                    if (IsReadingLineContent.Value)
                        continue;
                }

                switch (token?.Type)
                {
                    //Denote the start of a dialoge
                    case TokenType.At:
                        if (FindFirstLine) FindFirstLine = !FindFirstLine;

                        TryPopFromPromptCacheStack(promptCacheStack!, linesCollected + 1, ref output);

                        castSignatureCollection = new List<SyntaxToken>();
                        castSignatureString = new StringBuilder();
                        ChangeDialogueParserMode(DialogueParserMode.Dialogue);
                        _evaluatingCast = true;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Denote the start of a prompt
                    case TokenType.Prompt:
                        if (FindFirstLine) FindFirstLine = !FindFirstLine;
                        TryPopFromPromptCacheStack(promptCacheStack!, linesCollected + 1, ref output);

                        castSignatureCollection = new List<SyntaxToken>();
                        castSignatureString = new StringBuilder();
                        ChangeDialogueParserMode(DialogueParserMode.Prompt);
                        _evaluatingCast = true;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //While in At or Prompt Phase, get information between
                    //> (which should only be Cast>Expression>Voice)
                    // or (Cast>Expression/Voice)
                    case TokenType.OpenBracket:
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Only validate at the start or in between {} delimiters
                    case TokenType.Identifier:
                        if (!_evaluatingCast)
                        {
                            if (promptCacheStack?.Count() != 0 &&
                                promptCacheStack?.Peek()!.Item1.Item1.SignatureInfo?.CurrentRole == Role.Interrogative &&
                                IsCreatingPromptChoices == true)
                            {
                                var expectedType = Peek(1)?.Type;
                                if (expectedType != TokenType.CloseParentheses)
                                    throw new InvalidDataException($"Expected a {TokenType.CloseParentheses} at Line {token?.Line}, Position {token?.Position + 1}");
                                promptCacheStack?.Peek().Item2.Push(token?.Text!);
                            }
                            continue;
                        }
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        //Otherwise, the identifier will be in-between brackets
                        continue;

                    //Starting of internal method call, reference, or macro
                    case TokenType.OpenCurlyBracket:

                        continue;

                    //Acts as delimiter to known dependencies (like the dot operator .)
                    case TokenType.DoubleColon:
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //At start, means use the same cast character
                    case TokenType.Star:
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Ending of internal method call
                    case TokenType.CloseCurlyBracket:
                        continue;

                    //End of current line. If this is the last line in the dialogue set, it is interpretted
                    //as <<
                    case TokenType.CloseBracket:
                        // In this state, check if the next set of character is E: or V:
                        // If it's E: or none, expression is being evaluated
                        // Otherwise, if it's a V:, a vocal is being evaluated.
                        // This also means overriding the evaluationState from Expression
                        // to Voice
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    case TokenType.AnonymousCastSymbol:
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Leap out of the prompt or dialogue set (this denotes that this is the end of
                    //of dialogue.
                    case TokenType.DoubleOpenBracket:
                        if (_evaluatingCast) throw new InvalidDataException($"{token.Text} not expected");
                        if (promptCacheStack?.Count == 0) continue;
                        if (promptCacheStack?.Peek().Item1.Item1.SignatureInfo?.CurrentRole != Role.Interrogative) continue;
                        if (promptCacheStack?.Peek().Item2.Count != 0 && promptCacheStack?.Peek().Item2.Peek() != null)
                            _ = promptCacheStack?.Peek().Item2.Pop();
                        continue;


                    //This during a cast definition is for setting an expression (or voice) as null
                    //This can also bee used to put a pause, and have the dialogue continue playing after
                    //user feedback (as opposed to < where after the dialogue, the whole test would clear).
                    case TokenType.DoubleCloseBracket:
                        if (!_evaluatingCast) continue;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Direct reference to something (whatever that may be)
                    case TokenType.DollarSign:
                        continue;

                    //This denotes an macro call (especially when there are dependencies)
                    case TokenType.Exclamation:
                        continue;

                    //Valid in-between {} delimiters to seperate method calls
                    case TokenType.Line:
                        continue;

                    //Escape Character Prefix
                    case TokenType.ForwardSlash:
                        continue;

                    //You will mainly find a string within curly brackets, or being
                    //used as a choice for a prompt.
                    case TokenType.String:
                        if (promptCacheStack?.Count != 0 &&
                            promptCacheStack?.Peek().Item1.Item1.SignatureInfo?.CurrentRole == Role.Interrogative &&
                            IsCreatingPromptChoices == true)
                        {
                            var expectedType = Peek(1)?.Type;
                            if (expectedType != TokenType.CloseParentheses)
                                throw new InvalidDataException($"Expected a {TokenType.CloseParentheses} at Line {token?.Line}, Position {token?.Position + 1}");
                            promptCacheStack?.Peek().Item2?.Push(token?.Text!);
                        }
                        continue;

                    case TokenType.OpenParentheses:
                        if (!_evaluatingCast)
                        {
                            if (promptCacheStack?.Count != 0 &&
                            promptCacheStack?.Peek().Item1.Item1.SignatureInfo?.CurrentRole == Role.Interrogative)
                            {
                                var expected = Peek(1)?.Type;
                                IsCreatingPromptChoices = expected == TokenType.Identifier || expected == TokenType.String;
                                if (promptCacheStack?.Peek().Item2.Count != 0 && promptCacheStack?.Peek().Item2.Peek() != null)
                                    _ = promptCacheStack?.Peek().Item2.Pop();
                                continue;
                            }
                            if (!IsCreatingPromptChoices == true) throw new InvalidDataException($"Expected a {TokenType.String} or {TokenType.Identifier} at Line {token?.Line}, Position {token?.Position + 1}");
                        }
                        throw new InvalidDataException($"Invalid Token at Line {token?.Line}, Position {token?.Position}");

                    case TokenType.CloseParentheses:
                        if (!_evaluatingCast)
                        {
                            if (promptCacheStack?.Count != 0 &&
                            promptCacheStack?.Peek().Item1.Item1.SignatureInfo?.CurrentRole == Role.Interrogative &&
                            IsCreatingPromptChoices == true)
                            {
                                promptCacheStack?.Peek().Item1.Item1.SetNewChoice(promptCacheStack?.Peek().Item2.Peek()!, linesCollected + 1);
                                FindFirstLine = true;
                            }
                            continue;
                        }
                        throw new InvalidDataException($"Invalid Token at Line {token?.Line}, Position {token?.Position}");

                    case TokenType.WhiteSpace:
                        if (_evaluatingCast)
                        {
                            ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                            DefineCastSignature(castSignatureString, castSignatureCollection, CurrentMode, out (string? Character, string? Expression, string? Voice) cachedData, out CastMemberSignature definedSignature);

                            //Create a line
                            line = new DialogueLine()
                            {
                                Mode = (DialogueLineMode)CurrentMode,
                                CastName = definedSignature.IsPersistent == true ? (_PreviousCast ?? _DefaultCast).Character : cachedData.Character,
                                Expression = cachedData.Expression,
                                Voice = cachedData.Voice,
                                SignatureInfo = definedSignature
                            };
                            _evaluatingCast = false;
                            IsReadingLineContent = line != null;

                            if (definedSignature.IsPrimitive == true) _PreviousCast = cachedData;

                            continue;
                        }
                        if (!ReadyToBuild || FindFirstLine) continue;
                        line?.FinalizeBuilder();

                        output.ComposeNewLine(line);

                        linesCollected++;
                        if (line?.SignatureInfo?.CurrentRole != Role.Interrogative) continue;
                        promptCacheStack?.Push(((line, linesCollected), new Stack<string>()));
                        continue;
                }
            }
            return output;
        }

        private static void TryPopFromPromptCacheStack(Stack<((DialogueLine, int), Stack<string>)>? promptCacheStack, int linePos, ref DialogueScript output)
        {
            if (promptCacheStack == null) return;
            if (promptCacheStack?.Count != 0 &&
                                        promptCacheStack?.Peek().Item1.Item1 != null &&
                                        promptCacheStack?.Peek().Item2.Count == 0)
            {
                var prompt = promptCacheStack.Pop();
                var dialogueData = prompt.Item1.Item1;
                output.Lines[prompt.Item1.Item2].SetEndPointOnAllChoices(linePos);
                _PreviousCast = (dialogueData.CastName, dialogueData.Expression, dialogueData.Voice);
            }
        }

        private static void ConsumeCastSigData(List<SyntaxToken>? castSignatureCollection, StringBuilder castSignatureString, SyntaxToken? token)
        {
            castSignatureCollection?.Add(token ?? default!);
            castSignatureString.Append(token?.Text);
        }

        /// <summary>
        /// Defines the cast signature based on the given string
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="parsingMode"></param>
        private void DefineCastSignature(StringBuilder signatureString, List<SyntaxToken> queue, DialogueParserMode parsingMode, out (string? Character, string? Expression, string? Voice) output, out CastMemberSignature signatureDataOutput)
        {
            int evaluationPos = -1;
            int evaluationValue = 0;
            const string ExpressionCode = "E::";
            const string VoiceCode = "V::";

            void Next() => evaluationPos++;

            //Finalize Signature at the end
            CastMemberSignature signature = new CastMemberSignature();

            CastEvaluationMode mode = CastEvaluationMode.Expression;
            void RaiseCastEvaluationState() => mode = (CastEvaluationMode)(++evaluationValue);
            void LowerCastEvaluationState() => mode = (CastEvaluationMode)(--evaluationValue);

            output.Character = null;
            output.Expression = null;
            output.Voice = null;

            Next();

            var prefix = queue[evaluationPos];

            // First, evaluate token. Define the role.
            signature.CurrentRole = prefix.Type == TokenType.At ? Role.Declarative :
                                    prefix.Type == TokenType.Prompt ? Role.Interrogative :
                                    Role.Undefined;

            Next();

            var characterSymbol = queue[evaluationPos];

            // Based on what's given, decide if it's a Normal, Narrative, Anonymous, or Persistent
            signature.IsPrimitive = characterSymbol.Type == TokenType.Identifier;
            signature.IsNarrative = characterSymbol.Type == TokenType.WhiteSpace ||
                                      characterSymbol.Type == TokenType.CloseBracket;
            signature.IsAnonymous = characterSymbol.Type == TokenType.AnonymousCastSymbol;
            signature.IsPersistent = characterSymbol.Type == TokenType.Star;

            if (signature.IsNarrative == true)
            {
                output.Character = null;

                //Proceed to next symbol. If we get a white space, that is the end
                //of the definition.
                //However, if we get a Identifier, we know it's a full one.
                //We expect a voice on this one.
                //It's okay to have nothing, but whatever is there is always a voice.
                // For this, signatures are always partial
                signature.IsPartial = characterSymbol.Type == TokenType.WhiteSpace;
                signature.IsFull = characterSymbol.Type == TokenType.CloseBracket;

                if (signature.IsPartial == true)
                {
                    output.Voice = null;
                    output.Expression = null;
                    signatureDataOutput = signature;
                    return;
                }

                Next();

                if (queue[evaluationPos].Text == VoiceCode.First().ToString()
                    && queue[evaluationPos + 1].Type == TokenType.DoubleColon) { Next(); Next(); }
                if (queue[evaluationPos].Text == ExpressionCode.First().ToString()
                    && queue[evaluationPos + 1].Type == TokenType.DoubleColon)
                    throw new NotSupportedException("A Narrative Cast does not support Expressions");

                output.Voice = queue[evaluationPos]?.Text;
                output.Expression = null;

                //We are not expecting any information anymore.
                signatureDataOutput = signature;
                return;
            }

            RaiseCastEvaluationState();

            // Luckily, the Narrative Cast Signature is the only one that's different
            // from the rest of the signatures. Everything else follows the same procedure.
            // Check if there is a whitespace ahead. If there is. No need to evaluate further
            Next();

            if (queue[evaluationPos].Type == TokenType.WhiteSpace)
            {
                output.Character = signature.IsPersistent == true ? (_PreviousCast ?? _DefaultCast).Character : characterSymbol.Text; ;
                signature.IsPartial = true;
                signatureDataOutput = signature;
                return;
            }

            while (queue[evaluationPos].Type != TokenType.WhiteSpace)
            {
                if (queue[evaluationPos].Type == TokenType.DoubleColon)
                {
                    Next();
                    continue;
                }

                if (queue[evaluationPos].Type == TokenType.DoubleCloseBracket)
                {
                    Next();
                    RaiseCastEvaluationState();
                    continue;
                }

                // Otherwise, validate that there's a > in front before proceeding
                if (queue[evaluationPos].Type == TokenType.CloseBracket)
                {
                    Next();
                    if (queue[evaluationPos].Type == TokenType.DoubleColon)
                    {
                        continue;
                    }

                    var isQualifiedToken = queue[evaluationPos].Type == TokenType.Identifier ||
        queue[evaluationPos].Type == TokenType.Number;


                    // We have 2 outputs for this one: Expression or Voice
                    // Check for E:: or V::
                    if (isQualifiedToken &&
                        (queue[evaluationPos].Text == ExpressionCode[0].ToString() ||
                        queue[evaluationPos].Text == VoiceCode[0].ToString()))
                    {
                        mode = queue[evaluationPos].Text == ExpressionCode[0].ToString() ? CastEvaluationMode.Expression : CastEvaluationMode.Voice;

                        //Otherwise, see where we are. If one of them is null,
                        //check if the other one isn't. If it isn't, we have to fill
                        //information for the one that doesnt' have anything.
                        if (output.Expression == null && output.Voice != null && isQualifiedToken)
                        {
                            signature.IsInversed = true;
                            output.Expression = output.Voice;
                            Next();
                            continue;
                        }

                        if (output.Voice == null && output.Expression != null && isQualifiedToken)
                        {
                            signature.IsInversed = true;
                            output.Voice = output.Expression;
                            Next();
                            continue;
                        }

                        Next();
                        continue;
                    }

                }

                //Otherwise, check the phase
                if (mode == CastEvaluationMode.Expression)
                {
                    output.Expression = queue[evaluationPos].Text;
                    RaiseCastEvaluationState();
                    Next();
                    continue;
                }

                if (mode == CastEvaluationMode.Voice)
                {
                    output.Voice = queue[evaluationPos].Text;
                    LowerCastEvaluationState();
                    Next();
                    continue;
                }
            }
            output.Character = (signature.IsAnonymous == true ||
                                signature.IsNarrative == true) ? string.Empty :
                                signature.IsPersistent == true ? (_PreviousCast ?? _DefaultCast).Character : characterSymbol.Text;
            signature.IsPartial = (output.Expression != null && output.Voice == null) ||
                                    (output.Expression == null && output.Voice != null);
            signature.IsFull = !signature.IsPartial;
            signatureDataOutput = signature;
            return;
        }

        public void ChangeDialogueParserMode(DialogueParserMode mode) => CurrentMode = mode;


        private static SyntaxToken? Peek(int offset, bool includeSpaces = false)
        {
            if (Tokenizer == null) return null;
            try
            {
                var token = Tokenizer[_position + offset];

                while (true)
                {
                    token = Tokenizer[_position + offset];

                    if ((token?.Type == TokenType.WhiteSpace && includeSpaces == false) ||
                        token?.Type == TokenType.SingleLineComment ||
                        token?.Type == TokenType.MultilineComment)
                    {
                        _position++;
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
            _position++;
            return Current;
        }

        private static SyntaxToken? StepBack()
        {
            _position--;
            return Current;
        }
    }
}