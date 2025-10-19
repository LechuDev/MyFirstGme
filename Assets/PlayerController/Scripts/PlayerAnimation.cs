using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    #region Class Variables
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    //1 instanciar PlayerActionsInput o Script de Input de Acciones del Jugador
    private PlayerActionsInput _playerActionsInput;


    // Locomotion Animator Hashes
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");

    // Action Animator Hashes
    //2 guardar hashes de acciones 
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isGatheringHash = Animator.StringToHash("isGathering");
    //5Agregar hashes para cambiar mi parámetro de animación IsPlayingAction
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    //Matiz para actionHashes
    private int[] actionHashes;
    // Camera and Rotation Hashes
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

    private Vector3 _currentBlendInput = Vector3.zero;

    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue = 1f;
    private float _walkMaxBlendValue = 0.5f;
    #endregion
    //Función para inicializar referencias
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
        //3 obtener referencia al componente PlayerActionsInput 
        _playerActionsInput = GetComponent<PlayerActionsInput>();
        //6.-Se inicializa el array de hashes de acciones
        //Los hashes de las acciones se almacenan en el array
        //Conjunto de acciones que se interrumpen por locomoción
        actionHashes = new int[]
        {
            //isAttackingHash,
            isGatheringHash
        };
    }
    //Función para actualizar la animación cada frame
    private void Update()
    {
        UpdateAnimationState();
    }
    //Función para actualizar el estado de la animación
    private void UpdateAnimationState()
    {

        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
        bool isGrounded = _playerState.InGroundedState();
        //7.-Determinar si se está reproduciendo alguna acción dentro de actionHashes 
        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));


        bool isRunBlendValue = isRunning || isJumping || isFalling;

        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                              isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue :
                                                _playerLocomotionInput.MovementInput * _walkMaxBlendValue;
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
        //4 usar los estados de las acciones para actualizar los parámetros del animador
        _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
        _animator.SetBool(isGatheringHash, _playerActionsInput.GatherPressed);
        //8.-Actualizar el parámetro IsPlayingAction en el animador
        _animator.SetBool(isPlayingActionHash, isPlayingAction);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudHash, _currentBlendInput.magnitude);
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
}

