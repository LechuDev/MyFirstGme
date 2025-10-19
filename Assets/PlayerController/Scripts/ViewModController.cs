using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class ViewModController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera closeViewCamera;
    public CinemachineCamera farViewCamera;

    [Header("Input")]
    public InputActionReference switchViewAction;

    private bool isCloseView = true;

    private void OnEnable()
    {
        switchViewAction.action.performed += OnSwitchView;
        switchViewAction.action.Enable();
    }

    private void OnDisable()
    {
        switchViewAction.action.performed -= OnSwitchView;
        switchViewAction.action.Disable();
    }

    private void Start()
    {
        UpdateCameraPriority();
    }

    private void OnSwitchView(InputAction.CallbackContext context)
    {
        isCloseView = !isCloseView;
        UpdateCameraPriority();
    }

    private void UpdateCameraPriority()
    {
        if (isCloseView)
        {
            closeViewCamera.Priority = 10;
            farViewCamera.Priority = 0;
        }
        else
        {
            closeViewCamera.Priority = 0;
            farViewCamera.Priority = 10;
        }
    }
}