using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // ====================
    // EFECTO RECOGIDA
    // ====================
    [Header("Efecto recogida")]
    [SerializeField] private float escalaResalte = 1.35f;
    [SerializeField] private float tiempoResaltado = 0.35f;
    [SerializeField] private float duracionAnimacionResalte = 0.12f;

    private Coroutine rutinaFade;

    private Coroutine rutinaRosa;
    private Coroutine rutinaAmarilla;
    private Coroutine rutinaVerde;

    private FilaEncargoUI filaActivaRosa;
    private FilaEncargoUI filaActivaAmarilla;
    private FilaEncargoUI filaActivaVerde;

    // ====================
    // UNITY
    // ====================
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

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
        DesactivarTodasLasPlantillas();

        if (plantillaActiva != null && plantillaActiva.raiz != null)
            plantillaActiva.raiz.SetActive(true);
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
            if (plantilla.filas[i] != null && plantilla.filas[i].raiz != null)
                plantilla.filas[i].raiz.SetActive(false);
        }

        for (int i = 0; i < datos.Count && i < plantilla.filas.Length; i++)
        {
            FilaEncargoUI fila = plantilla.filas[i];

            if (fila == null)
                continue;

            fila.color = datos[i].color;

            if (fila.raiz != null)
                fila.raiz.SetActive(true);

            if (fila.iconoPez != null)
            {
                fila.iconoPez.sprite = ObtenerSpritePez(datos[i].color);
                fila.iconoPez.enabled = fila.iconoPez.sprite != null;
                fila.iconoPez.transform.localScale = fila.escalaOriginal;
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
        Vector3 escalaGrande = escalaOriginal * escalaResalte;

        float tiempo = 0f;

        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionAnimacionResalte;
            icono.transform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, t);

            yield return null;
        }

        icono.transform.localScale = escalaGrande;

        yield return new WaitForSeconds(tiempoResaltado);

        tiempo = 0f;

        while (tiempo < duracionAnimacionResalte)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionAnimacionResalte;
            icono.transform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, t);

            yield return null;
        }

        icono.transform.localScale = escalaOriginal;
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
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(Fade(0f, 1f));
    }

    // ====================
    // OCULTAR
    // ====================
    public void Ocultar()
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(Fade(1f, 0f));
    }

    // ====================
    // OCULTAR YA
    // ====================
    public void OcultarInstantaneo()
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // ====================
    // FADE
    // ====================
    private IEnumerator Fade(float inicio, float fin)
    {
        if (canvasGroup == null)
            yield break;

        float tiempo = 0f;
        float duracion = 0.25f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(inicio, fin, tiempo / duracion);
            yield return null;
        }

        canvasGroup.alpha = fin;
    }
}