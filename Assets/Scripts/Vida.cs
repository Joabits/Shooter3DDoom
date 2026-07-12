using UnityEngine;

public class Vida : MonoBehaviour
{
    public int vidaMax = 3;
    public bool esJugador = false;
    public AudioClip sonidoDano;
    private int vidaActual;

    void Awake()
    {
        vidaActual = vidaMax;
    }

    public void RecibirDano(int cantidad)
    {
        if (vidaActual <= 0) return; // ya estaba muerto

        vidaActual -= cantidad;
        if (sonidoDano != null) AudioSource.PlayClipAtPoint(sonidoDano, transform.position);

        if (esJugador && GestorJuego.Instancia != null)
            GestorJuego.Instancia.JugadorDanado(); // parpadeo rojo + HUD

        if (vidaActual <= 0) Morir();
    }

    public void Curar(int cantidad)
    {
        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMax); // tope en el maximo
        if (esJugador && GestorJuego.Instancia != null)
            GestorJuego.Instancia.ActualizarHUD();
    }

    void Morir()
    {
        if (esJugador)
        {
            // En vez de recargar directo, se muestra el menu de Game Over
            if (GestorJuego.Instancia != null) GestorJuego.Instancia.GameOver();
        }
        else
        {
            if (GestorJuego.Instancia != null) GestorJuego.Instancia.EnemigoEliminado();
            Destroy(gameObject);
        }
    }

    public int VidaActual()
    {
        return vidaActual;
    }
}
