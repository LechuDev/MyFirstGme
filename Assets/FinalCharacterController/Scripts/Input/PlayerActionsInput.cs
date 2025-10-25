// Importa las librerías necesarias de C# y Unity.
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// RESUMEN DE LA CLASE: PlayerActionsInput
// Esta clase gestiona los inputs de acciones específicas del jugador que no son de locomoción, como
// recolectar ('Gathering') y atacar ('Attacking'). Lee el estado de estas acciones desde el 'PlayerInputManager'
// y lo expone a través de propiedades booleanas. Implementa una lógica de "bandera" (flag), donde una acción
// se activa ('true') al ser presionada y debe ser desactivada ('false') manualmente por otro sistema,
// generalmente por un evento de animación al finalizar la acción.

// Define el orden de ejecución para que se actualice después del PlayerInputManager.
[DefaultExecutionOrder(-2)]
// Asegura que el GameObject tenga un componente PlayerState, ya que depende de él.
[RequireComponent(typeof(PlayerState))]
public class PlayerActionsInput : MonoBehaviour
{
    #region State & References
    // Referencia al componente PlayerState para consultar el estado actual del jugador.
    private PlayerState _playerState;
    // Propiedad pública que indica si se ha presionado la acción de recolectar. Se usa en PlayerAnimation.
    public bool GatherPressed { get; private set; }
    // Propiedad pública que indica si se ha presionado la acción de atacar. Se usa en PlayerAnimation.
    public bool AttackPressed { get; private set; }
    #endregion

    #region Unity Lifecycle
    // Se llama una vez cuando el script es cargado.
    private void Awake()
    {
        // Obtiene y almacena la referencia al componente PlayerState del mismo GameObject.
        _playerState = GetComponent<PlayerState>();
    }
    
    // El método Update se llama en cada frame.
    private void Update()
    {
        // Comprueba si la acción de recolectar fue presionada en este frame.
        if (PlayerInputManager.Instance.Gathering.WasPressedThisFrame())
            GatherPressed = true; // Si es así, activa la bandera 'GatherPressed'.

        // Comprueba si la acción de atacar fue presionada en este frame.
        if (PlayerInputManager.Instance.Attacking.WasPressedThisFrame())
            AttackPressed = true; // Si es así, activa la bandera 'AttackPressed'.

        // Lógica para evitar que se pueda recolectar mientras se está en el aire (saltando o cayendo).
        if (_playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping ||
            _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling)
        {
            GatherPressed = false; // Desactiva la bandera de recolección si el personaje está en el aire.
        }
    }
    #endregion

    #region Animation Events
    // Método público para ser llamado desde fuera (normalmente por un Animation Event) para resetear la bandera de recolección.
    public void SetGatherPressedFalse()
    {
        GatherPressed = false;
    }

    // Método público para ser llamado desde fuera (normalmente por un Animation Event) para resetear la bandera de ataque.
    public void SetAttackPressedFalse() 
    { 
        AttackPressed = false;
    }
    #endregion
}
