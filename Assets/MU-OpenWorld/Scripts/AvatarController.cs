using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#elif UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
using VRM;

namespace OpenWorld
{
    public class AvatarController : MonoBehaviour
    {
        // アニメーション関連
        public Avatar PlayerAvatar;
        public AnimatorController PlayerAnimator;

        public string avatarFilePath = "Assets/MU-OpenWorld/Models/Avatars/Moyu.vrm";

        private Rigidbody playerRigidbody;
        public GameObject AvatarObject;
        public Animator AvatarAnimator;
        public Transform AvatarTransform;
        private Transform cameraTransform;

        void Awake()
        {
            LoadAvatar();
        }

        private GameObject LoadFromPath(string filePath)
        {
            /* パスからVRMモデルを読み込む */
            var bytes = File.ReadAllBytes(filePath);
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            var meta = context.ReadMeta(false);
            context.Load();
            context.ShowMeshes();
            return context.Root;
        }

        private GameObject LoadVRM(string filePath)
        {
            /* VRMモデルを配置する */
            AvatarObject = LoadFromPath(filePath);
            if (AvatarObject == null) return null;

            AvatarObject.transform.parent = this.transform;
            AvatarObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            AvatarTransform = AvatarObject.GetComponent<Transform>();
            // 地面の高さに合わせる
            var position = this.transform.position;
            position.y = Ground.GetHeight(position.x, position.z) + 1f;
            this.transform.position = position;
            // アニメーションの設定
            AvatarAnimator = AvatarObject.GetComponent<Animator>();
            AvatarAnimator.avatar = PlayerAvatar;
            AvatarAnimator.runtimeAnimatorController = PlayerAnimator;
            return AvatarObject;
        }

        private void LoadAvatar()
        {
            /* デフォルトモデルの読み込み */
            if (LoadVRM(avatarFilePath) != null) return;

#if UNITY_EDITOR
            var filePath = EditorUtility.OpenFilePanel("vrmファイル(.vrm)", "", "vrm");
            avatarFilePath = filePath.ToString().Replace('\\', '/');
#elif UNITY_STANDALONE_WIN
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "vrmファイル(.vrm)|*.vrm";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;

            var filePath = Path.GetFullPath(openFileDialog.FileName);
            avatarFilePath = filePath.ToString().Replace('\\', '/');
#endif
            if (LoadVRM(avatarFilePath) != null) return;
        }
    }
}