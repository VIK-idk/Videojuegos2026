using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ====================
// GESTOR PROGRESO JUGADOR
// ====================
public class GestorProgresoJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
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

        SesionPartida.monedas += cantidad;
        MostrarMensajeMonedas(cantidad);
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

    private void MostrarMensajeMonedas(int cantidadGanada)
    {
        if (textoMonedasGanadas == null)
            return;

        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        rutinaMensaje = StartCoroutine(MostrarMensajeMonedasCoroutine(cantidadGanada));
    }

    private IEnumerator MostrarMensajeMonedasCoroutine(int cantidadGanada)
    {
        textoMonedasGanadas.text = "Monedas: " + SesionPartida.monedas + " (+" + cantidadGanada + ")";
        textoMonedasGanadas.enabled = true;

        yield return new WaitForSeconds(duracionMensajeMonedas);

        textoMonedasGanadas.enabled = false;
        rutinaMensaje = null;
    }
}