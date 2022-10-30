using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("castDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    public  class CastDefinitions : TagBase
    {
        public new Cast this[string name]
        {
            get { return GetElement<Cast>(name)!; }
        }

        public override void OnResolve()
        {
            base.OnResolve();
        }

        public Cast GetCast(string name) => this[name];
    }
}
