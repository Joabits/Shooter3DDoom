using UnityEngine;

// Botiquin: cura al jugador al tocarlo (sin pasar del maximo) y desaparece con un sonido
public class Botiquin : MonoBehaviour
{
    public int curacion = 2;
    public float velocidadGiro = 80f;
    public AudioClip sonidoRecoger;

    void Update()
    {
        // Gira despacio para que se vea que es un objeto recogible
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        Vida v = other.GetComponentInParent<Vida>();
        if (v == null || !v.esJugador) return;

        // Si ya tiene la vida al maximo no se consume
        if (v.VidaActual() >= v.vidaMax)
        {
            if (GestorJuego.Instancia != null)
                GestorJuego.Instancia.MostrarAviso("Vida al maximo", 1.5f);
            return;
        }

        v.Curar(curacion);
        if (sonidoRecoger != null) AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
        Destroy(gameObject);
    }
}
