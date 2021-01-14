using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;


namespace OpenWorld
{
    public class AvatarView : MonoBehaviour
    {
        public struct ScrollViewContent
        {
            public string FilePath;
            public VRM.VRMImporterContext Context;
            public VRM.VRMMetaObject Meta;
            public GameObject Content;
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
        private int selectedIndex = -1;

        void Awake()
        {
            contentDatas = new List<ScrollViewContent>();
        }


        public void GetAvatars(bool isPreviewSet)
        {
            string selectFilePath = selectedIndex != -1 ? contentDatas[selectedIndex].FilePath : Data.AppData.AvatarPath;

            if (contentDatas.Count != 0)
            {
                foreach (var content in contentDatas)
                {
                    Destroy(content.Content);
                }
                contentDatas.Clear();
            }

            string[] avatarPaths = Directory.GetFiles(Data.AvatarDataPath, "*.vrm", SearchOption.AllDirectories);
            for (int i=0; i<avatarPaths.Length; i++)
            {
                var filePath = Data.Separator(avatarPaths[i]);
                if (selectFilePath == filePath)
                {
                    selectedIndex = i;
                    CreateContent(filePath, i, true);
                }
                else
                {
                    CreateContent(filePath, i, false);
                }
            }
            if (isPreviewSet && selectedIndex != -1)
            {
                SelectAvatar(selectedIndex);
            }
        }
        
        private void CreateContent(string filePath, int index, bool isSelect)
        {
            var contentData = new ScrollViewContent();
            contentData.FilePath = filePath;
            contentData.Context = AvatarController.GetCentext(filePath);
            contentData.Meta = contentData.Context.ReadMeta(true);

            contentData.Content = Instantiate(AvatarScrollViewContent, AvatarScrollView);
            // タイトル
            var titleObject = contentData.Content.transform.Find("Title").gameObject;
            titleObject.GetComponent<TextMeshProUGUI>().text = contentData.Meta.Title != "" ? contentData.Meta.Title : "名前なし";
            // サムネイル
            var thumbnailObject = contentData.Content.transform.Find("ThumbnailMask").gameObject.transform.Find("Thumbnail").gameObject;
            var thumnail = thumbnailObject.GetComponent<RawImage>();
            thumnail.texture = contentData.Meta.Thumbnail;
            // バージョン
            var versionObject = contentData.Content.transform.Find("Version").gameObject;
            versionObject.GetComponent<TextMeshProUGUI>().text = contentData.Meta.Version != "" ? contentData.Meta.Version : "不明";
            // クリックアクション
            var buttonObject = contentData.Content.transform.Find("Button").gameObject;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => {SelectAvatar(index);});
            // 背景画像
            contentData.BackgroundImage = contentData.Content.transform.Find("Background").gameObject.GetComponent<Image>();
            contentData.BackgroundImage.color = isSelect ? SelectedColor : NormalColor;

            contentDatas.Add(contentData);
        }

        public void SelectAvatar(int index)
        {
            if (selectedIndex != -1)
                contentDatas[selectedIndex].BackgroundImage.color = NormalColor;
        
            contentDatas[index].BackgroundImage.color = SelectedColor;
            
            SetPreview(index);
            SetMeta(index);

            selectedIndex = index;
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

        public void SetPlayerAvatar()
        {
            if (selectedIndex == -1)
                return;

            Data.SetAvatar(contentDatas[selectedIndex].FilePath);
        }

        public void OpenFileBrowser()
        {
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", Data.AvatarDataPath, "", false, (string[] paths) => {  });
            GetAvatars(false);
        }
    }
}
