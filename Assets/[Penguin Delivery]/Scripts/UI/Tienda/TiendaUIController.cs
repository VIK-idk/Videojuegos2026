using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TiendaUIController : MonoBehaviour
{
    public static bool HayTiendaAbierta { get; private set; } = false;

    [Header("Panel")]
    [SerializeField] private GameObject panelTienda;

    [Header("Mercader")]
    [SerializeField] private MercaderAnimacion mercaderAnimacion;

    [Header("Habilidades")]
    [SerializeField] private CanvasGroup grupoHabilidades;
    [SerializeField] private PanelHabilidadUI panelHabilidadUI;

    [Header("Fade habilidades")]
    [SerializeField] private float duracionLlegadaMercader = 0.55f;
    [SerializeField] private float aparecerCuandoFalte = 0.1f;
    [SerializeField] private float duracionFadeHabilidades = 0.2f;

    [Header("Controladores")]
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    [Header("UI")]
    [SerializeField] private GameObject primerBotonTienda;

    [Header("Cerrar con mando")]
    [Tooltip("Normalmente Círculo/B en el Input Manager antiguo.")]
    [SerializeField] private KeyCode botonCerrarMando = KeyCode.JoystickButton1;

    private bool tiendaOcultaPorPausa = false;
    private bool cerrandoTienda = false;
    private Coroutine rutinaFadeHabilidades;

    public bool TiendaAbierta
    {
        get { return panelTienda != null && panelTienda.activeSelf; }
    }

    public bool TiendaOcultaPorPausa
    {
        get { return tiendaOcultaPorPausa; }
    }

    private void OnDisable()
    {
        HayTiendaAbierta = false;
    }

    private void Update()
    {
        if (tiendaOcultaPorPausa || !TiendaAbierta || cerrandoTienda)
            return;

        // Ya no usa JoystickButton0, porque ese mismo botón se usa para interactuar
        // y podía abrir y cerrar la tienda en el mismo frame.
        if (InputDetector.usandoMando && Input.GetKeyDown(botonCerrarMando))
        {
            CerrarTienda();
            return;
        }

        ActualizarSeleccionTienda();
    }

    public void AbrirTienda()
    {
        if (cerrandoTienda || TiendaAbierta)
            return;

        HayTiendaAbierta = true;
        tiendaOcultaPorPausa = false;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        PrepararHabilidadesOcultas();

        if (mercaderAnimacion != null)
            mercaderAnimacion.ReproducirLlegada();

        if (rutinaFadeHabilidades != null)
            StopCoroutine(rutinaFadeHabilidades);

        rutinaFadeHabilidades = StartCoroutine(AparecerHabilidadesTrasLlegada());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorCamara != null)
            controladorCamara.enabled = false;

        if (controladorJugador != null)
            controladorJugador.enabled = false;
    }

    public void CerrarTienda()
    {
        if (!TiendaAbierta || cerrandoTienda)
            return;

        StartCoroutine(CerrarTiendaCoroutine());
    }

    private IEnumerator CerrarTiendaCoroutine()
    {
        cerrandoTienda = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (panelHabilidadUI != null)
            panelHabilidadUI.LimpiarSeleccion();

        if (rutinaFadeHabilidades != null)
        {
            StopCoroutine(rutinaFadeHabilidades);
            rutinaFadeHabilidades = null;
        }

        Coroutine fadeSalida = null;

        if (grupoHabilidades != null)
            fadeSalida = StartCoroutine(FadeGrupoHabilidades(grupoHabilidades.alpha, 0f));

        if (mercaderAnimacion != null)
            yield return StartCoroutine(mercaderAnimacion.ReproducirDespedida());

        if (fadeSalida != null)
            yield return fadeSalida;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        tiendaOcultaPorPausa = false;
        cerrandoTienda = false;
        HayTiendaAbierta = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorCamara != null)
            controladorCamara.enabled = true;

        if (controladorJugador != null)
            controladorJugador.enabled = true;
    }

    public void OcultarInterfazPorPausa()
    {
        if (!TiendaAbierta)
            return;

        tiendaOcultaPorPausa = true;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestaurarInterfazTrasPausa()
    {
        if (!tiendaOcultaPorPausa)
            return;

        tiendaOcultaPorPausa = false;
        HayTiendaAbierta = true;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        MostrarHabilidadesInstantaneo();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PrepararSeleccionTienda();
    }

    private void PrepararHabilidadesOcultas()
    {
        if (grupoHabilidades == null)
            return;

        grupoHabilidades.alpha = 0f;
        grupoHabilidades.interactable = true;
        grupoHabilidades.blocksRaycasts = false;
    }

    private void MostrarHabilidadesInstantaneo()
    {
        if (grupoHabilidades == null)
            return;

        grupoHabilidades.alpha = 1f;
        grupoHabilidades.interactable = true;
        grupoHabilidades.blocksRaycasts = true;
    }

    private IEnumerator AparecerHabilidadesTrasLlegada()
    {
        float espera = Mathf.Max(0f, duracionLlegadaMercader - aparecerCuandoFalte);
        yield return new WaitForSecondsRealtime(espera);
        yield return StartCoroutine(FadeGrupoHabilidades(0f, 1f));

        rutinaFadeHabilidades = null;
        PrepararSeleccionTienda();
    }

    private IEnumerator FadeGrupoHabilidades(float alphaInicial, float alphaFinal)
    {
        if (grupoHabilidades == null)
            yield break;

        grupoHabilidades.interactable = true;
        grupoHabilidades.blocksRaycasts = false;

        float tiempo = 0f;

        while (tiempo < duracionFadeHabilidades)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = duracionFadeHabilidades <= 0f ? 1f : tiempo / duracionFadeHabilidades;
            grupoHabilidades.alpha = Mathf.Lerp(alphaInicial, alphaFinal, t);
            yield return null;
        }

        grupoHabilidades.alpha = alphaFinal;
        grupoHabilidades.interactable = true;
        grupoHabilidades.blocksRaycasts = alphaFinal > 0.99f;
    }

    private void ActualizarSeleccionTienda()
    {
        if (EventSystem.current == null)
            return;

        if (!InputDetector.DebeMostrarSeleccionUI)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);

            return;
        }

        if (grupoHabilidades != null && !grupoHabilidades.blocksRaycasts)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
            SeleccionarPrimerBotonTienda();
    }

    private void PrepararSeleccionTienda()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (InputDetector.DebeMostrarSeleccionUI)
            SeleccionarPrimerBotonTienda();
    }

    private void SeleccionarPrimerBotonTienda()
    {
        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (grupoHabilidades != null && !grupoHabilidades.blocksRaycasts)
            return;

        if (EventSystem.current == null || primerBotonTienda == null)
            return;

        if (!primerBotonTienda.activeInHierarchy)
            return;

        Selectable selectable = primerBotonTienda.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonTienda);
    }
}
