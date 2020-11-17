using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private GameObject player;

        public Mesh ground_mesh;
        public int mesh_point = 128;
        public float mesh_width = 128f;
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

            var _vertices = new Vector3[mesh_point * mesh_point];
            var _triangles = new int[(mesh_point - 1) * (mesh_point - 1) * 2 * 3];

            for (int ix=0; ix<mesh_point; ix++)
            {
                for (int iz=0; iz<mesh_point; iz++)
                {
                    float _half = (mesh_point - 1) / 2f;
                    float _dx = (ix - _half) / _half * mesh_width + x;
                    float _dz = (iz - _half) / _half * mesh_width + z;
                    float _x = _dx * world_scale;
                    float _z = _dz * world_scale;
                    float _y = Mathf.PerlinNoise(_x, _z) * world_height;
                    _vertices[ix * mesh_point + iz] = new Vector3(_dx, _y, _dz);
                }
            }
            for (int ix=0; ix<mesh_point-1; ix++)
            {
                for (int iz=0; iz<mesh_point-1; iz++)
                {
                    var _x  = mesh_point * ix;
                    var _nx = mesh_point * (ix + 1);
                    var _i = 6 * ((mesh_point-1) * ix + iz);
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
