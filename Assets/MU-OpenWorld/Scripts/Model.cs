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


namespace OpenWorld.Avatar
{
    public class Model : MonoBehaviour
    {
        // アニメーション関連
        public UnityEngine.Avatar PlayerAvatar;
        public RuntimeAnimatorController PlayerAnimator;

        public string AvatarFilePath = "";


        [System.NonSerialized]
        public GameObject AvatarObject = null;
        [System.NonSerialized]
        public Animator AvatarAnimator;
        [System.NonSerialized]
        public Transform AvatarTransform;


        private GameObject Load(string filePath)
        {
            /* VRMモデルを配置する */
            GameObject avatarObject;
            if (filePath == "" || filePath == null)
                avatarObject = Instantiate(App.ObjectData.FromID("default_avatar"), this.transform);
            else
                avatarObject = Util.GetAvatarFromPath(filePath);

            // 位置の初期設定
            AvatarTransform = Util.InitPosition(avatarObject.transform, this.transform);
            // アニメーションの初期設定
            AvatarAnimator = Util.InitAnimator(avatarObject, PlayerAvatar, PlayerAnimator);
            return avatarObject;
        }

        public void PlayerLoad()
        {
            if (AvatarFilePath == App.DataFile.AppData.AvatarPath && AvatarFilePath != "")
                return;

            AvatarFilePath = App.DataFile.AppData.AvatarPath;

            Quaternion localRotation = Quaternion.identity;
            if (AvatarObject != null)
            {
                localRotation = AvatarObject.transform.localRotation;
                Destroy(AvatarObject);
            } 

            AvatarObject = Load(AvatarFilePath);
            AvatarObject.transform.localRotation = localRotation;
        }
    }
}