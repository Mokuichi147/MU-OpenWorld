using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEditor.Animations;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#elif UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
using VRM;

namespace OpenWorld
{
    public class AvatarController : MonoBehaviour
    {
        // アニメーション関連
        public Avatar PlayerAvatar;
        public AnimatorController PlayerAnimator;

        [Range(0f, 200f)]
        public float LookSensitivity = 100f;
        public string avatarFilePath = "Assets/MU-OpenWorld/Models/Avatars/Moyu.vrm";

        private Rigidbody playerRigidbody;
        private GameObject avatarObject;
        private Animator avatarAnimator;
        private Transform avatarTransform;
        private Transform cameraTransform;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputActionTrace lookActionTrace;
        private InputAction escAction;
        private bool isMouseCenter = true;
        private Vector2 centerPosition;
        static float flameDeltaTime = 0.02f;
        private float animationTimeScale = 10f;

        void Start()
        {
            LoadAvatar();

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
                avatarAnimator.SetFloat("speed", 0f, 0.1f, flameDeltaTime);
                return;
            }
            var position = playerRigidbody.position;
            avatarAnimator.SetFloat("speed", Mathf.Sqrt(Mathf.Pow(move.x,2f)+Mathf.Pow(move.y,2f)), 0.1f, flameDeltaTime);
            // 歩き:1.25, 自転車(ゆっくり):3.0, 自転車(普通):5.0, 長距離世界記録:5.67
            var _dx = move.x * (5f / 50f);
            var _dy = move.y * (5f / 50f);
            var moveVector = new Vector3(_dx, 0f, _dy);
            moveVector = cameraTransform.rotation * moveVector;
            var rotation = avatarTransform.rotation;
            avatarTransform.rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(moveVector), flameDeltaTime * animationTimeScale);
            playerRigidbody.MovePosition(new Vector3(position.x + moveVector.x, position.y, position.z + moveVector.z));
        }

        private void OnApplicationQuit()
        {
            lookActionTrace.Dispose();
        }

        private GameObject LoadFromPath(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var context = new VRMImporterContext();
            context.ParseGlb(bytes);
            var meta = context.ReadMeta(false);
            context.Load();
            context.ShowMeshes();
            return context.Root;
        }

        private bool LoadVRM(string filePath)
        {
            avatarObject = LoadFromPath(filePath);
            if (avatarObject == null) return false;

            avatarObject.transform.parent = this.transform;
            avatarObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            avatarTransform = avatarObject.GetComponent<Transform>();
            // 地面の高さに合わせる
            var position = this.transform.position;
            position.y = Ground.GetHeight(position.x, position.z) + 1f;
            this.transform.position = position;
            // アニメーションの設定
            avatarAnimator = avatarObject.GetComponent<Animator>();
            avatarAnimator.avatar = PlayerAvatar;
            avatarAnimator.runtimeAnimatorController = PlayerAnimator;
            return true;
        }

        private void LoadAvatar()
        {
            if (LoadVRM(avatarFilePath)) return;

    #if UNITY_EDITOR
            var filePath = EditorUtility.OpenFilePanel("vrmファイル(.vrm)", "", "vrm");
            avatarFilePath = filePath.ToString().Replace('\\', '/');
    #elif UNITY_STANDALONE_WIN
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "vrmファイル(.vrm)|*.vrm";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;

            var filePath = Path.GetFullPath(openFileDialog.FileName);
            avatarFilePath = filePath.ToString().Replace('\\', '/');
    #endif
            if (LoadVRM(avatarFilePath)) return;
        }
    }
}