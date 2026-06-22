using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestorProgresoJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;

    [Header("UI monedas gameplay")]
    [SerializeField] private LobbyMonedasUI monedasGameplayUI;

    [Header("UI antigua opcional")]
    [SerializeField] private Text textoMonedasGanadas;

    [Header("Recompensas")]
    [SerializeField] private int monedasPorEncargo = 15;
    [SerializeField] private float duracionMensajeMonedas = 2f;

    private Coroutine rutinaMensaje;
    private bool intentoGuardado = false;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (monedasGameplayUI == null)
            monedasGameplayUI = FindFirstObjectByType<LobbyMonedasUI>();

        if (monedasGameplayUI != null)
            monedasGameplayUI.ActualizarMonedas(true);

        if (textoMonedasGanadas != null)
            textoMonedasGanadas.enabled = false;
    }

    public void DarMonedasPorEncargo()
    {
        AgregarMonedas(monedasPorEncargo);
    }

    public void AgregarMonedas(int cantidad)
    {
        if (cantidad <= 0)
            return;

        int monedasAntes = SesionPartida.monedas;

        SesionPartida.monedas += cantidad;

        int monedasDespues = SesionPartida.monedas;

        if (monedasGameplayUI != null)
        {
            monedasGameplayUI.MostrarGanancia(cantidad, monedasAntes, monedasDespues);
        }
        else
        {
            MostrarMensajeMonedasLegacy(cantidad, monedasAntes, monedasDespues);
        }
    }

    public int GetMonedasTotales()
    {
        return SesionPartida.monedas;
    }

    public void RegistrarIntentoActual()
    {
        if (intentoGuardado)
            return;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
            return;

        int puntosActuales = gameManager.GetPuntosActuales();
        ProgresoJugador.RegistrarPuntuacionIntento(puntosActuales);

        intentoGuardado = true;
    }

    private void MostrarMensajeMonedasLegacy(int cantidadGanada, int monedasAntes, int monedasDespues)
    {
        if (textoMonedasGanadas == null)
            return;

        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        rutinaMensaje = StartCoroutine(MostrarMensajeMonedasCoroutine(cantidadGanada, monedasDespues));
    }

    private IEnumerator MostrarMensajeMonedasCoroutine(int cantidadGanada, int monedasDespues)
    {
        textoMonedasGanadas.text = monedasDespues + " (+" + cantidadGanada + ")";
        textoMonedasGanadas.enabled = true;

        yield return new WaitForSecondsRealtime(duracionMensajeMonedas);

        textoMonedasGanadas.enabled = false;
        rutinaMensaje = null;
    }
}