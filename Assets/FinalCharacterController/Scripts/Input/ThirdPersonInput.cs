// Importa las librerías necesarias de C#, Unity y Cinemachine.
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

    // RESUMEN DE LA CLASE: ThirdPersonInput
    // Esta clase se encarga de manejar el input específico para la cámara en tercera persona,
    // concretamente el zoom con la rueda del ratón (scroll). Implementa la interfaz 'IThirdPersonMapActions'
    // para recibir los callbacks del Input System. Lee el valor del scroll, lo procesa aplicando una
    // velocidad y lo utiliza para ajustar la propiedad 'CameraDistance' de un componente 'Cinemachine3rdPersonFollow',
    // manteniendo el zoom dentro de unos límites mínimo y máximo.
    
    // Define el orden de ejecución para que se actualice después del PlayerInputManager.
    [DefaultExecutionOrder(-2)]
    public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
    {
        #region Input State
        // Propiedad pública que almacena el valor procesado del input de scroll.
        public Vector2 ScrollInput { get; private set; }
        #endregion

        // Referencia a la cámara virtual de Cinemachine que se va a controlar.
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        // Velocidad a la que se aplicará el zoom.
        [SerializeField] private float _cameraZoomSpeed = 0.1f;
        // Distancia mínima de la cámara (límite del zoom in).
        [SerializeField] private float _cameraMinZoom = 1f;
        // Distancia máxima de la cámara (límite del zoom out).
        [SerializeField] private float _cameraMaxZoom = 5f;

        #region Private References
        // Referencia al componente de Cinemachine que controla el seguimiento en tercera persona.
        private Cinemachine3rdPersonFollow _thirdPersonFollow;
        #endregion

        #region Startup
        private void Awake()
        {
            _thirdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }
        private void OnEnable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized - cannot enable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized - cannot disable");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }
        #endregion

        #region Unity Lifecycle
        // El método Update se llama en cada frame.
        private void Update()
        {
            // Ajusta la distancia de la cámara ('CameraDistance') sumando el valor del scroll.
            // Mathf.Clamp asegura que el resultado se mantenga siempre entre el mínimo y el máximo definidos.
            _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance + ScrollInput.y, _cameraMinZoom, _cameraMaxZoom);
        }

        // LateUpdate se llama después de todos los Updates. Es un buen lugar para resetear inputs.
        private void LateUpdate()
        {
            // Resetea el input del scroll a cero para que no se acumule en el siguiente frame.
            ScrollInput = Vector2.zero;
        }
        #endregion

        #region Input Callbacks
        // Este es el método de callback que se llama cuando se detecta la acción 'ScrollCamera'.
        public void OnScrollCamera(InputAction.CallbackContext context)
        {
            // Si el evento no es 'performed' (es decir, si no se ha completado la acción), no hace nada.
            if (!context.performed)
                return;

            // Lee el valor del scroll (un Vector2) desde el contexto del input.
            Vector2 scrollInput = context.ReadValue<Vector2>();
            // Procesa el valor: lo normaliza, lo invierte (para que el scroll hacia arriba acerque) y lo multiplica por la velocidad de zoom.
            ScrollInput = -1f * scrollInput.normalized * _cameraZoomSpeed;
        }
        #endregion
    }
