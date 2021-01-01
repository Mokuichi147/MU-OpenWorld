using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class Chunk : MonoBehaviour
    {
        private int x;
        private int z;


        void Awake()
        {
            x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);

            Data.Chunk chunk;

            if (!Data.IsChunkData(x, z))
            {
                chunk = new Data.Chunk();
                chunk.Prefabs = new List<Data.PrefabData>();
                Data.PrefabData ground = new Data.PrefabData();
                ground.ID = "Ground";
                ground.Position = this.transform.position;
                ground.Rotation = Quaternion.identity;
                ground.Scale = new Vector3(1f, 1f, 1f);
                chunk.Prefabs.Add(ground);
                Data.ChunkSave(x, z, chunk);
            }
            else
            {
                chunk = Data.ChunkLoad(x, z);
            }
            foreach (var prefab in chunk.Prefabs)
            {
                var prefabObject = Instantiate(PrefabID.GetPrefab(prefab.ID), prefab.Position, prefab.Rotation, this.transform);
                prefabObject.transform.localScale = prefab.Scale;
            }
        }
    }
}