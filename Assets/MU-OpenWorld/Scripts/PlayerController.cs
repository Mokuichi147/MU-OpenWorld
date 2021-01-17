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
        public bool isInputActive;
        public bool isPlayerActive;

        [Range(0f, 200f)]
        public float LookSensitivity = 100f;

        // GetComponentの回避
        public Rigidbody PlayerRigidbody;
        public Avatar.Model PlayerAvatarController;
        public Transform VCamera;
        public Transform MiniMapCamera;
        public Transform CameraRotate;
        public Transform PlayerRotate;

        // UI関連
        public App.GameManager GameManagerScript;

        // 入力関連
        private InputAction moveAction;
        private InputAction zoomAction;
        private InputAction lookAction;
        public InputActionTrace lookActionTrace = null;
        public InputActionTrace zoomActionTrace = null;
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
            PlayerRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            isInputActive = false;
            isPlayerActive = false;
        }


        void FixedUpdate()
        {
            if (!(isInputActive && isPlayerActive))
                return;

            // 視点操作
            if (isMouseCenter)
            {
                Quaternion camera_rot = CameraRotate.rotation;
                foreach (var look in lookActionTrace)
                {
                    var delta_pos = look.ReadValue<Vector2>();
                    float deltaX = delta_pos.x / Mathf.Pow((200f-LookSensitivity)/100f*0.26f+1.138f, 10f) * 2f;
                    float deltaY = -delta_pos.y / Mathf.Pow((200f-LookSensitivity)/100f*0.26f+1.138f, 10f) * 2f;
                    camera_rot *= Quaternion.Euler(deltaY, deltaX, 0f);
                }
                float eularX = camera_rot.eulerAngles.x;
                // 範囲を制限する
                eularX = eularX > 180f ? Mathf.Max(280f, eularX) : Mathf.Min(80f, eularX);

                CameraRotate.rotation = Quaternion.Euler(eularX, camera_rot.eulerAngles.y, 0f);
                MiniMapCamera.rotation = Quaternion.Euler(90f, camera_rot.eulerAngles.y, 0f);

                lookActionTrace.Clear();

                Vector3 vCameraPosition = VCamera.localPosition;
                foreach (var zoom in zoomActionTrace)
                {
                    var scroll = zoom.ReadValue<float>();
                    vCameraPosition += new Vector3(0f, 0f, scroll / 500f);
                }
                // 範囲を制限する
                vCameraPosition = new Vector3(0f, 0f, Mathf.Clamp(vCameraPosition.z, -10f, -1.2f));

                VCamera.localPosition = Vector3.Lerp(VCamera.localPosition, vCameraPosition, flameDeltaTime * animationTimeScale);
                zoomActionTrace.Clear();
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
            else if (hasDownJump && (isGround || rbVelocity.y == 0f))
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
            moveVector = Quaternion.Euler(0f, MiniMapCamera.eulerAngles.y, 0f) * moveVector;

            var rotation = PlayerAvatarController.AvatarTransform.rotation;
            PlayerAvatarController.AvatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            PlayerRotate.rotation = Quaternion.Lerp(PlayerRotate.rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale * 2f);
            var position = PlayerRigidbody.position;
            PlayerRigidbody.MovePosition(new Vector3(position.x + moveVector.x, position.y, position.z + moveVector.z));
        }

        private void OnApplicationQuit()
        {
            if (lookActionTrace != null)
            {
                lookActionTrace.Dispose();
                zoomActionTrace.Dispose();
                PlayerSave();
            }
        }

        public IEnumerator InitInput()
        {
            yield return null;
            /* 入力の初期設定 */
            var playerInput = this.GetComponent<PlayerInput>();
            moveAction = playerInput.currentActionMap["Move"];
            lookAction = playerInput.currentActionMap["Look"];
            lookActionTrace = new InputActionTrace();
            zoomAction = playerInput.currentActionMap["Zoom"];
            zoomActionTrace = new InputActionTrace();

            dashAction = playerInput.currentActionMap["Dash"];
            jumpAction = playerInput.currentActionMap["Jump"];
            menuAction = playerInput.currentActionMap["Menu"];
            menuAction.performed += (callback) =>
            {
                if (isMouseCenter && isInputActive && isPlayerActive)
                    GameManagerScript.ShowMenuView();
            };

            HideCursor();
            isInputActive = true;
        }

        public void InitPlayer()
        {
            PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            App.DataFile.Player player;
            if (App.DataFile.IsPlayerData())
            {
                player = App.DataFile.PlayerLoad();
                this.transform.position = player.Position;
                PlayerAvatarController.PlayerLoad();
            }
            else
            {
                player = App.DataFile.PlayerCreate();
                this.transform.position = player.Position;
                PlayerAvatarController.PlayerLoad();
                Avatar.Util.SetHeight(this.transform);
            }
            CameraRotate.rotation = player.CameraRotation;
            PlayerAvatarController.AvatarTransform.rotation = player.Rotation;

            isPlayerActive = true;
        }

        private void PlayerSave()
        {
            var player = new App.DataFile.Player();
            player.Position = this.transform.position;
            player.Rotation = PlayerAvatarController.AvatarTransform.rotation;
            player.CameraRotation = CameraRotate.rotation;
            App.DataFile.PlayerSave(player);
        }

        public void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            lookActionTrace.UnsubscribeFrom(lookAction);
            zoomActionTrace.UnsubscribeFrom(zoomAction);
            isMouseCenter = false;
        }

        public void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            lookActionTrace.SubscribeTo(lookAction);
            zoomActionTrace.SubscribeTo(zoomAction);
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