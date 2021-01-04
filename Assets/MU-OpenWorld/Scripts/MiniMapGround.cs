using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class MiniMapGround : MonoBehaviour
    {
        private Mesh meshObject;
        public MeshFilter Filter;

        static public int PointCount = 10;

        static int[] triangleArray = null;

        public void Awake()
        {
            if (triangleArray == null)
                MeshTriangle();
            
            Create();
        }

        private void MeshTriangle()
        {
            triangleArray = new int[(PointCount - 1) * (PointCount - 1) * 2 * 3];
                for (int ix=0; ix<PointCount-1; ix++)
                {
                    for (int iz=0; iz<PointCount-1; iz++)
                    {
                        var _x  = PointCount * ix;
                        var _nx = PointCount * (ix + 1);
                        var _i = 6 * ((PointCount-1) * ix + iz);
                        triangleArray[_i]   = _x  + iz;
                        triangleArray[_i+1] = _x  + iz + 1;
                        triangleArray[_i+2] = _nx + iz;
                        triangleArray[_i+3] = _nx + iz;
                        triangleArray[_i+4] = _x  + iz + 1;
                        triangleArray[_i+5] = _nx + iz + 1;
                    }
                }
        }

        private Vector3[] MeshPoint(float x, float z)
        {
            Vector3[] meshVerticeArray = new Vector3[PointCount * PointCount];
            for (int ix=0; ix<PointCount; ix++)
            {
                for (int iz=0; iz<PointCount; iz++)
                {
                    float _dx = (ix / (float)(PointCount-1) - 0.5f) * Ground.XWidth;
                    float _dz = (iz / (float)(PointCount-1) - 0.5f) * Ground.ZWidth;
                    float _y = Ground.GetHeight(_dx + x, _dz + z);
                    meshVerticeArray[ix * PointCount + iz] = new Vector3(_dx, _y, _dz);
                }
            }
            return meshVerticeArray;
        }

        public void Create()
        {
            float x = this.transform.position.x;
            float z = this.transform.position.z;

            meshObject = new Mesh();
            meshObject.Clear();

            var verticeArray = new Vector3[PointCount * PointCount];
            verticeArray = MeshPoint(x, z);

            meshObject.vertices = verticeArray;
            meshObject.triangles = triangleArray;

            meshObject.RecalculateNormals();

            Filter.sharedMesh = meshObject;
        }
    }
}