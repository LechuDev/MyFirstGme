using System;
using NUnit.Framework;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    #region Class Variables
    [Header("Componentes")]
    // _characterController: Componente CharacterController que gestiona colisiones y movimiento físico del personaje.
    [SerializeField] private CharacterController _characterController;
    // _playerCamera: Cámara del jugador usada para calcular direcciones y rotación.
    [SerializeField] private Camera _playerCamera;
    // RotationMismatch: diferencia angular entre la orientación del modelo y la dirección de la cámara (grados, con signo).
    public float RotationMismatch { get; private set; } = 0f;
    // IsRotatingToTarget: indica si el personaje está rotando para alinearse con la cámara/objetivo.
    public bool IsRotatingToTarget { get; private set; } = false;


    [Header("Base Movement")]
    // Parámetros de movimiento (ajustables desde el inspector)
    // Aceleración y velocidad de caminar
    public float walkAcceleration = 25f;
    public float walkSpeed = 2f;
    // Aceleración y velocidad de correr
    public float runAcceleration = 35f;
    public float runSpeed = 4f;
    // Aceleración y velocidad de sprint
    public float sprintAcceleration = 50f;
    public float sprintSpeed = 7f;
    // Aceleración en el aire (cuando no está grounded)
    public float inAirAcceleration = 0.15f;
    // Arrastre en suelo y aire
    public float drag = 20f;
    public float airDrag = 5f;
    // Parámetros de física vertical
    public float gravity = 25f;
    public float terminalVelocity = 50f; // velocidad terminal máxima en caída
    public float jumpSpeed = 1.0f; // magnitud de impulso de salto
    // Umbral para considerar que el personaje se está moviendo lateralmente
    public float movingThreshold = 0.01f;

    [Header("Animation")]
    // Velocidad de interpolación de la rotación del modelo del jugador
    public float playerModelRotationSpeed = 10f;
    // Tiempo que se mantiene la rotación automática hacia la cámara al iniciar el giro en idle
    public float rotateToTargetTime = 0.67f;

    [Header("Camera Settings")]
    // Sensibilidades de la cámara horizontal y vertical
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    // Límite vertical de giro de cámara (evita hacer voltear la cámara 180º)
    public float lookLimitV = 89f;

    [Header("Environment Details")]
    // Capas que se consideran 'suelo' para comprobaciones de grounded
    [SerializeField] private LayerMask _groundLayers;

    // Referencias a otros componentes del juego
    private PlayerLocomotionInput _playerLocomotionInput; // obtiene input de movimiento y cámara
    private PlayerState _playerState; // almacena el estado de movimiento (idle/walk/run...)

    // Rotación de la cámara y objetivo de rotación del personaje (en ejes X/Y, valores acumulados)
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    // Flags y temporizadores de estado
    private bool _jumpedLastFrame = false; // true si se saltó en el frame anterior
    private bool _isRotatingClockwise = false; // dirección de la rotación automática
    private float _rotatingToTargetTimer = 0f; // tiempo restante para rotación automática
    // Velocidad vertical aplicada manualmente para gravedad/salto
    private float _verticalVelocity = 0f;
    private float _antiBump; // pequeño empuje para evitar que el character 'rebote' al salir del suelo
    private float _stepOffset; // offset original del CharacterController para restaurarlo

    // Último estado de movimiento registrado (para transiciones entre aire/suelo)
    private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;

    #endregion

    #region Startup
    private void Awake()
    {
        // Inicializa referencias a componentes vecinos en el mismo GameObject
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        // antiBump se usa para dar un pequeño empuje y evitar pequeños bounces cuando se cambia grounded->air
        _antiBump = sprintSpeed;
        // guardamos el valor original de stepOffset para restaurarlo después de saltos
        _stepOffset = _characterController.stepOffset;
    }
    #endregion

    #region Update Logic
    private void Update()
    {
        // Flujo principal por frame: actualizar estado, física vertical y movimiento lateral
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

    // Determina las condiciones de locomoción según input y estado físico
    bool canRun = CanRun();
    bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;    // hay input analógico o WASD
    bool isMovingLaterally = IsMovingLaterally();                                   // velocidad lateral actual
    bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;  // sprint activo y movimiento lateral

    bool isWalking = isMovingLaterally && (!canRun || _playerLocomotionInput.WalkToggledOn); // si camina en vez de correr
    bool isGrounded = IsGrounded(); // comprobación física de suelo

        PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                            isSprinting ? PlayerMovementState.Sprinting :
                                            isMovingLaterally || isMovementInput ? PlayerMovementState.Running :
                                                                PlayerMovementState.Idling;

    // Aplica el estado lateral (idling/walking/running/sprinting)
    _playerState.SetPlayerMovementState(lateralState);

        //Control Airborne State
        // Control de estados aéreos: salto y caída según velocidad vertical
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
        else
        {
            _characterController.stepOffset = _stepOffset;
        }
    }

    private void HandleVerticalMovement()
    {
        // Manejo de la velocidad vertical: gravedad, salto y límite de velocidad
        bool isGrounded = _playerState.InGroundedState();

        // Aplica gravedad integrando por frame
        _verticalVelocity -= gravity * Time.deltaTime;

        // Si estamos en el suelo y vamos hacia abajo, fijamos una pequeña velocidad negativa para evitar 'bounces'
        if (isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -_antiBump;

        // Salto: cuando se pulsa salto y estamos en el suelo, aplicamos impulso vertical
        if (_playerLocomotionInput.JumpPressed && isGrounded)
        {
            // raíz cuadrada para calcular impulso apropiado según gravedad y jumpSpeed
            _verticalVelocity +=  Mathf.Sqrt(jumpSpeed * 3 * gravity);
            _jumpedLastFrame = true;
        }

        // Si hemos pasado de estar en grounded a no estarlo, compensamos con antiBump para suavizar transición
        if (_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
        {
            _verticalVelocity += _antiBump;
        }
        // Clamp de velocidad vertical a terminalVelocity
        if (Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
        {
            _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
        }
    }


    private void HandleLateralMovement()
    {
        //Create quick references for current state
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.InGroundedState();
        bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

        //State dependent acceleration and speed
        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isWalking ? walkAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                    isWalking ? walkSpeed :
                                    isSprinting ? sprintSpeed : runSpeed;

        // Calcula direcciones a partir de la cámara para movimiento relativo a cámara
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        // Aceleración aplicada según estado y tiempo
        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Aplicar arrastre (drag) para frenar la velocidad lateral
        float dragMagnitude = isGrounded ? drag : airDrag;
        Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        // Limitar la magnitud lateral y preservar componente Y por gravedad
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;
        // Si estamos en el aire, corregir velocidad contra pendientes empinadas
        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

        // Mover el CharacterController (se recomienda una sola llamada por frame)
        _characterController.Move(newVelocity * Time.deltaTime);
    }
    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        // Si la pendiente es demasiado pronunciada y estamos cayendo, proyectamos la velocidad sobre el plano de la superficie
        if (!validAngle && _verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);

        return velocity;

    }

    #endregion

    #region Late Update Logic
    private void LateUpdate()
    {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        // Actualiza la rotación de cámara acumulando input de look (sensibilidad aplicada)
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        // Actualiza el objetivo de rotación del personaje basado en la orientación actual y el input horizontal
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;

        float rotationTolerance = 90f;
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        // Indicador si existe una rotación automática en curso
        IsRotatingToTarget = _rotatingToTargetTimer > 0;

        // Si no está en idle, se rota inmediatamente hacia el objetivo (por locomoción)
        if (!isIdling)
        {
            RotatePlayerToTarget();
        }
        // Si está idle y existe un desajuste grande o la rotación automática está activa, actualiza la rotación idle
        else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
        {
            UpdateIdleRotation(rotationTolerance);
        }

        // Aplica la rotación calculada a la cámara
        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        // Calcula el ángulo entre la dirección del jugador y la dirección de la cámara (con signo)
        Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);

    }

    private void UpdateIdleRotation(float rotationTolerance)
    {
        //Initiate new rotation direction
        if (Mathf.Abs(RotationMismatch) > rotationTolerance)
        {
            _rotatingToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }
        _rotatingToTargetTimer -= Time.deltaTime;

        //Rotate player
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

    #endregion

    #region State Checks
    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);//cambiado de z a y despues regresado a z
        return lateralVelocity.magnitude > movingThreshold;
    }
    private bool IsGrounded()
    {
        // Determina si el personaje está en el suelo usando dos estrategias según el estado
        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        return grounded;
    }

    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);
        return grounded;
    }

    private bool IsGroundedWhileAirborne()
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        return _characterController.isGrounded && validAngle;
    }


    private bool CanRun()
    {
        //this means players is moving diagonally at 45 degrees or forward, if so, we can run
        return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
    }
    #endregion
}

