using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class AddPrefab : MonoBehaviour
    {
        public string id;

        private void OnApplicationQuit()
        {
            int x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            int z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);
            var chunk = Data.ChunkLoad(x, z);
            Data.PrefabData prefab = new Data.PrefabData();
            prefab.ID = id;
            prefab.Position = this.transform.position;
            prefab.Rotation = this.transform.rotation;
            prefab.Scale = this.transform.localScale;
            chunk.Prefabs.Add(prefab);
            Data.ChunkSave(x, z, chunk);
        }
    }
}