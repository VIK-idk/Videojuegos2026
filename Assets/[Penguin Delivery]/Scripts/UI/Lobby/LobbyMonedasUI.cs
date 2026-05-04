using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMonedasUI : MonoBehaviour
{
    [SerializeField] private Text textoMonedas;
    [SerializeField] private Text textoGasto;
    [SerializeField] private float duracionTextoGasto = 2f;

    private Coroutine rutinaGasto;

    private void Start()
    {
        ActualizarMonedas();
        OcultarGasto();
    }

    private void OnEnable()
    {
        ActualizarMonedas();
        OcultarGasto();
    }

    public void ActualizarMonedas()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "Monedas: " + SesionPartida.monedas;
        }
    }

    public void MostrarGasto(int cantidad)
    {
        if (textoGasto == null)
            return;

        if (rutinaGasto != null)
            StopCoroutine(rutinaGasto);

        rutinaGasto = StartCoroutine(MostrarGastoCoroutine(cantidad));
    }

    private IEnumerator MostrarGastoCoroutine(int cantidad)
    {
        textoGasto.text = "(-" + cantidad + ")";
        textoGasto.color = Color.red;
        textoGasto.enabled = true;

        yield return new WaitForSeconds(duracionTextoGasto);

        textoGasto.enabled = false;
        rutinaGasto = null;
    }

    private void OcultarGasto()
    {
        if (textoGasto != null)
            textoGasto.enabled = false;
    }
}