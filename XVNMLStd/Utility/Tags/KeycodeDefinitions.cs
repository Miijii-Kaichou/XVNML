using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycodeDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class KeycodeDefinitions : TagBase
    {
        [JsonProperty]private Keycode[]? _keyCodes;
        public Keycode[]? KeyCodes => _keyCodes;

        public Keycode? this[string name]
        {
            get { return GetKeyCode(name); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _keyCodes = Collect<Keycode>();
        }

        public Keycode? GetKeyCode(string name) => KeyCodes.First(keyCode => keyCode.TagName?.Equals(name) == true);
    }
}