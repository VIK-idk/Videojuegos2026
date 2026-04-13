using UnityEngine;
using UnityEngine.UI;

public class BotonHabilidadUI : MonoBehaviour
{
    public Habilidad habilidad;
    public PanelHabilidadUI panelHabilidadUI;
    public Text textoPrecio;
    public Button boton;

    private void Awake()
    {
        if (boton == null)
            boton = GetComponent<Button>();

        if (boton != null)
            boton.onClick.AddListener(SeleccionarHabilidad);
    }

    private void OnEnable()
    {
        ActualizarVisual();
    }

    private void SeleccionarHabilidad()
    {
        if (panelHabilidadUI != null)
            panelHabilidadUI.MostrarHabilidad(habilidad, this);
    }

    public void ActualizarVisual()
    {
        if (habilidad == null || textoPrecio == null)
            return;

        bool comprada = false;

        if (habilidad.Id == "x2_peces")
            comprada = SesionPartida.habilidadX2Comprada;

        if (habilidad.Id == "iman")
            comprada = SesionPartida.habilidadImanComprada;

        if (habilidad.Id == "quitar_strike")
            comprada = SesionPartida.habilidadQuitarStrikeComprada;

        if (comprada)
            textoPrecio.text = "Comprado";
        else
            textoPrecio.text = habilidad.Precio.ToString();
    }
}