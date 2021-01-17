using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OpenWorld.App
{
    public class DataFile : MonoBehaviour
    {
        [System.Serializable]
        public struct App
        {
            public string UserID;
            public string UserName;
            public string WorldPath;
            public string AvatarPath;
        }

        [System.Serializable]
        public struct World
        {
            public string Name;
            public float Seed;
            public float Scale;
        }

        [System.Serializable]
        public struct Player
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Quaternion CameraRotation;
        }

        public struct PrefabData
        {
            public string PrefabID;
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


        // デフォルトパス
        static public string RootPath = $"{Application.dataPath}/.user";
        static public string AppDataPath = $"{RootPath}/game.xml";
        static public string AvatarDataPath = $"{RootPath}/avatars";
        static public string WorldDataPath = $"{RootPath}/worlds";

        static public App AppData;


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
        static public void AppSave()
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
                AppData.UserID = System.Guid.NewGuid().ToString();
                AppData.UserName = "";
                AppData.WorldPath = "";
                AppData.AvatarPath = "";
                AppSave();
            }
            Debug.Log("App Loaded!");
        }

        static public void SetAvatar(string filePath)
        {
            AppData.AvatarPath = filePath;
            AppSave();
        }

        static public void SetWorld(string filePath)
        {
            AppData.WorldPath = filePath;
            AppSave();
        }


        // ワールド関連
        static public World WorldCreate()
        {
            var world = new World();
            world.Name = "New World";
            world.Seed = 50000f;//Random.Range(0f, 70000f);
            world.Scale = 0.004f;

            var worldFIleName = System.Guid.NewGuid().ToString();

            Directory.CreateDirectory($"{WorldDataPath}/{worldFIleName}");

            AppData.WorldPath = $"{WorldDataPath}/{worldFIleName}";
            WorldSave(world);
            AppSave();

            Debug.Log("World Created!");
            return world;
        }

        static public World WorldLoad(string worldFilePath)
        {
            World world = Load($"{worldFilePath}/world.xml", new World());
            AppData.WorldPath = worldFilePath;
            Debug.Log("World Loaded!");
            return world;
        }

        static public void WorldSave(World world)
        {
            Save($"{AppData.WorldPath}/world.xml", world);
            Debug.Log("World Saved!");
        }


        // プレイヤー関連
        static public bool IsPlayerData()
        {
            return File.Exists($"{AppData.WorldPath}/player.xml");
        }

        static public Player PlayerCreate()
        {
            var player = new Player();
            player.Position = Vector3.zero;
            player.Rotation = Quaternion.identity;
            player.CameraRotation = Quaternion.identity;

            Debug.Log("Player Created!");
            return player;
        }

        static public Player PlayerLoad()
        {
            Player player = Load($"{AppData.WorldPath}/player.xml", new Player());
            Debug.Log("Player Loaded!");
            return player;
        }

        static public void PlayerSave(Player player)
        {
            Save($"{AppData.WorldPath}/player.xml", player);
            Debug.Log("Player Saved!");
        }


        // チャンク関連
        static public bool IsChunkData(int x, int z)
        {
            string chunkName = x.ToString("x8") + "-" + z.ToString("x8");
            return File.Exists($"{AppData.WorldPath}/chunks/{chunkName}.xml");
        }

        static public Chunk ChunkLoad(int x, int z)
        {
            string chunkName = x.ToString("x8") + "-" + z.ToString("x8");
            Chunk chunk = Load($"{AppData.WorldPath}/chunks/{chunkName}.xml", new Chunk());
            return chunk;
        }

        static public void ChunkSave(Chunk chunk)
        {
            string chunkName = chunk.X.ToString("x8") + "-" + chunk.Z.ToString("x8");

            if (!Directory.Exists($"{AppData.WorldPath}/chunks"))
                Directory.CreateDirectory($"{AppData.WorldPath}/chunks");

            Save($"{AppData.WorldPath}/chunks/{chunkName}.xml", chunk);
        }
    }
}