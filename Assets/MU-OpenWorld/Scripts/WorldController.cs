using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
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
        public bool isActive = false;

        [Range(1, 64)]
        public int WorldDistance = 10;
        private int worldSize;
        private int[] worldMaxArray;

        public GameObject Player;

        private Vector3 referencePosition;
        public GameObject ChankObject;
        public GameObject[,] ChankObjectArray;

        private List<WorldShift> worldShiftList;
        static public List<Data.Chunk> chunkSaveList;


        public void Init()
        {
            // 配列の大きさを計算する
            InitWorld();

            worldShiftList = new List<WorldShift>();
            chunkSaveList = new List<Data.Chunk>();
        }

        public void InitWorld()
        {
            Data.World world;
            if (Data.AppData.PreWorldUUID != "")
                world = Data.WorldLoad(Data.AppData.PreWorldUUID);
            else
                world = Data.WorldCreate();
            Ground.WorldSeed = world.Seed;
            Ground.WorldScale = world.Scale;
            worldSize = WorldDistance * 2 + 1;
            worldMaxArray = Circle.HalfMax(WorldDistance);

            ChankObjectArray = new GameObject[worldSize, worldSize];
        }

        void Update()
        {
            if (!isActive)
                return;

            // マップの更新が必要か
            float xPosition = Mathf.Floor(Player.transform.position.x / Ground.XWidth);
            float zPosition = Mathf.Floor(Player.transform.position.z / Ground.ZWidth);
            Axis axis = Axis.None;
            if (xPosition > referencePosition.x)
            {
                axis = Axis.Xplus;
                referencePosition.x++;
            }
            else if (xPosition < referencePosition.x)
            {
                axis = Axis.Xminus;
                referencePosition.x--;
            }
            else if (zPosition > referencePosition.z)
            {
                axis = Axis.Zplus;
                referencePosition.z++;
            }
            else if (zPosition < referencePosition.z)
            {
                axis = Axis.Zminus;
                referencePosition.z--;
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
            {
                if (chunkSaveList.Count == 0)
                    return;
                var chunkSave = chunkSaveList[0];
                chunkSaveList.RemoveAt(0);
                Data.ChunkSave(chunkSave);
                return;
            }

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
            Destroy(ChankObjectArray[x, z]);

            switch (axis)
            {
                case Axis.Xplus:
                    for (int px=WorldDistance-worldMaxArray[index]; px<WorldDistance+worldMaxArray[index]; px++)
                        ChankObjectArray[px, z] = ChankObjectArray[px+1, z];
                    addPositionDiff = new Vector3((worldMaxArray[index]+referencePos.x) * Ground.XWidth, 0f, (indexDiff+referencePos.z) * Ground.ZWidth);
                    break;
                case Axis.Xminus:
                    for (int px=WorldDistance+worldMaxArray[index]-1; px>=WorldDistance-worldMaxArray[index]; px--)
                        ChankObjectArray[px+1, z] = ChankObjectArray[px, z];
                    addPositionDiff = new Vector3((-worldMaxArray[index]+referencePos.x) * Ground.XWidth, 0f, (indexDiff+referencePos.z) * Ground.ZWidth);
                    break;
                case Axis.Zplus:
                    for (int pz=WorldDistance-worldMaxArray[index]; pz<WorldDistance+worldMaxArray[index]; pz++)
                        ChankObjectArray[x, pz] = ChankObjectArray[x, pz+1];
                    addPositionDiff = new Vector3((indexDiff+referencePos.x) * Ground.XWidth, 0f, (worldMaxArray[index]+referencePos.z) * Ground.ZWidth);
                    break;
                case Axis.Zminus:
                    for (int pz=WorldDistance+worldMaxArray[index]-1; pz>=WorldDistance-worldMaxArray[index]; pz--)
                        ChankObjectArray[x, pz+1] = ChankObjectArray[x, pz];
                    addPositionDiff = new Vector3((indexDiff+referencePos.x) * Ground.XWidth, 0f, (-worldMaxArray[index]+referencePos.z) * Ground.ZWidth);
                    break;
                default:
                    Debug.Log("ChankObject shift error!");
                    return;
            }

            (x, z) = GetCircleEdgePoint(axis, index);
            ChankObjectArray[x, z] = Instantiate(ChankObject, addPositionDiff, Quaternion.identity, this.transform);
        }

        private void CreateChunk(int x, int z)
        {
            var xDiff = x - WorldDistance;
            var zDiff = z - WorldDistance;
            var position = new Vector3(Ground.XWidth*(xDiff+referencePosition.x), 0f, Ground.ZWidth*(zDiff+referencePosition.z));
            ChankObjectArray[x, z] = Instantiate(ChankObject, position, Quaternion.identity, this.transform);
        }

        public void GenerateWorld(int maxSize)
        {
            referencePosition = new Vector3(Mathf.Floor(Player.transform.position.x/Ground.XWidth), 0f, Mathf.Floor(Player.transform.position.z/Ground.ZWidth));

            int x = WorldDistance;
            int z = WorldDistance;

            /* ワールドを渦巻順に生成する */
            CreateChunk(x, z);

            int maxCount = Mathf.Min(maxSize, worldSize);
            for (int i=1; i<=maxCount; i++)
            {
                if (i == worldSize)
                {
                    for (int count=0; count<worldSize-1; count++)
                    {
                        z -= 1;
                        if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                            continue;
                        CreateChunk(x, z);
                    }
                    return;
                }
                for (int count=0; count<i; count++)
                {
                    z += i%2==0 ? 1 : -1;
                    if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                        continue;
                    CreateChunk(x, z);
                }
                for (int count=0; count<i; count++)
                {
                    x += i%2==0 ? 1 : -1;
                    if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                        continue;
                    CreateChunk(x, z);
                }
            }
            this.StartCoroutine(GenerateWorldCoroutine(maxSize+1));
        }

        public IEnumerator GenerateWorldCoroutine(int startSize)
        {
            int x = startSize%2==0 ? WorldDistance-startSize/2 : WorldDistance+startSize/2;
            int z = x;

            /* ワールドを渦巻順に生成する */
            for (int i=startSize; i<=worldSize; i++)
            {
                if (i == worldSize)
                {
                    for (int count=0; count<worldSize-1; count++)
                    {
                        z -= 1;
                        if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                            continue;
                        CreateChunk(x, z);
                        yield return null;
                    }
                    break;
                }
                for (int count=0; count<i; count++)
                {
                    z += i%2==0 ? 1 : -1;
                    if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                        continue;
                    CreateChunk(x, z);
                    yield return null;
                }
                for (int count=0; count<i; count++)
                {
                    x += i%2==0 ? 1 : -1;
                    if (WorldDistance-worldMaxArray[x] > z || z > WorldDistance+worldMaxArray[x])
                        continue;
                    CreateChunk(x, z);
                    yield return null;
                }
            }
            isActive = true;
        }
    }
}
