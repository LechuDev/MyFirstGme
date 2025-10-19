# 🎮 ¡Bienvenido a tu personaje en Unity! — Demo 1.3

¡Hola! 👋 Este proyecto te permite controlar un personaje en primera y tercera persona dentro de Unity. Es como tener tu propio héroe que camina, corre, salta y se mueve con estilo. Si estás empezando en Unity o simplemente quieres divertirte, ¡este proyecto es para ti!

---

## 🧩 ¿Qué hay en este proyecto?

- Un personaje que puedes mover con el teclado o control.
- Animaciones que hacen que se vea más real.
- Una escena lista para jugar.
- Archivos organizados para que no te pierdas.
- Diagramas para entender cómo funciona todo (¡si te gusta aprender más!).

---

## 🚀 ¿Cómo lo uso?

1. Abre Unity Hub.
2. Agrega la carpeta del proyecto: `Demo 1.3.1`.
3. Usa la misma versión de Unity que aparece en `ProjectSettings/ProjectVersion.txt` (¡así todo funciona sin errores!).
4. Abre la escena principal: `Assets/Scenes/Example_Scene.unity`.
5. Presiona el botón de Play ▶️ ¡y empieza a jugar!

---

## 🛠️ ¿Qué necesito?

- Unity instalado en tu computadora.
- Git LFS (solo si vas a editar o subir el proyecto a GitHub).
  - Para instalarlo en Windows, abre PowerShell y escribe:

    ```powershell
    git lfs install --local
    ```

---

## 📁 ¿Dónde están las cosas?

- `Assets/PlayerController/` → Aquí están los scripts, animaciones y prefabs.
- `Docs/diagrams/` → Aquí están los diagramas que explican cómo funciona el código.
- `ProjectSettings/` → Configuración del proyecto.

---

## 💡 ¿Quién hizo esto?

Este proyecto fue creado por [LechuDev](https://github.com/LechuDev), inspirado en tutoriales públicos como este:

🎥 [Tutorial en YouTube](https://www.youtube.com/watch?v=SwWZ-pklT9I&list=PLYvjPIZvaz-o-DIBhiHzSrrau9HKSmeEz&index=1)

---

## 📚 ¿Quieres aprender más?

Si te interesa cómo funciona el código por dentro, sigue leyendo. Aquí viene la parte técnica con diagramas para programadores y curiosos 🧠👇

---

# 🧠 Documentación técnica — Scripts y diagramas

Este proyecto incluye scripts en `Assets/Miprimerpersonaje/FinalCharacterController` que controlan el movimiento, animación e input del personaje.

### 🔄 Contrato breve (inputs/outputs/errores)

- **Input:** Componentes Unity como `CharacterController`, `Animator`, `Camera`, `Input System`.
- **Output:** Estados del personaje (caminar, correr, saltar, etc.).
- **Errores:** Si `PlayerInputManager.Instance` no está activo, el input no funciona y verás errores en consola.

---

## 📜 Scripts principales

- **PlayerState:** Guarda el estado del personaje (Idle, Walking, Running...).
- **PlayerController:** Controla cómo se mueve, salta y rota el personaje.
- **PlayerAnimation:** Actualiza el `Animator` según el estado y el input.
- **CharacterControllerUtils:** Ayuda a detectar el suelo y las superficies.
- **Input:** Incluye `PlayerLocomotionInput`, `PlayerActionsInput`, `ThirdPersonInput`, `PlayerInputManager`.

---

## 🖼️ Diagramas UML

Estos diagramas te ayudan a entender cómo se conectan las clases y métodos. Están en formato PNG y también puedes usar los archivos `.puml` si quieres generar tus propios diagramas con PlantUML.

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
![Global class diagram](Docs/diagrams/GlobalClasses.png)

### Mapa mental  
![Mindmap](Docs/diagrams/mindmap.png)

---

## 🧵 ¿Quieres los diagramas en otro formato?

Si prefieres los diagramas en SVG para que se vean mejor en la web, o quieres los archivos `.puml` para editarlos, ¡dímelo y los preparo!

---

Gracias por visitar este proyecto. ¡Diviértete creando y aprendiendo! 🎉

--- 

¿Te gustaría que también lo traduzca al inglés para que sea bilingüe en GitHub?
