using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.App
{
    public class ObjectData : MonoBehaviour
    {
        [System.Serializable]
        public struct PrefabInfo
        {
            public string ID;
            public GameObject PrefabObject;
            public string Name;
            public bool IsStatic;
            public bool IsItem;
        }

        public PrefabInfo[] PrefabsInspector;
        static public PrefabInfo[] Prefabs;
        static private Dictionary<string, int> idDict;

        public void Init()
        {
            Prefabs = PrefabsInspector;

            idDict = new Dictionary<string, int>();
            for (int i=0; i<Prefabs.Length; i++)
                idDict[Prefabs[i].ID] = i;
        }

        static public GameObject FromID(string id)
        {
            return Prefabs[idDict[id]].PrefabObject;
        }
    }
}