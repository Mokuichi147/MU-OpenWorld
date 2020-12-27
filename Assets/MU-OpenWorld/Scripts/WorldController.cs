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
        private int[] worldMaxArray;

        public GameObject Player;

        private Vector3 referencePosition;
        public GameObject GroundObject;
        public GameObject[] GroundObjectArray;

        private List<WorldShift> worldShiftList;


        void Awake()
        {
            // 配列の大きさを計算する
            InitWorld();

            worldShiftList = new List<WorldShift>();
        }

        public void InitWorld()
        {
            worldSize = WorldDistance * 2 + 1;
            worldMaxArray = Circle.HalfMax(WorldDistance);

            GroundObjectArray = new GameObject[worldSize * worldSize];
        }

        void Start()
        {
            referencePosition = Player.transform.position;

            GenerateWorld();
        }

        void Update()
        {
            // マップの更新が必要か
            Vector3 playerPosition = Player.transform.position;
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
                }
            }
        }
    }
}
