using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("移动参数 (Movement)")]
    [Tooltip("走路速度 (米/秒)")]
    public float walkSpeed = 6f;

    [Tooltip("按住Shift奔跑速度 (米/秒)")]
    public float sprintSpeed = 9.5f;

    [Tooltip("跳跃高度 (米)")]
    public float jumpHeight = 1.3f;

    [Tooltip("重力加速度")]
    public float gravity = 25f;

    [Header("视角参数 (Perspective & Camera)")]
    [Tooltip("第一人称摄像机")]
    public Camera playerCamera;

    [Tooltip("鼠标灵敏度")]
    public float mouseSensitivity = 0.15f;

    [Tooltip("上下抬头低头最大限制角度")]
    public float upDownLookLimit = 85f;

    [Header("第三人称视角配置 (按 V 键切换)")]
    [Tooltip("当前是否为第三人称视角")]
    public bool isThirdPerson = false;

    [Tooltip("第三人称相机拉远距离 (米)")]
    public float thirdPersonDistance = 3.5f;

    [Tooltip("第三人称相机肩部偏移 (X:右偏, Y:上偏)")]
    public Vector2 thirdPersonShoulderOffset = new Vector2(0.45f, 0.25f);

    [Tooltip("视角平滑过渡速度")]
    public float perspectiveSwitchSpeed = 12f;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _verticalRotation = 0f;
    private bool _cursorLocked = true;
    private Vector3 _currentCamLocalPos;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        // 自动清理冲突碰撞体，保留 CharacterController
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            if (!(col is CharacterController))
            {
                DestroyImmediate(col);
            }
        }

        // 胶囊体尺寸配置 (高2米，半径0.5米)
        _characterController.height = 2.0f;
        _characterController.radius = 0.5f;
        _characterController.center = Vector3.zero;
        _characterController.stepOffset = 0.4f;
        _characterController.slopeLimit = 45f;
        _characterController.skinWidth = 0.08f;

        // 自动寻找或配置摄像机
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    playerCamera = mainCam;
                    playerCamera.transform.SetParent(transform);
                }
                else
                {
                    GameObject camObj = new GameObject("PlayerCamera");
                    camObj.transform.SetParent(transform);
                    playerCamera = camObj.AddComponent<Camera>();
                }
            }
        }

        // 防止第一人称近裁剪面切掉胶囊体边缘
        playerCamera.nearClipPlane = 0.03f;
        _currentCamLocalPos = new Vector3(0f, 0.75f, 0.15f);

        SetCursorLock(true);
    }

    private void Update()
    {
        HandleCursorLockToggle();
        HandlePerspectiveToggle();
        HandleMouseLookAndCamera();
        HandleMovement();
    }

    private void HandleCursorLockToggle()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLock(!_cursorLocked);
        }
        if (!_cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLock(true);
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(!_cursorLocked);
        }
        if (!_cursorLocked && Input.GetMouseButtonDown(0))
        {
            SetCursorLock(true);
        }
#endif
    }

    private void SetCursorLock(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandlePerspectiveToggle()
    {
        // 按 V 键切换第一人称 / 第三人称
        bool vPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            vPressed = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.V))
        {
            vPressed = true;
        }
#endif
        if (vPressed)
        {
            isThirdPerson = !isThirdPerson;
        }
    }

    private void HandleMouseLookAndCamera()
    {
        if (!_cursorLocked) return;

        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseX = mouseDelta.x * mouseSensitivity;
            mouseY = mouseDelta.y * mouseSensitivity;
        }
#else
        mouseX = Input.GetAxis("Mouse X") * (mouseSensitivity * 10f);
        mouseY = Input.GetAxis("Mouse Y") * (mouseSensitivity * 10f);
#endif

        // 水平旋转玩家本体
        transform.Rotate(Vector3.up * mouseX);

        // 垂直俯仰角限制
        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -upDownLookLimit, upDownLookLimit);

        // 计算目标摄像机位置
        Vector3 targetLocalPos;
        Quaternion pitchRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

        if (!isThirdPerson)
        {
            // 第一人称模式：眼睛高度稍微靠前
            targetLocalPos = new Vector3(0f, 0.75f, 0.15f);
        }
        else
        {
            // 第三人称模式：相机后拉并带智能防穿墙检测
            float currentDist = thirdPersonDistance;
            Vector3 headPivotWorld = transform.position + Vector3.up * 1.0f;
            Vector3 backDirWorld = transform.rotation * pitchRotation * Vector3.back;

            // 发射球体射线检测背后的墙壁，防止相机穿出建筑外部
            int layerMask = ~LayerMask.GetMask("Ignore Raycast");
            if (Physics.SphereCast(headPivotWorld, 0.2f, backDirWorld, out RaycastHit hit, thirdPersonDistance, layerMask))
            {
                if (hit.collider != _characterController)
                {
                    currentDist = Mathf.Clamp(hit.distance - 0.25f, 0.4f, thirdPersonDistance);
                }
            }

            Vector3 shoulderOffset = new Vector3(thirdPersonShoulderOffset.x, thirdPersonShoulderOffset.y, 0f);
            targetLocalPos = new Vector3(0f, 0.8f, 0f) + pitchRotation * (shoulderOffset + Vector3.back * currentDist);
        }

        // 平滑插值切换相机视角
        _currentCamLocalPos = Vector3.Lerp(_currentCamLocalPos, targetLocalPos, Time.deltaTime * perspectiveSwitchSpeed);
        playerCamera.transform.localPosition = _currentCamLocalPos;
        playerCamera.transform.localRotation = pitchRotation;
    }

    private void HandleMovement()
    {
        bool isGrounded = _characterController.isGrounded;

        if (isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }

        float inputX = 0f;
        float inputZ = 0f;
        bool isSprinting = false;
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputZ += 1f;
            if (Keyboard.current.sKey.isPressed) inputZ -= 1f;
            if (Keyboard.current.aKey.isPressed) inputX -= 1f;
            if (Keyboard.current.dKey.isPressed) inputX += 1f;

            isSprinting = Keyboard.current.leftShiftKey.isPressed;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#else
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
#endif

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 horizontalMove = (transform.right * inputX + transform.forward * inputZ).normalized * currentSpeed;

        if (jumpPressed && isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }

        _velocity.y -= gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove + _velocity;
        _characterController.Move(finalMove * Time.deltaTime);
    }
}
