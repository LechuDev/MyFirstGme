using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    #region Class Variables
    [SerializeField] private bool holdToSprint = true;
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintToggledOn { get; private set; }
    public bool WalkToggledOn { get; private set; }

    #endregion

    #region Startup
    private void OnEnable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
         
            return;
        }
           
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.Enable();
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.SetCallbacks(this);
    }
    private void OnDisable()
    {
         if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.Disable();
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Late Update Logic
    private void LateUpdate()
    {
        JumpPressed = false;
    }
    #endregion

    #region Input Callbacks
    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
       
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnToggleSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintToggledOn = holdToSprint || !SprintToggledOn;
        }
        else if (context.canceled)
        {
            SprintToggledOn = !holdToSprint && SprintToggledOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        JumpPressed = true;
    }

    public void OnToggleWalk(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        WalkToggledOn = !WalkToggledOn;
    }
    #endregion
}