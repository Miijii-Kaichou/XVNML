using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("audio", typeof(AudioDefinitions), TagOccurance.Multiple)]
    public class Audio : TagBase
    {
        public string sourceFile;
        public override void OnResolve()
        {
            base.OnResolve();
            sourceFile = parameterInfo.paramters["src"].value.ToString();
        }
    }
}
