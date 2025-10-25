// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// RESUMEN DE LA CLASE: PlayerState
// Esta clase actúa como una máquina de estados simple para el movimiento del jugador.
// Su única responsabilidad es mantener y gestionar el estado de movimiento actual del personaje
// (por ejemplo, Idling, Walking, Running, Jumping). Proporciona métodos para que otros scripts
// puedan establecer un nuevo estado y consultar si el estado actual es uno de los que se consideran "en el suelo".
// Centralizar el estado aquí ayuda a que el resto del código sea más limpio y fácil de entender.
public class PlayerState : MonoBehaviour
{
    #region State Properties
    // Propiedad que almacena el estado de movimiento actual del jugador.
    // [field: SerializeField] permite que la variable privada sea visible y editable en el Inspector de Unity.
    // El jugador comienza en estado 'Idling' (inactivo) por defecto.
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    #endregion

    #region Public Methods
    // Método público para que otros scripts puedan cambiar el estado de movimiento del jugador.
    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        // Asigna el nuevo estado a la propiedad que almacena el estado actual.
        CurrentPlayerMovementState = playerMovementState;
    }

    // Método para verificar si el estado actual del jugador es un estado "en el suelo".
    // Se usa, por ejemplo, para decidir si el jugador puede saltar.
    public bool InGroundedState()
    {   
        // Llama al método auxiliar pasando el estado de movimiento actual.
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    // Método público que determina si un estado de movimiento específico (pasado como parámetro) corresponde a un estado "en el suelo".
    // Es útil para comprobaciones sin necesidad de cambiar el estado actual.
    public bool IsStateGroundedState(PlayerMovementState movementState)
    {
        // Devuelve 'true' si el estado es uno de los que ocurren en el suelo.
        return movementState == PlayerMovementState.Idling ||
               movementState == PlayerMovementState.Walking ||
               movementState == PlayerMovementState.Running ||
               movementState == PlayerMovementState.Sprinting;
    }
    #endregion
}
// Enumeración que define los posibles estados de movimiento del jugador.
// Usar una enumeración hace el código más legible y menos propenso a errores que usar números o strings.
public enum PlayerMovementState
{
    // El jugador está quieto.
    Idling = 0,
    // El jugador está caminando.
    Walking = 1,
    // El jugador está corriendo (con Shift).
    Running = 2,
    // El jugador está esprintando (movimiento normal).
    Sprinting = 3,
    // El jugador está en la fase ascendente de un salto.
    Jumping = 4,
    // El jugador está cayendo (o en la fase descendente de un salto).
    Falling = 5,
    // El jugador se mueve lateralmente (actualmente no se usa en la lógica principal).
    Strafing = 6,
}