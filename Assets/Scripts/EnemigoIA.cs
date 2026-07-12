using UnityEngine;
using UnityEngine.AI;

// IA del enemigo: persigue al jugador con NavMeshAgent y le dispara cuando lo ve de cerca
public class EnemigoIA : MonoBehaviour
{
    public float distanciaAtaque = 8f;
    public int dano = 1;
    public float cadencia = 1.8f;
    public float alcance = 30f;
    public AudioClip sonidoDisparo;

    private NavMeshAgent agente;
    private Transform jugador;
    private float proximo = 0f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        GameObject j = GameObject.FindGameObjectWithTag("Player");
        if (j != null) jugador = j.transform;
    }

    void Update()
    {
        if (jugador == null || !agente.isOnNavMesh) return;

        // Si la partida termino, el enemigo se queda quieto
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.Terminado())
        {
            agente.isStopped = true;
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaAtaque && PuedeVerAlJugador())
        {
            // Esta cerca y lo ve: se detiene y dispara
            agente.isStopped = true;
            if (Time.time >= proximo)
            {
                proximo = Time.time + cadencia;
                DispararAlJugador();
            }
        }
        else
        {
            // Lo persigue: el NavMeshAgent calcula solo el camino por el laberinto
            agente.isStopped = false;
            agente.SetDestination(jugador.position);
        }
    }

    bool PuedeVerAlJugador()
    {
        Vector3 origen = transform.position + Vector3.up * 1.4f;
        Vector3 objetivo = jugador.position + Vector3.up * 0.6f;
        Vector3 direccion = (objetivo - origen).normalized;

        if (Physics.Raycast(origen, direccion, out RaycastHit hit, alcance))
        {
            Vida v = hit.collider.GetComponentInParent<Vida>();
            return v != null && v.esJugador;
        }
        return false;
    }

    void DispararAlJugador()
    {
        if (sonidoDisparo != null) AudioSource.PlayClipAtPoint(sonidoDisparo, transform.position, 0.5f);

        Vector3 origen = transform.position + Vector3.up * 1.4f;
        Vector3 objetivo = jugador.position + Vector3.up * 0.6f;
        Vector3 direccion = (objetivo - origen).normalized;

        if (Physics.Raycast(origen, direccion, out RaycastHit hit, alcance))
        {
            Vida v = hit.collider.GetComponentInParent<Vida>();
            if (v != null && v.esJugador) v.RecibirDano(dano);
        }
    }
}
