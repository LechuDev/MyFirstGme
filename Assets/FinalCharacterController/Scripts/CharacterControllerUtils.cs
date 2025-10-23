using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    // Utilidades relacionadas con CharacterController.
    // Este archivo contiene funciones auxiliares que trabajan directamente con
    // un CharacterController para facilitar operaciones frecuentes como muestrear
    // la normal de la superficie bajo el personaje.
    public static class CharacterControllerUtils
{
        // Obtiene la normal de la superficie que está justo por debajo del CharacterController
        // usando un SphereCast. Se usa el radio del CharacterController como radio del sphere
        // y se lanza desde el centro (teniendo en cuenta el offset 'center') hacia abajo.
        // 
        // Parámetros:
        //  - characterController: instancia del CharacterController que se desea muestrear.
        //  - layerMask: (opcional) máscara de capas para filtrar qué colisiones considerar.
        //
        // Retorno:
        //  - Devuelve la normal del hit detectado. Si no se detecta ninguna colisión dentro
        //    de la distancia calculada, devuelve Vector3.up como normal por defecto.
        //
        // Notas de uso:
        //  - Ideal para alinear el movimiento con la pendiente del terreno o para calcular
        //    la inclinación del suelo en la lógica de movimiento.
        //  - No modifica el estado del CharacterController; sólo realiza una consulta.
        
        public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layerMask = default)
        {
        Vector3 normal = Vector3.up;
        // Calcular el centro del CharacterController teniendo en cuenta su offset 'center'.
        Vector3 center = characterController.transform.position + characterController.center;
        // Calcular la distancia del sphere cast utilizando la altura y el stepOffset del CharacterController. 
        float distance = characterController.height / 2f + characterController.stepOffset + 0.01f;
        // Realiza el SphereCast hacia abajo desde el centro del CharacterController.
        //que es una especie de rayo con grosor.
        // El SphereCast detecta colisiones con el suelo u otras superficies debajo del personaje.
        RaycastHit hit;
            // Si el SphereCast golpea algo dentro de la distancia especificada y en las capas indicadas,
            // actualiza la normal con la normal del hit detectado. sino devuelve Vector3.up.
            if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask))
            {
                normal = hit.normal;
            }

            return normal;
        }
    }