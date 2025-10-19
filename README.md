# Demo 1.3 — Documentación de scripts

Este README describe los scripts incluidos en `Assets/Miprimerpersonaje/FinalCharacterController`.

Incluye:

- Resumen de cada archivo .cs
- Lista de clases, variables y métodos principales
- Diagramas UML (PlantUML) por clase para visualizar relaciones y llamadas

Nota: Los diagramas están en formato PlantUML dentro de bloques de código ```plantuml``` para que puedas copiarlos a un archivo .puml y renderizarlos con PlantUML.

## Contrato breve (inputs/outputs/errores)

- Input: Componentes Unity (CharacterController, Animator, Camera, Input System)
- Output: Estados y comportamientos del personaje (movimiento, animación, input)
- Modo de error: si `PlayerInputManager.Instance` no está inicializado los componentes de input muestran errores en consola y no se habilitan los mapas.

## Scripts principales

### PlayerState (Assets/Miprimerpersonaje/FinalCharacterController/Scripts/PlayerState.cs)

- Propósito: Mantener el estado de movimiento del jugador (Idling, Walking, Running, ...)
- Clases/Tipos: `PlayerState` (MonoBehaviour), `PlayerMovementState` (enum)
- Miembros relevantes:
  - field: `PlayerMovementState CurrentPlayerMovementState { get; private set; }`
  - `SetPlayerMovementState(PlayerMovementState)`
  # Mi Proyecto - 1st & 3rd Person Controller (Parte 1 + Documentación)

  Hola — soy el desarrollador detrás de este proyecto. Este repositorio contiene la primera parte de un controlador que permite jugar en primera y tercera persona (1st & 3rd Person Controller). Aquí además encontrarás documentación técnica de los scripts incluidos en la demo.

  ---

  ## Descripción breve

  - Proyecto: 1st and 3rd Person Controller (Parte 1).
  - Contenido: scripts, escenas y assets necesarios para el controlador de personaje (movimiento, animaciones y prefabs).

  Este README combina la explicación de alto nivel (qué es el proyecto y cómo usarlo) con una guía técnica de los scripts principales y diagramas en formato PlantUML para facilitar la lectura y generación de diagramas.

  ## Qué incluye (resumen)

  - Carpeta `Assets/PlayerController` con scripts, animaciones y prefabs del personaje.
  - `ProjectSettings/` con la configuración del proyecto Unity usada para esta demo.
  - `.gitattributes` y `.gitignore` configurados para Unity y Git LFS (se han trackeado ficheros binarios comunes como `.fbx`, `.png`, `.wav`, etc.).
  - Documentación técnica (secciones siguientes) con resúmenes de clases y diagramas PlantUML.

  ## Inspiración y créditos

  - Inspirado por tutoriales y recursos públicos que enseñan controladores de personaje en Unity.
  - Referencia principal:

      [Tutorial (YouTube)](https://www.youtube.com/watch?v=SwWZ-pklT9I&list=PLYvjPIZvaz-o-DIBhiHzSrrau9HKSmeEz&index=1)

  ---

  ## Cómo abrir y ejecutar la demo (instalación rápida)

  1. Abre Unity Hub y añade la carpeta del proyecto: `Demo 1.3.1`.
  2. Abre el proyecto con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt` (recomendado usar la misma versión para evitar problemas de migración).
  3. Si usas Git: este repo está preparado para Git LFS; instala y autentica Git LFS en tu equipo si aún no lo tienes:

     - Windows (PowerShell):

       ```powershell
       git lfs install --local
       ```

  4. Abre la escena principal en `Assets/Scenes/Example_Scene.unity` y presiona Play.

  Notas:
  - `Library/` es una carpeta generada por Unity y está ignorada por `.gitignore`.
  - Si hay assets grandes que no subieron correctamente, asegúrate de tener Git LFS instalado y que tu cuenta de GitHub tenga cuota suficiente.

  ---

  ## Estructura de carpetas (resumen)

  # Demo 1.3 — Documentación de scripts

  Este README describe los scripts incluidos en `Assets/Miprimerpersonaje/FinalCharacterController` y proporciona la documentación técnica del controlador de personaje.

  Incluye:

  - Resumen de los scripts principales
  - PlantUML fuente en `Docs/diagrams/*.puml`
  - Diagrama UML (PNG) generado automáticamente en `Docs/diagrams/*.png`

  Nota: las imágenes (PNG) se generan automáticamente mediante un GitHub Action cuando se actualizan los archivos `.puml`. Si no ves las imágenes de inmediato, espera unos minutos o vuelve a cargar la página del repositorio en GitHub.

  ## Contrato breve (inputs/outputs/errores)

  - Input: Componentes Unity (CharacterController, Animator, Camera, Input System)
  - Output: Estados y comportamientos del personaje (movimiento, animación, input)
  - Modo de error: si `PlayerInputManager.Instance` no está inicializado los componentes de input muestran errores en consola y no se habilitan los mapas.

  ## Cómo abrir y ejecutar la demo (instalación rápida)

  1. Abre Unity Hub y añade la carpeta del proyecto: `Demo 1.3.1`.
  2. Abre el proyecto con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt`.
  3. Instala Git LFS si vas a clonar/editar el repositorio (recomendado):

     ```powershell
     git lfs install --local
     ```

  4. Abre la escena principal `Assets/Scenes/Example_Scene.unity` y presiona Play.

  ---

  ## Estructura de carpetas (resumen)

  - Assets/
    - PlayerController/
      - Animations/
      - Input/
      - Model/
      - Prefabs/
      - Presets/
      - Scripts/
  - Docs/diagrams/  (PlantUML .puml + imágenes generadas)
  - ProjectSettings/

  ---

  ## Scripts principales (resumen técnico)

  - PlayerState: mantiene el estado de movimiento del jugador (Idling, Walking, Running, Sprinting, Jumping, Falling, Strafing).
  - PlayerController: lógica de locomoción, salto, gravedad, rotación y control de cámara.
  - PlayerAnimation: actualización de parámetros del `Animator` en base al estado y el input.
  - CharacterControllerUtils: utilidades para detección de suelo y normales.
  - Input: `PlayerLocomotionInput`, `PlayerActionsInput`, `ThirdPersonInput`, `PlayerInputManager` (wrappers del Input System).

  Para ver la descripción detallada por cada clase y los diagramas, consulta la sección de Diagramas abajo.

  ---

  ## Diagramas (imágenes generadas)

  > Si las imágenes no aparecen aún, espera un par de minutos: el workflow de GitHub genera las PNG automáticamente cuando hay cambios en los `.puml`.

  ### PlayerState

  ![PlayerState diagram](Docs/diagrams/playerstate.png)

  ### PlayerController

  ![PlayerController diagram](Docs/diagrams/playercontroller.png)

  ### PlayerAnimation

  ![PlayerAnimation diagram](Docs/diagrams/playeranimation.png)

  ### CharacterControllerUtils

  ![CharacterControllerUtils diagram](Docs/diagrams/charactercontrollerutils.png)

  ### Input / Manager

  ![Input diagram](Docs/diagrams/input.png)

  ### Diagrama global

  ![Global class diagram](Docs/diagrams/global_class_diagram.png)

  ### Mapa mental

  ![Mindmap](Docs/diagrams/mindmap.png)

  ---

  ## PlantUML fuente

  Los archivos `.puml` están en `Docs/diagrams/`. Si quieres regenerar las imágenes localmente puedes usar PlantUML (Java/plantuml.jar) o Docker con la imagen `plantuml/plantuml`.

  Ejemplo (Docker):

  ```powershell
  docker run --rm -v "${PWD}:/workspace" plantuml/plantuml -tpng /workspace/Docs/diagrams/*.puml
  ```

  ---

  ## Roadmap y colaboración

  - Parte 2: interacción y combate.
  - Abre un issue para proponer cambios grandes o crea una PR desde un fork.

  ---

  ## Contacto

  - Perfil: https://github.com/LechuDev

  Gracias por revisar el proyecto — si quieres que genere las imágenes por mí (usando Docker aquí) o que espere a que el workflow genere las PNG en GitHub, dime y procedo.


