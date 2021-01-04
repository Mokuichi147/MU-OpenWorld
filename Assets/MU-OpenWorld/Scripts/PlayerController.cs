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
        public bool isActive;

        [Range(0f, 200f)]
        public float LookSensitivity = 100f;

        // GetComponentの回避
        public Rigidbody PlayerRigidbody;
        public AvatarController PlayerAvatarController;
        public Transform CameraTransform;

        // UI関連
        public GameManager GameManagerScript;

        // 入力関連
        private InputAction moveAction;
        private InputAction lookAction;
        public InputActionTrace lookActionTrace = null;
        private InputAction escAction;
        private InputAction shiftAction;

        private bool isMouseCenter = true;
        private bool isDash = false;

        // 移動速度 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
        private float walkSpeed = 1.5f;
        private float dashSpeed = 5f;

        // マジックナンバーの回避
        static private float frameParSecond = 50f;
        static private float flameDeltaTime = 1f / frameParSecond;
        static private float animationTimeScale = 10f;


        void Awake()
        {
            InitInput();
            PlayerRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            isActive = false;
        }


        void FixedUpdate()
        {
            if (!isActive)
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
                isDash = false;
                return;
            }
            if (!isDash)
                isDash = shiftAction.ReadValue<float>() == 1f;

            float moveAnimation;
            float moveSpeed;

            if (isDash)
            {
                moveAnimation = 2f;
                moveSpeed = dashSpeed;
            }
            else
            {
                moveAnimation = 1f;
                moveSpeed = walkSpeed;
            }

            PlayerAvatarController.AvatarAnimator.SetFloat("speed", moveAnimation, 0.1f, flameDeltaTime);
            var moveVector = new Vector3(move.x * (moveSpeed/frameParSecond), 0f, move.y * (moveSpeed/frameParSecond));
            moveVector = CameraTransform.rotation * moveVector;

            var rotation = PlayerAvatarController.AvatarTransform.rotation;
            PlayerAvatarController.AvatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            var position = PlayerRigidbody.position;
            PlayerRigidbody.MovePosition(new Vector3(position.x + moveVector.x, position.y, position.z + moveVector.z));
        }

        private void OnApplicationQuit()
        {
            if (lookActionTrace != null)
            {
                lookActionTrace.Dispose();
                PlayerSave();
            }
        }

        public void InitInput()
        {
            /* 入力の初期設定 */
            var playerInput = this.GetComponent<PlayerInput>();
            moveAction = playerInput.currentActionMap["Move"];
            lookAction = playerInput.currentActionMap["Look"];
            lookActionTrace = new InputActionTrace();

            shiftAction = playerInput.currentActionMap["Shift"];
            escAction = playerInput.currentActionMap["Esc"];
            escAction.performed += (callback) =>
            {
                if (isMouseCenter && isActive)
                    GameManagerScript.ShowMenuView();
            };
        }

        public void InitPlayer()
        {
            PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Data.Player player;
            if (Data.IsPlayerData())
            {
                player = Data.PlayerLoad();
                this.transform.position = player.Position;
                PlayerAvatarController.PlayerLoad(player.AvatarPath);
            }
            else
            {
                player = Data.PlayerCreate();
                this.transform.position = player.Position;
                PlayerAvatarController.PlayerLoad(player.AvatarPath);
                AvatarController.SetHeight(this.transform);
            }
            CameraTransform.rotation = player.Rotation;
            isActive = true;
        }

        private void PlayerSave()
        {
            var player = new Data.Player();
            player.Position = this.transform.position;
            player.Rotation = CameraTransform.rotation;
            player.AvatarPath = PlayerAvatarController.AvatarFilePath;
            Data.PlayerSave(player);
        }

        public void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            lookActionTrace.UnsubscribeFrom(lookAction);
            isMouseCenter = false;
        }

        public void HideCursor()
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