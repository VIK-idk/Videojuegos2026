using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// ====================
// UI ENCARGO
// ====================
public class UIEncargoLegacy : MonoBehaviour
{
    // ====================
    // CLASES INTERNAS
    // ====================
    [System.Serializable]
    private class PlantillaEncargoUI
    {
        public GameObject raiz;
        public Text textoTiempo;
        public FilaEncargoUI[] filas;
    }

    [System.Serializable]
    private class FilaEncargoUI
    {
        public GameObject raiz;
        public Image iconoPez;
        public Text textoCantidad;

        [HideInInspector] public ColorPez color;
        [HideInInspector] public Vector3 escalaOriginal = Vector3.one;
    }

    private struct DatosPezEncargo
    {
        public ColorPez color;
        public int actual;
        public int objetivo;

        public DatosPezEncargo(ColorPez color, int actual, int objetivo)
        {
            this.color = color;
            this.actual = actual;
            this.objetivo = objetivo;
        }
    }

    // ====================
    // PLANTILLAS
    // ====================
    [Header("Plantillas de encargo")]
    [SerializeField] private PlantillaEncargoUI plantilla1Tipo;
    [SerializeField] private PlantillaEncargoUI plantilla2Tipos;
    [SerializeField] private PlantillaEncargoUI plantilla3Tipos;

    // ====================
    // SPRITES PECES
    // ====================
    [Header("Sprites peces")]
    [SerializeField] private Sprite spritePezRosa;
    [SerializeField] private Sprite spritePezAmarillo;
    [SerializeField] private Sprite spritePezVerde;

    // ====================
    // PANEL
    // ====================
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Animacion entrada/salida")]
    [SerializeField] private float duracionMovimientoPanel = 0.35f;
    [SerializeField] private float distanciaEntradaIzquierda = 900f;
    [SerializeField] private AnimationCurve curvaMovimiento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ====================
    // AUDIO
    // ====================
    [Header("Audio - Encargos 2D")]
    [SerializeField] private AudioSource audioSourceEncargos;
    [SerializeField] private AudioMixerGroup grupoMixerEncargos;

    [Header("Clips de encargo")]
    [SerializeField] private AudioClip sonidoAparecerEncargo;
    [SerializeField] private AudioClip sonidoDesaparecerEncargo;
    [SerializeField] private AudioClip sonidoVictoriaEncargo;
    [SerializeField] private AudioClip sonidoDerrotaEncargo;

    [Header("Volumenes de encargo")]
    [SerializeField, Range(0f, 1f)] private float volumenAparecer = 1f;
    [SerializeField, Range(0f, 1f)] private float volumenDesaparecer = 1f;
    [SerializeField, Range(0f, 1f)] private float volumenVictoria = 1f;
    [SerializeField, Range(0f, 1f)] private float volumenDerrota = 1f;

    // ====================
    // EFECTO RECOGIDA
    // ====================
    [Header("Efecto recogida")]
    [SerializeField] private float escalaResalte = 1.35f;
    [SerializeField] private float tiempoResaltado = 0.2f;
    [SerializeField] private float duracionAnimacionResalte = 0.08f;

    private Coroutine rutinaFade;

    private Vector2 posicionOriginalPanel;
    private Vector2 posicionOcultaIzquierda;

    private Coroutine rutinaRosa;
    private Coroutine rutinaAmarilla;
    private Coroutine rutinaVerde;

    private FilaEncargoUI filaActivaRosa;
    private FilaEncargoUI filaActivaAmarilla;
    private FilaEncargoUI filaActivaVerde;
    private bool panelVisible = false;

    // ====================
    // UNITY
    // ====================
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelRect != null)
        {
            posicionOriginalPanel = panelRect.anchoredPosition;
            posicionOcultaIzquierda = posicionOriginalPanel + Vector2.left * distanciaEntradaIzquierda;
        }

        ConfigurarAudioSource();

        GuardarEscalasOriginales(plantilla1Tipo);
        GuardarEscalasOriginales(plantilla2Tipos);
        GuardarEscalasOriginales(plantilla3Tipos);

        DesactivarTodasLasPlantillas();
    }

    // ====================
    // ACTUALIZAR
    // ====================
    public void ActualizarUI(
        EncargoData encargo,
        float tiempo,
        int rosasActuales,
        int amarillosActuales,
        int verdesActuales)
    {
        if (encargo == null)
            return;

        List<DatosPezEncargo> datos = CrearListaPecesActivos(
            encargo,
            rosasActuales,
            amarillosActuales,
            verdesActuales
        );

        PlantillaEncargoUI plantilla = ObtenerPlantilla(datos.Count);

        ActivarSoloPlantilla(plantilla);
        ActualizarTiempo(plantilla, tiempo);
        LimpiarReferenciasActivas();
        RellenarPlantilla(plantilla, datos);
    }

    // ====================
    // CREAR LISTA ORDENADA
    // ====================
    private List<DatosPezEncargo> CrearListaPecesActivos(
        EncargoData encargo,
        int rosasActuales,
        int amarillosActuales,
        int verdesActuales)
    {
        List<DatosPezEncargo> datos = new List<DatosPezEncargo>();

        if (encargo.pecesRosas > 0)
        {
            datos.Add(new DatosPezEncargo(
                ColorPez.Rosa,
                rosasActuales,
                encargo.pecesRosas
            ));
        }

        if (encargo.pecesAmarillos > 0)
        {
            datos.Add(new DatosPezEncargo(
                ColorPez.Amarillo,
                amarillosActuales,
                encargo.pecesAmarillos
            ));
        }

        if (encargo.pecesVerdes > 0)
        {
            datos.Add(new DatosPezEncargo(
                ColorPez.Verde,
                verdesActuales,
                encargo.pecesVerdes
            ));
        }

        return datos;
    }

    // ====================
    // PLANTILLA
    // ====================
    private PlantillaEncargoUI ObtenerPlantilla(int cantidadTipos)
    {
        if (cantidadTipos <= 1)
            return plantilla1Tipo;

        if (cantidadTipos == 2)
            return plantilla2Tipos;

        return plantilla3Tipos;
    }

    private void ActivarSoloPlantilla(PlantillaEncargoUI plantillaActiva)
    {
        // No desactivamos y reactivamos la misma plantilla cada frame.
        // En el tutorial eso puede verse como un parpadeo justo después de entrar deslizándose.
        ActivarPlantillaSiHaceFalta(plantilla1Tipo, plantillaActiva == plantilla1Tipo);
        ActivarPlantillaSiHaceFalta(plantilla2Tipos, plantillaActiva == plantilla2Tipos);
        ActivarPlantillaSiHaceFalta(plantilla3Tipos, plantillaActiva == plantilla3Tipos);
    }

    private void ActivarPlantillaSiHaceFalta(PlantillaEncargoUI plantilla, bool activa)
    {
        if (plantilla == null || plantilla.raiz == null)
            return;

        if (plantilla.raiz.activeSelf != activa)
            plantilla.raiz.SetActive(activa);
    }

    private void DesactivarTodasLasPlantillas()
    {
        if (plantilla1Tipo != null && plantilla1Tipo.raiz != null)
            plantilla1Tipo.raiz.SetActive(false);

        if (plantilla2Tipos != null && plantilla2Tipos.raiz != null)
            plantilla2Tipos.raiz.SetActive(false);

        if (plantilla3Tipos != null && plantilla3Tipos.raiz != null)
            plantilla3Tipos.raiz.SetActive(false);
    }

    private void RellenarPlantilla(PlantillaEncargoUI plantilla, List<DatosPezEncargo> datos)
    {
        if (plantilla == null || plantilla.filas == null)
            return;

        for (int i = 0; i < plantilla.filas.Length; i++)
        {
            bool debeEstarActiva = i < datos.Count;

            if (plantilla.filas[i] != null && plantilla.filas[i].raiz != null &&
                plantilla.filas[i].raiz.activeSelf != debeEstarActiva)
            {
                plantilla.filas[i].raiz.SetActive(debeEstarActiva);
            }
        }

        for (int i = 0; i < datos.Count && i < plantilla.filas.Length; i++)
        {
            FilaEncargoUI fila = plantilla.filas[i];

            if (fila == null)
                continue;

            fila.color = datos[i].color;

            if (fila.iconoPez != null)
            {
                fila.iconoPez.sprite = ObtenerSpritePez(datos[i].color);
                fila.iconoPez.enabled = fila.iconoPez.sprite != null;
            }

            if (fila.textoCantidad != null)
            {
                fila.textoCantidad.text = datos[i].actual + "/" + datos[i].objetivo;
            }

            GuardarFilaActiva(fila);
        }
    }

    private void GuardarFilaActiva(FilaEncargoUI fila)
    {
        if (fila.color == ColorPez.Rosa)
            filaActivaRosa = fila;
        else if (fila.color == ColorPez.Amarillo)
            filaActivaAmarilla = fila;
        else if (fila.color == ColorPez.Verde)
            filaActivaVerde = fila;
    }

    private void LimpiarReferenciasActivas()
    {
        filaActivaRosa = null;
        filaActivaAmarilla = null;
        filaActivaVerde = null;
    }

    // ====================
    // TIEMPO
    // ====================
    private void ActualizarTiempo(PlantillaEncargoUI plantilla, float tiempo)
    {
        if (plantilla == null || plantilla.textoTiempo == null)
            return;

        if (float.IsInfinity(tiempo))
            plantilla.textoTiempo.text = "∞";
        else
            plantilla.textoTiempo.text = tiempo.ToString("F1") + "s";
    }

    // ====================
    // SPRITES
    // ====================
    private Sprite ObtenerSpritePez(ColorPez color)
    {
        if (color == ColorPez.Rosa)
            return spritePezRosa;

        if (color == ColorPez.Amarillo)
            return spritePezAmarillo;

        return spritePezVerde;
    }

    // ====================
    // RESALTAR PEZ
    // ====================
    public void ResaltarPezRecogido(ColorPez color)
    {
        if (color == ColorPez.Rosa)
        {
            IniciarResalte(filaActivaRosa, ref rutinaRosa);
        }
        else if (color == ColorPez.Amarillo)
        {
            IniciarResalte(filaActivaAmarilla, ref rutinaAmarilla);
        }
        else if (color == ColorPez.Verde)
        {
            IniciarResalte(filaActivaVerde, ref rutinaVerde);
        }
    }

    private void IniciarResalte(FilaEncargoUI fila, ref Coroutine rutina)
    {
        if (fila == null || fila.iconoPez == null)
            return;

        if (rutina != null)
            StopCoroutine(rutina);

        fila.iconoPez.transform.localScale = fila.escalaOriginal;
        rutina = StartCoroutine(AnimarResalte(fila.iconoPez, fila.escalaOriginal));
    }

    private IEnumerator AnimarResalte(Image icono, Vector3 escalaOriginal)
    {
        if (icono == null)
            yield break;

        Transform iconoTransform = icono.transform;

        Vector3 escalaGrande = escalaOriginal * escalaResalte;

        // Aseguramos que empieza desde su tamaño normal.
        iconoTransform.localScale = escalaOriginal;

        float tiempo = 0f;

        // Subir una sola vez.
        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracionAnimacionResalte);
            iconoTransform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, t);

            yield return null;
        }

        iconoTransform.localScale = escalaGrande;

        // Se queda arriba un momento.
        yield return new WaitForSeconds(tiempoResaltado);

        tiempo = 0f;

        // Volver a tamaño normal.
        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracionAnimacionResalte);
            iconoTransform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, t);

            yield return null;
        }

        iconoTransform.localScale = escalaOriginal;
    }

    // ====================
    // ESCALAS ORIGINALES
    // ====================
    private void GuardarEscalasOriginales(PlantillaEncargoUI plantilla)
    {
        if (plantilla == null || plantilla.filas == null)
            return;

        for (int i = 0; i < plantilla.filas.Length; i++)
        {
            if (plantilla.filas[i] == null)
                continue;

            if (plantilla.filas[i].iconoPez != null)
                plantilla.filas[i].escalaOriginal = plantilla.filas[i].iconoPez.transform.localScale;
        }
    }

    // ====================
    // MOSTRAR
    // ====================
    public void Mostrar()
    {
        // Evita volver a lanzar la entrada y su sonido si el encargo ya está visible.
        if (panelVisible)
            return;

        panelVisible = true;
        ReproducirClip(sonidoAparecerEncargo, volumenAparecer);

        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(MoverPanel(
            posicionOcultaIzquierda,
            posicionOriginalPanel,
            0f,
            1f
        ));
    }

    // ====================
    // OCULTAR
    // ====================
    public void Ocultar()
    {
        // Evita sonidos de salida duplicados si ya estaba oculto.
        if (!panelVisible)
            return;

        panelVisible = false;
        ReproducirClip(sonidoDesaparecerEncargo, volumenDesaparecer);

        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(MoverPanel(
            posicionOriginalPanel,
            posicionOcultaIzquierda,
            1f,
            0f
        ));
    }

    // ====================
    // OCULTAR YA
    // ====================
    public void OcultarInstantaneo()
    {
        panelVisible = false;

        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (panelRect != null)
            panelRect.anchoredPosition = posicionOcultaIzquierda;
    }

    // ====================
    // SONIDOS DE RESULTADO
    // ====================
    public void ReproducirVictoria()
    {
        ReproducirClip(sonidoVictoriaEncargo, volumenVictoria);
    }

    public void ReproducirDerrota()
    {
        ReproducirClip(sonidoDerrotaEncargo, volumenDerrota);
    }

    private void ConfigurarAudioSource()
    {
        if (audioSourceEncargos == null)
            audioSourceEncargos = GetComponent<AudioSource>();

        if (audioSourceEncargos == null)
            audioSourceEncargos = gameObject.AddComponent<AudioSource>();

        audioSourceEncargos.playOnAwake = false;
        audioSourceEncargos.loop = false;
        audioSourceEncargos.spatialBlend = 0f;
        audioSourceEncargos.dopplerLevel = 0f;

        if (grupoMixerEncargos != null)
            audioSourceEncargos.outputAudioMixerGroup = grupoMixerEncargos;
    }

    private void ReproducirClip(AudioClip clip, float volumen)
    {
        if (clip == null)
            return;

        if (audioSourceEncargos == null)
            ConfigurarAudioSource();

        if (audioSourceEncargos == null)
            return;

        audioSourceEncargos.PlayOneShot(clip, Mathf.Clamp01(volumen));
    }

    // ====================
    // MOVER PANEL
    // ====================
    private IEnumerator MoverPanel(
    Vector2 posicionInicio,
    Vector2 posicionFinal,
    float alphaInicio,
    float alphaFinal)
    {
        if (canvasGroup == null || panelRect == null)
            yield break;

        float tiempo = 0f;

        canvasGroup.alpha = alphaInicio;
        panelRect.anchoredPosition = posicionInicio;

        while (tiempo < duracionMovimientoPanel)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracionMovimientoPanel);
            float curva = curvaMovimiento != null ? curvaMovimiento.Evaluate(t) : t;

            panelRect.anchoredPosition = Vector2.Lerp(posicionInicio, posicionFinal, curva);
            canvasGroup.alpha = Mathf.Lerp(alphaInicio, alphaFinal, t);

            yield return null;
        }

        panelRect.anchoredPosition = posicionFinal;
        canvasGroup.alpha = alphaFinal;
    }
}