using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRM;

namespace OpenWorld.Avatar
{
    public class Util : MonoBehaviour
    {
        static public VRMImporterContext GetContext(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            return context;
        }

        static public GameObject GetAvatar(VRMImporterContext context)
        {
            context.Load();
            context.ShowMeshes();
            return context.Root;
        }

        static public GameObject GetAvatarFromPath(string filePath)
        {
            /* パスからVRMモデルを読み込む */
            var context = GetContext(filePath);
            return GetAvatar(context);
        }

        static public Transform InitPosition(Transform avatarTransform, Transform parent)
        {
            avatarTransform.parent = parent;
            avatarTransform.localPosition = Vector3.zero;
            return avatarTransform;
        }

        static public Animator InitAnimator(GameObject avatarObject, UnityEngine.Avatar avatar, RuntimeAnimatorController animator)
        {
            var avatarAnimator = avatarObject.GetComponent<Animator>();
            avatarAnimator.avatar = avatar;
            avatarAnimator.runtimeAnimatorController = animator;

            return avatarAnimator;
        }

        static public void SetHeight(Transform parent, float addHeight=1f)
        {
            // 地面の高さに合わせる
            var position = parent.position;
            position.y = Ground.GetHeight(position.x, position.z) + addHeight;
            parent.position = position;
        }
    }
}