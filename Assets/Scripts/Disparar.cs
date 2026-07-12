using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Disparar : MonoBehaviour
{
    // Datos de cada arma, editables desde el Inspector
    [System.Serializable]
    public class DatosArma
    {
        public string nombre = "Arma";
        public int dano = 2;
        public float cadencia = 0.5f;
        public int balasMax = 8;
        public float tiempoRecarga = 1.5f;
        public bool automatica = false;  // true: dispara manteniendo pulsado el boton
        public float pitchSonido = 1f;   // tono del sonido (mas agudo = arma ligera)
        public Sprite sprite;            // imagen del arma en el HUD
        [System.NonSerialized] public int balasActuales;
    }

    public Camera camara;
    public float alcance = 100f;
    public AudioClip sonidoDisparo;
    public GameObject muzzle;
    public Image imagenArma;             // Image del Canvas donde se dibuja el arma

    [Header("Armas (Q alterna, 1 y 2 eligen directo)")]
    public DatosArma[] armas;

    private int armaActual = 0;
    private AudioSource fuente;
    private float proximo = 0f;
    private bool recargando = false;

    void Awake()
    {
        fuente = GetComponent<AudioSource>();
        // Seguridad: si nadie configuro armas, crear una por defecto
        if (armas == null || armas.Length == 0) armas = new DatosArma[] { new DatosArma() };
        foreach (DatosArma a in armas) a.balasActuales = a.balasMax;
    }

    void Start()
    {
        if (muzzle != null) muzzle.SetActive(false);
        AplicarArma();
    }

    DatosArma Arma() { return armas[armaActual]; }

    void Update()
    {
        if (recargando) return;

        
        // Cambio de arma: Q alterna, 1 y 2 seleccionan directo
        if (Input.GetKeyDown(KeyCode.Q)) CambiarArma((armaActual + 1) % armas.Length);
        if (Input.GetKeyDown(KeyCode.Alpha1)) CambiarArma(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CambiarArma(1);

        // Recarga manual con R (tras una espera)
        if (Input.GetKeyDown(KeyCode.R) && Arma().balasActuales < Arma().balasMax)
        {
            StartCoroutine(Recargar());
            return;
        }

        // Semiautomatica: un clic por disparo. Automatica: basta mantener pulsado
        bool quiereDisparar = Arma().automatica ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (quiereDisparar && Time.time >= proximo)
        {
            if (Arma().balasActuales <= 0)
            {
                // Sin balas: el HUD avisa que hay que recargar
                if (GestorJuego.Instancia != null) GestorJuego.Instancia.ActualizarHUD();
                return;
            }
            proximo = Time.time + Arma().cadencia;
            Disparo();
        }
    }

    void CambiarArma(int indice)
    {
        if (indice == armaActual || indice < 0 || indice >= armas.Length) return;
        armaActual = indice;
        AplicarArma();
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.MostrarAviso(Arma().nombre, 1.2f);
            GestorJuego.Instancia.ActualizarHUD();
        }
    }

    // Refleja el arma actual en el HUD (cambia el sprite)
    void AplicarArma()
    {
        if (imagenArma != null && Arma().sprite != null)
            imagenArma.sprite = Arma().sprite;
    }

    IEnumerator Recargar()
    {
        recargando = true;
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.ActualizarHUD();

        yield return new WaitForSeconds(Arma().tiempoRecarga);

        Arma().balasActuales = Arma().balasMax;
        recargando = false;
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.ActualizarHUD();
    }

    void Disparo()
    {
        Arma().balasActuales--;
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.ActualizarHUD();

        if (sonidoDisparo != null)
        {
            fuente.pitch = Arma().pitchSonido;
            fuente.PlayOneShot(sonidoDisparo);
        }
        if (muzzle != null)
        {
            muzzle.SetActive(true);
            Invoke("ApagarMuzzle", 0.05f);
        }
        

        Ray ray = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, alcance))
        {
            Vida v = hit.collider.GetComponentInParent<Vida>();
            if (v != null) v.RecibirDano(Arma().dano);
        }


    }

    void ApagarMuzzle()
    {
        if (muzzle != null)
        {
            muzzle.SetActive(false);
        }
    }

    public int BalasActuales() { return Arma().balasActuales; }
    public int BalasMax() { return Arma().balasMax; }
    public bool Recargando() { return recargando; }
    public string NombreArma() { return Arma().nombre; }
}
