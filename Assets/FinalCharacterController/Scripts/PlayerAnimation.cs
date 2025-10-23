using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
//Esta clase maneja las animaciones del jugador.

public class PlayerAnimation : MonoBehaviour
{
    // Referencia al componente Animator que controla las animaciones del jugador.
    [SerializeField] private Animator _animator;
    // Velocidad de mezcla para las transiciones de locomoción.
    [SerializeField] private float locomotionBlendSpeed = 4f;
    // Referencia al componente PlayerLocomotionInput que maneja la entrada de locomoción del jugador.
    private PlayerLocomotionInput _playerLocomotionInput;
    // Referencia al componente PlayerState que maneja el estado del jugador.
    private PlayerState _playerState;
    // Referencia al componente PlayerController que maneja el control del jugador.
    private PlayerController _playerController;
    // Referencia al componente PlayerActionsInput que maneja las acciones del jugador.
    private PlayerActionsInput _playerActionsInput;

    // Locomotion
    // Hashes para los parámetros del Animator relacionados con la locomoción.
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");

    // Actions
    // Hashes para los parámetros del Animator relacionados con las acciones del jugador.
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isGatheringHash = Animator.StringToHash("isGathering");
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    // Array de hashes de acciones para verificar si alguna acción está en progreso.
    private int[] actionHashes;

    // Camera/Rotation
    // Hashes para los parámetros del Animator relacionados con la cámara y la rotación del jugador.
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
    // Vector3 que almacena la entrada de mezcla actual para la locomoción.
    private Vector3 _currentBlendInput = Vector3.zero;
    // Valores máximos de mezcla para diferentes estados de locomoción.
    private float _sprintMaxBlendValue = 1.5f;
    // Valores máximos de mezcla para diferentes estados de locomoción.
    private float _runMaxBlendValue = 1.0f;
    // Valores máximos de mezcla para diferentes estados de locomoción. 
    private float _walkMaxBlendValue = 0.5f;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
        _playerActionsInput = GetComponent<PlayerActionsInput>();

        actionHashes = new int[] { isGatheringHash };
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
        bool isGrounded = _playerState.InGroundedState();
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
        _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
        _animator.SetBool(isGatheringHash, _playerActionsInput.GatherPressed);
        _animator.SetBool(isPlayingActionHash, isPlayingAction);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
}