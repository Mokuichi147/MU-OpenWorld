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
            App.DataFile.PrefabData ground = new App.DataFile.PrefabData();
            ground.PrefabID = "Ground";
            ground.Position = this.transform.position;
            ground.Rotation = Quaternion.identity;
            ground.Scale = Vector3.one;
            chunk.Prefabs.Add(ground);
        }

        private void SetPrefabs()
        {
            foreach (var prefab in chunk.Prefabs)
            {
                var prefabObject = Instantiate(App.ObjectData.FromID(prefab.PrefabID), prefab.Position, prefab.Rotation, this.transform);
                prefabObject.transform.localScale = prefab.Scale;
            }
        }
    }
}