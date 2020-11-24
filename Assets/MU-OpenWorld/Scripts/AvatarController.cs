using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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
        private Animator avatar_animator;

        private Transform camera_rotate;

        InputAction move;
        InputAction look;
        InputActionTrace look_trace;
        InputAction esc;
        Rigidbody player_rb;
        private bool mouse_center = true;
        private Vector2 pre_mouse_pos = new Vector2(0f, 0f);
        private Vector2 center_pos;

        void Start()
        {
            avatar_filepath = "Assets/MU-OpenWorld/Models/Avatars/Moyu.vrm";
            LoadAvatar();

            var player_input = this.GetComponent<PlayerInput>();
            move = player_input.currentActionMap["Move"];
            look = player_input.currentActionMap["Look"];
            look_trace = new InputActionTrace();
            look_trace.SubscribeTo(look);

            esc = player_input.currentActionMap["Esc"];
            esc.performed += (callback) =>
            {
                if (mouse_center)
                {
                    Cursor.visible = true;
                    look_trace.UnsubscribeFrom(look);
                    mouse_center = false;
                }
                else
                {
                    look_trace.SubscribeTo(look);
                    mouse_center = true;
                }
            };

            camera_rotate = this.transform;
            player_rb = this.GetComponent<Rigidbody>();

            center_pos = new Vector2(Mathf.Round(Screen.width/2f), Mathf.Round(Screen.height/2f));
            pre_mouse_pos = center_pos;
            Mouse.current.WarpCursorPosition(center_pos);
            Cursor.visible = false;
        }

        void FixedUpdate()
        {
            // 視点操作
            if (mouse_center)
            {
                Quaternion camera_rot = camera_rotate.rotation;
                foreach (var _look in look_trace)
                {
                    var mouse_pos = _look.ReadValue<Vector2>();
                    var delta_pos = mouse_pos - pre_mouse_pos;
                    camera_rot *= Quaternion.Euler(0f, Mathf.Round(delta_pos.x/2f)/5f, 0f);
                    pre_mouse_pos = mouse_pos;
                }
                Mouse.current.WarpCursorPosition(center_pos);
                Cursor.visible = false;
                camera_rotate.rotation = camera_rot;
                pre_mouse_pos = center_pos;
                look_trace.Clear();
            }
            var _pos = player_rb.position;
            var _move = move.ReadValue<Vector2>();
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var _dx = _move.x * (5f / 50f);
            var _dy = _move.y * (5f / 50f);
            avatar_animator.SetFloat("speed", Mathf.Sqrt(Mathf.Pow(_move.x,2f)+Mathf.Pow(_move.y,2f)), 0.1f, Time.deltaTime);
            var move_vec = new Vector3(_dx, 0f, _dy);
            move_vec = this.transform.rotation * move_vec;
            player_rb.MovePosition(new Vector3(_pos.x + move_vec.x, _pos.y, _pos.z + move_vec.z));
        }

        private void OnApplicationQuit()
        {
            look_trace.Dispose();
        }

        private bool LoadVRM(string file_path)
        {
            avatar = VRM.VRMImporter.LoadFromPath(file_path);
            if (avatar == null) return false;

            avatar.transform.parent = this.transform;
            avatar.transform.localPosition = new Vector3(0f, 0f, 0f);
            // 地面の高さに合わせる
            var _pos = this.transform.position;
            _pos.y = Ground.GetHeight(_pos.x, _pos.z) + 1f;
            this.transform.position = _pos;
            // アニメーションの設定
            avatar_animator = avatar.GetComponent<Animator>();
            avatar_animator.avatar = player_avatar;
            avatar_animator.runtimeAnimatorController = player_animator;
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