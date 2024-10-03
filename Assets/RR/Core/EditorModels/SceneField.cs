using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.Core.EditorModels
{
    // Taken from: https://answers.unity.com/questions/242794/inspector-field-for-scene-asset.html
    [Serializable]
    public class SceneField
    {
        [field: SerializeField] public Object SceneAsset { get; }
        [field: SerializeField] public string SceneName { get; } = "";

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }
    }
}
