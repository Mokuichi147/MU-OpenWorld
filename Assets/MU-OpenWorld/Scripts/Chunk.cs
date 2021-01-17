using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.World
{
    public class Chunk : MonoBehaviour
    {
        private int x;
        private int z;

        private App.DataFile.Chunk chunk;

        public App.DataFile.PrefabData[] DefaultObjects;


        void Awake()
        {
            x = (int)Mathf.Floor(this.transform.position.x / Ground.XWidth);
            z = (int)Mathf.Floor(this.transform.position.z / Ground.ZWidth);

            if (!App.DataFile.IsChunkData(x, z))
            {
                Create();
                World.ChunkController.chunkSaveList.Add(chunk);
            }
            else
            {
                chunk = App.DataFile.ChunkLoad(x, z);
            }

            SetPrefabs();
        }


        private void Create()
        {
            chunk = new App.DataFile.Chunk();
            chunk.X = x;
            chunk.Z = z;
            chunk.Prefabs = new List<App.DataFile.PrefabData>();
            for (int i=0; i<DefaultObjects.Length; i++)
            {
                var prefabObject = DefaultObjects[i];
                if (prefabObject.IsLocalPosition)
                {
                    prefabObject.Position += this.transform.position;
                }
                chunk.Prefabs.Add(prefabObject);
            }
        }

        private void SetPrefabs()
        {
            foreach (var prefab in chunk.Prefabs)
            {
                var prefabObject = Instantiate(App.ObjectData.FromID(prefab.PrefabID), prefab.Position, Quaternion.Euler(prefab.Rotation), this.transform);
                prefabObject.transform.localScale = prefab.Scale;
            }
        }
    }
}