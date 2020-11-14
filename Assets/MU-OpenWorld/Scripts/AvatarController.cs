using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Windows.Forms;
using VRM;

public class AvatarController : MonoBehaviour
{
    private string avatar_filepath;
    private GameObject avatar;

    void Start()
    {
        OpenFileDialog open_file_dialog = new OpenFileDialog();
        open_file_dialog.Filter = "vrmファイル(.vrm)|*.vrm";
        open_file_dialog.ShowDialog();
        var _filepath = Path.GetFullPath(open_file_dialog.FileName);
        avatar_filepath = _filepath.ToString().Replace('\\', '/');

        avatar = VRM.VRMImporter.LoadFromPath(avatar_filepath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
