using UnityEngine;
using UnityEngine.UI;

public class MejorPuntuacionMural : MonoBehaviour
{
    [SerializeField] private Text textoMejorPuntuacion;
    [SerializeField] private string prefijo = "Mejor puntuacion: ";

    private void Start()
    {
        ActualizarTexto();
    }

    public void ActualizarTexto()
    {
        if (textoMejorPuntuacion == null)
            return;

        textoMejorPuntuacion.text = prefijo + ProgresoJugador.GetMejorPuntuacion();
    }
}