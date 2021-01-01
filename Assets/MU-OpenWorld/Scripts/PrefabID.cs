using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class PrefabID : MonoBehaviour
    {
        [System.Serializable]
        public struct PrefabData
        {
            public string ID;
            public GameObject PrefabObject;
        }

        public PrefabData[] PrefabsInspector;
        static public PrefabData[] Prefabs;
        static private Dictionary<string, int> idDict;

        public void Init()
        {
            Prefabs = PrefabsInspector;

            idDict = new Dictionary<string, int>();
            for (int i=0; i<Prefabs.Length; i++)
                idDict[Prefabs[i].ID] = i;
        }

        static public GameObject GetPrefab(string id)
        {
            return Prefabs[idDict[id]].PrefabObject;
        }
    }
}