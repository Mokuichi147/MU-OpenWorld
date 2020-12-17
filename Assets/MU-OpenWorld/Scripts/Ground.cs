using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class Ground : MonoBehaviour
    {
        public Mesh MeshObject;
        public GameObject WaterSurfaceObject;

        static public int ZPointCount = 65;
        static public float ZWidth = 32f;
        static private float triangleScale = ZWidth / (ZPointCount - 1);
        static private Vector3 triangleDiff = new Vector3(Mathf.Sqrt(3) / 2f * triangleScale, 0f, triangleScale);
        static private int XPointCount = (int)Mathf.Round(ZWidth / triangleDiff.x) + 1;
        static public float XWidth = triangleDiff.x * (XPointCount - 1);
        static public float Height = 32f;

        static public float WorldSeed = 50000f;
        static public float WorldScale = 0.004f;

        static private int[] triangleArray = null;

        static public float GetHeight(float x, float z)
        {
            float _x = (x + WorldSeed) * WorldScale;
            float _z = (z + WorldSeed) * WorldScale;
            float _y = 0.97f * Mathf.PerlinNoise(_x, _z) + 0.03f * Mathf.PerlinNoise(_x * 10f, _z * 10f);
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
            return _y * Height;
        }
        
        void Awake()
        {
            var pos = this.transform.position;
            this.name = $"Ground_{pos.x}_{pos.z}";
            Create(pos.x, pos.z);
        }

        void OnDestroy()
        {
            Destroy(MeshObject);
            Destroy(WaterSurfaceObject);
        }

        private Vector3[] MeshPoint(float x, float z)
        {
            Vector3[] meshVerticeArray = new Vector3[XPointCount * ZPointCount];

            for (int ix=0; ix<XPointCount; ix++)
            {
                float _dx = XWidth/-2f + ix*triangleDiff.x;
                for (int iz=0; iz<ZPointCount; iz++)
                {
                    float _dz = ZWidth/-2f + iz*triangleDiff.z + ix%2 * (triangleDiff.z/2f);
                    float _y = GetHeight(_dx + x, _dz + z);
                    meshVerticeArray[ix * ZPointCount + iz] = new Vector3(_dx, _y, _dz);
                }
            }
            return meshVerticeArray;
        }

        private void Create(float x, float z)
        {
            MeshObject = new Mesh();
            MeshObject.name = $"GroundMesh_{x}_{z}";
            MeshObject.Clear();

            var verticeArray = new Vector3[XPointCount * ZPointCount];

            verticeArray = MeshPoint(x, z);

            if (triangleArray == null)
            {
                triangleArray = new int[(XPointCount - 1) * (ZPointCount - 1) * 2 * 3];
                for (int ix=0; ix<XPointCount-1; ix++)
                {
                    for (int iz=0; iz<ZPointCount-1; iz++)
                    {
                        var _x  = ZPointCount * ix;
                        var _nx = ZPointCount * (ix + 1);
                        var _i = 6 * ((ZPointCount-1) * ix + iz);
                        if (ix % 2 == 0)
                        {
                            triangleArray[_i]   = _x  + iz;
                            triangleArray[_i+1] = _x  + iz + 1;
                            triangleArray[_i+2] = _nx + iz;
                            triangleArray[_i+3] = _nx + iz;
                            triangleArray[_i+4] = _x  + iz + 1;
                            triangleArray[_i+5] = _nx + iz + 1;
                        }
                        else
                        {
                            triangleArray[_i]   = _nx  + iz;
                            triangleArray[_i+1] = _x  + iz;
                            triangleArray[_i+2] = _nx + iz + 1;
                            triangleArray[_i+3] = _x + iz;
                            triangleArray[_i+4] = _x  + iz + 1;
                            triangleArray[_i+5] = _nx + iz + 1;
                        }
                    }
                }
            }
            
            MeshObject.vertices = verticeArray;
            MeshObject.triangles = triangleArray;

            MeshObject.RecalculateNormals();
            var filter = this.GetComponent<MeshFilter>();
            filter.sharedMesh = MeshObject;
            var collider = this.GetComponent<MeshCollider>();
            collider.sharedMesh = MeshObject;

            WaterSurfaceObject = this.transform.GetChild(0).gameObject;
            WaterSurfaceObject.name = $"WaterSurface_{x}_{z}";
            WaterSurfaceObject.transform.localScale = new Vector3(XWidth/10f, 1f, ZWidth/10f);
        }
    }
}
