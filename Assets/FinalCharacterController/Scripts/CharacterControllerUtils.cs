// Importa las librerías necesarias de C# y Unity.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    // RESUMEN DE LA CLASE: CharacterControllerUtils
    // Esta es una clase estática, lo que significa que no necesita ser instanciada en un GameObject.
    // Contiene métodos de utilidad (funciones auxiliares) que extienden la funcionalidad del componente
    // 'CharacterController' de Unity. Su propósito es encapsular lógica compleja o repetitiva
    // relacionada con el CharacterController en un solo lugar, para que pueda ser reutilizada
    // fácilmente por otros scripts, como el PlayerController.
    public static class CharacterControllerUtils
    {        
        // MÉTODO: GetNormalWithSphereCast
        // Propósito: Obtiene la normal de la superficie directamente debajo del personaje.
        // Cómo funciona: Lanza un rayo grueso (SphereCast) hacia abajo desde el centro del CharacterController.
        // Esto es más robusto que un Raycast simple, ya que es menos propenso a fallar en bordes o terrenos irregulares.
        // Se usa actualmente en: PlayerController, en el método 'IsGrounded' y 'HandleSteepWalls' para detectar la inclinación del suelo.
        public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layerMask = default)
        {
        // Inicializa la normal a 'Vector3.up' (apuntando hacia arriba). Este es el valor por defecto si no se encuentra suelo.
        Vector3 normal = Vector3.up;
        // Calcula la posición del centro del CharacterController en el mundo, sumando el offset 'center' a la posición del transform.
        Vector3 center = characterController.transform.position + characterController.center;
        // Calcula la distancia máxima del SphereCast. Es la mitad de la altura del personaje más su 'stepOffset',
        // con un pequeño extra para asegurar que detecte el suelo incluso si está ligeramente separado.
        float distance = characterController.height / 2f + characterController.stepOffset + 0.01f;
        
        // Variable para almacenar la información de la colisión si el SphereCast tiene éxito.
        RaycastHit hit;
            // Lanza el SphereCast.
            // Parámetros: origen, radio, dirección, variable de salida, distancia máxima, máscara de capas.
            // Si el SphereCast golpea un colisionador que pertenece a la 'layerMask'...
            if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask))
            {
                // ...actualiza la variable 'normal' con la normal de la superficie en el punto de impacto.
                normal = hit.normal;
            }

            // Devuelve la normal encontrada, o 'Vector3.up' si no se encontró nada.
            return normal;
        }
    }