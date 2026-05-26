using System.Collections;
using UnityEngine;

// ====================
// GESTOR ENCARGOS TEST
// ====================
public class GestorEncargosTest : MonoBehaviour
{
    // ====================
    // REFERENCIAS
    // ====================
    [Header("Managers")]
    [SerializeField] private PecesTestManager pecesManager;
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private GameManager gameManager;

    [Header("Rey Morsa")]
    [SerializeField] private ReyMorsaAnimacion reyMorsaAnimacion;

    [Header("UI")]
    [SerializeField] private UIEncargoLegacy uiEncargo;
    [SerializeField] private UIEstadoEncargoLegacy uiEstado;

    // ====================
    // AJUSTES
    // ====================
    [Header("Configuracion")]
    [SerializeField] private bool iniciarAutomaticamente = true;
    [SerializeField] private bool guardarRecompensas = true;
    [SerializeField] private int puntosPorEncargo = 100;
    [SerializeField] private float esperaEntreEncargos = 2f;
    [SerializeField] private float esperaPrimerEncargo = 3f;

    [Header("Inicio primer encargo")]
    [SerializeField] private bool esperarMovimientoParaPrimerEncargo = true;
    [SerializeField] private float inputMinimoParaEmpezar = 0.1f;

    // ====================
    // ESTADO
    // ====================
    [Header("Estado actual")]
    [SerializeField] private EncargoData encargoActual;

    [SerializeField] private int pecesRosasActuales = 0;
    [SerializeField] private int pecesAmarillosActuales = 0;
    [SerializeField] private int pecesVerdesActuales = 0;

    [SerializeField] private float tiempoRestante = 0f;
    [SerializeField] private bool sistemaIniciado = false;

    private bool encargoTerminado = false;
    private bool esperandoPrimerEncargo = false;

    // ====================
    // Debug y dificultad
    // ====================
    [SerializeField] private GestorProgresoJugador gestorProgresoJugador;

    [Header("DEBUG / TESTING")]
    [SerializeField] private bool permitirAtajosTesting = true;
    [SerializeField] private KeyCode teclaTiempoRapido = KeyCode.K;
    [SerializeField] private KeyCode teclaCompletarEncargo = KeyCode.L;
    [SerializeField] private KeyCode teclaSumarPuntos = KeyCode.N;
    [SerializeField] private float tiempoDebugForzado = 1f;

    [Header("Dificultad")]
    [SerializeField] private int pecesMinimosTotales = 5;
    [SerializeField] private int pecesMaximosTotales = 10;
    [SerializeField] private float tiempoMinimoEncargo = 15f;
    [SerializeField] private float tiempoMaximoEncargo = 24f;

    // ====================
    // TUTORIAL
    // ====================
    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private int pecesTutorial = 5;

    [SerializeField] private bool cargarGameplayAlCompletarTutorial = false;
    [SerializeField] private string escenaGameplayDespuesTutorial = "Gameplay";

    private bool tutorialActivo = false;

    // ====================
    // INICIO
    // ====================
    private void Start()
    {
        encargoActual = null;
        tiempoRestante = 0f;

        pecesRosasActuales = 0;
        pecesAmarillosActuales = 0;
        pecesVerdesActuales = 0;

        sistemaIniciado = false;
        encargoTerminado = false;
        esperandoPrimerEncargo = false;

        if (uiEncargo != null)
        {
            uiEncargo.OcultarInstantaneo();
        }

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (gestorProgresoJugador == null)
        {
            gestorProgresoJugador = FindFirstObjectByType<GestorProgresoJugador>();
        }

        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        }

        if (iniciarAutomaticamente)
        {
            IniciarSistema();
        }
    }

    // ====================
    // UPDATE
    // ====================
    private void Update()
    {
        if (!sistemaIniciado)
            return;

        if (esperandoPrimerEncargo)
            return;

        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        // ====================
        // SOLO TESTING
        // ====================
        if (permitirAtajosTesting)
        {
            if (Input.GetKeyDown(teclaTiempoRapido))
            {
                tiempoRestante = tiempoDebugForzado;

                if (uiEncargo != null)
                {
                    uiEncargo.ActualizarUI(
                        encargoActual,
                        tiempoRestante,
                        pecesRosasActuales,
                        pecesAmarillosActuales,
                        pecesVerdesActuales);
                }
            }

            if (Input.GetKeyDown(teclaCompletarEncargo))
            {
                if (encargoActual.enProceso)
                {
                    CompletarEncargo();
                    return;
                }
            }

            if (Input.GetKeyDown(teclaSumarPuntos))
            {
                if (gameManager != null)
                {
                    gameManager.SumarPuntos(250);
                }
            }
        }

        // En tutorial no baja el tiempo.
        if (!tutorialActivo)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0f)
            {
                tiempoRestante = 0f;
                FallarEncargo();
                return;
            }
        }

        if (uiEncargo != null)
        {
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }

    // ====================
    // INICIAR
    // ====================
    public void IniciarSistema()
    {
        if (sistemaIniciado)
            return;

        sistemaIniciado = true;

        if (tutorialManager != null && tutorialManager.DebeMostrarTutorial())
        {
            tutorialActivo = true;
            tutorialManager.IniciarTutorial();
            IniciarEncargoTutorial();
        }
        else
        {
            StartCoroutine(EsperarPrimerEncargo());
        }
    }

    // ====================
    // PRIMER ENCARGO
    // ====================
    private IEnumerator EsperarPrimerEncargo()
    {
        esperandoPrimerEncargo = true;

        if (uiEstado != null)
        {
            uiEstado.MostrarRecolecta(999f);
        }

        if (esperarMovimientoParaPrimerEncargo)
        {
            while (!JugadorHaEmpezadoAMoverse())
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(esperaPrimerEncargo);

        if (uiEstado != null)
        {
            uiEstado.Ocultar();
        }

        esperandoPrimerEncargo = false;
        IniciarNuevoEncargo();
    }

    private bool JugadorHaEmpezadoAMoverse()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        bool seMovio = inputMovimiento.magnitude > inputMinimoParaEmpezar;
        bool salto = Input.GetButtonDown("Saltar");

        return seMovio || salto;
    }

    // ====================
    // TERMINADO
    // ====================
    public bool EstaEncargoTerminado()
    {
        return encargoTerminado;
    }

    // ====================
    // NUEVO ENCARGO
    // ====================
    private void IniciarNuevoEncargo()
    {
        encargoActual = GenerarEncargoAleatorio();

        encargoActual.enProceso = true;
        encargoActual.completado = false;
        encargoActual.fallado = false;

        pecesRosasActuales = 0;
        pecesAmarillosActuales = 0;
        pecesVerdesActuales = 0;

        tiempoRestante = encargoActual.tiempoLimite;
        encargoTerminado = false;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();

            pecesManager.SetColoresActivos(
                encargoActual.pecesRosas > 0,
                encargoActual.pecesAmarillos > 0,
                encargoActual.pecesVerdes > 0);

            pecesManager.ActivarPecesAleatorios();
        }

        if (uiEncargo != null)
        {
            uiEncargo.Mostrar();
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }

    // ====================
    // GENERAR
    // ====================
    private EncargoData GenerarEncargoAleatorio()
    {
        EncargoData nuevo = new EncargoData();

        int suma = 0;

        while (suma < pecesMinimosTotales || suma > pecesMaximosTotales)
        {
            nuevo.pecesRosas = Random.Range(0, 6);
            nuevo.pecesAmarillos = Random.Range(0, 6);
            nuevo.pecesVerdes = Random.Range(0, 6);

            suma = nuevo.pecesRosas + nuevo.pecesAmarillos + nuevo.pecesVerdes;
        }

        nuevo.tiempoLimite = Random.Range(tiempoMinimoEncargo, tiempoMaximoEncargo);

        return nuevo;
    }

    // ====================
    // REGISTRAR PEZ
    // ====================
    public void RegistrarPezRecogido(ColorPez color, int cantidad)
    {
        if (!sistemaIniciado)
            return;

        if (encargoActual == null)
            return;

        if (!encargoActual.enProceso)
            return;

        if (encargoTerminado)
            return;

        if (cantidad < 1)
            cantidad = 1;

        bool seCompletoUnColor = false;
        ColorPez colorCompletado = color;

        if (color == ColorPez.Rosa)
        {
            int antes = pecesRosasActuales;
            pecesRosasActuales = Mathf.Min(pecesRosasActuales + cantidad, encargoActual.pecesRosas);

            if (antes < encargoActual.pecesRosas && pecesRosasActuales >= encargoActual.pecesRosas)
                seCompletoUnColor = true;
        }
        else if (color == ColorPez.Amarillo)
        {
            int antes = pecesAmarillosActuales;
            pecesAmarillosActuales = Mathf.Min(pecesAmarillosActuales + cantidad, encargoActual.pecesAmarillos);

            if (antes < encargoActual.pecesAmarillos && pecesAmarillosActuales >= encargoActual.pecesAmarillos)
                seCompletoUnColor = true;
        }
        else if (color == ColorPez.Verde)
        {
            int antes = pecesVerdesActuales;
            pecesVerdesActuales = Mathf.Min(pecesVerdesActuales + cantidad, encargoActual.pecesVerdes);

            if (antes < encargoActual.pecesVerdes && pecesVerdesActuales >= encargoActual.pecesVerdes)
                seCompletoUnColor = true;
        }

        if (seCompletoUnColor)
        {
            if (pecesManager != null)
            {
                pecesManager.DesactivarPecesActivosDeColor(colorCompletado);
            }

            ActualizarColoresPendientes();
        }

        if (uiEncargo != null)
        {
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);

            uiEncargo.ResaltarPezRecogido(color);
        }

        ComprobarEncargo();
    }

    // ====================
    // PENDIENTES
    // ====================
    private void ActualizarColoresPendientes()
    {
        if (pecesManager == null || encargoActual == null)
            return;

        bool rosaPendiente = pecesRosasActuales < encargoActual.pecesRosas;
        bool amarilloPendiente = pecesAmarillosActuales < encargoActual.pecesAmarillos;
        bool verdePendiente = pecesVerdesActuales < encargoActual.pecesVerdes;

        pecesManager.SetColoresActivos(rosaPendiente, amarilloPendiente, verdePendiente);
    }

    // ====================
    // COMPROBAR
    // ====================
    private void ComprobarEncargo()
    {
        if (pecesRosasActuales >= encargoActual.pecesRosas &&
            pecesAmarillosActuales >= encargoActual.pecesAmarillos &&
            pecesVerdesActuales >= encargoActual.pecesVerdes)
        {
            CompletarEncargo();
        }
    }

    // ====================
    // COMPLETAR
    // ====================
    private void CompletarEncargo()
    {
        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.completado = true;

        if (reyMorsaAnimacion != null)
        {
            reyMorsaAnimacion.Aplaudir();
        }

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (guardarRecompensas)
        {
            if (gameManager != null)
            {
                gameManager.SumarPuntos(puntosPorEncargo);
            }

            if (gestorProgresoJugador != null)
            {
                gestorProgresoJugador.DarMonedasPorEncargo();
            }
        }

        if (uiEstado != null)
        {
            uiEstado.MostrarCompletado();
        }

        if (tutorialActivo)
        {
            tutorialActivo = false;
            StartCoroutine(FinalizarTutorialYEmpezarJuegoNormal());
        }
        else
        {
            StartCoroutine(EsperarYSiguiente());
        }
    }

    // ====================
    // FALLAR
    // ====================
    private void FallarEncargo()
    {
        if (encargoActual == null)
            return;

        if (encargoTerminado)
            return;

        encargoTerminado = true;

        encargoActual.enProceso = false;
        encargoActual.fallado = true;

        if (reyMorsaAnimacion != null)
        {
            reyMorsaAnimacion.Enojar();
        }

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        bool ultimoStrike = false;

        if (strikeManager != null)
        {
            ultimoStrike = strikeManager.GetCurrentStrikes() + 1 >= strikeManager.GetMaxStrikes();
            strikeManager.AddStrike(false);
        }

        if (uiEstado != null)
        {
            if (ultimoStrike)
            {
                uiEstado.MostrarMensajePersonalizado(
                    "ENCARGO FALLIDO\nVuelve a tu celda a descansar",
                    Color.red,
                    2f
                );
            }
            else
            {
                uiEstado.MostrarFallado();
            }
        }

        if (ultimoStrike)
        {
            StartCoroutine(EsperarYVolverALobby());
        }
        else
        {
            StartCoroutine(EsperarYSiguiente());
        }
    }

    // ====================
    // ESPERAR
    // ====================
    private IEnumerator EsperarYSiguiente()
    {
        yield return new WaitForSeconds(esperaEntreEncargos);

        if (uiEncargo != null)
        {
            uiEncargo.Ocultar();
        }

        yield return new WaitForSeconds(0.5f);

        IniciarNuevoEncargo();
    }

    private IEnumerator EsperarYVolverALobby()
    {
        yield return new WaitForSeconds(2f);

        if (uiEncargo != null)
        {
            uiEncargo.Ocultar();
        }

        if (strikeManager != null)
        {
            strikeManager.IrALobby();
        }
    }

    // ====================
    // TUTORIAL
    // ====================
    private void IniciarEncargoTutorial()
    {
        encargoActual = new EncargoData();

        encargoActual.pecesRosas = pecesTutorial;
        encargoActual.pecesAmarillos = 0;
        encargoActual.pecesVerdes = 0;

        encargoActual.enProceso = true;
        encargoActual.completado = false;
        encargoActual.fallado = false;

        pecesRosasActuales = 0;
        pecesAmarillosActuales = 0;
        pecesVerdesActuales = 0;

        tiempoRestante = Mathf.Infinity;
        encargoTerminado = false;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
            pecesManager.SetColoresActivos(true, false, false);
            pecesManager.ActivarTodosLosPecesTutorial();
        }

        if (uiEncargo != null)
        {
            uiEncargo.Mostrar();
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }

    public void SaltarTutorialYEmpezarJuegoNormal()
    {
        tutorialActivo = false;

        if (pecesManager != null)
            pecesManager.ReiniciarTodosLosPeces();

        if (cargarGameplayAlCompletarTutorial)
        {
            SceneLoader.CargarEscena(escenaGameplayDespuesTutorial);
            return;
        }

        IniciarNuevoEncargo();
    }

    private IEnumerator FinalizarTutorialYEmpezarJuegoNormal()
    {
        if (tutorialManager != null)
        {
            tutorialManager.OcultarIndicadoresTutorial();
            tutorialManager.MarcarTutorialComoCompletado();
            yield return StartCoroutine(tutorialManager.MostrarMensajeFinalTutorial());
        }

        if (uiEncargo != null)
            uiEncargo.Ocultar();

        yield return new WaitForSeconds(0.5f);

        if (cargarGameplayAlCompletarTutorial)
        {
            SceneLoader.CargarEscena(escenaGameplayDespuesTutorial);
            yield break;
        }

        IniciarNuevoEncargo();
    }

    public void ActivarPecesTutorial()
    {
        if (!tutorialActivo)
            return;

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
            pecesManager.SetColoresActivos(true, false, false);
            pecesManager.ActivarTodosLosPecesTutorial();
        }

        if (uiEncargo != null)
        {
            uiEncargo.Mostrar();
            uiEncargo.ActualizarUI(
                encargoActual,
                tiempoRestante,
                pecesRosasActuales,
                pecesAmarillosActuales,
                pecesVerdesActuales);
        }
    }
}