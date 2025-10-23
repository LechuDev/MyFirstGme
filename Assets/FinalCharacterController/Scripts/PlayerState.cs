using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Clase que maneja el estado de movimiento del jugador.
public class PlayerState : MonoBehaviour
{
    // Propiedad que almacena el estado de movimiento actual del jugador. 
    // En base al valor inicial, el jugador comienza en estado Idling (inactivo).
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    // Método para establecer el estado de movimiento del jugador.
    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        // Actualiza la propiedad CurrentPlayerMovementState con el nuevo estado proporcionado.
        CurrentPlayerMovementState = playerMovementState;
    }
    // Método para verificar si el jugador está en un estado de movimiento que indica que está en el suelo.
    public bool InGroundedState()
    {   // Llama a IsStateGroundedState pasando el estado de movimiento actual del jugador.
        return IsStateGroundedState(CurrentPlayerMovementState);
    }
    // Método estático que determina si un estado de movimiento específico corresponde a un estado en el suelo.
    public bool IsStateGroundedState(PlayerMovementState movementState)
    {
        return movementState == PlayerMovementState.Idling ||
               movementState == PlayerMovementState.Walking ||
               movementState == PlayerMovementState.Running ||
               movementState == PlayerMovementState.Sprinting;
    }
}
// Enumeración que define los posibles estados de movimiento del jugador.
public enum PlayerMovementState
{
    // Idling es un estado donde el jugador está inactivo.
    Idling = 0,
// Walking es un estado donde el jugador está caminando a una velocidad lenta.
    Walking = 1,
    // Running es un estado donde el jugador está corriendo a una velocidad moderada.
    Running = 2,
    // Sprinting es un estado donde el jugador está corriendo a máxima velocidad.
    Sprinting = 3,
    // Jumping es un estado donde el jugador está saltando.
    Jumping = 4,
    // Falling es un estado donde el jugador está cayendo.
    Falling = 5,
    // Strafing es un estado donde el jugador se mueve lateralmente.
    Strafing = 6,
}