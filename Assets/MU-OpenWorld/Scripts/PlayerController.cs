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
        public Transform MiniMapPlayer;

        // UI関連
        public GameManager GameManagerScript;

        // 入力関連
        private InputAction moveAction;
        private InputAction lookAction;
        public InputActionTrace lookActionTrace = null;
        private InputAction menuAction;
        private InputAction dashAction;
        private InputAction jumpAction;

        private bool isMouseCenter = true;
        private bool isDash = false;
        private bool isGround = false;

        private int upJumpCount = 0;
        private int maxUpJumpCount = 5;

        private float farstYUpVelocity = 3f;
        private float yUpVelocity = 0.7f;

        private bool hasUpJump = false;
        private bool hasDownJump = false;

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

            // 地面に足がついているか
            RaycastHit hit;
            var rayStartPosition = this.transform.position + Vector3.up * 0.2f;
            var rayTarget = this.transform.TransformDirection(Vector3.down);
            if (Physics.Raycast(rayStartPosition, rayTarget, out hit, 0.4f))
            {
                Debug.DrawRay(rayStartPosition, rayTarget * hit.distance, Color.yellow);
                isGround = true;
            }
            else
            {
                isGround = false;
            }
            Debug.DrawRay(rayStartPosition, rayTarget * 0.4f, Color.white);

            // ジャンプ
            var jump = jumpAction.ReadValue<float>();

            var rbVelocity = PlayerRigidbody.velocity;
            if (isGround && jump==1f && upJumpCount==0)
            {
                PlayerRigidbody.velocity = rbVelocity + new Vector3(0f, farstYUpVelocity, 0f);
                hasUpJump = true;
                upJumpCount++;
            }
            else if (hasUpJump && jump==1f && upJumpCount<maxUpJumpCount)
            {
                PlayerRigidbody.velocity = rbVelocity + new Vector3(0f, yUpVelocity, 0f);
                upJumpCount++;
            }
            else if (hasUpJump && rbVelocity.y > 0f)
            {
                upJumpCount = maxUpJumpCount;
            }
            else if (hasUpJump && rbVelocity.y <= 0f)
            {
                hasUpJump = false;
                hasDownJump = true;
            }
            else if (hasDownJump && isGround)
            {
                hasUpJump = false;
                hasDownJump = false;
                upJumpCount = 0;
            }

            float hasJumpAnimation = hasUpJump || hasDownJump ? 1f : 0f;
            PlayerAvatarController.AvatarAnimator.SetFloat("hasJump", hasJumpAnimation, 0.1f, flameDeltaTime);

            float jumpAnimation = hasDownJump ? 1f : 0f;
            PlayerAvatarController.AvatarAnimator.SetFloat("jump", jumpAnimation, 0.3f, flameDeltaTime);
            

            // 移動
            var move = moveAction.ReadValue<Vector2>();
            if (move.x == 0f && move.y == 0f)
            {
                PlayerAvatarController.AvatarAnimator.SetFloat("speed", 0f, 0.1f, flameDeltaTime);
                isDash = false;
                return;
            }

            if (!isDash)
                isDash = dashAction.ReadValue<float>() == 1f;

            float moveSpeed;

            if (isDash)
            {
                PlayerAvatarController.AvatarAnimator.SetFloat("speed", 2f, 0.1f, flameDeltaTime);
                moveSpeed = dashSpeed;
            }
            else
            {
                PlayerAvatarController.AvatarAnimator.SetFloat("speed", 1f, 0.1f, flameDeltaTime);
                moveSpeed = walkSpeed;
            }

            var moveVector = new Vector3(move.x * (moveSpeed/frameParSecond), 0f, move.y * (moveSpeed/frameParSecond));
            moveVector = CameraTransform.rotation * moveVector;

            var rotation = PlayerAvatarController.AvatarTransform.rotation;
            PlayerAvatarController.AvatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            MiniMapPlayer.rotation = Quaternion.LookRotation(moveVector);
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

            dashAction = playerInput.currentActionMap["Dash"];
            jumpAction = playerInput.currentActionMap["Jump"];
            menuAction = playerInput.currentActionMap["Menu"];
            menuAction.performed += (callback) =>
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