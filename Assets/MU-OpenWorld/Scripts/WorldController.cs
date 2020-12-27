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
        public GameObject[,] GroundObjectArray;

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

            GroundObjectArray = new GameObject[worldSize, worldSize];
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

        private (int, int) GetCircleEdgePoint(Axis axis, int index, bool invert=false)
        {
            /* 二次元配列の座標から一次元配列の座標の位置を返す */
            if (invert)
                axis = (Axis)(-1 * (int)axis);

            switch (axis)
            {
                case Axis.Xplus:
                    return (WorldDistance+worldMaxArray[index], index);
                case Axis.Xminus:
                    return (WorldDistance-worldMaxArray[index], index);
                case Axis.Zplus:
                    return (index, WorldDistance+worldMaxArray[index]);
                case Axis.Zminus:
                    return (index, WorldDistance-worldMaxArray[index]);
                default:
                    Debug.Log("get array point error!");
                    return (-1, -1);
            }
        }

        private void GroundArrayShift(Vector3 referencePos, Axis axis, int index)
        {
            /* 地面のオブジェクトを管理する配列の中で指定した行か列を更新する */
            Vector3 addPositionDiff;
            float indexDiff = index - WorldDistance;

            var (x, z) = GetCircleEdgePoint(axis, index, invert: true);
            Destroy(GroundObjectArray[x, z]);

            switch (axis)
            {
                case Axis.Xplus:
                    for (int px=WorldDistance-worldMaxArray[index]; px<WorldDistance+worldMaxArray[index]; px++)
                        GroundObjectArray[px, z] = GroundObjectArray[px+1, z];
                    addPositionDiff = new Vector3(worldMaxArray[index] * Ground.XWidth, 0f, indexDiff * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    break;
                case Axis.Xminus:
                    for (int px=WorldDistance+worldMaxArray[index]-1; px>=WorldDistance-worldMaxArray[index]; px--)
                        GroundObjectArray[px+1, z] = GroundObjectArray[px, z];
                    addPositionDiff = new Vector3(-worldMaxArray[index] * Ground.XWidth, 0f, indexDiff * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    break;
                case Axis.Zplus:
                    for (int pz=WorldDistance-worldMaxArray[index]; pz<WorldDistance+worldMaxArray[index]; pz++)
                        GroundObjectArray[x, pz] = GroundObjectArray[x, pz+1];
                    addPositionDiff = new Vector3(indexDiff * Ground.XWidth, 0f, worldMaxArray[index] * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    break;
                case Axis.Zminus:
                    for (int pz=WorldDistance+worldMaxArray[index]-1; pz>=WorldDistance-worldMaxArray[index]; pz--)
                        GroundObjectArray[x, pz+1] = GroundObjectArray[x, pz];
                    addPositionDiff = new Vector3(indexDiff * Ground.XWidth, 0f, -worldMaxArray[index] * Ground.ZWidth) + new Vector3(referencePos.x, 0f, referencePos.z);
                    break;
                default:
                    Debug.Log("GroundObject shift error!");
                    return;
            }

            (x, z) = GetCircleEdgePoint(axis, index);
            GroundObjectArray[x, z] = Instantiate(GroundObject, addPositionDiff, Quaternion.identity, this.transform);
        }

        private void GenerateWorld()
        {
            /* ワールドを生成する */
            for (int x=0; x<worldSize; x++)
            {
                var xDiff = x - WorldDistance;
                for (int z=WorldDistance-worldMaxArray[x]; z<=WorldDistance+worldMaxArray[x]; z++)
                {
                    var zDiff = z - WorldDistance;
                    var position = new Vector3(Ground.XWidth*xDiff+referencePosition.x, 0f, Ground.ZWidth*zDiff+referencePosition.z);
                    GroundObjectArray[x, z] = Instantiate(GroundObject, position, Quaternion.identity, this.transform);
                }
            }
        }
    }
}
