using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum PasoTutorial
    {
        Ninguno,
        RebotarEnMorsa,
        RecogerPrimerPez,
        ExplicarStrikes,
        CompletarPrimerEncargo,
        Finalizado
    }

    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialTitulo;
    [SerializeField] private Text tutorialTexto;
    [SerializeField] private Text tutorialSkipTexto;

    [Header("Indicadores visuales opcionales")]
    [SerializeField] private GameObject indicadorMorsa;
    [SerializeField] private GameObject indicadorEncargo;
    [SerializeField] private GameObject indicadorStrikes;

    [Header("Configuracion")]
    [SerializeField] private KeyCode teclaSaltarTutorial = KeyCode.Tab;
    [SerializeField] private float tiempoMinimoLectura = 1.5f;
    [SerializeField] private float duracionPasoStrikes = 2.5f;
    [SerializeField] private float duracionMensajeFinal = 4f;

    [Header("Referencias")]
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private GestorEncargosTest gestorEncargos;

    [Header("SOLO TESTING")]
    [SerializeField] private bool forzarTutorialSiempre = false;
    [SerializeField] private KeyCode teclaResetTutorial = KeyCode.Y;

    private const string CLAVE_TUTORIAL_COMPLETADO = "TUTORIAL_COMPLETADO";

    private bool tutorialActivo = false;
    private bool tutorialGuardadoComoCompletado = false;
    private PasoTutorial pasoActual = PasoTutorial.Ninguno;

    private float bloqueoHasta = 0f;
    private bool pezRecogidoPendiente = false;

    public bool DebeMostrarTutorial()
    {
        if (forzarTutorialSiempre)
            return true;

        return PlayerPrefs.GetInt(CLAVE_TUTORIAL_COMPLETADO, 0) == 0;
    }

    [ContextMenu("Resetear tutorial guardado")]
    public void ResetearTutorialGuardado()
    {
        PlayerPrefs.DeleteKey(CLAVE_TUTORIAL_COMPLETADO);
        PlayerPrefs.Save();
    }

    public bool TutorialActivo()
    {
        return tutorialActivo;
    }

    private void Awake()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        OcultarTodosLosIndicadores();
    }

    private void Start()
    {
        if (strikeManager == null)
            strikeManager = FindFirstObjectByType<StrikeManager>();

        if (gestorEncargos == null)
            gestorEncargos = FindFirstObjectByType<GestorEncargosTest>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(teclaResetTutorial))
        {
            ResetearTutorialGuardado(); // SOLO TESTING
            Debug.Log("Tutorial reseteado"); // SOLO TESTING
        }

        if (!tutorialActivo)
            return;

        if (Input.GetKeyDown(teclaSaltarTutorial))
        {
            SaltarTutorial();
            return;
        }

        // Si el jugador recogió el pez demasiado rápido,
        // el paso avanzará cuando termine el tiempo mínimo de lectura.
        if (pasoActual == PasoTutorial.RecogerPrimerPez && pezRecogidoPendiente && PuedeAvanzarPaso())
        {
            pezRecogidoPendiente = false;
            AvanzarAExplicacionStrikes();
        }
    }

    public void IniciarTutorial()
    {
        tutorialActivo = true;
        tutorialGuardadoComoCompletado = false;
        pezRecogidoPendiente = false;
        pasoActual = PasoTutorial.RebotarEnMorsa;

        MostrarPanel();
        MostrarPasoReboteMorsa();
    }

    public void NotificarReboteEnMorsa()
    {
        if (!tutorialActivo)
            return;

        if (pasoActual != PasoTutorial.RebotarEnMorsa)
            return;

        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false); 

        pasoActual = PasoTutorial.RecogerPrimerPez;
        MostrarPasoRecogerPez();
    }

    public void NotificarPrimerPezRecogido()
    {
        if (!tutorialActivo)
            return;

        if (pasoActual != PasoTutorial.RecogerPrimerPez)
            return;

        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false); 

        if (!PuedeAvanzarPaso())
        {
            pezRecogidoPendiente = true;
            return;
        }

        AvanzarAExplicacionStrikes();
    }

    private void AvanzarAExplicacionStrikes()
    {
        pasoActual = PasoTutorial.ExplicarStrikes;
        StartCoroutine(SecuenciaExplicacionStrikes());
    }

    private IEnumerator SecuenciaExplicacionStrikes()
    {
        MostrarPasoExplicarStrikes();

        if (strikeManager != null)
            yield return StartCoroutine(strikeManager.ParpadearStrikeDemo(2f));
        else
            yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(duracionPasoStrikes);

        if (!tutorialActivo)
            yield break;

        if (indicadorStrikes != null)
            indicadorStrikes.SetActive(false); 

        pasoActual = PasoTutorial.CompletarPrimerEncargo;
        MostrarPasoCompletarEncargo();
    }

    public IEnumerator MostrarMensajeFinalTutorial()
    {
        pasoActual = PasoTutorial.Finalizado;

        if (tutorialTitulo != null)
            tutorialTitulo.text = "Tutorial completado";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Ahora completa los encargos antes de que acabe el tiempo.";
        }

        ActualizarTextoSkip(false);
        OcultarTodosLosIndicadores();
        MostrarPanel();

        yield return new WaitForSeconds(duracionMensajeFinal);

        tutorialActivo = false;
        OcultarPanel();
    }

    public void MarcarTutorialComoCompletado()
    {
        if (tutorialGuardadoComoCompletado)
            return;

        tutorialGuardadoComoCompletado = true;
        tutorialActivo = false;

        PlayerPrefs.SetInt(CLAVE_TUTORIAL_COMPLETADO, 1);
        PlayerPrefs.Save();
    }

    private void SaltarTutorial()
    {
        StartCoroutine(SaltarTutorialCoroutine());
    }

    private IEnumerator SaltarTutorialCoroutine()
    {
        MarcarTutorialComoCompletado();
        OcultarTodosLosIndicadores();

        yield return StartCoroutine(MostrarMensajeFinalTutorial());

        if (gestorEncargos != null)
        {
            gestorEncargos.SaltarTutorialYEmpezarJuegoNormal();
        }
    }

    private void MostrarPasoReboteMorsa()
    {
        if (tutorialTitulo != null)
            tutorialTitulo.text = "Tutorial";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Rebota sobre las morsas para impulsarte y conseguir peces.";
        }

        MostrarSoloIndicador(indicadorMorsa);
        ActualizarTextoSkip(true);
        BloquearLectura();
    }

    private void MostrarPasoRecogerPez()
    {
        if (tutorialTitulo != null)
            tutorialTitulo.text = "Peces";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Muy bien. Ahora recoge un pez.";
        }

        ActualizarTextoSkip(true);
        BloquearLectura();
    }

    private void MostrarPasoExplicarStrikes()
    {
        if (tutorialTitulo != null)
            tutorialTitulo.text = "Strikes";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Si fallas un encargo, recibirás un strike.\n\n" +
                "Si acumulas 3 strikes, volverás al lobby.";
        }

        MostrarIndicadores(indicadorStrikes, indicadorEncargo);
        ActualizarTextoSkip(true);
    }

    private void MostrarPasoCompletarEncargo()
    {
        if (tutorialTitulo != null)
            tutorialTitulo.text = "Encargo";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Ahora completa este primer encargo.";
        }

        ActualizarTextoSkip(true);
        BloquearLectura();
    }

    private void MostrarPanel()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void OcultarPanel()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void ActualizarTextoSkip(bool mostrar)
    {
        if (tutorialSkipTexto == null)
            return;

        if (mostrar)
            tutorialSkipTexto.text = "Pulsa " + teclaSaltarTutorial + " para saltar el tutorial";
        else
            tutorialSkipTexto.text = "";
    }

    private void MostrarSoloIndicador(GameObject indicador)
    {
        OcultarTodosLosIndicadores();

        if (indicador != null)
            indicador.SetActive(true);
    }

    private void MostrarIndicadores(GameObject indicadorA, GameObject indicadorB)
    {
        OcultarTodosLosIndicadores();

        if (indicadorA != null)
            indicadorA.SetActive(true);

        if (indicadorB != null)
            indicadorB.SetActive(true);
    }

    private void OcultarTodosLosIndicadores()
    {
        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        if (indicadorEncargo != null)
            indicadorEncargo.SetActive(false);

        if (indicadorStrikes != null)
            indicadorStrikes.SetActive(false);
    }

    private void BloquearLectura()
    {
        bloqueoHasta = Time.time + tiempoMinimoLectura;
    }

    private bool PuedeAvanzarPaso()
    {
        return Time.time >= bloqueoHasta;
    }

    public void OcultarIndicadoresTutorial()
    {
        OcultarTodosLosIndicadores();
    }
}