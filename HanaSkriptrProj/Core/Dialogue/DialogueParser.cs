using System.Diagnostics.SymbolStore;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Lexer;

namespace XVNML.Core.Dialogue
{

    internal class DialogueParser
    {
        public string Source { get; }

        private DialogueParserMode CurrentMode;

        private CastEvaluationMode CastEvaluationState;

        //For any persistence
        (string? Character, string? Expression, string? Voice) Cache;

        //private CommandState? CommandState;

        private bool? IsReadingLineContent = false;

        private int castStateValue = 0;

        private static Tokenizer? Tokenizer;
        private static bool _Conflict;
        private static int _position = -1;

        private static SyntaxToken? Current => Peek(0, true);

        private bool _evaluatingCast = false;

        public DialogueParser(string dialogueSource, out DialogueScript output)
        {
            Source = dialogueSource;
            Console.WriteLine(Source);
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

            // Used to define a Cast Signature (based on documentation)
            var castSignatureCollection = new List<SyntaxToken>();
            var castSignatureString = new StringBuilder();

            var isEscaping = false;

            for (int i = 0; i < Tokenizer.Length; i++)
            {
                if (_Conflict) return null;

                Next();

                SyntaxToken? token = Current;

                Console.WriteLine(token?.Text);

                if (IsReadingLineContent ?? false)
                {
                    line?.AppendContent(token?.Text!);
                    IsReadingLineContent = token?.Type != TokenType.OpenBracket;
                    continue;
                }

                switch (token?.Type)
                {
                    //Denote the start of a dialoge
                    case TokenType.At:
                        castSignatureCollection = new List<SyntaxToken>();
                        castSignatureString = new StringBuilder();
                        ChangeDialogueParserMode(DialogueParserMode.Dialogue);
                        _evaluatingCast = true;
                        ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                        continue;

                    //Denote the start of a prompt
                    case TokenType.Prompt:
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
                        if (Peek(1)?.Type == TokenType.WhiteSpace)
                        {
                            //Invalid order
                            return output;
                        }
                        continue;

                    //Only validate at the start or in between {} delimiters
                    case TokenType.Identifier:
                        if (!_evaluatingCast) continue;
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

                    case TokenType.WhiteSpace:
                        if (_evaluatingCast)
                        {
                            ConsumeCastSigData(castSignatureCollection, castSignatureString, token);
                            DefineCastSignature(castSignatureString, castSignatureCollection, CurrentMode, out (string? Character, string? Expression, string? Voice) cachedData, out CastMemberSignature definedSignature);


                            //Create a Prompt Line
                            line = new DialogueLine()
                            {
                                Mode = (DialogueLineMode)CurrentMode,
                                CastName = definedSignature.IsPersistent == true ? Cache.Character : cachedData.Character,
                                Expression = cachedData.Expression,
                                Voice = cachedData.Voice,
                                SignatureInfo = definedSignature
                            };
                            _evaluatingCast = false;
                            IsReadingLineContent = line != null;

                            if(definedSignature.IsPrimitive == true) Cache = cachedData;
                            continue;
                        }
                        line?.FinalizeBuilder();
                        output.ComposeNewLine(line);
                        continue;
                }
            }

            return output;
        }

        private static void ConsumeCastSigData(List<SyntaxToken>? castSignatureCollection, StringBuilder castSignatureString, SyntaxToken? token)
        {
            castSignatureCollection?.Add(token ?? default(SyntaxToken)!);
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
            CastMemberSignature signature = new();

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
                CastEvaluationState = CastEvaluationMode.Voice;

                output.Character = null;

                //Proceed to next symbol. If we get a white space, that is the end
                //of the definition.
                //However, if we get a Identifier, we know it's a full one.
                //We expect a voice on this one.
                //It's okay to have nothing, but whatever is there is always a voice.
                // For this, signatures are always partial
                signature.IsPartial = queue[evaluationPos + 1].Type == TokenType.WhiteSpace;
                signature.IsFull = characterSymbol.Type == TokenType.CloseBracket;

                if (signature.IsPartial == true)
                {
                    output.Voice = null;
                    output.Expression = null;
                    signatureDataOutput = signature;
                    return;
                }

                Next();

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
                output.Character = signature.IsPersistent == true ? Cache.Character : characterSymbol.Text; ;
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
                                signature.IsPersistent == true ? Cache.Character : characterSymbol.Text;
            signature.IsPartial = (output.Expression != null && output.Voice == null) ||
                                    (output.Expression == null && output.Voice != null);
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
    }
}