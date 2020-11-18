﻿using System.Collections;
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


public class AvatarController : MonoBehaviour
{
    private string avatar_filepath;
    private GameObject avatar;

    InputAction move;

    void Start()
    {
        avatar_filepath = "Assets/MU-OpenWorld/Models/Avatars/AliciaSolid.vrm";
        avatar = VRM.VRMImporter.LoadFromPath(avatar_filepath);
        if (avatar != null)
        {
            avatar.transform.parent = this.transform;
            avatar.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        else
        {
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
            avatar = VRM.VRMImporter.LoadFromPath(avatar_filepath);
            if (avatar != null)
            {
                avatar.transform.parent = this.transform;
                avatar.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }

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
}
