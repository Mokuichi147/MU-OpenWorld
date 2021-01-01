using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class Chunk : MonoBehaviour
    {
        private int x;
        private int z;

        private Data.Chunk chunk;


        void Awake()
        {
            x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);

            if (!Data.IsChunkData(x, z))
            {
                ChunkCreate();
                WorldController.chunkSaveList.Add(chunk);
            }
            else
            {
                chunk = Data.ChunkLoad(x, z);
            }

            SetPrefabs();
        }


        private void ChunkCreate()
        {
            chunk = new Data.Chunk();
            chunk.X = x;
            chunk.Z = z;
            chunk.Prefabs = new List<Data.PrefabData>();
            Data.PrefabData ground = new Data.PrefabData();
            ground.ID = "Ground";
            ground.Position = this.transform.position;
            ground.Rotation = Quaternion.identity;
            ground.Scale = new Vector3(1f, 1f, 1f);
            chunk.Prefabs.Add(ground);
        }

        private void SetPrefabs()
        {
            foreach (var prefab in chunk.Prefabs)
            {
                var prefabObject = Instantiate(PrefabID.GetPrefab(prefab.ID), prefab.Position, prefab.Rotation, this.transform);
                prefabObject.transform.localScale = prefab.Scale;
            }
        }
    }
}