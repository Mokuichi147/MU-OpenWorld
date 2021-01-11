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


        public Transform AvatarScrollView;
        public GameObject AvatarScrollViewContent;

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
            }
        }
    }
}
