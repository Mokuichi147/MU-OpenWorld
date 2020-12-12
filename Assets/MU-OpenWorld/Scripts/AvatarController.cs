using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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
        public Avatar player_avatar;
        public AnimatorController player_animator;

        [Range(0f, 200f)]
        public float look_sensitivity = 100f;
        public string filepath = "Assets/MU-OpenWorld/Models/Avatars/Moyu.vrm";

        private Rigidbody rb;
        private GameObject avatar;
        private Animator avatar_animator;
        private Transform avater_rotate;
        private Transform camera_rotate;

        private InputAction move;
        private InputAction look;
        private InputActionTrace look_trace;
        private InputAction esc;
        private bool mouse_center = true;
        private Vector2 center_pos;
        static float delta_time = 0.02f;
        private float time_scale = 10f;

        void Start()
        {
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

            camera_rotate = GameObject.Find("CameraRotate").GetComponent<Transform>();
            rb = this.GetComponent<Rigidbody>();

            center_pos = new Vector2(Mathf.Round(Screen.width/2f), Mathf.Round(Screen.height/2f));
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
                    var delta_pos = _look.ReadValue<Vector2>();
                    camera_rot *= Quaternion.Euler(0f, delta_pos.x/Mathf.Pow((200f-look_sensitivity)/100f*0.26f+1.138f, 10f)*2f, 0f);
                }
                Mouse.current.WarpCursorPosition(center_pos);
                Cursor.visible = false;
                camera_rotate.rotation = camera_rot;
                look_trace.Clear();
            }

            var _move = move.ReadValue<Vector2>();
            if (_move.x == 0f && _move.y == 0f)
            {
                avatar_animator.SetFloat("speed", 0f, 0.1f, delta_time);
                return;
            }
            var _pos = rb.position;
            avatar_animator.SetFloat("speed", Mathf.Sqrt(Mathf.Pow(_move.x,2f)+Mathf.Pow(_move.y,2f)), 0.1f, delta_time);
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var _dx = _move.x * (5f / 50f);
            var _dy = _move.y * (5f / 50f);
            var move_vec = new Vector3(_dx, 0f, _dy);
            move_vec = camera_rotate.rotation * move_vec;
            var _rot = avater_rotate.rotation;
            avater_rotate.rotation = Quaternion.Lerp(_rot, Quaternion.LookRotation(move_vec), delta_time * time_scale);
            rb.MovePosition(new Vector3(_pos.x + move_vec.x, _pos.y, _pos.z + move_vec.z));
        }

        private void OnApplicationQuit()
        {
            look_trace.Dispose();
        }

        private GameObject LoadFromPath(string file_path)
        {
            var bytes = File.ReadAllBytes(file_path);
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            var meta = context.ReadMeta(false);
            context.Load();
            context.ShowMeshes();
            return context.Root;
        }

        private bool LoadVRM(string file_path)
        {
            avatar = LoadFromPath(file_path);
            if (avatar == null) return false;

            avatar.transform.parent = this.transform;
            avatar.transform.localPosition = new Vector3(0f, 0f, 0f);
            avater_rotate = avatar.GetComponent<Transform>();
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
            if (LoadVRM(filepath)) return;

    #if UNITY_EDITOR
            var _filepath = EditorUtility.OpenFilePanel("vrmファイル(.vrm)", "", "vrm");
            filepath = _filepath.ToString().Replace('\\', '/');
    #elif UNITY_STANDALONE_WIN
            OpenFileDialog open_file_dialog = new OpenFileDialog();
            open_file_dialog.Filter = "vrmファイル(.vrm)|*.vrm";

            if (open_file_dialog.ShowDialog() == DialogResult.Cancel) return;

            var _filepath = Path.GetFullPath(open_file_dialog.FileName);
            filepath = _filepath.ToString().Replace('\\', '/');
    #endif
            if (LoadVRM(filepath)) return;
        }
    }
}