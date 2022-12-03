using System;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("sceneDefinitions", new Type[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class SceneDefinitions : TagBase
    {
        public Scene[]? Scenes => Collect<Scene>();
        public Scene? this[string name]
        {
            get { return GetScene(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Scene? GetScene(string name) => this[name];
    }
}
