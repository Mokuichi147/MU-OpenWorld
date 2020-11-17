using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Windows.Forms;
using VRM;


public class AvatarController : MonoBehaviour
{
    private string avatar_filepath;
    private GameObject avatar;

    InputAction move;

    void Start()
    {
        OpenFileDialog open_file_dialog = new OpenFileDialog();
        open_file_dialog.Filter = "vrmファイル(.vrm)|*.vrm";

        if (open_file_dialog.ShowDialog() == DialogResult.Cancel) return;
        
        var _filepath = Path.GetFullPath(open_file_dialog.FileName);
        avatar_filepath = _filepath.ToString().Replace('\\', '/');

        avatar = VRM.VRMImporter.LoadFromPath(avatar_filepath);
        avatar.transform.parent = this.transform;

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
