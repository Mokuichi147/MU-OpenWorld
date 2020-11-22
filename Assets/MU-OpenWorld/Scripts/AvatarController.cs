﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;
#if UNITY_EDITOR
using UnityEditor;
#elif UNITY_STANDALONE_WIN
using System.IO;
using System.Windows.Forms;
#endif
using VRM;

namespace OpenWorld
{
    public class AvatarController : MonoBehaviour
    {
        public Avatar player_avatar;
        public AnimatorController player_animator;

        private string avatar_filepath;
        private GameObject avatar;
        private Animator avatar_anim;

        InputAction move;

        void Start()
        {
            avatar_filepath = "Assets/MU-OpenWorld/Models/Avatars/Moyu.vrm";
            LoadAvatar();

            move = this.GetComponent<PlayerInput>().currentActionMap["Move"];
        }

        void FixedUpdate()
        {
            var _pos = this.transform.position;
            var _move = move.ReadValue<Vector2>();
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var _dx = _move.x * (5f / 50f);
            var _dy = _move.y * (5f / 50f);
            this.transform.position = new Vector3(_pos.x + _dx, _pos.y, _pos.z + _dy);
        }

        private bool LoadVRM(string file_path)
        {
            avatar = VRM.VRMImporter.LoadFromPath(file_path);
            if (avatar == null) return false;

            avatar.transform.parent = this.transform;
            avatar.transform.localPosition = new Vector3(0f, 1f, 0f);
            // 地面の高さに合わせる
            var _pos = this.transform.position;
            _pos.y = WorldController.GetGroundHeight(_pos.x, _pos.z) + 1f;
            this.transform.position = _pos;
            // アニメーションの設定
            avatar_anim = avatar.GetComponent<Animator>();
            avatar_anim.avatar = player_avatar;
            avatar_anim.runtimeAnimatorController = player_animator;
            return true;
        }

        private void LoadAvatar()
        {
            if (LoadVRM(avatar_filepath)) return;

    #if UNITY_EDITOR
            var _filepath = EditorUtility.OpenFilePanel("vrmファイル(.vrm)", "", "vrm");
            avatar_filepath = _filepath.ToString().Replace('\\', '/');
    #elif UNITY_STANDALONE_WIN
            OpenFileDialog open_file_dialog = new OpenFileDialog();
            open_file_dialog.Filter = "vrmファイル(.vrm)|*.vrm";

            if (open_file_dialog.ShowDialog() == DialogResult.Cancel) return;

            var _filepath = Path.GetFullPath(open_file_dialog.FileName);
            avatar_filepath = _filepath.ToString().Replace('\\', '/');
    #endif
            if (LoadVRM(avatar_filepath)) return;
        }
    }
}