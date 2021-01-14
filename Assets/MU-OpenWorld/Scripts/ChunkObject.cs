using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class ChunkObject : MonoBehaviour
    {
        public string id;

        public void Add()
        {
            int x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            int z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);
            var chunk = Data.ChunkLoad(x, z);
            chunk.X = x;
            chunk.Z = z;
            Data.PrefabData prefab = new Data.PrefabData();
            prefab.PrefabID = id;
            prefab.Position = this.transform.position;
            prefab.Rotation = this.transform.rotation;
            prefab.Scale = this.transform.localScale;
            chunk.Prefabs.Add(prefab);
            Data.ChunkSave(chunk);
        }
    }
}