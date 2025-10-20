using Unity.VisualScripting;
using UnityEngine;

// Gestiona el estado de movimiento del jugador y utilidades relacionadas
public class PlayerState : MonoBehaviour
{
    // Propiedad serializada para ver/editar en el Inspector.
    // field: SerializeField permite serializar la auto-propiedad.
    // CurrentPlayerMovementState guarda el estado actual y es de solo lectura desde fuera.
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;

    // Cambia el estado de movimiento del jugador.
    // Se usa en otros sistemas para notificar cambios de comportamiento (ej. animaciones, físicas).
    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    // Devuelve true si el estado actual se considera "en tierra" (grounded).
    // Es un wrapper conveniente alrededor de IsStateGroundedState, usando el estado actual.
    public bool InGroundedState()
    {
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    // Determina si un PlayerMovementState concreto cuenta como "grounded".
    // Esto centraliza la lógica para decidir qué estados permiten acciones que requieren estar en tierra.
    public bool IsStateGroundedState(PlayerMovementState movementState)
    {
        // Retorna true para estados que representan estar en contacto con el suelo.
        return movementState == PlayerMovementState.Idling ||
               movementState == PlayerMovementState.Walking ||
               movementState == PlayerMovementState.Running ||
               movementState == PlayerMovementState.Sprinting;
    }
}

// Enum que define todos los estados de movimiento que maneja el sistema.
// Usar enum hace el código más legible y evitable el uso de "magic numbers".
public enum PlayerMovementState
{
    Idling = 0,   // Quieto en su lugar
    Walking = 1,  // Movimiento a velocidad de andar
    Running = 2,  // Movimiento a velocidad de carrera
    Sprinting = 3,// Movimiento acelerado (sprint)
    Jumping = 4,  // Estado de salto (no grounded hasta aterrizar)
    Falling = 5,  // Estado de caída libre
    Strafing = 6, // Movimiento lateral manteniendo orientación (opcional)
}