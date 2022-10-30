using System.Diagnostics.SymbolStore;
using System.Runtime.CompilerServices;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.Core.Dialogue
{

    public class DialogueLine
    {
        string? _castName;
        public string? CastName
        {
            get
            {
                return _castName;
            }
            set
            {
                _castName = value;
                GenerateCast();
            }
        }

        string? _expression;
        public string? Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                _expression = value;
                GenerateExpression();
            }
        }

        string? _voice;
        public string? Voice
        {
            get
            {
                return _voice;
            }
            set
            {
                _voice = value;
                GenerateVoice();
            }
        }

        private StringBuilder _ContentStringBuilder = new StringBuilder();
        public string? Content { get; private set; }
        public string?[] PromptContent { get; private set; } = new string[_DefaultPromptCapacity];

        public DialogueLineMode Mode { get; set; }
        internal CastMemberSignature SignatureInfo { get; set; }

        private const int _DefaultPromptCapacity = 4;

        /// <summary>
        /// Resolves expression states on object to fully control it
        /// in code
        /// </summary>
        void GenerateExpression([CallerMemberName] string expName = "")
        {

        }

        /// <summary>
        /// Resolves voice states on object to fully control it
        /// in code
        /// </summary>
        void GenerateVoice([CallerMemberName] string voiceName = "")
        {

        }

        /// <summary>
        /// Resolves Cast Association when used in other part of XVNNML document
        /// </summary>
        /// <param name="castName"></param>
        void GenerateCast([CallerMemberName] string castName = "")
        {

        }

        internal void AppendContent(string text) => _ContentStringBuilder.Append(text);
        internal void CreatePrompt(params string[]? choices)
        {
            var size = choices?.Length;
            if(size == PromptContent.Length)
            {
                PromptContent = choices ?? Array.Empty<string>();
                return;
            }

            PromptContent = new string[(size ?? 0)];
            PromptContent = choices ?? Array.Empty<string>();
        }
        internal void FinalizeBuilder() => Content = _ContentStringBuilder.ToString().TrimEnd('<', '>');
    }
}