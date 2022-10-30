using XVNML.Core;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("proxy", TagOccurance.PragmaOnce)]
    public class Proxy : TagBase
    {
        public string? engine;
        public string? target;
        public TargetLanguage lang;

        public override void OnResolve()
        {
            base.OnResolve();
            engine = parameterInfo?.paramters["engine"].value?.ToString()!;
            target = parameterInfo?.paramters["target"].value?.ToString()!;
            lang = Enum.Parse<TargetLanguage>(parameterInfo?.paramters["lang"].value?.ToString()!);
        }
    }
}
