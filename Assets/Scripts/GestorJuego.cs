using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el estado de la partida: HUD, contador de enemigos, victoria, game over y reinicio
public class GestorJuego : MonoBehaviour
{
    public static GestorJuego Instancia;

    [Header("Referencias del jugador")]
    public Vida vidaJugador;
    public Disparar armaJugador;

    [Header("HUD")]
    public Text textoVida;
    public Text textoMunicion;
    public Text textoEnemigos;
    public Text textoAviso;
    public Image pantallaDano;
    public GameObject panelGameOver;
    public GameObject panelVictoria;

    [Header("Sonido")]
    public AudioClip sonidoVictoria;

    private int enemigosRestantes;
    private float finAviso;
    private bool terminado = false;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        enemigosRestantes = FindObjectsByType<EnemigoIA>(FindObjectsSortMode.None).Length;
        ActualizarHUD();
    }

    void Update()
    {
        // Desvanecer el parpadeo rojo de dano
        if (pantallaDano != null && pantallaDano.color.a > 0f)
        {
            Color c = pantallaDano.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.2f);
            pantallaDano.color = c;
        }

        // Ocultar el aviso cuando pasa su tiempo
        if (textoAviso != null && textoAviso.enabled && Time.unscaledTime > finAviso)
            textoAviso.enabled = false;
    }

    public void ActualizarHUD()
    {
        if (textoVida != null && vidaJugador != null)
            textoVida.text = "Vida: " + Mathf.Max(0, vidaJugador.VidaActual()) + "/" + vidaJugador.vidaMax;

        if (textoMunicion != null && armaJugador != null)
        {
            if (armaJugador.Recargando())
                textoMunicion.text = "Recargando " + armaJugador.NombreArma() + "...";
            else if (armaJugador.BalasActuales() <= 0)
                textoMunicion.text = armaJugador.NombreArma() + ": 0/" + armaJugador.BalasMax() + "  [pulsa R]";
            else
                textoMunicion.text = armaJugador.NombreArma() + ": " + armaJugador.BalasActuales() + "/" + armaJugador.BalasMax();
        }

        if (textoEnemigos != null)
            textoEnemigos.text = "Enemigos: " + enemigosRestantes;
    }

    // Llamado por Vida cuando el jugador recibe dano: parpadeo rojo en pantalla
    public void JugadorDanado()
    {
        if (pantallaDano != null)
        {
            Color c = pantallaDano.color;
            c.a = 0.45f;
            pantallaDano.color = c;
        }
        ActualizarHUD();
    }

    public void EnemigoEliminado()
    {
        enemigosRestantes--;
        ActualizarHUD();
        if (enemigosRestantes <= 0)
            MostrarAviso("¡No quedan enemigos! Ve a la meta verde", 4f);
    }

    public bool QuedanEnemigos()
    {
        return enemigosRestantes > 0;
    }

    public void MostrarAviso(string mensaje, float duracion)
    {
        if (textoAviso == null) return;
        textoAviso.text = mensaje;
        textoAviso.enabled = true;
        finAviso = Time.unscaledTime + duracion;
    }

    // Victoria doble: la Meta solo llama aqui cuando ya no quedan enemigos
    public void Victoria()
    {
        if (terminado) return;
        terminado = true;
        if (sonidoVictoria != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(sonidoVictoria, Camera.main.transform.position);
        TerminarPartida(panelVictoria);
    }

    public void GameOver()
    {
        if (terminado) return;
        terminado = true;
        TerminarPartida(panelGameOver);
    }

    void TerminarPartida(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);

        // Apagar los controles del jugador y congelar el juego
        if (armaJugador != null)
        {
            armaJugador.enabled = false;
            PrimeraPersona pp = armaJugador.GetComponent<PrimeraPersona>();
            if (pp != null) pp.enabled = false;
        }
        Time.timeScale = 0f;

        // Liberar el cursor para poder pulsar el boton
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Conectado al boton Reintentar
    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool Terminado()
    {
        return terminado;
    }
}
