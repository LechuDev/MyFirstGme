using UnityEngine;

[DefaultExecutionOrder(-3)]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;
    public PlayerControls PlayersControls { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void OnEnable()
    {
     
            PlayersControls = new PlayerControls();
            PlayersControls.Enable();
        
    }

    private void OnDisable()
    {
        PlayersControls.Disable();
    }
}