using UnityEngine;
using UnityEngine.InputSystem;

namespace PvPShotMode.MapSDK
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class SimpleFPSController : MonoBehaviour
    {
        public Camera viewCamera;
        [Min(.1f)] public float moveSpeed = 5f;
        [Min(1f)] public float sprintMultiplier = 1.6f;
        [Min(.1f)] public float jumpVelocity = 5f;
        [Min(.01f)] public float lookSensitivity = .08f;
        [Range(1f, 89f)] public float verticalLookLimit = 85f;

        private Rigidbody body;
        private Vector2 moveInput;
        private bool sprintHeld;
        private bool jumpQueued;
        private float pitch;
        private float lastGroundedAt = -10f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>(true);
            pitch = viewCamera == null ? 0f : NormalizeAngle(viewCamera.transform.localEulerAngles.x);
        }

        private void OnEnable()
        {
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);

            if (Cursor.lockState == CursorLockMode.Locked && mouse != null)
            {
                Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
                transform.Rotate(0f, look.x, 0f, Space.World);
                pitch = Mathf.Clamp(pitch - look.y, -verticalLookLimit, verticalLookLimit);
                if (viewCamera != null) viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            if (keyboard == null)
            {
                moveInput = Vector2.zero;
                sprintHeld = false;
                return;
            }

            moveInput = new Vector2(
                ReadAxis(keyboard.aKey.isPressed, keyboard.dKey.isPressed),
                ReadAxis(keyboard.sKey.isPressed, keyboard.wKey.isPressed));
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (keyboard.spaceKey.wasPressedThisFrame) jumpQueued = true;
        }

        private void FixedUpdate()
        {
            if (body == null) return;
            float speed = moveSpeed * (sprintHeld ? sprintMultiplier : 1f);
            Vector3 desired = (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;
            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(desired.x, velocity.y, desired.z);
            if (jumpQueued && Time.time - lastGroundedAt <= .15f)
            {
                body.linearVelocity = new Vector3(desired.x, jumpVelocity, desired.z);
                lastGroundedAt = -10f;
            }
            jumpQueued = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (Vector3.Dot(collision.GetContact(i).normal, Vector3.up) >= .6f)
                {
                    lastGroundedAt = Time.time;
                    return;
                }
        }

        private static float ReadAxis(bool negative, bool positive) =>
            (positive ? 1f : 0f) - (negative ? 1f : 0f);

        private static float NormalizeAngle(float angle) => angle > 180f ? angle - 360f : angle;

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
