using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OpenWorld
{
    public class AvatarView : MonoBehaviour
    {
        public struct Avatar
        {
            public string FilePath;
            public VRM.VRMImporterContext Context;
            public VRM.VRMMetaObject Meta;
        }

        // アバター一覧関連
        public Transform AvatarScrollView;
        public GameObject AvatarScrollViewContent;

        // プレビュー関連
        public Transform AvatarParent;
        private GameObject avatarObject = null;

        // 詳細関連
        public TextMeshProUGUI Author;
        public TextMeshProUGUI ContactInformation;
        public TextMeshProUGUI Reference;
        public TextMeshProUGUI AllowedUser;
        public TextMeshProUGUI ViolentUssage;
        public TextMeshProUGUI SexualUssage;
        public TextMeshProUGUI CommercialUssage;
        public TextMeshProUGUI LicenseType;
        public TextMeshProUGUI OtherLicenseUrl;

        public AvatarController AvatarControllerScript;

        private List<Avatar> avatarDatas;


        public void GetAvatars()
        {
            avatarDatas = new List<Avatar>();

            string[] avatarPaths = Directory.GetFiles(Data.AvatarDataPath, "*.vrm", SearchOption.AllDirectories);
            for (int i=0; i<avatarPaths.Length; i++)
            {
                var avatarData = new Avatar();
                avatarData.FilePath = Data.Separator(avatarPaths[i]);
                avatarData.Context = AvatarController.GetCentext(avatarPaths[i]);
                avatarData.Meta = avatarData.Context.ReadMeta(true);

                avatarDatas.Add(avatarData);

                var scrollViewContent = Instantiate(AvatarScrollViewContent, AvatarScrollView);
                // タイトル
                var titleObject = scrollViewContent.transform.Find("Title").gameObject;
                titleObject.GetComponent<TextMeshProUGUI>().text = avatarData.Meta.Title != "" ? avatarData.Meta.Title : "名前なし";
                // サムネイル
                var thumbnailObject = scrollViewContent.transform.Find("ThumbnailMask").gameObject.transform.Find("Thumbnail").gameObject;
                var thumnail = thumbnailObject.GetComponent<RawImage>();
                thumnail.texture = avatarData.Meta.Thumbnail;
                // バージョン
                var versionObject = scrollViewContent.transform.Find("Version").gameObject;
                versionObject.GetComponent<TextMeshProUGUI>().text = avatarData.Meta.Version != "" ? avatarData.Meta.Version : "不明";
                // クリックアクション
                var buttonObject = scrollViewContent.transform.Find("Button").gameObject;
                var index = i;
                buttonObject.GetComponent<Button>().onClick.AddListener(() => {SelectAvatar(index);});
            }
        }

        public void SelectAvatar(int index)
        {
            SetPreview(index);
            SetMeta(index);
        }

        private void SetPreview(int index)
        {
            if (avatarObject != null)
            {
                Destroy(avatarObject);
            }
            var avatarPrefab = AvatarController.LoadFromPath(avatarDatas[index].FilePath);
            avatarObject = Instantiate(avatarPrefab, AvatarParent);
            // レイヤーの変更
            foreach (var child in avatarObject.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI3D");
            }
            // アニメーターの設定
            AvatarController.InitAnimator(avatarObject, AvatarControllerScript.PlayerAvatar, AvatarControllerScript.PlayerAnimator);
        }

        private void SetMeta(int index)
        {
            Author.text = avatarDatas[index].Meta.Author;
            ContactInformation.text = avatarDatas[index].Meta.ContactInformation;
            Reference.text = avatarDatas[index].Meta.Reference;
            AllowedUser.text = $"{avatarDatas[index].Meta.AllowedUser}";
            ViolentUssage.text = $"{avatarDatas[index].Meta.ViolentUssage}";
            SexualUssage.text = $"{avatarDatas[index].Meta.SexualUssage}";
            CommercialUssage.text = $"{avatarDatas[index].Meta.CommercialUssage}";
            LicenseType.text = $"{avatarDatas[index].Meta.LicenseType}";
            OtherLicenseUrl.text = avatarDatas[index].Meta.OtherLicenseUrl;
        }
    }
}
