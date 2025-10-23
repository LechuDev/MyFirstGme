using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-2)]
//Hereda de MonoBehaviour e implementa la interfaz IPlayerLocomotionMapActions para manejar las acciones de locomoción del jugador.
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    #region Class Variables
    // Variable serializada para determinar si el sprint se activa manteniendo presionado el botón.
    //Se modifica desde el inspector de Unity.
    [SerializeField] private bool holdToSprint = true;
    // Propiedades públicas para acceder a las entradas de movimiento, mirada, salto y estado de sprint/caminata.
    // Entrada de movimiento del jugador.
    public Vector2 MovementInput { get; private set; }
    // Entrada de mirada del jugador.
    public Vector2 LookInput { get; private set; }
    // Indica si el botón de salto ha sido presionado.
    //Se modifica desde el método OnJump. osea desde la entrada del jugador.
    public bool JumpPressed { get; private set; }
    // Indica si el sprint está activado.
    public bool SprintToggledOn { get; private set; }//Al inicio vale false.
    // Indica si la caminata está activada.
    public bool WalkToggledOn { get; private set; }
    #endregion

    #region Startup
    //Funciones que se utilizan para habilitar y deshabilitar el mapa de locomoción del jugador.
    //OnEnable se llama cuando el objeto se habilita en la escena.
    private void OnEnable()
    {
        // Verifica si los controles del jugador están inicializados antes de habilitar el mapa de locomoción.
        if (PlayerInputManager.Instance?.PlayerControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }
        // Habilita el mapa de locomoción del jugador y establece los callbacks para manejar las entradas.
        PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Enable();
        PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.SetCallbacks(this);
    }
    //OnDisable se llama cuando el objeto se deshabilita en la escena.
    private void OnDisable()
    {
        // Verifica si los controles del jugador están inicializados antes de deshabilitar el mapa de locomoción.
        if (PlayerInputManager.Instance?.PlayerControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }
        // Deshabilita el mapa de locomoción del jugador y elimina los callbacks.
        PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Disable();
        PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Late Update Logic
    //LateUpdate se llama una vez por frame, después de todas las actualizaciones de frame. 
    // Es decir después de Update y FixedUpdate.
    // Aquí se restablece el estado de JumpPressed a false para que el salto solo se registre una vez por pulsación.
    private void LateUpdate()
    {
        JumpPressed = false;
    }
    #endregion

    #region Input Callbacks
    //Funciones o metodos que manejan las entradas del jugador para movimiento, mirada, sprint, salto y caminata.
    // Maneja la entrada de movimiento del jugador.
    public void OnMovement(InputAction.CallbackContext context)
    {
        // Lee el valor de la entrada de movimiento y lo asigna a la propiedad MovementInput.
        MovementInput = context.ReadValue<Vector2>();
    }
    // Maneja la entrada de mirada del jugador.
    public void OnLook(InputAction.CallbackContext context)
    {
        // Lee el valor de la entrada de mirada y lo asigna a la propiedad LookInput.
        LookInput = context.ReadValue<Vector2>();
    }
    // Maneja la entrada de sprint del jugador. Modo Hold o Toggle. Mantener presionado o presionar una vez.
    public void OnToggleSprint(InputAction.CallbackContext context)
    {
        // Asignaciones con operador logico (|| (or) y && (and)) 
        //True || False =  siempre da True
        //False && True = siempre da False
        if (context.performed)
        {
            // Si holdToSprint es verdadero, activa el sprint mientras se mantiene presionado el botón.
            SprintToggledOn = holdToSprint || !SprintToggledOn;//Cambia SprintToggledOn a true si holdToSprint es true o si SprintToggledOn es false.
        }
        else if (context.canceled)
        {

            // Si holdToSprint es falso, desactiva el sprint cuando se suelta el botón.
            SprintToggledOn = !holdToSprint && SprintToggledOn;//Cambia SprintToggledOn a false si holdToSprint es false y si SprintToggledOn es true.
        }
    }
    // Maneja la entrada de salto del jugador.
    public void OnJump(InputAction.CallbackContext context)
    {
        // Solo registra el salto si la acción fue realizada (botón presionado). 
        if (!context.performed)
            return;

        JumpPressed = true;
    }
    // Maneja la entrada de caminata del jugador.
    public void OnToggleWalk(InputAction.CallbackContext context)
    {
        // Solo alterna el estado de caminata si la acción fue realizada (botón presionado).
        if (!context.performed)
            return;

        WalkToggledOn = !WalkToggledOn;
    }
    #endregion
}
//En resumen esta clase gestiona las entradas de locomoción del jugador, incluyendo movimiento, mirada, salto, sprint y caminata, utilizando el sistema de entrada de Unity.
//Contiene propiedades para acceder a estas entradas y métodos para manejar las acciones correspondientes.
//Interfiere con Las clase PlayerInputManager y PlayerControls para manejar las entradas del jugador.
//Se asegura de que las entradas se procesen correctamente y proporciona una interfaz clara para otras partes del juego que necesiten acceder a estas entradas.
//Descripcion de variables y metodos en comentarios dentro del codigo.
// MovementInput: Almacena la entrada de movimiento del jugador.
// LookInput: Almacena la entrada de mirada del jugador.
// JumpPressed: Indica si se ha presionado el botón de salto.
// SprintToggledOn: Indica si el sprint está activado.
// WalkToggledOn: Indica si la caminata está activada.
// OnEnable y OnDisable: Habilitan y deshabilitan el mapa de locomoción del jugador.
// LateUpdate: Restablece el estado de JumpPressed después de cada frame.
// OnMovement
//Este metodo lee la entrada de movimiento del jugador y la almacena en MovementInput.
// OnLook
//Este metodo lee la entrada de mirada del jugador y la almacena en LookInput.
// OnToggleSprint
//Este metodo maneja la lógica para activar o desactivar el sprint según si se mantiene presionado el botón o no.
//Descripcion de logica interna de OnToggleSprint:
//Si holdToSprint es verdadero, SprintToggledOn se establece en verdadero cuando se
// OnJump
//Este metodo establece JumpPressed en true cuando se presiona el botón de salto.
// OnToggleWalk
//Este metodo alterna el estado de caminata cada vez que se presiona el botón de caminata.
