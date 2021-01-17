using UnityEngine;
using UnityEditor;

namespace OpenWorld.World
{
    [CustomEditor(typeof(ChunkObject))]
    public class ChunkObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ChunkObject chunkObjectScript = target as ChunkObject;

            if (GUILayout.Button("Chunk Add This GameObject"))
            {
                chunkObjectScript.Add();
            }
        }
    }
}