using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

// Controla el cambio entre dos cámaras de Cinemachine (vista cercana y vista lejana)
// Usa Input System para escuchar una acción que cambia la vista
public class ViewModController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    // Referencias a las cámaras de Cinemachine que se alternarán por prioridad
    // Nota: en la práctica se suele usar CinemachineVirtualCamera; aquí se mantiene el tipo que usas
    public CinemachineCamera closeViewCamera;
    public CinemachineCamera farViewCamera;

    [Header("Input")]
    // InputActionReference permite asignar desde el Inspector una acción del nuevo Input System
    // .action es la InputAction concreta; con Enable/Disable controlamos su lectura
    public InputActionReference switchViewAction;

    // Estado interno: true = vista cercana activa (closeViewCamera con mayor prioridad)
    private bool isCloseView = true;

    // Se suscribe al callback y habilita la acción cuando este componente se activa
    private void OnEnable()
    {
        // Añadimos el listener al evento performed de la InputAction
        // performed se dispara cuando la acción se considera ejecutada según su tipo (p.ej. botón presionado)
        switchViewAction.action.performed += OnSwitchView;
        // Habilitamos la acción para que reciba eventos
        switchViewAction.action.Enable();
    }

    // Se desuscribe y deshabilita la acción cuando el componente se desactiva
    private void OnDisable()
    {
        // Quitamos el listener para evitar llamadas a objetos desactivados o leak de referencias
        switchViewAction.action.performed -= OnSwitchView;
        // Deshabilitamos la acción para ahorrar procesamiento y evitar input inesperado
        switchViewAction.action.Disable();
    }

    // Start se usa para asegurar que el estado inicial de prioridades se configure al comenzar la escena
    private void Start()
    {
        UpdateCameraPriority();
    }

    // Callback invocado cuando la acción de cambio de vista se ejecuta
    // context contiene info adicional del evento (fase, valor, control origen), no usada aquí
    private void OnSwitchView(InputAction.CallbackContext context)
    {
        // Alterna el booleano que marca la vista activa
        isCloseView = !isCloseView;
        // Aplica la nueva prioridad a las cámaras para que Cinemachine haga la transición
        UpdateCameraPriority();
    }

    // Aplica prioridades a las cámaras para que Cinemachine elija cuál está activa
    // Cinemachine elige la cámara con mayor Priority; las transiciones respetan blend settings
    private void UpdateCameraPriority()
    {
        if (isCloseView)
        {
            // Close view tiene prioridad mayor; far view queda en prioridad baja
            closeViewCamera.Priority = 10;
            farViewCamera.Priority = 0;
        }
        else
        {
            // Far view tomada, close view bajada
            closeViewCamera.Priority = 0;
            farViewCamera.Priority = 10;
        }
    }
}