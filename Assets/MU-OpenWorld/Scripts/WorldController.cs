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
        private int worldSize;

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
            // クオリティ別の草シェーダを設定する
            grassMaterialArray = new Material[3];
            grassMaterialArray[0] = GrassLowMaterial;
            grassMaterialArray[1] = GrassMediumMaterial;
            grassMaterialArray[2] = GrassHighMaterial;

            // 配列の大きさを計算する
            worldSize = WorldDistance * 2 + 1;
            grassSize = GrassDistance * 2 + 1;
            colliderSize = ColliderDistance * 2 + 1;
            GroundObjectArray = new GameObject[worldSize * worldSize];
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
            // マップの更新が必要か
            playerPosition = player.transform.position;
            Axis axis = Axis.None;
            if (playerPosition.x > referencePosition.x + Ground.XWidth / 2f)
            {
                axis = Axis.Xplus;
                referencePosition.x += Ground.XWidth;
            }
            else if (playerPosition.x < referencePosition.x - Ground.XWidth / 2f)
            {
                axis = Axis.Xminus;
                referencePosition.x -= Ground.XWidth;
            }
            else if (playerPosition.z > referencePosition.z + Ground.ZWidth / 2f)
            {
                axis = Axis.Zplus;
                referencePosition.z += Ground.ZWidth;
            }
            else if (playerPosition.z < referencePosition.z - Ground.ZWidth / 2f)
            {
                axis = Axis.Zminus;
                referencePosition.z -= Ground.ZWidth;
            }

            // ワールドの境界を行き来した場合に何度も更新しないための処理
            if (axis != Axis.None)    
            {
                var worldShiftTempArray = new List<WorldShift>();
                worldShiftTempArray.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance});
                for (int i=1; i<=WorldDistance; i++)
                {
                    worldShiftTempArray.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance+i});
                    worldShiftTempArray.Add(new WorldShift() {ReferencePosition=referencePosition, Axis=axis, Index=WorldDistance-i});
                }
                var maxCount = worldShiftList.Count;
                if (maxCount != 0)
                {
                    if ((int)worldShiftList[maxCount-1].Axis == -1 * (int)axis && worldShiftList[maxCount-1].Index == 0)
                    {
                        for (int i=1; i<=System.Math.Min(worldSize, maxCount); i++)
                        {
                            worldShiftList.RemoveAt(maxCount - i);
                            worldShiftTempArray.RemoveAt(worldSize - i);
                        }
                    }
                }
                worldShiftList.AddRange(worldShiftTempArray);
            }

            // マップを更新する必要がない場合
            if (worldShiftList.Count == 0)
                return;

            // ワールドを更新する
            var shiftData = worldShiftList[0];
            worldShiftList.RemoveAt(0);

            GroundArrayShift(shiftData.ReferencePosition, shiftData.Axis, shiftData.Index);
        }

        static int GetArrayPoint(Axis axis, int index, int distance, int distanceDiff=0, bool invert=false)
        {
            /* 二次元配列の座標から一次元配列の座標の位置を返す */
            if (invert)
                axis = (Axis)(-1 * (int)axis);

            int size = (distance + distanceDiff) * 2 + 1;

            switch (axis)
            {
                case Axis.Xplus:
                    return ((size - 1) - distanceDiff) * size + distanceDiff + index;
                case Axis.Xminus:
                    return distanceDiff * size + distanceDiff + index;
                case Axis.Zplus:
                    return (distanceDiff + index) * size + (size - 1) - distanceDiff;
                case Axis.Zminus:
                    return (distanceDiff + index) * size + distanceDiff;
                default:
                    Debug.Log("get array point error!");
                    return -1;
            }
        }

        private void SetGrassLOD(int index, LOD lod)
        {
            /* 草のシェーダーを切り替える */
            Material[] materialArray = GrassMeshRendererArray[index].sharedMaterials;
            materialArray[1] = grassMaterialArray[(int)lod];
            GrassMeshRendererArray[index].sharedMaterials = materialArray;
        }

        private void ColliderArrayShift(Axis axis, int worldIndex)
        {
            /* コライダーを管理する配列の中で指定した行か列を更新する */
            if (Mathf.Abs(worldIndex - WorldDistance) > ColliderDistance)
                return;
            
            int worldPoint, colliderPoint;
            int colliderIndex = worldIndex - WorldDistance + ColliderDistance;
            int colliderDiff = WorldDistance - ColliderDistance;

            switch (axis)
            {
                case Axis.Xplus:
                    MeshColliderArray[colliderIndex].enabled = false;
                    for (int x=1; x<colliderSize; x++)
                        System.Array.Copy(MeshColliderArray, colliderIndex+x*colliderSize, MeshColliderArray, colliderIndex+(x-1)*colliderSize, 1);
                    colliderPoint = (colliderSize-1) * colliderSize + colliderIndex;
                    worldPoint = (colliderDiff+colliderSize-1)*worldSize + colliderDiff + colliderIndex;
                    break;
                case Axis.Xminus:
                    MeshColliderArray[(colliderSize-1)*colliderSize+colliderIndex].enabled = false;
                    for (int x=colliderSize-1; x>0; x--)
                        System.Array.Copy(MeshColliderArray, colliderIndex+(x-1)*colliderSize, MeshColliderArray, colliderIndex+x*colliderSize, 1);
                    colliderPoint = colliderIndex;
                    worldPoint = colliderDiff*worldSize + colliderDiff + colliderIndex;
                    break;
                case Axis.Zplus:
                    MeshColliderArray[colliderIndex*colliderSize].enabled = false;
                    System.Array.Copy(MeshColliderArray, colliderIndex*colliderSize+1, MeshColliderArray, colliderIndex*colliderSize, colliderSize-1);
                    colliderPoint = (colliderIndex+1) * colliderSize - 1;
                    worldPoint = colliderDiff*worldSize + 2*colliderDiff*colliderIndex + colliderDiff + colliderPoint;
                    break;
                case Axis.Zminus:
                    MeshColliderArray[(colliderIndex+1)*colliderSize-1].enabled = false;
                    System.Array.Copy(MeshColliderArray, colliderIndex*colliderSize, MeshColliderArray, colliderIndex*colliderSize+1, colliderSize-1);
                    colliderPoint = colliderIndex * colliderSize;
                    worldPoint = colliderDiff*worldSize + 2*colliderDiff*colliderIndex + colliderDiff + colliderPoint;
                    break;
                default:
                    Debug.Log("grassMaterialArray shift error!");
                    return;

            }
            MeshColliderArray[colliderPoint] = GroundObjectArray[worldPoint].GetComponent<MeshCollider>();
            MeshColliderArray[colliderPoint].enabled = true;
        }

        private void GrassArrayShift(Axis axis, int worldIndex)
        {
            /* 草のシェーダを管理する配列の中で指定した行か列を更新する */
            if (Mathf.Abs(worldIndex - WorldDistance) > GrassDistance)
                return;
            
            int worldPoint, grassPoint;
            int grassIndex = worldIndex - WorldDistance + GrassDistance;
            int grassDiff = WorldDistance - GrassDistance;

            if (Mathf.Abs(worldIndex - WorldDistance) <= GrassHighDistance)
            {
                int highIndex = grassIndex - GrassDistance + GrassHighDistance;
                int highDiff = GrassDistance - GrassHighDistance;
                SetGrassLOD(GetArrayPoint(axis, highIndex, GrassHighDistance, distanceDiff: highDiff, invert: true), LOD.Medium);
            }
            
            switch (axis)
            {
                case Axis.Xplus:
                    SetGrassLOD(grassIndex, LOD.Low);
                    for (int x=1; x<grassSize; x++)
                        System.Array.Copy(GrassMeshRendererArray, grassIndex+x*grassSize, GrassMeshRendererArray, grassIndex+(x-1)*grassSize, 1);
                    grassPoint = (grassSize-1) * grassSize + grassIndex;
                    worldPoint = (grassDiff+grassSize-1)*worldSize + grassDiff + grassIndex;
                    break;
                case Axis.Xminus:
                    SetGrassLOD((grassSize-1)*grassSize+grassIndex, LOD.Low);
                    for (int x=grassSize-1; x>0; x--)
                        System.Array.Copy(GrassMeshRendererArray, grassIndex+(x-1)*grassSize, GrassMeshRendererArray, grassIndex+x*grassSize, 1);
                    grassPoint = grassIndex;
                    worldPoint = grassDiff*worldSize + grassDiff + grassIndex;
                    break;
                case Axis.Zplus:
                    SetGrassLOD(grassIndex*grassSize, LOD.Low);
                    System.Array.Copy(GrassMeshRendererArray, grassIndex*grassSize+1, GrassMeshRendererArray, grassIndex*grassSize, grassSize-1);
                    grassPoint = (grassIndex+1) * grassSize - 1;
                    worldPoint = grassDiff*worldSize + 2*grassDiff*grassIndex + grassDiff + grassPoint;
                    break;
                case Axis.Zminus:
                    SetGrassLOD((grassIndex+1)*grassSize-1, LOD.Low);
                    System.Array.Copy(GrassMeshRendererArray, grassIndex*grassSize, GrassMeshRendererArray, grassIndex*grassSize+1, grassSize-1);
                    grassPoint = grassIndex * grassSize;
                    worldPoint = grassDiff*worldSize + 2*grassDiff*grassIndex + grassDiff + grassPoint;
                    break;
                default:
                    Debug.Log("grassMaterialArray shift error!");
                    return;
            }
            GrassMeshRendererArray[grassPoint] = GroundObjectArray[worldPoint].GetComponent<MeshRenderer>();
            SetGrassLOD(grassPoint, LOD.Medium);

            if (Mathf.Abs(worldIndex - WorldDistance) <= GrassHighDistance)
            {
                int highIndex = grassIndex - GrassDistance + GrassHighDistance;
                int highDiff = GrassDistance - GrassHighDistance;
                SetGrassLOD(GetArrayPoint(axis, highIndex, GrassHighDistance, distanceDiff: highDiff), LOD.High);
            }
        }

        private void GroundArrayShift(Vector3 referencePos, Axis axis, int index)
        {
            /* 地面のオブジェクトを管理する配列の中で指定した行か列を更新する */
            Vector3 addPositionDiff;
            int createIndex;
            float indexDiff = index - WorldDistance;

            switch (axis)
            {
                case Axis.Xplus:
                    Destroy(GroundObjectArray[index]);
                    for (int x=1; x<worldSize; x++)
                        System.Array.Copy(GroundObjectArray, index+x*worldSize, GroundObjectArray, index+(x-1)*worldSize, 1);
                    addPositionDiff = new Vector3((float)WorldDistance * Ground.XWidth, 0f, indexDiff * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    createIndex = (worldSize - 1) * worldSize + index;
                    break;
                case Axis.Xminus:
                    Destroy(GroundObjectArray[(worldSize-1)*worldSize+index]);
                    for (int x=worldSize-1; x>0; x--)
                        System.Array.Copy(GroundObjectArray, index+(x-1)*worldSize, GroundObjectArray, index+x*worldSize, 1);
                    addPositionDiff = new Vector3((float)(-1*WorldDistance) * Ground.XWidth, 0f, indexDiff * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    createIndex = index;
                    break;
                case Axis.Zplus:
                    Destroy(GroundObjectArray[index*worldSize]);
                    System.Array.Copy(GroundObjectArray, index*worldSize+1, GroundObjectArray, index*worldSize, worldSize-1);
                    addPositionDiff = new Vector3(indexDiff * Ground.XWidth, 0f, (float)WorldDistance * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    createIndex = (index + 1) * worldSize - 1;
                    break;
                case Axis.Zminus:
                    Destroy(GroundObjectArray[(index+1)*worldSize-1]);
                    System.Array.Copy(GroundObjectArray, index*worldSize, GroundObjectArray, index*worldSize+1, worldSize-1);
                    addPositionDiff = new Vector3(indexDiff * Ground.XWidth, 0f, (float)(-1*WorldDistance) * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    createIndex = index * worldSize;
                    break;
                default:
                    Debug.Log("GroundObject shift error!");
                    return;
            }
            GroundObjectArray[createIndex] = Instantiate(GroundObject, addPositionDiff, Quaternion.identity, this.transform);
            GrassArrayShift(axis, index);
            ColliderArrayShift(axis, index);
        }

        private void GenerateWorld()
        {
            /* ワールドを生成する */
            for (int x=0; x<worldSize; x++)
            {
                var _x = x * worldSize;
                var xDiff = x - WorldDistance;
                for (int z=0; z<worldSize; z++)
                {
                    var zDiff = z - WorldDistance;
                    var position = new Vector3(Ground.XWidth*xDiff+referencePosition.x, 0f, Ground.ZWidth*zDiff+referencePosition.z);
                    GroundObjectArray[_x+z] = Instantiate(GroundObject, position, Quaternion.identity, this.transform);

                    // 草の表示切替用
                    if (Mathf.Abs(xDiff) <= GrassHighDistance && Mathf.Abs(zDiff) <= GrassHighDistance)
                    {
                        var xGrass = xDiff + GrassDistance;
                        var zGrass = zDiff + GrassDistance;
                        GrassMeshRendererArray[xGrass*grassSize + zGrass] = GroundObjectArray[_x+z].GetComponent<MeshRenderer>();
                        SetGrassLOD(xGrass*grassSize + zGrass, LOD.High);
                    }
                    else if (Mathf.Abs(xDiff) <= GrassDistance && Mathf.Abs(zDiff) <= GrassDistance)
                    {
                        var xGrass = xDiff + GrassDistance;
                        var zGrass = zDiff + GrassDistance;
                        GrassMeshRendererArray[xGrass*grassSize + zGrass] = GroundObjectArray[_x+z].GetComponent<MeshRenderer>();
                        SetGrassLOD(xGrass*grassSize + zGrass, LOD.Medium);
                    }
                    // メッシュコライダー切替用
                    if (Mathf.Abs(xDiff) <= ColliderDistance && Mathf.Abs(zDiff) <= ColliderDistance)
                    {
                        var xCollider = xDiff + ColliderDistance;
                        var zCollider = zDiff + ColliderDistance;
                        MeshColliderArray[xCollider*colliderSize + zCollider] = GroundObjectArray[_x+z].GetComponent<MeshCollider>();
                        MeshColliderArray[xCollider*colliderSize + zCollider].enabled = true;
                    }
                }
            }
        }
    }
}
