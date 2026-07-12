using UnityEngine;

// Hace que el sprite del enemigo mire siempre hacia la camara del jugador (estilo Doom)
public class EnemigoBillboard : MonoBehaviour
{
    private Camera camaraJugador;

    void Start()
    {
        camaraJugador = Camera.main;
    }

    void LateUpdate()
    {
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
            return;
        }

        // Gira solo en horizontal para que el sprite no se incline
        Vector3 direccion = transform.position - camaraJugador.transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direccion);
    }
}
