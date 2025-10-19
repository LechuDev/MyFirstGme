using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-2)]
//Clase para manejar las acciones del jugador .IPlayerActionMapActions
public class PlayerActionsInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
{
    #region Class Variables
    //Implementation de variables de clase aquí
    public bool AttackPressed { get; private set; }
    public bool GatherPressed { get; private set; }
    //instancia de PlayerLocomotionInput
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    #endregion
    #region Startup
    //Función para inicializar referencias
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }
    //Métodos OnEnable y OnDisable para habilitar y deshabilitar el mapa de acciones del jugador
    private void OnEnable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }
        PlayerInputManager.Instance.PlayersControls.PlayerActionMap.Enable();
        PlayerInputManager.Instance.PlayersControls.PlayerActionMap.SetCallbacks(this);
    }
    private void OnDisable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }
        PlayerInputManager.Instance.PlayersControls.PlayerActionMap.Disable();
        PlayerInputManager.Instance.PlayersControls.PlayerActionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Late Update Logic
    private void Update()
    {
            //Reset action inputs when player is moving, jumping, or falling
        if (_playerLocomotionInput.MovementInput != Vector2.zero ||
            _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping ||
            _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling)
        {
            GatherPressed = false;
            //AttackPressed = false;
        }
    }

    public void SetGatherPressedFalse()
    {
        GatherPressed = false;
    }

    public void SetAttackPressedFalse()
    {
        AttackPressed = false;
    }
    #endregion

    #region Input Callbacks
    //Implementation de Interfaz IPlayerActionMapActions Callbacks aquí
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        AttackPressed = true;

    }
    public void OnGather(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        GatherPressed = true;

    }
    #endregion
}
