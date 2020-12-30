using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace OpenWorld
{
    public class PlayerController : MonoBehaviour
    {
        public bool IsActive = false;

        [Range(0f, 200f)]
        public float LookSensitivity = 100f;

        // GetComponentの回避
        public Rigidbody PlayerRigidbody;
        public AvatarController PlayerAvatarController;
        public Transform CameraTransform;

        // UI関連
        public GameObject MenuView;

        // 入力関連
        private InputAction moveAction;
        static private InputAction lookAction;
        static private InputActionTrace lookActionTrace;
        private InputAction escAction;

        static private bool isMouseCenter = true;

        // マジックナンバーの回避
        static private float frameParSecond = 50f;
        static private float flameDeltaTime = 1f / frameParSecond;
        static private float animationTimeScale = 10f;


        public void Init()
        {
            InitInput();
            InitPlayer();
            HideCursor();
            IsActive = true;
        }

        void FixedUpdate()
        {
            if (!IsActive)
                return;

            // 視点操作
            if (isMouseCenter)
            {
                Quaternion camera_rot = CameraTransform.rotation;
                foreach (var look in lookActionTrace)
                {
                    var delta_pos = look.ReadValue<Vector2>();
                    camera_rot *= Quaternion.Euler(0f, delta_pos.x/Mathf.Pow((200f-LookSensitivity)/100f*0.26f+1.138f, 10f)*2f, 0f);
                }
                CameraTransform.rotation = camera_rot;
                lookActionTrace.Clear();
            }

            var move = moveAction.ReadValue<Vector2>();
            if (move.x == 0f && move.y == 0f)
            {
                PlayerAvatarController.AvatarAnimator.SetFloat("speed", 0f, 0.1f, flameDeltaTime);
                return;
            }

            PlayerAvatarController.AvatarAnimator.SetFloat("speed", Mathf.Sqrt(Mathf.Pow(move.x,2f)+Mathf.Pow(move.y,2f)), 0.1f, flameDeltaTime);
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var moveSpeed = 5f;
            var moveVector = new Vector3(move.x * (moveSpeed/frameParSecond), 0f, move.y * (moveSpeed/frameParSecond));
            moveVector = CameraTransform.rotation * moveVector;

            var rotation = PlayerAvatarController.AvatarTransform.rotation;
            PlayerAvatarController.AvatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            var position = PlayerRigidbody.position;
            PlayerRigidbody.MovePosition(new Vector3(position.x + moveVector.x, position.y, position.z + moveVector.z));
        }

        private void OnApplicationQuit()
        {
            if (IsActive)
            {
                lookActionTrace.Dispose();
                PlayerSave();
            }
        }

        private void InitInput()
        {
            /* 入力の初期設定 */
            var playerInput = this.GetComponent<PlayerInput>();
            moveAction = playerInput.currentActionMap["Move"];
            lookAction = playerInput.currentActionMap["Look"];
            lookActionTrace = new InputActionTrace();

            escAction = playerInput.currentActionMap["Esc"];
            escAction.performed += (callback) =>
            {
                if (isMouseCenter)
                    GameManager.ShowMenuView();
            };
        }

        private void InitPlayer()
        {
            Data.Player player;
            if (Data.IsPlayerData())
            {
                player = Data.PlayerLoad();
                PlayerRigidbody.position = player.Position;
                PlayerAvatarController.PlayerLoad(player.AvatarPath);
            }
            else
            {
                player = Data.PlayerCreate();
                PlayerRigidbody.position = player.Position;
                PlayerAvatarController.PlayerLoad(player.AvatarPath);
                AvatarController.SetHeight(this.transform);
            }
            CameraTransform.rotation = player.Rotation;
        }

        private void PlayerSave()
        {
            var player = new Data.Player();
            player.Position = PlayerRigidbody.position;
            player.Rotation = CameraTransform.rotation;
            player.AvatarPath = PlayerAvatarController.AvatarFilePath;
            Data.PlayerSave(player);
        }

        static public void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            lookActionTrace.UnsubscribeFrom(lookAction);
            isMouseCenter = false;
        }

        static public void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            lookActionTrace.SubscribeTo(lookAction);
            isMouseCenter = true;
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