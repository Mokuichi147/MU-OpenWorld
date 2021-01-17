using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.World
{
    public class ChunkObject : MonoBehaviour
    {
        public string id;

        public void Add()
        {
            int x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            int z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);
            var chunk = App.DataFile.ChunkLoad(x, z);
            chunk.X = x;
            chunk.Z = z;
            App.DataFile.PrefabData prefab = new App.DataFile.PrefabData();
            prefab.PrefabID = id;
            prefab.Position = this.transform.position;
            prefab.Rotation = this.transform.rotation;
            prefab.Scale = this.transform.localScale;
            chunk.Prefabs.Add(prefab);
            App.DataFile.ChunkSave(chunk);
        }
    }
}