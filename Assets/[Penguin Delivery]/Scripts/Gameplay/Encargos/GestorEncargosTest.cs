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

    [Header("UI")]
    [SerializeField] private UIEncargoLegacy uiEncargo;
    [SerializeField] private UIEstadoEncargoLegacy uiEstado;

    // ====================
    // AJUSTES
    // ====================
    [Header("Configuracion")]
    [SerializeField] private bool iniciarAutomaticamente = true;
    [SerializeField] private int puntosPorEncargo = 100;
    [SerializeField] private float esperaEntreEncargos = 2f;
    [SerializeField] private float esperaPrimerEncargo = 3f;
    [SerializeField] private float tiempoMensajeRecolecta = 3f;

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
    // Debug y dificultad (solo para pruebas)
    // ====================
    [SerializeField] private GestorProgresoJugador gestorProgresoJugador;
    [Header("DEBUG / TESTING")]
    [SerializeField] private bool permitirAtajosTesting = true;

    // K = bajar el tiempo restante del encargo a 1 segundo
    [SerializeField] private KeyCode teclaTiempoRapido = KeyCode.K;

    // L = completar el encargo al instante
    [SerializeField] private KeyCode teclaCompletarEncargo = KeyCode.L;

    // N = sumar 250 puntos de golpe
    [SerializeField] private KeyCode teclaSumarPuntos = KeyCode.N;

    // Valor usado por K
    [SerializeField] private float tiempoDebugForzado = 1f;
    // ====================

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
    [SerializeField] private int pecesTutorial = 3;

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

        if (permitirAtajosTesting && Input.GetKeyDown(teclaTiempoRapido))
        {
            if (encargoActual != null && !encargoTerminado)
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
        }

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

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            FallarEncargo();
            return;
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
        // ====================
        // SOLO TESTING
        // Estos atajos son solo para pruebas durante desarrollo.
        // Borrarlos o desactivarlos en la versión final.
        // ====================
        if (permitirAtajosTesting)
        {
            // K = deja el encargo a 1 segundo para probar fallos o cambios rápidos
            if (Input.GetKeyDown(teclaTiempoRapido))
            {
                if (encargoActual != null && !encargoTerminado)
                {
                    tiempoRestante = tiempoDebugForzado; // SOLO TESTING

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
            }

            // L = completa el encargo actual al instante
            if (Input.GetKeyDown(teclaCompletarEncargo))
            {
                if (encargoActual != null && !encargoTerminado && encargoActual.enProceso)
                {
                    CompletarEncargo(); // SOLO TESTING
                    return; // SOLO TESTING, evita seguir ejecutando el Update este frame
                }
            }

            // N = suma 250 puntos de golpe
            if (Input.GetKeyDown(teclaSumarPuntos))
            {
                if (gameManager != null)
                {
                    gameManager.SumarPuntos(250); // SOLO TESTING
                }
            }
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
        if (uiEstado != null)
        {
            uiEstado.MostrarRecolecta(tiempoMensajeRecolecta);
        }

        yield return new WaitForSeconds(esperaPrimerEncargo);

        esperandoPrimerEncargo = false;
        IniciarNuevoEncargo();
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

        if (tutorialActivo && tutorialManager != null)
        {
            tutorialManager.NotificarPrimerPezRecogido();
        }

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

        if (pecesManager != null)
        {
            pecesManager.ReiniciarTodosLosPeces();
        }

        if (gameManager != null)
        {
            gameManager.SumarPuntos(puntosPorEncargo);
        }
        if (gestorProgresoJugador != null)
        {
            gestorProgresoJugador.DarMonedasPorEncargo();
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

    //=====================
    // TUTORIAL
    //=====================
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

    public void SaltarTutorialYEmpezarJuegoNormal()
    {
        if (!tutorialActivo)
            return;

        tutorialActivo = false;

        if (pecesManager != null)
            pecesManager.ReiniciarTodosLosPeces();

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

        IniciarNuevoEncargo();
    }
}