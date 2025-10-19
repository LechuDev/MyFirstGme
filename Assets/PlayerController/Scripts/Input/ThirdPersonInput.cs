using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-2)]
public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
{
    #region Class Variables
    // ScrollInput: vector que almacena la entrada de scroll (rueda del ratón o touchpad).
    // Se usa en Update para ajustar la distancia de la cámara y se resetea en LateUpdate.
    public Vector2 ScrollInput { get; private set; }

    // Referencia al componente de Cinemachine que controla el seguimiento en 3ª persona
    [SerializeField] private CinemachineThirdPersonFollow _virtualCamera;
    // Velocidad de zoom aplicada al valor de scroll normalizado
    [SerializeField] private float _cameraZoomSpeed = 0.1f;
    // Límites mínimo y máximo de distancia de cámara
    [SerializeField] private float _cameraMinZoom = 1f;
    [SerializeField] private float _cameraMaxZoom = 5f;

    // Referencia cacheada al componente CinemachineThirdPersonFollow extraído de _virtualCamera
    private CinemachineThirdPersonFollow _thirdPersonFollow;
    #endregion

    #region Startup
    private void Awake()
    {
        // Cachea la referencia al componente ThirdPersonFollow para manipular CameraDistance
        _thirdPersonFollow = _virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
    }
    private void OnEnable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }

        // Habilita el mapa de input ThirdPerson y registra esta clase como callback
        PlayerInputManager.Instance.PlayersControls.ThirdPersonMap.Enable();
        PlayerInputManager.Instance.PlayersControls.ThirdPersonMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.Instance?.PlayersControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }

        // Deshabilita el mapa de input y elimina los callbacks para evitar referencias cuando el objeto está inactivo
        PlayerInputManager.Instance.PlayersControls.ThirdPersonMap.Disable();
        PlayerInputManager.Instance.PlayersControls.ThirdPersonMap.RemoveCallbacks(this);
    }
    #endregion

    #region Update
    private void Update()
    {
        // Ajusta la distancia de la cámara en función del ScrollInput, manteniéndola dentro de los límites
        _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance + ScrollInput.y, _cameraMinZoom, _cameraMaxZoom);
    }

    private void LateUpdate()
    {
        // Resetea el input de scroll cada frame tardío para que el scroll sea por evento y no acumulativo
        ScrollInput = Vector2.zero;
    }
    #endregion

    #region Input Callbacks
    public void OnScrollCamera(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Cuando el input de scroll se realiza, leemos el valor (puede venir como Vector2 del ratón)
        Vector2 scrollInput = context.ReadValue<Vector2>();
        // Normalizamos y aplicamos velocidad de zoom, invertimos signo para que 'scroll hacia adelante' acerque la cámara
        ScrollInput = -1f * scrollInput.normalized * _cameraZoomSpeed;
    }
    #endregion
}
