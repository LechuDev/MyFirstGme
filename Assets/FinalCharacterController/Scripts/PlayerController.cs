using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    #region Class Variables

    [Header("Components")]
    // Referencias serializadas al CharacterController y la cámara del jugador.
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;
    // Propiedades públicas para acceder a la discrepancia de rotación y el estado de rotación hacia el objetivo.
    // Esta discrepancia representa la diferencia angular entre la dirección del jugador y la dirección de la cámara en el plano horizontal (XZ).
    public float RotationMismatch { get; private set; } = 0f;
    // Indica si el jugador está actualmente rotando automáticamente para alinearse con la cámara.
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Base Movement")]
    // Parámetros de movimiento del jugador.
    // Estos valores pueden ser ajustados desde el inspector de Unity.
    // Aceleración y velocidad para caminar, correr y esprintar.
    // Estos valores determinan qué tan rápido puede acelerar y la velocidad máxima en cada estado de movimiento.
    public float walkAcceleration = 25f;
    public float walkSpeed = 2f;
    public float runAcceleration = 35f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 50f;
    public float sprintSpeed = 7f;
    // Otros parámetros relacionados con el movimiento.
    // Estos valores afectan la física del movimiento del jugador.
    public float drag = 20f;
    public float gravity = 25f;
    // Parámetros de movimiento vertical
    // Estos valores afectan el salto y el movimiento en el aire del jugador.
    public float jumpSpeed = 0.8f;
    public float inAirAcceleration = 25f;
    public float inAirDrag = 5f;
    // Velocidad terminal para limitar la velocidad máxima de caída del jugador.
    public float terminalVelocity = 50f;
    // Parámetro para determinar si el jugador se está moviendo lateralmente.
    public float movingThreshold = 0.01f;

    [Header("Animation")]
    // Parámetros relacionados con la animación del jugador.
    // Estos valores afectan la rotación del modelo del jugador.
    // Player model rotation speed representa la velocidad a la que el modelo del jugador rota para alinearse con la dirección del movimiento.
    //    controla CUÁN rápido gira el modelo (suavizado). Se usa en RotatePlayerToTarget con Quaternion.Lerp(..., playerModelRotationSpeed * Time.deltaTime). Valor mayor → giro más rápido; valor pequeño → giro más lento y suave.
    public float playerModelRotationSpeed = 10f;
    // rotateToTargetTime representa el tiempo que tarda el jugador en rotar para alinearse con la dirección de la cámara cuando está inactivo.
    //    rotateToTargetTime: tiempo en segundos que dura el temporizador _rotatingToTargetTimer cuando se inicia la alineación con la cámara estando en idle. Determina CUÁNTO tiempo se mantiene la rotación automática tras detectarse una discrepancia grande de ángulo.
    public float rotateToTargetTime = 0.67f;

    [Header("Camera Settings")]
    // Parámetros relacionados con la cámara del jugador.
    // Los valores afectan la sensibilidad y los límites de rotación de la cámara.
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Environment Details")]
    // Máscara de capas para detectar las superficies del suelo.
    [SerializeField] private LayerMask _groundLayers;
    // instancia de PlayerLocomotionInput para acceder a las entradas del jugador.
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private bool _jumpedLastFrame = false;
    private bool _isRotatingClockwise = false;
    private float _rotatingToTargetTimer = 0f;
    private float _verticalVelocity = 0f;
    private float _antiBump;
    private float _stepOffset;

    private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
    #endregion

    #region Startup
    // Awake Inicializa las referencias a los componentes necesarios.
    // Se llama una vez al inicio del juego.
    // Aquí se obtienen las referencias a los componentes PlayerLocomotionInput y PlayerState,
    // y se inicializan las variables relacionadas con el movimiento vertical.
    private void Awake()
    {
        // Obtener referencias a los componentes PlayerLocomotionInput y PlayerState.
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        //PlayerState es el script que maneja los estados del jugador (correr, saltar, caer, etc).
        _playerState = GetComponent<PlayerState>();
        // Inicializar variables relacionadas con el movimiento vertical.
        // Configura el valor anti-bump para evitar que el jugador se quede atascado en superficies 
        // irregulares al aterrizar.
        _antiBump = sprintSpeed;
        // Configura el valor stepOffset para permitir que el jugador suba pequeños escalones. en base a lo que tenga el character controller.
        _stepOffset = _characterController.stepOffset;
    }
    #endregion

    #region Update Logic
    // Update se llama una vez por frame.
    // Aquí se actualizan el estado de movimiento del jugador y se manejan los movimientos verticales y laterales.

    private void Update()
    {
        // Actualiza el estado de movimiento del jugador basado en las entradas y condiciones actuales.
        UpdateMovementState();
        // Debug.Log 
        print(_characterController.velocity);
        // Maneja el movimiento vertical del jugador, incluyendo la gravedad y el salto.
        HandleVerticalMovement();
        // Maneja el movimiento lateral del jugador basado en las entradas de movimiento.
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        // Guardar el último estado de movimiento para comparaciones futuras. Utilizando la propiedad CurrentPlayerMovementState de PlayerState.
        _lastMovementState = _playerState.CurrentPlayerMovementState;
        // Determinar si el jugador puede correr basado en la dirección de la entrada de movimiento.
        bool canRun = CanRun();
        // Determinar el estado de movimiento lateral basado en las entradas del jugador.
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;             //order
        // Determinar si el jugador se está moviendo lateralmente.
        bool isMovingLaterally = IsMovingLaterally();                                            //matters
        // Determinar si el jugador está esprintando. basado en la entrada de sprint y si se está moviendo lateralmente.
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;          //order
        // Determinar si el jugador está caminando. basado en si se está moviendo lateralmente,
        // y (si no puede correr o si la entrada de caminata está activada).
        bool isWalking = isMovingLaterally && (!canRun || _playerLocomotionInput.WalkToggledOn); //matters
        // Determinar si el jugador está en el suelo.
        bool isGrounded = IsGrounded();
        // Actualizar el estado de movimiento del jugador basado en las condiciones actuales.
        // Para priorizar los estados:
        // 1) Si está esprintando, establecer el estado a Sprinting.
        // 2) Si está caminando, establecer el estado a Walking.
        // 3) Si se está moviendo lateralmente o hay entrada de movimiento, establecer el estado a Running.
        // 4) Si no se está moviendo, establecer el estado a Idling.
        // Esto asegura que el estado de movimiento refleje con precisión las acciones del jugador.
        PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                           isSprinting ? PlayerMovementState.Sprinting :
                                           isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;
        // Finalmente, actualizar el estado de movimiento del jugador.
        // El método SetPlayerMovementState maneja la transición al nuevo estado.
        _playerState.SetPlayerMovementState(lateralState);

        // Control Airborn State
        // Si el jugador no está en el suelo y no acaba de saltar, establecer el estado a Falling.
        // Esto evita que el jugador quede atrapado en el estado de salto cuando está cayendo.
        if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
        {
            // Si el jugador acaba de saltar, establecer el estado a Jumping.
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            // Esto asegura que el estado de movimiento refleje con precisión las acciones del jugador.
            //_jumpedLastFrame se usa para evitar que el estado de salto se sobrescriba inmediatamente al caer.
            _jumpedLastFrame = false;
            // Desactivar el stepOffset mientras está en el aire para evitar problemas de colisión.
            _characterController.stepOffset = 0f;
        }
        // Si el jugador no está en el suelo y no acaba de saltar, establecer el estado a Falling.
        else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
        {
            // Si el jugador está cayendo, establecer el estado a Falling.
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            // Esto asegura que el estado de movimiento refleje con precisión las acciones del jugador.
            //_jumpedLastFrame se usa para evitar que el estado de salto se sobrescriba inmediatamente al caer.
            _jumpedLastFrame = false;
            // Desactivar el stepOffset mientras está en el aire para evitar problemas de colisión.
            _characterController.stepOffset = 0f;
        }
        // Si el jugador no está en el suelo y no acaba de saltar, establecer el estado a Falling.
        else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
        {// Si el jugador está cayendo, establecer el estado a Falling.
            _characterController.stepOffset = _stepOffset;
        }
    }

    // Maneja el movimiento vertical del jugador, incluyendo la gravedad y el salto.
    // Este método ajusta la velocidad vertical del jugador basado en si está en el suelo, en el aire o saltando.
    private void HandleVerticalMovement()
    {
        // Crear una referencia rápida para el estado de suelo actual. Utilizando el método InGroundedState() de PlayerState.
        bool isGrounded = _playerState.InGroundedState();
        // Aplicar gravedad al jugador.
        _verticalVelocity -= gravity * Time.deltaTime;
        // Si el jugador está en el suelo y se está moviendo hacia abajo, 
        // aplicar un pequeño impulso hacia arriba
        // para evitar que se quede atascado al bajar por superficies irregulares.
        if (isGrounded && _verticalVelocity < 0)
        {
            // Antibump Impulso pequeño hacia arriba.
            _verticalVelocity = -_antiBump;
        }
        // Si el jugador ha presionado el botón de salto y está en el suelo,
        //  aplicar la velocidad de salto. para iniciar el salto.
        if (_playerLocomotionInput.JumpPressed && isGrounded)
        {
            // Aplicar velocidad de salto.
            // Explicacion de la operacion: Se utiliza la formula de la velocidad de salto
            // que es la raiz cuadrada de (2 * gravedad * altura deseada) en este caso el 3 representa la altura del salto.
            // jumpSpeed es la velocidad inicial del salto.y gravity es la aceleracion hacia abajo.
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * gravity * 3);
            // Reiniciar el estado de salto presionado.
            _jumpedLastFrame = true;
        }
        // Si el jugador estaba en un estado de suelo y ahora no lo está, 
        // aplicar el impulso anti-bump.
        // Para evitar que el jugador se quede atascado al caer desde superficies irregulares.
        if (_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
        {
            _verticalVelocity += _antiBump;
        }

        // Clamp at terminal velocity
        // Para evitar caídas excesivamente rápidas.
        // si la velocidad vertical absoluta excede la velocidad terminal, ajustarla a la velocidad terminal.
        if (Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
        {
            //Se pone -1 porque la velocidad terminal es hacia abajo (negativa).
            _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
            //todo: Si terminalVelocity se declara negativa, quitar el -1f
        }
    }
    // Maneja el movimiento lateral del jugador basado en las entradas de movimiento.
    // Al decir movimiento lateral, nos referimos al movimiento en el plano horizontal (ejes XZ),
    // Movimiento hacia adelante/atrás e izquierda/derecha.
    private void HandleLateralMovement()
    {
        // Create quick references for current state
        // Crear referencias rápidas para el estado actual.
        // Determinar si el jugador está esprintando, en el suelo y caminando.
        // isSprinting es true si el estado de movimiento actual es igual a el estado de esprintar.
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        // isGrounded es true si el jugador está en un estado de suelo.
        bool isGrounded = _playerState.InGroundedState();
        // isWalking es true si el estado de movimiento actual es igual a el estado de caminar.
        bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

        // State dependent acceleration and speed
        // Determinar la aceleración lateral y la velocidad máxima basadas en el estado actual del jugador.
        // Es decir, si el jugador está en el aire, caminando, esprintando o corriendo la velocidad y aceleracion cambian.
        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isWalking ? walkAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;

        // Determinar la velocidad máxima lateral basada en el estado actual del jugador.
        // Esto asegura que el jugador no exceda la velocidad permitida para su estado actual.
        float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                      isWalking ? walkSpeed :
                                      isSprinting ? sprintSpeed : runSpeed;
        // Explicación completa del cálculo del movimiento lateral del jugador.
        // Primero, se obtienen las direcciones XZ de la cámara para alinear el movimiento con la vista del jugador.
        // Esto con ayuda de:
        // La dirección hacia adelante de la cámara (en el plano XZ)
        // Para obtener la dirección hacia adelante de la cámara en el plano XZ,   
        // se crea un nuevo vector que toma las componentes X y Z de la dirección hacia adelante de la cámara,
        // y se establece la componente Y en 0. Luego, este vector se normaliza para obtener una dirección unitaria.
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        // Después se obtiene la dirección derecha de la cámara (en el plano XZ)
        // Similarmente, para obtener la dirección derecha de la cámara en el plano XZ,
        // se crea un nuevo vector que toma las componentes X y Z de la dirección derecha de la cámara,
        // se establece la componente Y en 0, y luego se normaliza.
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        // Luego, se calcula la dirección de movimiento combinando las entradas del jugador con las direcciones de la cámara.
        // La entrada de movimiento del jugador es un vector 2D (x, y),
        // donde 'x' representa el movimiento lateral (izquierda/derecha) y 'y' representa el movimiento hacia adelante/atrás.
        // Se multiplica la entrada 'x' por la dirección derecha de la cámara y la entrada 'y' por la dirección hacia adelante de la cámara,
        // y se suman ambos resultados para obtener la dirección de movimiento en 3D.
        Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;
        //Lo que hace esto es que si se presiona adelante (y positivo) y derecha (x positivo), el jugador se moverá en diagonal hacia adelante-derecha,
        //y la dirección de movimiento reflejará esa combinación de entradas.
        //Esto permite que el movimiento del jugador sea intuitivo y esté alineado con la perspectiva de la cámara


        // Aplicar aceleración basada en la entrada del jugador.
        // Se calcula el cambio de movimiento multiplicando la dirección de movimiento por la aceleración lateral
        // y el tiempo delta para obtener un valor de cambio consistente por frame.
        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        // Calcular la nueva velocidad sumando el cambio de movimiento a la velocidad actual del CharacterController.
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player
        // Aplicar drag (resistencia) al jugador para simular la fricción y evitar movimientos excesivos.
        // El drag se aplica en función de si el jugador está en el suelo o en el aire.
        float dragMagnitude = isGrounded ? drag : inAirDrag;
        // Calcular la cantidad de drag a aplicar en este frame.
        Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;
        // Restar el drag de la nueva velocidad.
        // Si la magnitud de la nueva velocidad es mayor que el drag, restar el drag.
        // Si no, establecer la velocidad a cero para evitar que se invierta el movimiento
        //Esto asegura que el jugador se desacelere suavemente cuando no hay entrada de movimiento.
        newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        // Clamp lateral velocity and reapply vertical velocity
        // Limitar la velocidad lateral a la velocidad máxima permitida para el estado actual del jugador.
        // Se crea un nuevo vector que toma las componentes X y Z de la nueva velocidad,
        // y establece la componente Y en 0 para aislar el movimiento lateral.
        // Luego, se utiliza Vector3.ClampMagnitude para limitar la magnitud de este vector a clampLateralMagnitude.
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
        // Reaplicar la velocidad vertical calculada previamente.
        newVelocity.y += _verticalVelocity;
        // Handle steep walls when in air
        // Manejar colisiones con paredes empinadas cuando el jugador está en el aire.
        // Si el jugador no está en el suelo, llamar a HandleSteepWalls para ajustar la velocidad
        // en función de las normales de las paredes empinadas.
        //si si está en el suelo, no hacer nada.
        // Esto evita que el jugador se quede atascado o atraviese paredes empinadas al caer.
        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

        // Move character (Unity suggests only calling this once per tick)
        // Mover el CharacterController utilizando la nueva velocidad calculada.
        // Se multiplica la nueva velocidad por el tiempo delta para obtener un valor consistente por frame
        _characterController.Move(newVelocity * Time.deltaTime);
    }
    // Maneja las colisiones con paredes empinadas cuando el jugador está en el aire.
    // Este método ajusta la velocidad del jugador para evitar que se quede atascado o atraviese paredes empinadas.
    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        // Realizar un sphere cast para obtener la normal de la superficie con la que el jugador está colisionando.
        // Utiliza el método GetNormalWithSphereCast de CharacterControllerUtils,
        // pasando el CharacterController y las capas de suelo relevantes.
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        // Calcular el ángulo entre la normal de la superficie y el vector hacia arriba.
        // Esto determina qué tan empinada es la superficie. Dandonos un valor de 0° para superficies planas y 90° para paredes verticales.
        // En trigonometría esto es el ángulo entre dos vectores.
        float angle = Vector3.Angle(normal, Vector3.up);
        // Determinar si el ángulo es mayor que el límite de pendiente del CharacterController.
        // Si el ángulo es mayor, significa que el jugador está colisionando con una pared empinada.
        bool validAngle = angle <= _characterController.slopeLimit;
        // Si el ángulo no es válido (es decir, es una pared empinada) y el jugador se está moviendo hacia abajo,
        // proyectar la velocidad del jugador en el plano definido por la normal de la superficie.
        // Esto ajusta la velocidad para evitar que el jugador se quede atascado o atraviese la pared.
        if (!validAngle && _verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);

        return velocity;
    }
    #endregion

    #region Late Update Logic
    // LateUpdate se llama una vez por frame, después de Update.
    // Aquí se actualiza la rotación de la cámara y del jugador.
    // Esto asegura que la cámara siga la rotación del jugador de manera suave y responsiva.
    private void LateUpdate()
    {
        UpdateCameraRotation();
    }
    // Actualiza la rotación de la cámara y del jugador basándose en la entrada del jugador.

    private void UpdateCameraRotation()
    {
        // _cameraRotation.x representa la rotación horizontal (yaw) de la cámara.
        // Actualizar la rotación horizontal de la cámara sumando la entrada horizontal del jugador CON la sensibilidad.
        // Esto hace que mover el ratón hacia la derecha gire la cámara hacia la derecha.
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        // _cameraRotation.y representa la rotación vertical (pitch) de la cámara.
        // Actualizar la rotación vertical de la cámara restando la entrada vertical del jugador CON la sensibilidad.
        // Esto hace que mover el ratón hacia arriba gire la cámara hacia abajo.
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);
        // Actualizar la rotación objetivo del jugador para que siga la rotación horizontal de la cámara.
        // Esto hace que el jugador gire para mirar en la misma dirección que la cámara.
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;
        // rotationTolerance define el umbral de discrepancia de rotación para iniciar la rotación automática.
        // Es decir el limite de angulo a partir del cual el jugador empieza a rotar automaticamente para alinear con la camara estando en idle.
        float rotationTolerance = 90f;
        //todo: ajustar rotationTolerance en el inspector
        // IsIdling es true si el jugador está en estado de idling.
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        // Actualizar el temporizador de rotación hacia el objetivo.
        // Si el temporizador es mayor que cero, decrementar el temporizador.
        // Esto controla cuánto tiempo el jugador seguirá rotando automáticamente hacia la cámara.
        IsRotatingToTarget = _rotatingToTargetTimer > 0;

        // ROTATE if we're not idling
        // Si el jugador no está en estado de idling, 
        // rotar el jugador hacia la dirección de la cámara.
        // Esto asegura que el jugador siempre siga la dirección de la cámara mientras se mueve.
        if (!isIdling)
        {
            //Mod Comentado para que el jugador no rote automaticamente al mover la camara cuando está en movimiento.
            //Mod Aunque puede ser modificado para que rote solo si se mueve mucho la camara
            //Mod Cambiando rotationTolerance a un valor bajo
            // Esta linea rota el jugador hacia la dirección de la cámara.
            //RotatePlayerToTarget();
        }
        // If rotation mismatch not within tolerance, or rotate to target is active, ROTATE
        // Si la discrepancia de rotación no está dentro del umbral de tolerancia,
        // o si la rotación hacia el objetivo está activa, actualizar la rotación en estado de idling.
        // esto hace que el jugador rote automáticamente para alinearse con la cámara cuando está inactivo.
        else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
        {
            UpdateIdleRotation(rotationTolerance);
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        // Get angle between camera and player
        Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
    }
    // Actualiza la rotación del jugador cuando está inactivo
    // es decir cuando no se está moviendo con la camara. 
    // en base a la discrepancia de rotación con la cámara.
    private void UpdateIdleRotation(float rotationTolerance)
    {
        // Si la diferencia de rotación con la cámara supera el umbral, iniciar rotación automática.
        if (Mathf.Abs(RotationMismatch) > rotationTolerance)
        {
            // Activar temporizador de rotación.
            _rotatingToTargetTimer = rotateToTargetTime;

            // Determinar dirección: horario si la discrepancia es positiva.
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }

        // Reducir el tiempo restante de rotación automática.
        _rotatingToTargetTimer -= Time.deltaTime;

        // Aplicar rotación solo si la dirección coincide con la discrepancia.
        if (_isRotatingClockwise && RotationMismatch > 0f ||
            !_isRotatingClockwise && RotationMismatch < 0f)
        {
            // RotatePlayerToTarget(); 
            //mod Comentado para desactivar rotación automática. cuando está en idle.
        }
    }

    // Rota el jugador suavemente hacia la dirección objetivo basada en la cámara.
    private void RotatePlayerToTarget()
    {
        // Calcular la rotación objetivo en el eje Y basada en la rotación objetivo del jugador.
        // Luego, interpolar suavemente la rotación actual del jugador hacia la rotación objetivo.
        // Esto permite que el jugador gire suavemente hacia la dirección de la cámara.
        // Cuando se llama esta función, el jugador gira para alinearse con la cámara.
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }
    #endregion

    #region State Checks
    //Esta funcion determina si el jugador se está moviendo lateralmente basado en su velocidad actual. 
    //Se utiliza para decidir si el jugador puede caminar, correr o esprintar.
    private bool IsMovingLaterally()
    {// Opcion1
     // Obtiene la velocidad lateral del jugador (ignorando la componente vertical).
     // Crea un nuevo vector que toma las componentes X y Z de la velocidad del CharacterController,
     // y establece la componente Y en 0 para aislar el movimiento lateral.
     // Luego, calcula la magnitud de este vector para determinar qué tan rápido se está moviendo lateralmente.
     //Esto permite detectar el movimiento lateral inmediatamente basado en la entrada del jugador,
     //en lugar de esperar a que la velocidad física del CharacterController cambie.
     //Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
     // Compara la magnitud de la velocidad lateral con un umbral para determinar si el jugador se está moviendo.
     //Esto es útil para decidir si el jugador puede caminar, correr o sprintar.
     //return lateralVelocity.magnitude > movingThreshold;
     //Opcion2
     // Usa la entrada en lugar de la velocidad para detectar movimiento lateral inmediatamente
     // return _playerLocomotionInput.MovementInput.magnitude > movingThreshold;
     //Opcion3
     // Combina ambas opciones:
     //Lo que hace esta función es determinar si el jugador se está moviendo lateralmente utilizando dos criterios:
     // 1) input inmediato:
        bool inputMove = _playerLocomotionInput.MovementInput.magnitude > movingThreshold;

        // 2) movimiento real (ignorar componente vertical):
        Vector3 lateralVel = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        bool actualMove = lateralVel.magnitude > movingThreshold;

        // 3) combinar: true si hay intención O si ya hay velocidad
        return inputMove || actualMove;

    }
    //Esta funcion determina si el jugador está en el suelo utilizando diferentes métodos según su estado actual. 
    //Esto es importante para manejar correctamente el movimiento y las transiciones de estado del jugador.
    private bool IsGrounded()
    {
        // Verifica si el jugador está en el suelo utilizando diferentes métodos según su estado actual.
        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

        return grounded;
    }
    //Esta funcion determina si el jugador está en el suelo cuando ya está en un estado de suelo. 
    //Utiliza una comprobación de esfera para detectar colisiones con el suelo.
    // Esto es útil para mantener la precisión del estado de suelo cuando el jugador ya está en el suelo.
    // Se realiza una comprobación de esfera en la posición del jugador, ajustada por el radio del CharacterController,
    // para detectar si hay colisiones con las capas de suelo relevantes.
    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);
        return grounded;
    }
//Esta funcion determina si el jugador está en el suelo cuando está en el aire. 
    //Utiliza un SphereCast para detectar colisiones con el suelo.
    // Esto es útil para detectar cuándo el jugador aterriza después de estar en el aire.
    // Se realiza un SphereCast desde el centro del CharacterController hacia abajo,
    // utilizando el radio del CharacterController y la altura ajustada por el stepOffset,
    // para detectar si hay colisiones con las capas de suelo relevantes.
    private bool IsGroundedWhileAirborne()
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;
        return _characterController.isGrounded && validAngle;
    }
    //Esta funcion determina si el jugador puede correr basado en la entrada de movimiento actual. 
    //Lo que 
    private bool CanRun()
    {
        // Esta linea verifica si la componente Y (adelante/atrás) de la entrada de movimiento es mayor o igual que la componente X (izquierda/derecha).
        // Lo que permite correr hacia adelante o hacia atrás, pero no lateralmente.
        //return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);

        //Esta linea verifica si la magnitud absoluta de la componente Y es mayor o igual que la magnitud absoluta de la componente X.
        //Lo que permite correr hacia adelante, atrás o lateralmente, siempre que la componente Y tenga mayor o igual magnitud que la componente X.
        //es decir siempre que el jugador se mueva más en la dirección adelante/atrás que en la dirección izquierda/derecha.
        return Mathf.Abs(_playerLocomotionInput.MovementInput.y) >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);

    }
    #endregion
}
