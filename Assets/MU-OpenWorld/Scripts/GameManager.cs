using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class GameManager : MonoBehaviour
    {
        // UI関連
        public GameObject TitleView;
        public GameObject MenuView;

        // その他
        public PrefabID PrefabIDScript;
        public WorldController WorldScript;
        public PlayerController PlayerScript;


        void Awake()
        {
            Data.AppLoad();
            PrefabIDScript.Init();

            TitleView.SetActive(true);
            MenuView.SetActive(false);
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
            MenuView.SetActive(true);
            PlayerScript.ShowCursor();
        }

        public void HideMenuView()
        {
            MenuView.SetActive(false);
            PlayerScript.HideCursor();
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