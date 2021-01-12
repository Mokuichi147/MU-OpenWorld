using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace OpenWorld
{
    public class AvatarView : MonoBehaviour
    {
        public struct ScrollViewContent
        {
            public string FilePath;
            public VRM.VRMImporterContext Context;
            public VRM.VRMMetaObject Meta;
            public Image BackgroundImage;
        }

        // アバター一覧関連
        public Transform AvatarScrollView;
        public GameObject AvatarScrollViewContent;

        public Color NormalColor;
        public Color SelectedColor;

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

        private List<ScrollViewContent> contentDatas;
        private int preSelected = -1;


        public void GetAvatars()
        {
            contentDatas = new List<ScrollViewContent>();

            string[] avatarPaths = Directory.GetFiles(Data.AvatarDataPath, "*.vrm", SearchOption.AllDirectories);
            for (int i=0; i<avatarPaths.Length; i++)
            {
                var contentData = new ScrollViewContent();
                contentData.FilePath = Data.Separator(avatarPaths[i]);
                contentData.Context = AvatarController.GetCentext(avatarPaths[i]);
                contentData.Meta = contentData.Context.ReadMeta(true);

                var scrollViewContent = Instantiate(AvatarScrollViewContent, AvatarScrollView);
                // タイトル
                var titleObject = scrollViewContent.transform.Find("Title").gameObject;
                titleObject.GetComponent<TextMeshProUGUI>().text = contentData.Meta.Title != "" ? contentData.Meta.Title : "名前なし";
                // サムネイル
                var thumbnailObject = scrollViewContent.transform.Find("ThumbnailMask").gameObject.transform.Find("Thumbnail").gameObject;
                var thumnail = thumbnailObject.GetComponent<RawImage>();
                thumnail.texture = contentData.Meta.Thumbnail;
                // バージョン
                var versionObject = scrollViewContent.transform.Find("Version").gameObject;
                versionObject.GetComponent<TextMeshProUGUI>().text = contentData.Meta.Version != "" ? contentData.Meta.Version : "不明";
                // クリックアクション
                var buttonObject = scrollViewContent.transform.Find("Button").gameObject;
                var index = i;
                buttonObject.GetComponent<Button>().onClick.AddListener(() => {SelectAvatar(index);});
                // 背景画像
                contentData.BackgroundImage = scrollViewContent.transform.Find("Background").gameObject.GetComponent<Image>();
                contentData.BackgroundImage.color = NormalColor;

                contentDatas.Add(contentData);
            }
        }

        public void SelectAvatar(int index)
        {
            if (preSelected >= 0)
                contentDatas[preSelected].BackgroundImage.color = NormalColor;
        
            contentDatas[index].BackgroundImage.color = SelectedColor;
            
            SetPreview(index);
            SetMeta(index);

            preSelected = index;
        }

        private void SetPreview(int index)
        {
            if (avatarObject != null)
            {
                Destroy(avatarObject);
            }
            // モデル読み込み
            avatarObject = AvatarController.LoadFromPath(contentDatas[index].FilePath);
            // 初期化
            avatarObject.transform.parent = AvatarParent;
            avatarObject.transform.localPosition = Vector3.zero;
            avatarObject.transform.localRotation = Quaternion.identity;
            avatarObject.transform.localScale = Vector3.one;
            // アニメーターの設定
            AvatarController.InitAnimator(avatarObject, AvatarControllerScript.PlayerAvatar, AvatarControllerScript.PlayerAnimator);
            // レイヤーの変更
            foreach (var child in avatarObject.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI3D");
            }
        }

        private void SetMeta(int index)
        {
            Author.text = contentDatas[index].Meta.Author;
            ContactInformation.text = contentDatas[index].Meta.ContactInformation;
            Reference.text = contentDatas[index].Meta.Reference;
            AllowedUser.text = $"{contentDatas[index].Meta.AllowedUser}";
            ViolentUssage.text = $"{contentDatas[index].Meta.ViolentUssage}";
            SexualUssage.text = $"{contentDatas[index].Meta.SexualUssage}";
            CommercialUssage.text = $"{contentDatas[index].Meta.CommercialUssage}";
            LicenseType.text = $"{contentDatas[index].Meta.LicenseType}";
            OtherLicenseUrl.text = contentDatas[index].Meta.OtherLicenseUrl;
        }
    }
}
