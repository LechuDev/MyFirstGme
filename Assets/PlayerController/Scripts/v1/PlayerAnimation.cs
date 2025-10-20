using System;                       // Espacio de nombres estándar (no usado directamente aquí pero común)
using System.Linq;                  // Provee LINQ, usado para el método Any
using System.Runtime.CompilerServices;// No usado directamente en este script pero a veces presente por templates
using UnityEngine;                  // API principal de Unity

// Componente que sincroniza el estado del juego y las entradas con el Animator
public class PlayerAnimation : MonoBehaviour
{
    #region Class Variables

    // Referencia al Animator que controla las animaciones del jugador
    [SerializeField] private Animator _animator;

    // Velocidad con la que se interpola el blend de locomoción (menor = más suave)
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    // Referencias a otros componentes del jugador que proveen input y estado
    private PlayerLocomotionInput _playerLocomotionInput; // componente que contiene el input de movimiento (Vector2 MovementInput, etc.)
    private PlayerState _playerState;                     // componente que contiene el estado del jugador (enum de movement states, grounded checks)
    private PlayerController _playerController;           // componente que maneja rotación, mismatches, etc.
       //1 instanciar PlayerActionsInput o Script de Input de Acciones del Jugador
    private PlayerActionsInput _playerActionsInput;       // componente que contiene inputs de acciones (AttackPressed, GatherPressed)

    // Locomotion Animator Hashes
    // Crear hashes de strings mejora el rendimiento frente a usar strings directos en cada Set/Get
    
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");

    // Action Animator Hashes
    // Hashes para parámetros booleanos de acciones (ataque, recolección, etc.)
    //2 guardar hashes de acciones 
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isGatheringHash = Animator.StringToHash("isGathering");

     //5Agregar hashes para cambiar mi parámetro de animación IsPlayingAction
    // Hash para indicar si se está reproduciendo cualquier acción que bloquea locomoción
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");

    // Array que agrupa las acciones que consideramos "reproductoras" o interruptoras de locomoción
    private int[] actionHashes;

    // Hashes relacionados con la cámara/rotación
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

    // Valor actual interpolado del blend de input X/Y/magnitude (se usa Vector3 para x,y,magnitude)
    private Vector3 _currentBlendInput = Vector3.zero;

    // Valores máximos del blend para distintos estados de movimiento
    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue = 1f;
    private float _walkMaxBlendValue = 0.5f;

    #endregion

    // Awake: instancia referencias y configura estructuras necesarias antes de Start
    private void Awake()
    {
        // Obtener componentes obligatorios en el mismo GameObject
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>(); // debe exponer MovementInput (Vector2)
        _playerState = GetComponent<PlayerState>();                     // expone CurrentPlayerMovementState y InGroundedState()
        _playerController = GetComponent<PlayerController>();           // expone IsRotatingToTarget y RotationMismatch
        _playerActionsInput = GetComponent<PlayerActionsInput>();       // expone AttackPressed, GatherPressed, etc.

        // Inicializar el array de hashes de acciones que consideramos como "reproduciendo una acción"
        // Aquí se agregan los hashes cuyos parámetros booleanos en el Animator indican que se está ejecutando una acción.
        actionHashes = new int[]
        {
            // isAttackingHash, // comentado: si no quieres que ataque interrumpa locomoción, lo dejas fuera
            isGatheringHash
        };
    }

    // Update: se ejecuta cada frame y actualiza el Animator según el estado y el input
    private void Update()
    {
        UpdateAnimationState();
    }

    // Actualiza todos los parámetros relevantes del Animator en base al estado actual del jugador
    private void UpdateAnimationState()
    {
        // Lectura de estados del PlayerState (delegamos la lógica de estado fuera de este script)
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;

        // InGroundedState() normalmente devuelve true si el jugador está tocando el suelo
        bool isGrounded = _playerState.InGroundedState();

        // Determina si se está reproduciendo alguna de las acciones almacenadas en actionHashes
        // Any itera sobre el array y GetBool(hash) consulta el parámetro booleano en el Animator
        // Retorna true si al menos una acción está activa
        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

        // Determina si debemos usar el valor de "run" para el blend (run, jump o falling usan el mismo rango)
        bool isRunBlendValue = isRunning || isJumping || isFalling;

        // Calcula el objetivo de blend en base al input y al multiplicador del estado (sprint/run/walk)
        // _playerLocomotionInput.MovementInput expected: Vector2 (x, y)
        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                              isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue :
                                                _playerLocomotionInput.MovementInput * _walkMaxBlendValue;

        // Interpolación suave entre el blend actual y el objetivo.
        // Vector3.Lerp(a, b, t) devuelve un punto entre a y b según t en [0,1].
        // Se multiplica locomotionBlendSpeed por Time.deltaTime para que la velocidad sea frame-rate independent.
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        // Actualizar parámetros booleanos del Animator (estado del suelo, idling, falling, jumping, rotación)
        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);

        // Actualizar parámetros de acciones a partir de los inputs de acción
        // Se usan directamente los booleans expuestos por PlayerActionsInput (AttackPressed, GatherPressed)
        _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
        _animator.SetBool(isGatheringHash, _playerActionsInput.GatherPressed);

        // Actualizar en el Animator si se está reproduciendo alguna acción que debería bloquear o alterar locomoción
        _animator.SetBool(isPlayingActionHash, isPlayingAction);

        // Actualizar parámetros float del Animator con el blend actual
        // inputX, inputY son usados para blend trees 2D; inputMagnitude puede controlar velocidad/estado
        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudHash, _currentBlendInput.magnitude);

        // Parámetro que refleja la diferencia angular entre la orientación actual y la deseada (puede usarse para transiciones)
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
}