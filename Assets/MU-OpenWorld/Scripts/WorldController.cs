using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private GameObject player;

        public Mesh ground_mesh;
        public float world_scale  = 5f;
        public int   world_width  = 32;
        public float world_height = 1f;

        void Start()
        {
            player = GameObject.Find("Player");
            var pos = player.transform.position;
            CreateGround(pos.x, pos.z);
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void CreateGround(float x, float z)
        {
            ground_mesh = new Mesh();
            ground_mesh.Clear();

            var _vertices = new Vector3[world_width * world_width];
            var _triangles = new int[(world_width - 1) * (world_width - 1) * 2 * 3];

            for (int i=0; i<world_width*world_width; i++)
            {
                float _x = (x + i % world_width) * world_scale;
                float _z = (z + i / world_width) * world_scale;
                float _y = Mathf.PerlinNoise(_x, _z) * world_height;
                _vertices[i] = new Vector3(_x, _y, _z);
            }
            for (int i=0; i<(world_width-1)*(world_width-1); i++)
            {
                _triangles[6*i]   = i;
                _triangles[6*i+1] = i + 1;
                _triangles[6*i+2] = i + world_width;
                _triangles[6*i+3] = i + 1;
                _triangles[6*i+4] = i + world_width + 1;
                _triangles[6*i+5] = i + world_width;
            }
            ground_mesh.vertices = _vertices;
            ground_mesh.triangles = _triangles;

            ground_mesh.RecalculateNormals();
            var filter = this.GetComponent<MeshFilter>();
            filter.sharedMesh = ground_mesh;
            var collider = this.GetComponent<MeshCollider>();
            collider.sharedMesh = ground_mesh;
        }
    }
}
