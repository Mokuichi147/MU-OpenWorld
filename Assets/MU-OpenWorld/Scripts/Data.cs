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
            public int X;
            public int Z;
            public List<PrefabData> Prefabs;
        }


        // デフォルト値
        static public string RootPath = $"{Application.dataPath}/.user";
        static public string AppDataPath = $"{RootPath}/game.xml";
        static public string AvatarDataPath = $"{RootPath}/avatars";
        static public string WorldDataPath = $"{RootPath}/worlds";

        static public App AppData;
        static private string worldUUID;


        void OnApplicationQuit()
        {
            AppSave();
        }

        static public string Separator(string path)
        {
            return path.Replace("\\", "/");
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


        static private void CheckExistsDirectorys()
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory($"{RootPath}");

            if (!Directory.Exists(WorldDataPath))
                Directory.CreateDirectory($"{WorldDataPath}");

            if (!Directory.Exists(AvatarDataPath))
                Directory.CreateDirectory($"{AvatarDataPath}");
        }


        // アプリケーション関連
        static private void AppSave()
        {
            Save(AppDataPath, AppData);
            Debug.Log("App Saved!");
        }

        static public void AppLoad()
        {
            CheckExistsDirectorys();

            if (File.Exists(AppDataPath))
            {
                AppData = Load(AppDataPath, new App());
            }
            else
            {
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

            AppData.Worlds.Add(new WorldPath() {UUID=world.UUID, Path=$"{RootPath}/worlds/{world.UUID}"});

            Directory.CreateDirectory($"{RootPath}/worlds/{world.UUID}");

            worldUUID = world.UUID;
            WorldSave(world);

            AppData.PreWorldUUID = world.UUID;
            AppSave();
            Debug.Log("World Created!");
            return world;
        }

        static public World WorldLoad(string uuid)
        {
            World world = Load($"{RootPath}/worlds/{uuid}/world.xml", new World());
            AppData.PreWorldUUID = uuid;
            worldUUID = uuid;
            Debug.Log("World Loaded!");
            return world;
        }

        static public void WorldSave(World world)
        {
            Save($"{RootPath}/worlds/{worldUUID}/world.xml", world);
            Debug.Log("World Saved!");
        }


        // プレイヤー関連
        static public bool IsPlayerData()
        {
            return File.Exists($"{RootPath}/worlds/{worldUUID}/player.xml");
        }

        static public Player PlayerCreate()
        {
            var player = new Player();
            player.Position = new Vector3(0f, 0f, 0f);
            player.Rotation = Quaternion.identity;
            player.AvatarPath = "";

            Debug.Log("Player Created!");
            return player;
        }

        static public Player PlayerLoad()
        {
            Player player = Load($"{RootPath}/worlds/{worldUUID}/player.xml", new Player());
            Debug.Log("Player Loaded!");
            return player;
        }

        static public void PlayerSave(Player player)
        {
            Save($"{RootPath}/worlds/{worldUUID}/player.xml", player);
            Debug.Log("Player Saved!");
        }


        // チャンク関連
        static public bool IsChunkData(int x, int z)
        {
            string chunkName = x.ToString("x8") + "-" + z.ToString("x8");
            return File.Exists($"{RootPath}/worlds/{worldUUID}/chunks/{chunkName}.xml");
        }

        static public Chunk ChunkLoad(int x, int z)
        {
            string chunkName = x.ToString("x8") + "-" + z.ToString("x8");
            Chunk chunk = Load($"{RootPath}/worlds/{worldUUID}/chunks/{chunkName}.xml", new Chunk());
            return chunk;
        }

        static public void ChunkSave(Chunk chunk)
        {
            string chunkName = chunk.X.ToString("x8") + "-" + chunk.Z.ToString("x8");
            if (!Directory.Exists($"{RootPath}/worlds/{worldUUID}/chunks"))
                Directory.CreateDirectory($"{RootPath}/worlds/{worldUUID}/chunks");
            Save($"{RootPath}/worlds/{worldUUID}/chunks/{chunkName}.xml", chunk);
        }
    }
}