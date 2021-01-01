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
            public Quaternion Rotation;
            public string AvatarPath;
        }

        public struct PrefabData
        {
            public string ID;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        [System.Serializable]
        public struct Chunk
        {
            public List<PrefabData> Prefabs;
        }


        // デフォルト値
        static private string rootPath = Application.dataPath + "/UserData";
        static private string appDataPath = rootPath + "/GameData.xml";

        static public App AppData;
        static private string worldUUID;


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
            Debug.Log("App Saved!");
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
            Debug.Log("App Loaded!");
        }


        // ワールド関連
        static public World WorldCreate()
        {
            var world = new World();
            world.UUID = System.Guid.NewGuid().ToString();
            world.Seed = 50000f;//Random.Range(0f, 70000f);
            world.Scale = 0.004f;

            AppData.Worlds.Add(new WorldPath() {UUID=world.UUID, Path=$"{rootPath}/worlds/{world.UUID}"});

            Directory.CreateDirectory($"{rootPath}/worlds/{world.UUID}");

            worldUUID = world.UUID;
            WorldSave(world);

            AppData.PreWorldUUID = world.UUID;
            AppSave();
            Debug.Log("World Created!");
            return world;
        }

        static public World WorldLoad(string uuid)
        {
            World world = Load($"{rootPath}/worlds/{uuid}/world.xml", new World());
            AppData.PreWorldUUID = uuid;
            worldUUID = uuid;
            Debug.Log("World Loaded!");
            return world;
        }

        static public void WorldSave(World world)
        {
            Save($"{rootPath}/worlds/{worldUUID}/world.xml", world);
            Debug.Log("World Saved!");
        }


        // プレイヤー関連
        static public bool IsPlayerData()
        {
            return File.Exists($"{rootPath}/worlds/{worldUUID}/player.xml");
        }

        static public Player PlayerCreate()
        {
            var player = new Player();
            player.Position = new Vector3(0f, 0f, 0f);
            player.Rotation = Quaternion.identity;
            player.AvatarPath = $"{Application.dataPath}/MU-OpenWorld/Models/Avatars/Moyu.vrm";

            PlayerSave(player);
            Debug.Log("Player Created!");
            return player;
        }

        static public Player PlayerLoad()
        {
            Player player = Load($"{rootPath}/worlds/{worldUUID}/player.xml", new Player());
            Debug.Log("Player Loaded!");
            return player;
        }

        static public void PlayerSave(Player player)
        {
            Save($"{rootPath}/worlds/{worldUUID}/player.xml", player);
            Debug.Log("Player Saved!");
        }


        // チャンク関連
        static public bool IsChunkData(int x, int z)
        {
            string chankName = x.ToString("x8") + "-" + z.ToString("x8");
            return File.Exists($"{rootPath}/worlds/{worldUUID}/chanks/{chankName}.xml");
        }

        static public Chunk ChunkLoad(int x, int z)
        {
            string chankName = x.ToString("x8") + "-" + z.ToString("x8");
            Chunk chank = Load($"{rootPath}/worlds/{worldUUID}/chanks/{chankName}.xml", new Chunk());
            return chank;
        }

        static public void ChunkSave(int x, int z, Chunk chank)
        {
            string chankName = x.ToString("x8") + "-" + z.ToString("x8");
            if (!Directory.Exists($"{rootPath}/worlds/{worldUUID}/chanks"))
                Directory.CreateDirectory($"{rootPath}/worlds/{worldUUID}/chanks");
            Save($"{rootPath}/worlds/{worldUUID}/chanks/{chankName}.xml", chank);
        }
    }
}