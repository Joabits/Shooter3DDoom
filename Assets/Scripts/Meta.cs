using UnityEngine;

// Zona de meta: termina el nivel solo si ya no quedan enemigos (condicion doble de victoria)
public class Meta : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Vida v = other.GetComponentInParent<Vida>();
        if (v == null || !v.esJugador || GestorJuego.Instancia == null) return;

        if (GestorJuego.Instancia.QuedanEnemigos())
            GestorJuego.Instancia.MostrarAviso("¡Elimina a todos los enemigos antes de escapar!", 3f);
        else
            GestorJuego.Instancia.Victoria();
    }
}
