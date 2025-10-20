using UnityEngine;

// Clase de utilidades para trabajar con CharacterController
public class CharacterControllerUtils : MonoBehaviour
{
    // Método estático que obtiene la normal del suelo debajo del personaje usando un SphereCast
    // Parámetros:
    // - characterController: el componente CharacterController del personaje
    // - layerMask: opcional, sirve para filtrar qué capas serán detectadas por el SphereCast (por ejemplo, ignorar "Player" y solo detectar "Ground")
    // Devuelve:
    // - Un Vector3 que representa la normal de la superficie detectada
    // - Si no hay colisión, devuelve Vector3.up (es decir, (0, 1, 0), que representa una superficie plana hacia arriba)
    public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layerMask = default)
    {
        // Valor por defecto: si no hay colisión, asumimos que la normal es hacia arriba
        // Vector3.up es un vector unitario que apunta en dirección vertical positiva (0, 1, 0)
        Vector3 normal = Vector3.up;

        // Calculamos el centro del CharacterController en el mundo
        // characterController.center es una posición local (relativa al objeto)
        // Por eso se suma a transform.position, que es la posición global del GameObject
        Vector3 center = characterController.transform.position + characterController.center;

        // Calculamos la distancia del SphereCast hacia abajo
        // characterController.height / 2f: desde el centro hasta la base del collider
        // stepOffset: distancia que el personaje puede subir automáticamente (como un escalón)
        // Se resta para evitar que el SphereCast se detenga antes de tocar el suelo
        // Se suma 0.1f como margen para asegurar que el cast llegue al suelo
        float distance = characterController.height / 2f - characterController.stepOffset + 0.1f;

        // RaycastHit es una estructura que almacena información sobre el impacto del cast:
        // - Punto de impacto
        // - Normal de la superficie
        // - Collider golpeado
        // - Distancia, etc.
        RaycastHit hit;

        // Physics.SphereCast lanza un "cast" en forma de esfera desde un punto en una dirección
        // Parámetros:
        // - center: punto de origen del cast
        // - characterController.radius: radio de la esfera (igual al del collider del personaje)
        // - Vector3.down: dirección del cast (hacia abajo)
        // - out hit: variable donde se guarda la información del impacto
        // - distance: distancia máxima del cast
        // - layerMask: capas que serán consideradas en la colisión
        if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask))
        {
            // Si hubo colisión, usamos la normal de la superficie golpeada
            normal = hit.normal;
        }

        // Retornamos la normal detectada (o Vector3.up si no hubo colisión)
        return normal;
    }
}
