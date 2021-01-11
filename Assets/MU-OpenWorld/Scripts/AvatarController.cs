﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
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
        public RuntimeAnimatorController PlayerAnimator;

        public string AvatarFilePath;

        private Rigidbody playerRigidbody;

        [System.NonSerialized]
        public GameObject AvatarObject;
        [System.NonSerialized]
        public Animator AvatarAnimator;
        [System.NonSerialized]
        public Transform AvatarTransform;


        static public VRMImporterContext GetCentext(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            return context;
        }

        static private GameObject LoadFromPath(string filePath)
        {
            /* パスからVRMモデルを読み込む */
            var context = GetCentext(filePath);
            context.Load();
            context.ShowMeshes();
            return context.Root;
        }

        static public Transform InitPosition(GameObject avatarObject, Transform parent)
        {
            avatarObject.transform.parent = parent;
            avatarObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            var avatarTransform = avatarObject.GetComponent<Transform>();

            return avatarTransform;
        }

        static public void SetHeight(Transform parent, float addHeight=1f)
        {
            // 地面の高さに合わせる
            var position = parent.position;
            position.y = Ground.GetHeight(position.x, position.z) + addHeight;
            parent.position = position;
        }

        static public Animator InitAnimator(GameObject avatarObject, Avatar avatar, RuntimeAnimatorController animator)
        {
            var avatarAnimator = avatarObject.GetComponent<Animator>();
            avatarAnimator.avatar = avatar;
            avatarAnimator.runtimeAnimatorController = animator;

            return avatarAnimator;
        }

        private GameObject LoadVRM(string filePath)
        {
            /* VRMモデルを配置する */
            GameObject avatarObject;
            if (filePath == "")
                avatarObject = Instantiate(PrefabID.GetPrefab("DefaultAvatar"), this.transform);
            else
                avatarObject = LoadFromPath(filePath);

            // 位置の初期設定
            AvatarTransform = InitPosition(avatarObject, this.transform);
            // アニメーションの初期設定
            AvatarAnimator = InitAnimator(avatarObject, PlayerAvatar, PlayerAnimator);
            return avatarObject;
        }

        public void PlayerLoad(string avatarPath)
        {
            AvatarFilePath = avatarPath;
            AvatarObject = LoadVRM(AvatarFilePath);
        }

        private void SelectPath()
        {
            #if UNITY_EDITOR
                var filePath = EditorUtility.OpenFilePanel("vrmファイル(.vrm)", "", "vrm");
                AvatarFilePath = filePath.ToString().Replace('\\', '/');
            #elif UNITY_STANDALONE_WIN
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "vrmファイル(.vrm)|*.vrm";

                if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;

                var filePath = Path.GetFullPath(openFileDialog.FileName);
                AvatarFilePath = filePath.ToString().Replace('\\', '/');
            #endif
        }
    }
}