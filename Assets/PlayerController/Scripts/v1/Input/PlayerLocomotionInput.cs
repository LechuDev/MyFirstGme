using UnityEngine;
using UnityEngine.InputSystem;

// Asegura que este componente se ejecute antes que otros por defecto (por ejemplo, antes que scripts que lean input en Update)
[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    #region Class Variables
    // Si true, mantener la tecla/entrada para sprint; si false, alternar sprint con cada pulsación
    [SerializeField] private bool holdToSprint = true;

    // Propiedades públicas de solo lectura para que otros sistemas (PlayerAnimation, PlayerController, etc.) lean el input
    public Vector2 MovementInput { get; private set; } // Vector2: x = lateral, y = adelante/atrás
    public Vector2 LookInput { get; private set; }     // Vector2: input de cámara/ratón/joystick derecho
    public bool JumpPressed { get; private set; }      // Pulsación de salto (se reinicia cada frame en LateUpdate)
    public bool SprintToggledOn { get; private set; }  // Estado actual de sprint (según holdToSprint y entradas)
    public bool WalkToggledOn { get; private set; }    // Estado de caminar alternado (toggle)
    #endregion

    #region Startup
    // Se activa cuando el GameObject entra en escena o el componente se habilita
    private void OnEnable()
    {
        // Comprobación defensiva: PlayerInputManager debe haber inicializado PlayersControls
        // Uso del operador ?. para evitar NullReferenceException si Instance es null
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }

        // Habilita el mapa de acciones de locomoción y registra este objeto como receptor de callbacks
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.Enable();
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.SetCallbacks(this);
    }

    // Se llama cuando el GameObject se desactiva o el componente se deshabilita
    private void OnDisable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }

        // Deshabilita el mapa de locomoción y remueve los callbacks para evitar llamadas a objetos desactivados
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.Disable();
        PlayerInputManager.Instance.PlayersControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Late Update Logic
    // LateUpdate se ejecuta después de Update; aquí se reinician flags que deben durar solo un frame
    private void LateUpdate()
    {
        // JumpPressed se marca true en el callback OnJump cuando se recibe la acción.
        // Se reinicia aquí para que otras lógicas que se ejecuten en Update o FixedUpdate puedan leerlo una vez por frame.
        JumpPressed = false;
    }
    #endregion

    #region Input Callbacks
    // Callback del mapa de Input: lectura de movimiento (Vector2)
    public void OnMovement(InputAction.CallbackContext context)
    {
        // Lee el valor actual (por ejemplo, WASD, stick izquierdo, teclado) y lo guarda en la propiedad pública
        MovementInput = context.ReadValue<Vector2>();
    }

    // Callback de la entrada de mirada/cámara
    public void OnLook(InputAction.CallbackContext context)
    {
        // Lee valores de ratón o stick derecho
        LookInput = context.ReadValue<Vector2>();
    }

    // Callback para toggle/hold de sprint según el InputAction configurado
    public void OnToggleSprint(InputAction.CallbackContext context)
    {
        // context.performed se dispara cuando la acción se considera realizada (p. ej. al presionar)
        if (context.performed)
        {
            // Si holdToSprint es true: SprintToggledOn = true (mantenerá sprint mientras la tecla esté presionada)
            // Si holdToSprint es false: alterna el estado (toggle) cada vez que se realiza la acción
            SprintToggledOn = holdToSprint || !SprintToggledOn;
        }
        else if (context.canceled)
        {
            // Cuando la acción se cancela (por ejemplo, se suelta la tecla) debemos actualizar el estado solo si usamos holdToSprint == true
            // Si holdToSprint es false, no hacemos nada en cancel.
            // Aquí la expresión resulta en:
            // - Si holdToSprint == false => !holdToSprint == true -> SprintToggledOn = SprintToggledOn (sin cambio)
            // - Si holdToSprint == true => !holdToSprint == false -> SprintToggledOn = false (al soltar)
            SprintToggledOn = !holdToSprint && SprintToggledOn;
        }
    }

    // Callback de salto
    public void OnJump(InputAction.CallbackContext context)
    {
        // Solo nos interesa cuando la acción se realiza (p. ej. button down),
        // no cuando está en performed = false o canceled
        if (!context.performed)
            return;

        // Se marca JumpPressed true; será reiniciado en LateUpdate para que otras partes lo lean por un frame
        JumpPressed = true;
    }

    // Callback para alternar caminar (toggle)
    public void OnToggleWalk(InputAction.CallbackContext context)
    {
        // Solo actuar cuando la acción se realiza
        if (!context.performed)
            return;

        // Alterna el estado de caminar
        WalkToggledOn = !WalkToggledOn;
    }
    #endregion
}