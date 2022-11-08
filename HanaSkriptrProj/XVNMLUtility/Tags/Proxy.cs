using XVNML.Core;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("proxy", TagOccurance.PragmaOnce)]
    sealed class Proxy : TagBase
    {
        public string? engine;
        public string? target;
        public TargetLanguage lang;

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            engine = parameterInfo?["engine"]!.ToString()!;
            target = parameterInfo?["target"]!.ToString()!;
            lang = Enum.Parse<TargetLanguage>(parameterInfo?.paramters["lang"].value?.ToString()!);
        }
    }
}
