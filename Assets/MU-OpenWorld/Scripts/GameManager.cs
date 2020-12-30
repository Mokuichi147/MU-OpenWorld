using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class GameManager : MonoBehaviour
    {
        // UI関連
        public GameObject TitleView;
        [SerializeField]
        private GameObject menuViewTemp;
        static public GameObject MenuView;

        // その他
        public WorldController WorldScript;
        public PlayerController PlayerScript;

        void Awake()
        {
            // インスペクター上で指定した値をstatic変数として使用する
            MenuView = menuViewTemp;

            TitleView.SetActive(true);
            MenuView.SetActive(false);
        }

        void GameStart()
        {
            TitleView.SetActive(false);
            WorldScript.Init();
            PlayerScript.Init();
        }

        static public void ShowMenuView()
        {
            MenuView.SetActive(true);
            PlayerController.ShowCursor();
        }

        static public void HideMenuView()
        {
            MenuView.SetActive(false);
            PlayerController.HideCursor();
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