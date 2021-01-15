using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class GameManager : MonoBehaviour
    {
        public enum Mode
        {
            Title,
            Game,
            Menu
        }

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
        public AvatarController AvatarScript;
        public AvatarView AvatarViewScript;

        private Mode mode;


        void Awake()
        {
            Data.AppLoad();
            PrefabIDScript.Init();

            mode = Mode.Title;

            TitleView.SetActive(true);
            GameView.SetActive(false);
            MenuView.SetActive(false);
            LicenseView.SetActive(false);
            AvatarView.SetActive(false);
        }

        public void GameStart()
        {
            mode = Mode.Game;

            TitleView.SetActive(false);
            GameView.SetActive(true);
            WorldScript.Init();
            PlayerScript.InitPlayer();
            WorldScript.GenerateWorld(5);
            StartCoroutine(PlayerScript.InitInput());
        }

        public void ShowMenuView()
        {
            mode = Mode.Menu;

            GameView.SetActive(false);
            MenuView.SetActive(true);
            PlayerScript.ShowCursor();
        }

        public void HideMenuView()
        {
            mode = Mode.Game;

            GameView.SetActive(true);
            MenuView.SetActive(false);
            PlayerScript.HideCursor();
        }

        public void ShowLicenseView()
        {
            LicenseView.SetActive(true);
            TitleView.SetActive(false);
            MenuView.SetActive(false);
        }

        public void HideLicenseView()
        {
            LicenseView.SetActive(false);
            if (mode == Mode.Title)
                TitleView.SetActive(true);
            else
                MenuView.SetActive(true);
        }

        public void ShowAvatarView()
        {
            PlayerCamera.SetActive(false);
            AvatarView.SetActive(true);
            TitleView.SetActive(false);
            MenuView.SetActive(false);
            StartCoroutine(AvatarViewScript.GetAvatars(true));
        }

        public void HideAvatarView()
        {
            AvatarViewScript.SetPlayerAvatar();

            PlayerCamera.SetActive(true);
            AvatarView.SetActive(false);
            if (mode == Mode.Title)
                TitleView.SetActive(true);
            else
            {
                MenuView.SetActive(true);
                AvatarScript.PlayerLoad();
            }
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