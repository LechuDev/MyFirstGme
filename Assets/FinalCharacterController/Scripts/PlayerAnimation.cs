// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// RESUMEN DE LA CLASE: PlayerAnimation
// Esta clase es el puente entre la lógica del juego y el sistema de animación del personaje (Animator).
// Se encarga de leer el estado del jugador (movimiento, acciones) desde otros componentes como
// 'PlayerLocomotionInput', 'PlayerState' y 'PlayerActionsInput', y de actualizar los parámetros
// del Animator correspondientes (e.g., 'inputX', 'isJumping', 'isAttacking'). Esto permite que las
// animaciones del personaje se sincronicen con las acciones y el estado del jugador en tiempo real.

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerLocomotionInput))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerActionsInput))]
public class PlayerAnimation : MonoBehaviour
{
    #region Serialized Fields
    // Referencia al componente Animator que controla las animaciones.
    [SerializeField] private Animator _animator;
    // Velocidad con la que se suaviza la transición entre valores de animación de locomoción.
    [SerializeField] private float locomotionBlendSpeed = 4f;
    #endregion

    #region Component References
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    private PlayerActionsInput _playerActionsInput;
    #endregion
    
    #region Animator Parameter Hashes

    // Hashes de los parámetros del Animator. Usar StringToHash es más eficiente que usar strings directamente en cada Update.
    // Se usan para establecer los valores en el Animator.
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isGatheringHash = Animator.StringToHash("isGathering");
    // Parámetro que indica si se está ejecutando cualquier animación de acción (para bloquear otros movimientos).
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    private int[] actionHashes;

    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    // Discrepancia entre la rotación del personaje y la cámara, para animaciones de giro.
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
    #endregion // End of Animator Parameter Hashes

    #region Private State
    private Vector3 _currentBlendInput = Vector3.zero;
    private readonly float _sprintMaxBlendValue = 1.5f;
    private readonly float _runMaxBlendValue = 1.0f;
    private readonly float _walkMaxBlendValue = 0.5f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Obtiene y almacena las referencias a los componentes necesarios del mismo GameObject.
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
        _playerActionsInput = GetComponent<PlayerActionsInput>();
        // Inicializa un array con los hashes de las animaciones de acción para una comprobación más fácil.
        actionHashes = new int[] { isGatheringHash };
    }

    private void Update()
    {
        // Llama al método que actualiza el Animator en cada frame.
        UpdateAnimationState();
    }
    #endregion

    #region Animation Logic
    private void UpdateAnimationState()
    {
        // Comprueba si el personaje está en el suelo.
        bool isGrounded = _playerState.InGroundedState();
        // Comprueba si alguna animación de acción (como recolectar) está activa.
        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

        // Variables locales para determinar el estado de animación actual.
        float blendValue;
        bool isJumping = false;
        bool isFalling = false;
        bool isIdling = false;

        // Determina el valor de 'blend' (para la velocidad de la animación de locomoción) y los estados booleanos según el estado de movimiento actual.
        switch (_playerState.CurrentPlayerMovementState)
        {
            case PlayerMovementState.Running: // Con Shift, ahora usa el valor de mezcla de Sprint
                blendValue = _sprintMaxBlendValue;
                break;
            case PlayerMovementState.Sprinting: // Sin Shift, ahora usa el valor de mezcla de Run
                blendValue = _runMaxBlendValue;
                break;
            case PlayerMovementState.Jumping:
                isJumping = true;
                blendValue = _runMaxBlendValue;
                break;
            case PlayerMovementState.Falling:
                isFalling = true;
                blendValue = _runMaxBlendValue;
                break;
            case PlayerMovementState.Idling:
                isIdling = true;
                blendValue = 0;
                break;
            default: // Walking
                blendValue = _walkMaxBlendValue;
                break;
        }

        // Calcula el vector de input objetivo para el blend tree, multiplicando el input de movimiento por el valor de blend.
        Vector2 inputTarget = _playerLocomotionInput.MovementInput * blendValue;

        // Suaviza la transición del valor de input actual al valor objetivo para evitar cambios bruscos en la animación.
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        // Establece todos los parámetros booleanos en el Animator.
        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
        // Estos valores se leen de PlayerActionsInput, que gestiona su estado.
        _animator.SetBool(isAttackingHash, _playerActionsInput.AttackPressed);
        _animator.SetBool(isGatheringHash, _playerActionsInput.GatherPressed);
        _animator.SetBool(isPlayingActionHash, isPlayingAction);

        // Establece los parámetros flotantes (floats) en el Animator para controlar el blend tree de locomoción.
        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        // Informa al Animator sobre la diferencia de rotación para las animaciones de giro.
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
    #endregion
}