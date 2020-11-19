using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private string avatar_filepath;
        private GameObject avatar;

        InputAction move;

        void Start()
        {
            avatar_filepath = "Assets/MU-OpenWorld/Models/Avatars/AliciaSolid.vrm";
            LoadAvatar();

            move = this.GetComponent<PlayerInput>().currentActionMap["Move"];
        }

        void FixedUpdate()
        {
            var _pos = this.transform.position;
            var _move = move.ReadValue<Vector2>();
            var _dx = _move.x * 0.5f;
            var _dy = _move.y * 0.5f;
            this.transform.position = new Vector3(_pos.x + _dx, _pos.y, _pos.z + _dy);
        }

        private bool LoadVRM(string file_path)
        {
            avatar = VRM.VRMImporter.LoadFromPath(file_path);
            if (avatar == null) return false;

            avatar.transform.parent = this.transform;
            avatar.transform.localPosition = new Vector3(0f, 0f, 0f);
            var _player = GameObject.Find("World").GetComponent<WorldController>();
            var _pos = this.transform.position;
            _pos.y = _player.GetGroundHeight(_pos.x, _pos.z) + 2f;
            this.transform.position = _pos;
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