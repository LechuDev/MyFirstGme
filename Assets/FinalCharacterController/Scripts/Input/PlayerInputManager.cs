// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

    // RESUMEN DE LA CLASE: PlayerInputManager
    // Esta clase actúa como un Singleton centralizado para gestionar todas las entradas (inputs) del jugador.
    // Su propósito es crear una única instancia del 'PlayerControls' (el asset de Input System) y exponer
    // todas las acciones de entrada (Movimiento, Salto, Ataque, etc.) como propiedades públicas.
    // De esta forma, cualquier otro script en el juego puede acceder al estado de los controles de forma
    // sencilla y consistente a través de 'PlayerInputManager.Instance', evitando la necesidad de que cada
    // script gestione su propia instancia de inputs, lo que optimiza el rendimiento y organiza el código.

    // Define el orden de ejecución de este script para que se ejecute muy temprano, asegurando que los inputs estén listos para otros scripts.
    [DefaultExecutionOrder(-3)]
    public class PlayerInputManager : MonoBehaviour
    {
        #region Singleton & Controls
        public static PlayerInputManager Instance; // Variable estática para implementar el patrón Singleton, accesible globalmente.
        public PlayerControls PlayerControls { get; private set; } // Propiedad para acceder a la instancia de los controles generados por el Input System.

        // Locomotion Actions
        public InputAction Movement { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction ToggleSprint { get; private set; }
        public InputAction ToggleWalk { get; private set; }

        // Other Actions
        public InputAction Gathering { get; private set; }
        public InputAction Attacking { get; private set; }
        public InputAction SwitchCamera { get; private set; }
        public InputAction ScrollCamera { get; private set; }
        #endregion

        #region Unity Lifecycle
        // Se llama una vez cuando el script es cargado.
        private void Awake()
        {
            // Implementación del patrón Singleton. Si ya existe una instancia y no es esta, se destruye.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Asigna esta instancia como la única instancia global.
            Instance = this;
            // Evita que este GameObject se destruya al cargar una nueva escena, manteniendo el gestor de inputs persistente.
            DontDestroyOnLoad(gameObject);
        }

        // Se llama cuando el GameObject se activa.
        private void OnEnable()
        {
            // Crea una nueva instancia del objeto PlayerControls, que contiene todos los mapas de acciones y bindings.
            PlayerControls = new PlayerControls();

            // Cache actions for easy access
            // Almacena en caché las referencias a cada acción específica para un acceso más rápido y limpio desde otros scripts.
            // Esto evita tener que buscar la acción por su nombre ("string") repetidamente.
            Movement = PlayerControls.PlayerLocomotionMap.Movement;
            Look = PlayerControls.PlayerLocomotionMap.Look;
            Jump = PlayerControls.PlayerLocomotionMap.Jump;
            ToggleSprint = PlayerControls.PlayerLocomotionMap.ToggleSprint;
            ToggleWalk = PlayerControls.PlayerLocomotionMap.ToggleWalk;

            // Acciones del mapa 'PlayerActionsMap'.
            Gathering = PlayerControls.PlayerActionsMap.Gathering;
            Attacking = PlayerControls.PlayerActionsMap.Attacking;
            SwitchCamera = PlayerControls.PlayerActionsMap.SwitchCamera;

            // Acción del mapa 'ThirdPersonMap'.
            ScrollCamera = PlayerControls.ThirdPersonMap.ScrollCamera;

            // Habilita todos los mapas de acciones y sus acciones para que empiecen a escuchar los inputs del hardware.
            PlayerControls.Enable();
        }

        // Se llama cuando el GameObject se desactiva.
        private void OnDisable()
        {
            // Deshabilita todos los mapas de acciones para dejar de escuchar inputs, liberando recursos.
            PlayerControls.Disable();
        }
        #endregion
    }