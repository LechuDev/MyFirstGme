// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// RESUMEN DE LA CLASE: PlayerLocomotionInput
// Esta clase se encarga de leer y procesar específicamente los inputs relacionados con la locomoción
// del personaje (movimiento, vista, salto, correr, caminar). Actúa como un intermediario que consulta
// el estado de las acciones desde el 'PlayerInputManager' centralizado y lo almacena en propiedades
// públicas. Otros scripts, como 'PlayerController', leen estas propiedades para ejecutar la lógica
// de movimiento, manteniendo así la lectura de inputs separada de la lógica de movimiento del personaje.

// Define el orden de ejecución para que este script se ejecute después del PlayerInputManager pero antes del PlayerController.
[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour
{
    #region Settings
    [SerializeField] private bool holdToSprint = true;
    #endregion

    #region Input State
    // Propiedades públicas que almacenan el estado actual de los inputs de locomoción.
    // Otros scripts leen estas propiedades para saber qué acción debe realizar el personaje.
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintToggledOn { get; private set; } = false;
    public bool SprintPressed { get; private set; } = false;
    public bool WalkToggledOn { get; private set; }
    #endregion

    #region Unity Lifecycle
    // El método Update se llama en cada frame.
    private void Update()
    {
        // Lee el valor actual de la acción 'Movement' (e.g., del joystick o WASD) desde el PlayerInputManager y lo guarda.
        MovementInput = PlayerInputManager.Instance.Movement.ReadValue<Vector2>();
        // Lee el valor actual de la acción 'Look' (e.g., del movimiento del ratón) y lo guarda.
        LookInput = PlayerInputManager.Instance.Look.ReadValue<Vector2>();
        // Comprueba si la acción 'Jump' (salto) fue presionada en este frame exacto.
        JumpPressed = PlayerInputManager.Instance.Jump.WasPressedThisFrame();

        // Si se presionó el botón de alternar caminar en este frame...
        if (PlayerInputManager.Instance.ToggleWalk.WasPressedThisFrame())
            WalkToggledOn = !WalkToggledOn; // ...invierte el estado de 'WalkToggledOn' (si era true, ahora es false, y viceversa).

        // Lógica para el sprint, dependiendo de si está configurado para 'mantener' o 'alternar'.
        if (holdToSprint)
        {
            // Si es 'mantener', la propiedad 'SprintPressed' es verdadera mientras la tecla esté presionada.
            SprintPressed = PlayerInputManager.Instance.ToggleSprint.IsPressed();
        }
        // Si no es 'mantener', entonces es 'alternar'.
        else if (PlayerInputManager.Instance.ToggleSprint.WasPressedThisFrame())
        {
            // Si se presionó la tecla de sprint en este frame, invierte el estado de 'SprintToggledOn'.
            SprintToggledOn = !SprintToggledOn;
        }
    }
    #endregion
}
