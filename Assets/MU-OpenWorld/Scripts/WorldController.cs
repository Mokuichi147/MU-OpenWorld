using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private GameObject player;

        public Mesh ground_mesh;
        public int world_width = 32;
        private float world_scale = 0.02f;
        private float world_height = 32f;

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

            for (int ix=0; ix<world_width; ix++)
            {
                for (int iz=0; iz<world_width; iz++)
                {
                    float _x = x + ix * world_scale;
                    float _z = z + iz * world_scale;
                    float _y = Mathf.PerlinNoise(_x, _z) * world_height;
                    _vertices[ix * world_width + iz] = new Vector3(x + ix, _y, z + iz);
                }
            }
            for (int ix=0; ix<world_width-1; ix++)
            {
                for (int iz=0; iz<world_width-1; iz++)
                {
                    var _x  = world_width * ix;
                    var _nx = world_width * (ix + 1);
                    var _i = 6 * ((world_width-1) * ix + iz);
                    _triangles[_i]   = _x  + iz;
                    _triangles[_i+1] = _x  + iz + 1;
                    _triangles[_i+2] = _nx + iz;
                    _triangles[_i+3] = _nx + iz;
                    _triangles[_i+4] = _x  + iz + 1;
                    _triangles[_i+5] = _nx + iz + 1;
                }
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
