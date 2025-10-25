// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// RESUMEN DE LA CLASE: PlayerController
// Esta es la clase principal que controla al personaje. Se encarga de leer los datos de los scripts de input
// y traducirlos en movimiento. Gestiona la física del personaje (gravedad, salto, velocidad, aceleración)
// usando un CharacterController. También maneja la rotación del personaje y de la cámara, asegurando
// que el movimiento se sienta fluido y responda correctamente a las acciones del jugador en diferentes
// estados (caminar, correr, saltar, etc.) y modos de cámara (primera y tercera persona).

// Define el orden de ejecución de este script para que se ejecute antes que otros scripts por defecto.
[DefaultExecutionOrder(-1)]
// Asegura que el GameObject tenga los componentes necesarios para que este script funcione.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerLocomotionInput))]
[RequireComponent(typeof(PlayerState))]
public class PlayerController : MonoBehaviour
{
    #region Components & Settings
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    private Camera _playerCamera; // La buscaremos por nombre

    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Base Movement")]
    public float walkAcceleration = 25f;
    public float walkSpeed = 2f;
    public float runAcceleration = 50f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 30f;
    public float sprintSpeed = 7f;

    public float drag = 20f;
    public float gravity = 25f;

    public float jumpSpeed = 0.8f;
    public float inAirAcceleration = 25f;
    public float inAirDrag = 5f;
    public float terminalVelocity = 50f;
    public float movingThreshold = 0.01f;

    [Header("Animation")]
    public float playerModelRotationSpeed = 10f;
    public float rotateToTargetTime = 0.67f;
    public float rotationTolerance = 90f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Environment Details")]
    [SerializeField] private LayerMask _groundLayers;
    #endregion

    #region Private State & References
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private bool _jumpedLastFrame = false;
    private bool _isRotatingClockwise = false;
    private float _rotatingToTargetTimer = 0f;
    private float _verticalVelocity = 0f;
    private float _antiBump;
    private float _stepOffset;

    private Vector3 _cameraForwardXZ;
    private Vector3 _cameraRightXZ;

    private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
    #endregion

    #region Startup
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _antiBump = sprintSpeed;
        _stepOffset = _characterController.stepOffset;

        // Busca la cámara llamada "dummy cam" para evitar asignaciones manuales.
        GameObject dummyCamObject = GameObject.Find("dummy cam");
        if (dummyCamObject != null)
        {
            _playerCamera = dummyCamObject.GetComponent<Camera>();
        }
        else
            Debug.LogError("¡No se encontró la cámara 'dummy cam'! La rotación de la cámara no funcionará.");
    }
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void LateUpdate()
    {
        UpdateCameraRotation();
    }
    #endregion

    #region Movement Logic
    private void UpdateMovementState()
    {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

        bool hasEnoughInput = _playerLocomotionInput.MovementInput.magnitude > movingThreshold;
        bool shiftPressed = _playerLocomotionInput.SprintPressed || _playerLocomotionInput.SprintToggledOn;

        // Prioridad: Walk (toggle) > Run (shift) > Sprint (movement) > Idle
        PlayerMovementState lateralState;
        if (_playerLocomotionInput.WalkToggledOn && hasEnoughInput)
            lateralState = PlayerMovementState.Walking;
        else if (shiftPressed && hasEnoughInput)
            lateralState = PlayerMovementState.Running;
        else if (hasEnoughInput)
            lateralState = PlayerMovementState.Sprinting;
        else
            lateralState = PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);

        bool isGrounded = IsGrounded();
        if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        }
        else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        }
        else if (_characterController.isGrounded)
        {
            _characterController.stepOffset = _stepOffset;
        }
    }

    private void HandleVerticalMovement()
    {
        bool isGrounded = _playerState.InGroundedState();
        _verticalVelocity -= gravity * Time.deltaTime;

        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -_antiBump;
        }

        if (_playerLocomotionInput.JumpPressed && isGrounded)
        {
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * gravity * 3);
            _jumpedLastFrame = true;
        }

        if (_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
        {
            _verticalVelocity += _antiBump;
        }

        if (Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
        {
            _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
        }
    }

    private void HandleLateralMovement()
    {
        bool isGrounded = _playerState.InGroundedState();
        float lateralAcceleration;
        float clampLateralMagnitude;

        switch (_playerState.CurrentPlayerMovementState)
        {
            case PlayerMovementState.Walking:
                lateralAcceleration = walkAcceleration;
                clampLateralMagnitude = walkSpeed;
                break;
            case PlayerMovementState.Sprinting:
                lateralAcceleration = sprintAcceleration;
                clampLateralMagnitude = sprintSpeed;
                break;
            case PlayerMovementState.Running:
                lateralAcceleration = runAcceleration;
                clampLateralMagnitude = runSpeed;
                break;
            default: // Idling, Jumping, Falling
                lateralAcceleration = isGrounded ? walkAcceleration : inAirAcceleration;
                clampLateralMagnitude = sprintSpeed; // Allow high speed in air
                break;
        }

        Vector3 movementDirection = _cameraRightXZ * _playerLocomotionInput.MovementInput.x + _cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        float dragMagnitude = isGrounded ? drag : inAirDrag;
        Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;

        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;

        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

        _characterController.Move(newVelocity * Time.deltaTime);
    }

    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        if (!validAngle && _verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);

        return velocity;
    }
    #endregion

    #region Rotation Logic
    private void UpdateCameraRotation()
    {
        if (_playerCamera == null) return;

        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        // La lógica anterior acumulaba la rotación, causando un giro exponencial.
        // Ahora, la rotación objetivo del jugador simplemente sigue la rotación de la cámara.
        _playerTargetRotation.x = _cameraRotation.x;

        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        IsRotatingToTarget = _rotatingToTargetTimer > 0;

        // Si la cámara de primera persona está activa, el personaje siempre rota con la cámara.
        if (CameraSwitcher.Instance != null && CameraSwitcher.Instance.IsFirstPersonCameraActive)
        {
            RotatePlayerToTarget();
        }
        // Lógica de rotación original para tercera persona.
        else if (!isIdling)
        {
            RotatePlayerToTarget();
        }
        else if (isIdling && (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget))
        {
            UpdateIdleRotation(rotationTolerance);
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        // Cache camera vectors for the next Update cycle
        _cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        _cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

        Vector3 crossProduct = Vector3.Cross(transform.forward, _cameraForwardXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, _cameraForwardXZ);
    }
    // Actualiza la rotación del jugador cuando está inactivo
    // es decir cuando no se está moviendo con la cámara.
    // en base a la discrepancia de rotación con la cámara.
    private void UpdateIdleRotation(float rotationTolerance)
    {
        if (Mathf.Abs(RotationMismatch) > rotationTolerance)
        {
            _rotatingToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }

        _rotatingToTargetTimer -= Time.deltaTime;

        if (_isRotatingClockwise && RotationMismatch > 0f ||
            !_isRotatingClockwise && RotationMismatch < 0f)
        {
            RotatePlayerToTarget();
        }
    }

    private void RotatePlayerToTarget()
    {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }


    private bool IsGrounded()
    {
        if (!_characterController.isGrounded)
            return false;

        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        return angle <= _characterController.slopeLimit;
    }

}
#endregion
