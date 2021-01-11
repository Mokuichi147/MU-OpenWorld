using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class GameManager : MonoBehaviour
    {
        // UI関連
        public GameObject TitleView;
        public GameObject GameView;
        public GameObject MenuView;
        public GameObject LicenseView;
        public GameObject AvatarView;

        // カメラ関連
        public GameObject PlayerCamera;

        // その他
        public PrefabID PrefabIDScript;
        public WorldController WorldScript;
        public PlayerController PlayerScript;
        public AvatarView AvatarViewScript;


        void Awake()
        {
            Data.AppLoad();
            PrefabIDScript.Init();

            TitleView.SetActive(true);
            GameView.SetActive(false);
            MenuView.SetActive(false);
            LicenseView.SetActive(false);
            AvatarView.SetActive(false);
        }

        void Start()
        {
            TitleView.SetActive(false);
            WorldScript.Init();
            PlayerScript.InitPlayer();
            WorldScript.GenerateWorld();
            HideMenuView();
        }

        public void ShowMenuView()
        {
            GameView.SetActive(false);
            MenuView.SetActive(true);
            PlayerScript.ShowCursor();
        }

        public void HideMenuView()
        {
            GameView.SetActive(true);
            MenuView.SetActive(false);
            PlayerScript.HideCursor();
        }

        public void ShowLicenseView()
        {
            LicenseView.SetActive(true);
            MenuView.SetActive(false);
        }

        public void HideLicenseView()
        {
            LicenseView.SetActive(false);
            MenuView.SetActive(true);
        }

        public void ShowAvatarView()
        {
            PlayerCamera.SetActive(false);
            AvatarView.SetActive(true);
            MenuView.SetActive(false);
            AvatarViewScript.GetAvatars();
        }

        public void HideAvatarView()
        {
            PlayerCamera.SetActive(true);
            AvatarView.SetActive(false);
            MenuView.SetActive(true);
        }

        public void QuitApplication()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

}