using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace OpenWorld
{
    public class PlayerController : MonoBehaviour
    {
        [Range(0f, 200f)]
        public float LookSensitivity = 100f;

        private Rigidbody playerRigidbody;
        public Transform cameraTransform;
        private AvatarController Avatar;

        // 入力関連
        private InputAction moveAction;
        private InputAction lookAction;
        private InputActionTrace lookActionTrace;
        private InputAction escAction;

        private bool isMouseCenter = true;
        private Vector2 centerPosition;

        // マジックナンバーの回避
        static private float flameDeltaTime = 0.02f;
        static private float animationTimeScale = 10f;


        void Awake()
        {
            Avatar = this.GetComponent<AvatarController>();
            InputAwake();
        }

        void Start()
        {
            cameraTransform = GameObject.Find("CameraRotate").GetComponent<Transform>();
            playerRigidbody = this.GetComponent<Rigidbody>();

            centerPosition = new Vector2(Mathf.Round(Screen.width/2f), Mathf.Round(Screen.height/2f));
            Mouse.current.WarpCursorPosition(centerPosition);
            Cursor.visible = false;
        }

        void FixedUpdate()
        {
            // 視点操作
            if (isMouseCenter)
            {
                Quaternion camera_rot = cameraTransform.rotation;
                foreach (var look in lookActionTrace)
                {
                    var delta_pos = look.ReadValue<Vector2>();
                    camera_rot *= Quaternion.Euler(0f, delta_pos.x/Mathf.Pow((200f-LookSensitivity)/100f*0.26f+1.138f, 10f)*2f, 0f);
                }
                cameraTransform.rotation = camera_rot;
                lookActionTrace.Clear();
            }

            var move = moveAction.ReadValue<Vector2>();
            if (move.x == 0f && move.y == 0f)
            {
                Avatar.AvatarAnimator.SetFloat("speed", 0f, 0.1f, flameDeltaTime);
                return;
            }
            var position = playerRigidbody.position;
            Avatar.AvatarAnimator.SetFloat("speed", Mathf.Sqrt(Mathf.Pow(move.x,2f)+Mathf.Pow(move.y,2f)), 0.1f, flameDeltaTime);
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var _dx = move.x * (5f / 50f);
            var _dy = move.y * (5f / 50f);
            var moveVector = new Vector3(_dx, 0f, _dy);
            moveVector = cameraTransform.rotation * moveVector;
            var rotation = Avatar.AvatarTransform.rotation;
            Avatar.AvatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            playerRigidbody.MovePosition(new Vector3(position.x + moveVector.x, position.y, position.z + moveVector.z));
        }

        private void OnApplicationQuit()
        {
            lookActionTrace.Dispose();
        }

        private void InputAwake()
        {
            /* 入力の初期設定 */
            var playerInput = this.GetComponent<PlayerInput>();
            moveAction = playerInput.currentActionMap["Move"];
            lookAction = playerInput.currentActionMap["Look"];
            lookActionTrace = new InputActionTrace();
            lookActionTrace.SubscribeTo(lookAction);

            escAction = playerInput.currentActionMap["Esc"];
            escAction.performed += (callback) =>
            {
                if (isMouseCenter)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    lookActionTrace.UnsubscribeFrom(lookAction);
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    lookActionTrace.SubscribeTo(lookAction);
                }
                isMouseCenter = !isMouseCenter;
            };
        }
    }
}