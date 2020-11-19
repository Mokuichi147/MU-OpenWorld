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
        public float mesh_width = 256f;
        private float mesh_height = 16f;

        public Gradient mesh_color;

        public float seed = 50000f;
        private float world_scale = 0.05f;

        void Start()
        {
            player = GameObject.Find("Player");
            var pos = player.transform.position;
            CreateGround(pos.x, pos.z);
        }


        public float GetGroundHeight(float x, float z)
        {
            float _x = (x + seed) * world_scale;
            float _z = (z + seed) * world_scale;
            float _y = Mathf.PerlinNoise(_x, _z);
            // 0～1 s(水面下),m,l(山)の閾値
            float l = 0.6f;
            float s = 0.3f;
            // リスケール後の最大値
            float l_m = 0.7f;
            float m_m = 0.1f;
            float s_m = 0.5f;

            if (_y > l)
                _y = (_y-l) / (1f-l) * l_m + m_m + s_m;
            else if (_y > s)
                _y = (_y-s) / (l-s) * m_m + s_m;
            else
                _y = _y / s * s_m;
            
            // 海面が0になるようにする
            _y -= (s_m - 0.01f);
            return _y * mesh_height;
        }

        private Texture2D CreateTexture(Vector3[] vertices)
        {
            float min_height = 0f;
            float max_height = mesh_height;

            Color[] color_map = new Color[vertices.Length];
            for (int i=0; i<vertices.Length; i++)
            {
                float raito = Mathf.InverseLerp(min_height, max_height, vertices[i].y);
                color_map[i] = mesh_color.Evaluate(raito);
            }
            Texture2D _texture = new Texture2D(mesh_point, mesh_point);
            _texture.SetPixels(color_map);
            _texture.Apply();
            return _texture;
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
                    float _half = mesh_width / 2f;
                    float _dx = (ix / (float)(mesh_point-1) - 0.5f) * _half + x;
                    float _dz = (iz / (float)(mesh_point-1) - 0.5f) * _half + z;
                    float _y = GetGroundHeight(_dx, _dz);
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
            var renderer = this.GetComponent<MeshRenderer>();
            renderer.sharedMaterial.mainTexture = CreateTexture(_vertices);
        }
    }
}
