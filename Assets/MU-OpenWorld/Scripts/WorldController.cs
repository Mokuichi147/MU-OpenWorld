using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    enum LOD
    {
        Low    = 0,
        Medium = 1,
        High   = 2
    }

    enum Axis
    {
        None   =  0,
        Xplus  =  1,
        Xminus = -1,
        Zplus  =  2,
        Zminus = -2
    }

    struct WorldShift
    {
        public Vector3 ReferencePosition;
        public Axis Axis;
        public int Index;
    }

    public class WorldController : MonoBehaviour
    {
        [Range(1, 64)]
        public int WorldDistance = 10;
        private int worldSizs;

        [Range(0, 32)]
        public int GrassDistance = 2;
        [Range(0, 16)]
        public int GrassHighDistance = 1;
        private int grassSize;

        public int ColliderDistance = 1;
        private int colliderSize;

        private GameObject player;
        private Vector3 playerPosition;

        private Vector3 referencePosition;
        public GameObject GroundObject;
        public GameObject[] GroundObjectArray;

        public Material GrassLowMaterial;
        public Material GrassMediumMaterial;
        public Material GrassHighMaterial;
        private Material[] grassMaterialArray;

        public MeshRenderer[] GrassMeshRendererArray;
        public MeshCollider[] MeshColliderArray;

        private List<WorldShift> worldShiftList;


        void Awake()
        {
            grassMaterialArray = new Material[3];
            grassMaterialArray[0] = GrassLowMaterial;
            grassMaterialArray[1] = GrassMediumMaterial;
            grassMaterialArray[2] = GrassHighMaterial;

            worldSizs = WorldDistance * 2 + 1;
            grassSize = GrassDistance * 2 + 1;
            colliderSize = ColliderDistance * 2 + 1;
            GroundObjectArray = new GameObject[worldSizs * worldSizs];
            GrassMeshRendererArray = new MeshRenderer[grassSize * grassSize];
            MeshColliderArray = new MeshCollider[colliderSize * colliderSize];

            worldShiftList = new List<WorldShift>();
        }

        void Start()
        {
            player = GameObject.Find("Player");
            referencePosition = player.transform.position;
            playerPosition = referencePosition;

            GenerateWorld();
        }

        void Update()
        {
            playerPosition = player.transform.position;
            Axis axis = Axis.None;
            if (playerPosition.x > referencePosition.x + Ground.mesh_xwidth / 2f)
            {
                axis = Axis.Xplus;
                referencePosition.x += Ground.mesh_xwidth;
            }
            else if (playerPosition.x < referencePosition.x - Ground.mesh_xwidth / 2f)
            {
                axis = Axis.Xminus;
                referencePosition.x -= Ground.mesh_xwidth;
            }
            else if (playerPosition.z > referencePosition.z + Ground.mesh_zwidth / 2f)
            {
                axis = Axis.Zplus;
                referencePosition.z += Ground.mesh_zwidth;
            }
            else if (playerPosition.z < referencePosition.z - Ground.mesh_zwidth / 2f)
            {
                axis = Axis.Zminus;
                referencePosition.z -= Ground.mesh_zwidth;
            }

            if (axis != Axis.None)    
            {
                var world_shift_temp = new List<WorldShift>();
                world_shift_temp.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance});
                for (int i=1; i<=WorldDistance; i++)
                {
                    world_shift_temp.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance+i});
                    world_shift_temp.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance-i});
                }
                var max_count = worldShiftList.Count;
                if (max_count != 0)
                {
                    if ((int)worldShiftList[max_count-1].Axis == -1 * (int)axis && worldShiftList[max_count-1].Index == 0)
                    {
                        for (int i=1; i<=System.Math.Min(worldSizs, max_count); i++)
                        {
                            worldShiftList.RemoveAt(max_count - i);
                            world_shift_temp.RemoveAt(worldSizs - i);
                        }
                    }
                }
                worldShiftList.AddRange(world_shift_temp);
            }

            if (worldShiftList.Count == 0)
                return;

            var shift_data = worldShiftList[0];
            worldShiftList.RemoveAt(0);

            GroundShift(shift_data.ReferencePosition, shift_data.Axis, shift_data.Index);
        }

        static int GetArrayPoint(Axis axis, int index, int distance, int distance_diff=0, bool invert=false)
        {
            if (invert)
                axis = (Axis)(-1 * (int)axis);

            int size = (distance + distance_diff) * 2 + 1;

            switch (axis)
            {
                case Axis.Xplus:
                    return ((size - 1) - distance_diff) * size + distance_diff + index;
                case Axis.Xminus:
                    return distance_diff * size + distance_diff + index;
                case Axis.Zplus:
                    return (distance_diff + index) * size + (size - 1) - distance_diff;
                case Axis.Zminus:
                    return (distance_diff + index) * size + distance_diff;
                default:
                    Debug.Log("get array point error!");
                    return -1;
            }
        }

        private void SetGrassLOD(int index, LOD lod)
        {
            Material[] _materials = GrassMeshRendererArray[index].sharedMaterials;
            _materials[1] = grassMaterialArray[(int)lod];
            GrassMeshRendererArray[index].sharedMaterials = _materials;
        }

        private void ColiderShift(Axis axis, int world_index)
        {
            if (Mathf.Abs(world_index - WorldDistance) > ColliderDistance)
                return;
            
            int world_point, collider_point;
            int collider_index = world_index - WorldDistance + ColliderDistance;
            int collider_diff = WorldDistance - ColliderDistance;

            switch (axis)
            {
                case Axis.Xplus:
                    MeshColliderArray[collider_index].enabled = false;
                    for (int x=1; x<colliderSize; x++)
                        System.Array.Copy(MeshColliderArray, collider_index+x*colliderSize, MeshColliderArray, collider_index+(x-1)*colliderSize, 1);
                    collider_point = (colliderSize-1) * colliderSize + collider_index;
                    world_point = (collider_diff+colliderSize-1)*worldSizs + collider_diff + collider_index;
                    break;
                case Axis.Xminus:
                    MeshColliderArray[(colliderSize-1)*colliderSize+collider_index].enabled = false;
                    for (int x=colliderSize-1; x>0; x--)
                        System.Array.Copy(MeshColliderArray, collider_index+(x-1)*colliderSize, MeshColliderArray, collider_index+x*colliderSize, 1);
                    collider_point = collider_index;
                    world_point = collider_diff*worldSizs + collider_diff + collider_index;
                    break;
                case Axis.Zplus:
                    MeshColliderArray[collider_index*colliderSize].enabled = false;
                    System.Array.Copy(MeshColliderArray, collider_index*colliderSize+1, MeshColliderArray, collider_index*colliderSize, colliderSize-1);
                    collider_point = (collider_index+1) * colliderSize - 1;
                    world_point = collider_diff*worldSizs + 2*collider_diff*collider_index + collider_diff + collider_point;
                    break;
                case Axis.Zminus:
                    MeshColliderArray[(collider_index+1)*colliderSize-1].enabled = false;
                    System.Array.Copy(MeshColliderArray, collider_index*colliderSize, MeshColliderArray, collider_index*colliderSize+1, colliderSize-1);
                    collider_point = collider_index * colliderSize;
                    world_point = collider_diff*worldSizs + 2*collider_diff*collider_index + collider_diff + collider_point;
                    break;
                default:
                    Debug.Log("grassMaterialArray shift error!");
                    return;

            }
            MeshColliderArray[collider_point] = GroundObjectArray[world_point].GetComponent<MeshCollider>();
            MeshColliderArray[collider_point].enabled = true;
        }

        private void GrassShift(Axis axis, int world_index)
        {
            if (Mathf.Abs(world_index - WorldDistance) > GrassDistance)
                return;
            
            int world_point, grass_point;
            int grass_index = world_index - WorldDistance + GrassDistance;
            int grass_diff = WorldDistance - GrassDistance;

            if (Mathf.Abs(world_index - WorldDistance) <= GrassHighDistance)
            {
                int high_index = grass_index - GrassDistance + GrassHighDistance;
                int high_diff = GrassDistance - GrassHighDistance;
                SetGrassLOD(GetArrayPoint(axis, high_index, GrassHighDistance, distance_diff: high_diff, invert: true), LOD.Medium);
            }
            
            switch (axis)
            {
                case Axis.Xplus:
                    SetGrassLOD(grass_index, LOD.Low);
                    for (int x=1; x<grassSize; x++)
                        System.Array.Copy(GrassMeshRendererArray, grass_index+x*grassSize, GrassMeshRendererArray, grass_index+(x-1)*grassSize, 1);
                    grass_point = (grassSize-1) * grassSize + grass_index;
                    world_point = (grass_diff+grassSize-1)*worldSizs + grass_diff + grass_index;
                    break;
                case Axis.Xminus:
                    SetGrassLOD((grassSize-1)*grassSize+grass_index, LOD.Low);
                    for (int x=grassSize-1; x>0; x--)
                        System.Array.Copy(GrassMeshRendererArray, grass_index+(x-1)*grassSize, GrassMeshRendererArray, grass_index+x*grassSize, 1);
                    grass_point = grass_index;
                    world_point = grass_diff*worldSizs + grass_diff + grass_index;
                    break;
                case Axis.Zplus:
                    SetGrassLOD(grass_index*grassSize, LOD.Low);
                    System.Array.Copy(GrassMeshRendererArray, grass_index*grassSize+1, GrassMeshRendererArray, grass_index*grassSize, grassSize-1);
                    grass_point = (grass_index+1) * grassSize - 1;
                    world_point = grass_diff*worldSizs + 2*grass_diff*grass_index + grass_diff + grass_point;
                    break;
                case Axis.Zminus:
                    SetGrassLOD((grass_index+1)*grassSize-1, LOD.Low);
                    System.Array.Copy(GrassMeshRendererArray, grass_index*grassSize, GrassMeshRendererArray, grass_index*grassSize+1, grassSize-1);
                    grass_point = grass_index * grassSize;
                    world_point = grass_diff*worldSizs + 2*grass_diff*grass_index + grass_diff + grass_point;
                    break;
                default:
                    Debug.Log("grassMaterialArray shift error!");
                    return;
            }
            GrassMeshRendererArray[grass_point] = GroundObjectArray[world_point].GetComponent<MeshRenderer>();
            SetGrassLOD(grass_point, LOD.Medium);

            if (Mathf.Abs(world_index - WorldDistance) <= GrassHighDistance)
            {
                int high_index = grass_index - GrassDistance + GrassHighDistance;
                int high_diff = GrassDistance - GrassHighDistance;
                SetGrassLOD(GetArrayPoint(axis, high_index, GrassHighDistance, distance_diff: high_diff), LOD.High);
            }
        }

        private void GroundShift(Vector3 r_pos, Axis axis, int index)
        {
            Vector3 add_diff_pos;
            int create_index;
            float index_diff = index - WorldDistance;

            switch (axis)
            {
                case Axis.Xplus:
                    Destroy(GroundObjectArray[index]);
                    for (int x=1; x<worldSizs; x++)
                        System.Array.Copy(GroundObjectArray, index+x*worldSizs, GroundObjectArray, index+(x-1)*worldSizs, 1);
                    add_diff_pos = new Vector3((float)WorldDistance * Ground.mesh_xwidth, 0f, index_diff * Ground.mesh_zwidth) + r_pos;
                    create_index = (worldSizs - 1) * worldSizs + index;
                    break;
                case Axis.Xminus:
                    Destroy(GroundObjectArray[(worldSizs-1)*worldSizs+index]);
                    for (int x=worldSizs-1; x>0; x--)
                        System.Array.Copy(GroundObjectArray, index+(x-1)*worldSizs, GroundObjectArray, index+x*worldSizs, 1);
                    add_diff_pos = new Vector3((float)(-1*WorldDistance) * Ground.mesh_xwidth, 0f, index_diff * Ground.mesh_zwidth) + r_pos;
                    create_index = index;
                    break;
                case Axis.Zplus:
                    Destroy(GroundObjectArray[index*worldSizs]);
                    System.Array.Copy(GroundObjectArray, index*worldSizs+1, GroundObjectArray, index*worldSizs, worldSizs-1);
                    add_diff_pos = new Vector3(index_diff * Ground.mesh_xwidth, 0f, (float)WorldDistance * Ground.mesh_zwidth) + r_pos;
                    create_index = (index + 1) * worldSizs - 1;
                    break;
                case Axis.Zminus:
                    Destroy(GroundObjectArray[(index+1)*worldSizs-1]);
                    System.Array.Copy(GroundObjectArray, index*worldSizs, GroundObjectArray, index*worldSizs+1, worldSizs-1);
                    add_diff_pos = new Vector3(index_diff * Ground.mesh_xwidth, 0f, (float)(-1*WorldDistance) * Ground.mesh_zwidth) + r_pos;
                    create_index = index * worldSizs;
                    break;
                default:
                    Debug.Log("GroundObject shift error!");
                    return;
            }
            GroundObjectArray[create_index] = Instantiate(GroundObject, add_diff_pos, Quaternion.identity, this.transform);
            GrassShift(axis, index);
            ColiderShift(axis, index);
        }

        private void GenerateWorld()
        {
            for (int x=0; x<worldSizs; x++)
            {
                var _x = x * worldSizs;
                var x_diff = x - WorldDistance;
                for (int z=0; z<worldSizs; z++)
                {
                    var z_diff = z - WorldDistance;
                    var _pos = new Vector3(Ground.mesh_xwidth*x_diff+referencePosition.x, 0f, Ground.mesh_zwidth*z_diff+referencePosition.z);
                    GroundObjectArray[_x+z] = Instantiate(GroundObject, _pos, Quaternion.identity, this.transform);

                    // 草の表示切替用
                    if (Mathf.Abs(x_diff) <= GrassHighDistance && Mathf.Abs(z_diff) <= GrassHighDistance)
                    {
                        var grass_x = x_diff + GrassDistance;
                        var grass_z = z_diff + GrassDistance;
                        GrassMeshRendererArray[grass_x*grassSize + grass_z] = GroundObjectArray[_x+z].GetComponent<MeshRenderer>();
                        SetGrassLOD(grass_x*grassSize + grass_z, LOD.High);
                    }
                    else if (Mathf.Abs(x_diff) <= GrassDistance && Mathf.Abs(z_diff) <= GrassDistance)
                    {
                        var grass_x = x_diff + GrassDistance;
                        var grass_z = z_diff + GrassDistance;
                        GrassMeshRendererArray[grass_x*grassSize + grass_z] = GroundObjectArray[_x+z].GetComponent<MeshRenderer>();
                        SetGrassLOD(grass_x*grassSize + grass_z, LOD.Medium);
                    }
                    // メッシュコライダー切替用
                    if (Mathf.Abs(x_diff) <= ColliderDistance && Mathf.Abs(z_diff) <= ColliderDistance)
                    {
                        var collider_x = x_diff + ColliderDistance;
                        var collider_z = z_diff + ColliderDistance;
                        MeshColliderArray[collider_x*colliderSize + collider_z] = GroundObjectArray[_x+z].GetComponent<MeshCollider>();
                        MeshColliderArray[collider_x*colliderSize + collider_z].enabled = true;
                    }
                }
            }
        }
    }
}
