// Importa las librerías necesarias de C#, Unity y Cinemachine.
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections.Generic;

// RESUMEN DE LA CLASE: CameraSwitcher
// Esta clase gestiona el cambio entre diferentes cámaras virtuales de Cinemachine.
// Funciona como un Singleton para ser fácilmente accesible desde otros scripts.
// Al presionar la tecla de cambio de cámara (definida en el Input System), cicla a través de una
// lista de cámaras virtuales, cambiando su propiedad 'Priority'. Cinemachine automáticamente
// activa la cámara con la prioridad más alta. También expone una propiedad para saber si la
// cámara de primera persona (la primera de la lista) está activa, permitiendo a otros scripts
// adaptar su comportamiento.
public class CameraSwitcher : MonoBehaviour
{
    #region Serialized Fields
    // Atributo para mostrar una descripción útil en el Inspector de Unity.
    [Tooltip("Lista de cámaras virtuales de Cinemachine para alternar.")]
    // Permite asignar la lista de cámaras desde el Inspector.
    [SerializeField]
    private List<CinemachineVirtualCameraBase> cameras;

    [Tooltip("Prioridad para la cámara activa.")]
    [SerializeField]
    private int activeCameraPriority = 10; // Valor de prioridad que se asignará a la cámara que esté activa.

    [Tooltip("Prioridad para las cámaras inactivas.")]
    [SerializeField]
    private int inactiveCameraPriority = 0; // Valor de prioridad para todas las cámaras que no estén activas.
    #endregion

    #region Singleton & State
    // Propiedad estática para implementar el patrón Singleton. Permite el acceso global a esta instancia.
    public static CameraSwitcher Instance { get; private set; }

    // Propiedad pública de solo lectura que devuelve 'true' si la cámara de primera persona está activa.
    // Se asume que la cámara de primera persona es la que está en el índice 0 de la lista.
    public bool IsFirstPersonCameraActive => cameras != null && cameras.Count > 0 && _currentCameraIndex == 0;
    
    // Índice para llevar la cuenta de cuál es la cámara activa actualmente en la lista.
    private int _currentCameraIndex = 0;
    #endregion

    #region Unity Lifecycle
    // Se llama una vez cuando el script es cargado.
    private void Awake()
    {
        // Implementación del patrón Singleton para asegurar que solo haya una instancia de esta clase.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Asigna esta instancia como la única instancia global.
        Instance = this;
    }

    // Se llama cuando el GameObject se activa.
    private void OnEnable()
    {
        // Asegurarse de que la primera cámara esté activa al iniciar
        // Comprueba si la lista de cámaras no es nula y tiene al menos una cámara.
        if (cameras != null && cameras.Count > 0)
        {
            // Llama al método para establecer las prioridades iniciales, activando la primera cámara.
            UpdateCameraPriorities();
        }
    }
    
    // El método Update se llama en cada frame.
    private void Update()
    {
        // Comprueba si la acción 'SwitchCamera' fue presionada en este frame exacto, a través del PlayerInputManager.
        if (PlayerInputManager.Instance.SwitchCamera.WasPressedThisFrame())
        {
            // Si se presionó, llama al método para cambiar a la siguiente cámara.
            SwitchToNextCamera();
        }
    }
    #endregion

    #region Camera Logic
    private void SwitchToNextCamera()
    {
        // Comprobación de seguridad para evitar errores si no hay cámaras asignadas.
        if (cameras == null || cameras.Count == 0)
        {
            Debug.LogWarning("No hay cámaras asignadas en el CameraSwitcher.");
            return;
        }

        // Avanza al siguiente índice de la lista. El operador '%' (módulo) asegura que si llega al final, vuelva al inicio (0).
        _currentCameraIndex = (_currentCameraIndex + 1) % cameras.Count;

        // Actualiza las prioridades de todas las cámaras para activar la nueva cámara seleccionada.
        UpdateCameraPriorities();
    }

    private void UpdateCameraPriorities()
    {
        // Recorre toda la lista de cámaras.
        for (int i = 0; i < cameras.Count; i++)
        {
            var cam = cameras[i];
            // Usa un operador ternario para asignar la prioridad: si el índice 'i' es el de la cámara actual,
            // le da la prioridad de activa; de lo contrario, le da la prioridad de inactiva.
            cameras[i].Priority = (i == _currentCameraIndex) ? activeCameraPriority : inactiveCameraPriority;
        }
        // Muestra un mensaje en la consola para depuración, indicando qué cámara está ahora activa.
        Debug.Log($"Cámara activa cambiada a: {cameras[_currentCameraIndex].name}");
    }
    #endregion
}
