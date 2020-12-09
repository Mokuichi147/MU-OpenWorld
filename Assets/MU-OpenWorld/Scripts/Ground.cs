using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class Ground : MonoBehaviour
    {
        public Mesh mesh;
        public GameObject water_surface;

        static public int mesh_point = 64;
        static public float mesh_width = 32f;
        static public float mesh_height = 32f;

        static public float seed = 50000f;
        static public float scale = 0.004f;

        static int[] _triangles = null;

        static public float GetHeight(float x, float z)
        {
            float _x = (x + seed) * scale;
            float _z = (z + seed) * scale;
            float _y = Mathf.PerlinNoise(_x, _z);
            // 0～1 s(水面下),m,l(山)の閾値
            float l = 0.55f;
            float s = 0.40f;
            // リスケール後の最大値
            float l_m = 1.3f;
            float m_m = 0.15f;
            float s_m = 0.3f;

            if (_y > l)
                _y = (_y-l) / (1f-l) * l_m + m_m + s_m;
            else if (_y > s)
                _y = (_y-s) / (l-s) * m_m + s_m;
            else
                _y = _y / s * s_m;
            
            // 海面が0になるようにする
            _y -= (s_m - 0.001f);
            return _y * mesh_height;
        }
        
        void Awake()
        {
            var pos = this.transform.position;
            this.name = $"Ground_{pos.x}_{pos.z}";
            Create(pos.x, pos.z);
        }

        void OnDestroy()
        {
            Destroy(mesh);
            Destroy(water_surface);
        }

        private Vector3[] MeshPoint(float x, float z, int count, float width)
        {
            Vector3[] _mesh_vertices = new Vector3[count * count];
            for (int ix=0; ix<count; ix++)
            {
                for (int iz=0; iz<count; iz++)
                {
                    float _dx = (ix / (float)(count-1) - 0.5f) * width;
                    float _dz = (iz / (float)(count-1) - 0.5f) * width;
                    float _y = GetHeight(_dx + x, _dz + z);
                    _mesh_vertices[ix * count + iz] = new Vector3(_dx, _y, _dz);
                }
            }
            return _mesh_vertices;
        }

        private void Create(float x, float z)
        {
            mesh = new Mesh();
            mesh.name = $"GroundMesh_{x}_{z}";
            mesh.Clear();

            var _vertices = new Vector3[mesh_point * mesh_point];

            _vertices = MeshPoint(x, z, mesh_point, mesh_width);

            if (_triangles == null)
            {
                _triangles = new int[(mesh_point - 1) * (mesh_point - 1) * 2 * 3];
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
            }
            
            mesh.vertices = _vertices;
            mesh.triangles = _triangles;

            mesh.RecalculateNormals();
            var filter = this.GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var collider = this.GetComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            water_surface = this.transform.GetChild(0).gameObject;
            water_surface.name = $"WaterSurface_{x}_{z}";
            water_surface.transform.localScale = new Vector3(mesh_width/10f, 1f, mesh_width/10f);
        }
    }
}
