using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;

// Configura la escena del parcial: enemigos (prefab + NavMesh), HUD, meta, botiquines y GestorJuego.
// Se puede ejecutar desde el menu (Herramientas > Configurar Parcial) o por linea de comandos con -executeMethod
public static class ConfiguradorParcial
{
    const string RUTA_ESCENA = "Assets/Scenes/SampleScene.unity";

    static readonly Vector3[] POS_ENEMIGOS =
    {
        new Vector3(-16f, 0f, 5f),
        new Vector3(-3f, 0f, 14f),
        new Vector3(16f, 0f, -3.5f),
        new Vector3(-17f, 0f, -8f),
        new Vector3(6f, 0f, 14f),
    };

    static readonly Vector3[] POS_BOTIQUINES =
    {
        new Vector3(4f, 0f, -11.5f),
        new Vector3(-16f, 0f, 13.5f),
        new Vector3(14f, 0f, 5f),
    };

    static readonly Vector3 POS_META = new Vector3(21.5f, 0f, 21.5f);

    [MenuItem("Herramientas/Configurar Parcial")]
    public static void Configurar()
    {
        AssetDatabase.Refresh();
        var escena = EditorSceneManager.OpenScene(RUTA_ESCENA);

        // ---- Cargar assets existentes ----
        AudioClip clipDisparo = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/disparo.wav");
        AudioClip clipEnemigoDano = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/enemigo_dano.wav");
        AudioClip clipJugadorDano = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/jugador_dano.wav");
        AudioClip clipVictoria = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/victoria.wav");
        AudioClip clipBotiquin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/botiquin.wav");
        Material matEnemigo = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materiales/MatEnemigo.mat");
        Texture2D texBotiquin = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/botiquin.png");

        if (clipBotiquin == null) Debug.LogWarning("[Configurador] No se encontro botiquin.wav (el botiquin no sonara)");

        // ---- Limpiar lo generado en ejecuciones anteriores (permite re-ejecutar) ----
        foreach (var e in Object.FindObjectsByType<EnemigoIA>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(e.gameObject);
        foreach (var b in Object.FindObjectsByType<Botiquin>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(b.gameObject);
        foreach (var m in Object.FindObjectsByType<Meta>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(m.gameObject);
        foreach (var g in Object.FindObjectsByType<GestorJuego>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(g.gameObject);
        foreach (var s in Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(s.gameObject);

        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[Configurador] No existe el Canvas"); return; }
        string[] uiViejas = { "HUD_Mira", "HUD_Vida", "HUD_Municion", "HUD_Enemigos", "HUD_Aviso", "HUD_PantallaDano", "PanelGameOver", "PanelVictoria" };
        foreach (string n in uiViejas)
        {
            Transform t = canvasGO.transform.Find(n);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // ---- Materiales nuevos ----
        Material matBotiquin = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materiales/MatBotiquin.mat");
        if (matBotiquin == null)
        {
            matBotiquin = new Material(matEnemigo); // copia la config transparente URP
            matBotiquin.SetTexture("_BaseMap", texBotiquin);
            matBotiquin.SetTexture("_MainTex", texBotiquin);
            matBotiquin.SetFloat("_Cull", 0f); // visible por ambos lados
            AssetDatabase.CreateAsset(matBotiquin, "Assets/Materiales/MatBotiquin.mat");
        }

        Material matMeta = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materiales/MatMeta.mat");
        if (matMeta == null)
        {
            matMeta = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color verde = new Color(0.15f, 0.95f, 0.35f);
            matMeta.SetColor("_BaseColor", verde);
            matMeta.EnableKeyword("_EMISSION");
            matMeta.SetColor("_EmissionColor", verde * 2.5f);
            AssetDatabase.CreateAsset(matMeta, "Assets/Materiales/MatMeta.mat");
        }

        // ---- Jugador ----
        GameObject jugador = GameObject.Find("Jugador");
        jugador.tag = "Player";
        jugador.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // que no empiece mirando la pared
        Vida vidaJugador = jugador.GetComponent<Vida>();
        vidaJugador.vidaMax = 5;
        vidaJugador.sonidoDano = clipJugadorDano;
        Disparar armaJugador = jugador.GetComponent<Disparar>();

        // ---- Dos armas: escopeta (semiautomatica) y metralleta (automatica y rapida) ----
        TextureImporter tiMetra = AssetImporter.GetAtPath("Assets/Sprites/metralleta.png") as TextureImporter;
        if (tiMetra != null && (tiMetra.textureType != TextureImporterType.Sprite || tiMetra.spriteImportMode != SpriteImportMode.Single))
        {
            tiMetra.textureType = TextureImporterType.Sprite;
            tiMetra.spriteImportMode = SpriteImportMode.Single; // un solo sprite, como arma.png
            tiMetra.SaveAndReimport();
        }
        Sprite spriteEscopeta = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arma.png");
        Sprite spriteMetralleta = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/metralleta.png");
        if (spriteMetralleta == null) Debug.LogWarning("[Configurador] No se encontro metralleta.png como Sprite");

        armaJugador.armas = new Disparar.DatosArma[]
        {
            new Disparar.DatosArma { nombre = "Escopeta",   dano = 2, cadencia = 0.5f,  balasMax = 8,  tiempoRecarga = 1.5f, automatica = false, pitchSonido = 1f,   sprite = spriteEscopeta },
            new Disparar.DatosArma { nombre = "Metralleta", dano = 1, cadencia = 0.12f, balasMax = 30, tiempoRecarga = 2f,   automatica = true,  pitchSonido = 1.3f, sprite = spriteMetralleta },
        };
        GameObject armaUI = GameObject.Find("Arma");
        if (armaUI != null) armaJugador.imagenArma = armaUI.GetComponent<Image>();
        EditorUtility.SetDirty(armaJugador);

        GameObject camara = GameObject.Find("Main Camera");
        if (camara != null) camara.tag = "MainCamera";

        // ---- Prefab del enemigo ----
        GameObject tmpEnemigo = new GameObject("Enemigo");
        var capsula = tmpEnemigo.AddComponent<CapsuleCollider>();
        capsula.center = new Vector3(0f, 0.9f, 0f); capsula.height = 1.8f; capsula.radius = 0.35f;
        var agente = tmpEnemigo.AddComponent<NavMeshAgent>();
        agente.speed = 3f; agente.acceleration = 12f; agente.angularSpeed = 720f;
        agente.stoppingDistance = 2f; agente.radius = 0.35f; agente.height = 1.8f; agente.baseOffset = 0f;
        var vidaEnemigo = tmpEnemigo.AddComponent<Vida>();
        vidaEnemigo.vidaMax = 4; vidaEnemigo.esJugador = false; vidaEnemigo.sonidoDano = clipEnemigoDano;
        var ia = tmpEnemigo.AddComponent<EnemigoIA>();
        ia.sonidoDisparo = clipDisparo; ia.dano = 1; ia.cadencia = 1.8f; ia.distanciaAtaque = 8f;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<MeshCollider>());
        visual.transform.SetParent(tmpEnemigo.transform);
        visual.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        visual.transform.localScale = new Vector3(1.4f, 1.9f, 1f);
        var rendVisual = visual.GetComponent<MeshRenderer>();
        rendVisual.sharedMaterial = matEnemigo;
        rendVisual.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visual.AddComponent<EnemigoBillboard>();

        GameObject prefabEnemigo = PrefabUtility.SaveAsPrefabAsset(tmpEnemigo, "Assets/Prefabs/Enemigo.prefab");
        Object.DestroyImmediate(tmpEnemigo);

        // ---- Prefab del botiquin ----
        GameObject tmpBotiquin = new GameObject("Botiquin");
        var esfera = tmpBotiquin.AddComponent<SphereCollider>();
        esfera.isTrigger = true; esfera.radius = 0.7f; esfera.center = new Vector3(0f, 0.7f, 0f);
        var scriptBotiquin = tmpBotiquin.AddComponent<Botiquin>();
        scriptBotiquin.curacion = 2; scriptBotiquin.sonidoRecoger = clipBotiquin;

        GameObject visualBot = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visualBot.name = "Visual";
        Object.DestroyImmediate(visualBot.GetComponent<MeshCollider>());
        visualBot.transform.SetParent(tmpBotiquin.transform);
        visualBot.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        visualBot.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        var rendBot = visualBot.GetComponent<MeshRenderer>();
        rendBot.sharedMaterial = matBotiquin;
        rendBot.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject prefabBotiquin = PrefabUtility.SaveAsPrefabAsset(tmpBotiquin, "Assets/Prefabs/Botiquin.prefab");
        Object.DestroyImmediate(tmpBotiquin);

        // ---- GestorJuego ----
        GameObject gestorGO = new GameObject("GestorJuego");
        GestorJuego gestor = gestorGO.AddComponent<GestorJuego>();
        gestor.vidaJugador = vidaJugador;
        gestor.armaJugador = armaJugador;
        gestor.sonidoVictoria = clipVictoria;

        // ---- HUD ----
        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Transform canvas = canvasGO.transform;

        Image mira = CrearImagen(canvas, "HUD_Mira", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f), new Color(1f, 1f, 1f, 0.85f));
        gestor.textoVida = CrearTexto(canvas, "HUD_Vida", fuente, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 15f), new Vector2(320f, 40f), "Vida: 5/5", 28, TextAnchor.LowerLeft, Color.white);
        gestor.textoMunicion = CrearTexto(canvas, "HUD_Municion", fuente, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 15f), new Vector2(380f, 40f), "Balas: 8/8", 28, TextAnchor.LowerRight, Color.white);
        gestor.textoEnemigos = CrearTexto(canvas, "HUD_Enemigos", fuente, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -15f), new Vector2(320f, 40f), "Enemigos: 4", 28, TextAnchor.UpperRight, Color.white);
        Text aviso = CrearTexto(canvas, "HUD_Aviso", fuente, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(800f, 44f), "", 30, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.2f));
        aviso.enabled = false;
        gestor.textoAviso = aviso;

        // Parpadeo rojo de dano (pantalla completa, no bloquea clics)
        Image flash = CrearImagen(canvas, "HUD_PantallaDano", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 0f, 0f, 0f));
        StretchCompleto(flash.rectTransform);
        gestor.pantallaDano = flash;

        // ---- Paneles de fin de partida ----
        gestor.panelGameOver = CrearPanelFinal(canvas, "PanelGameOver", fuente, "GAME OVER", new Color(0.95f, 0.25f, 0.2f), "Reintentar", gestor);
        gestor.panelVictoria = CrearPanelFinal(canvas, "PanelVictoria", fuente, "¡VICTORIA!", new Color(0.3f, 0.95f, 0.4f), "Jugar de nuevo", gestor);

        // ---- NavMesh (se hornea ANTES de colocar enemigos para que no estorben) ----
        GameObject navGO = new GameObject("NavMesh");
        var superficie = navGO.AddComponent<NavMeshSurface>();
        superficie.collectObjects = CollectObjects.All;
        superficie.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        superficie.BuildNavMesh();
        if (superficie.navMeshData != null)
            AssetDatabase.CreateAsset(superficie.navMeshData, "Assets/Scenes/NavMesh-Parcial.asset");
        Debug.Log("[Configurador] NavMesh horneado: " + (superficie.navMeshData != null));

        // ---- Colocar enemigos, botiquines y meta ----
        for (int i = 0; i < POS_ENEMIGOS.Length; i++)
        {
            GameObject e = (GameObject)PrefabUtility.InstantiatePrefab(prefabEnemigo);
            e.name = "Enemigo (" + (i + 1) + ")";
            e.transform.position = POS_ENEMIGOS[i];
        }
        for (int i = 0; i < POS_BOTIQUINES.Length; i++)
        {
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(prefabBotiquin);
            b.name = "Botiquin (" + (i + 1) + ")";
            b.transform.position = POS_BOTIQUINES[i];
        }

        GameObject meta = new GameObject("Meta");
        meta.transform.position = POS_META;
        var caja = meta.AddComponent<BoxCollider>();
        caja.isTrigger = true; caja.size = new Vector3(2.5f, 3f, 2.5f); caja.center = new Vector3(0f, 1.5f, 0f);
        meta.AddComponent<Meta>();
        GameObject pilar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pilar.name = "Visual";
        Object.DestroyImmediate(pilar.GetComponent<CapsuleCollider>());
        pilar.transform.SetParent(meta.transform);
        pilar.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        pilar.transform.localScale = new Vector3(1.6f, 1.4f, 1.6f);
        pilar.GetComponent<MeshRenderer>().sharedMaterial = matMeta;

        // ---- Guardar ----
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.SaveAssets();
        Debug.Log("[Configurador] COMPLETADO OK: " + POS_ENEMIGOS.Length + " enemigos, " + POS_BOTIQUINES.Length + " botiquines, meta, HUD y NavMesh");
    }

    // ---------- Helpers de UI ----------

    static RectTransform CrearRect(Transform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 tam)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = anchorMin; // pivote igual al ancla para colocar facil
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
        return rt;
    }

    static void StretchCompleto(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
    }

    static Text CrearTexto(Transform padre, string nombre, Font fuente, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 tam, string contenido, int tamFuente, TextAnchor alineacion, Color color)
    {
        var rt = CrearRect(padre, nombre, anchorMin, anchorMax, pos, tam);
        var txt = rt.gameObject.AddComponent<Text>();
        txt.font = fuente; txt.text = contenido; txt.fontSize = tamFuente;
        txt.alignment = alineacion; txt.color = color; txt.raycastTarget = false;
        var sombra = rt.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.85f); sombra.effectDistance = new Vector2(1.5f, -1.5f);
        return txt;
    }

    static Image CrearImagen(Transform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 tam, Color color)
    {
        var rt = CrearRect(padre, nombre, anchorMin, anchorMax, pos, tam);
        rt.pivot = new Vector2(0.5f, 0.5f);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color; img.raycastTarget = false;
        return img;
    }

    static GameObject CrearPanelFinal(Transform canvas, string nombre, Font fuente, string titulo, Color colorTitulo, string textoBoton, GestorJuego gestor)
    {
        var rtPanel = CrearRect(canvas, nombre, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        StretchCompleto(rtPanel);
        var fondo = rtPanel.gameObject.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.78f); // fondo oscuro que bloquea la vista

        Text tituloTxt = CrearTexto(rtPanel.transform, "Titulo", fuente, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 110f), titulo, 72, TextAnchor.MiddleCenter, colorTitulo);
        tituloTxt.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        tituloTxt.rectTransform.anchoredPosition = new Vector2(0f, 60f);

        // Boton
        var rtBoton = CrearRect(rtPanel.transform, "BotonReintentar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 60f));
        rtBoton.pivot = new Vector2(0.5f, 0.5f);
        rtBoton.anchoredPosition = new Vector2(0f, -50f);
        var imgBoton = rtBoton.gameObject.AddComponent<Image>();
        imgBoton.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        var boton = rtBoton.gameObject.AddComponent<Button>();
        boton.targetGraphic = imgBoton;
        UnityEventTools.AddPersistentListener(boton.onClick, new UnityAction(gestor.Reintentar));

        Text txtBoton = CrearTexto(rtBoton.transform, "Texto", fuente, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 60f), textoBoton, 28, TextAnchor.MiddleCenter, new Color(0.1f, 0.1f, 0.1f));
        txtBoton.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        txtBoton.rectTransform.anchoredPosition = Vector2.zero;

        rtPanel.gameObject.SetActive(false);
        return rtPanel.gameObject;
    }
}
