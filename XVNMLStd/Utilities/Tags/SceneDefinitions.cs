using Newtonsoft.Json;
using System;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("sceneDefinitions", new Type[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class SceneDefinitions : TagBase
    {
        [JsonProperty] private Scene[]? _scenes;
        public Scene[]? Scenes => _scenes;

        public Scene? this[string name]
        {
            get { return GetScene(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _scenes = Collect<Scene>();
        }

        public Scene? GetScene(string name) => Scenes.First(scene => scene.TagName?.Equals(name) == true);
    }
}
