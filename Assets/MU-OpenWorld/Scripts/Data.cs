using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OpenWorld
{
    public class Data : MonoBehaviour
    {
        public struct WorldPath
        {
            public string UUID;
            public string Path;
        }

        [System.Serializable]
        public struct App
        {
            public string UUID;
            public string UserName;
            public string PreWorldUUID;
            public List<WorldPath> Worlds;
        }

        [System.Serializable]
        public struct World
        {
            public string UUID;
            public float Seed;
            public float Scale;
        }

        [System.Serializable]
        public struct Player
        {
            public Vector3 Position;
            public int Distance;
        }

        [System.Serializable]
        public struct Chunk
        {
            public int X;
            public int Y;

            public Vector3[] MeshPoints;
            public float[] YDiff;

            public bool IsWatar;
        }


        // デフォルト値
        static private string rootPath = Application.dataPath + "/UserData";
        static private string appDataPath = rootPath + "/GameData.xml";

        static public App AppData;


        void Awake()
        {
            AppLoad();
        }

        void OnApplicationQuit()
        {
            AppSave();
        }


        static private void Save<T>(string path, T data)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stream, data);
            }
        }

        static private T Load<T>(string path, T any)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(stream);
            }
        }


        // アプリケーション関連
        static private void AppSave()
        {
            Save(appDataPath, AppData);
        }

        static public void AppLoad()
        {
            if (File.Exists(appDataPath))
            {
                AppData = Load(appDataPath, new App());
            }
            else
            {
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory($"{rootPath}");
                AppData = new App();
                AppData.UUID = System.Guid.NewGuid().ToString();
                AppData.UserName = "";
                AppData.Worlds = new List<WorldPath>();
                AppData.PreWorldUUID = "";
                AppSave();
            }
        }


        // ワールド関連
        static public World WorldCreate()
        {
            AppLoad();

            var world = new World();
            world.UUID = System.Guid.NewGuid().ToString();
            world.Seed = 50000f;//Random.Range(0f, 70000f);
            world.Scale = 0.004f;

            AppData.Worlds.Add(new WorldPath() {UUID=world.UUID, Path=$"{rootPath}/worlds/{world.UUID}"});

            Directory.CreateDirectory($"{rootPath}/worlds/{world.UUID}");
            WorldSave(world);

            AppData.PreWorldUUID = world.UUID;
            AppSave();

            return world;
        }

        static public World WorldLoad(string uuid)
        {
            World world = Load($"{rootPath}/worlds/{uuid}/world.xml", new World());
            return world;
        }

        static public void WorldSave(World world)
        {
            Save($"{rootPath}/worlds/{world.UUID}/world.xml", world);
        }
    }
}