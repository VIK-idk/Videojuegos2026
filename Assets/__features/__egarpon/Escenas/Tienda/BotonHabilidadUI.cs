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
        {
            boton = GetComponent<Button>();
        }

        if (boton != null)
        {
            boton.onClick.AddListener(SeleccionarHabilidad);
        }

        ActualizarVisual();
    }

    private void SeleccionarHabilidad()
    {
        if (panelHabilidadUI != null)
        {
            panelHabilidadUI.MostrarHabilidad(habilidad, this);
        }
    }

    public void ActualizarVisual()
    {
        if (habilidad == null || textoPrecio == null)
        {
            return;
        }

        if (habilidad.Comprado)
        {
            textoPrecio.text = "Comprado";
        }
        else
        {
            textoPrecio.text = habilidad.Precio.ToString();
        }
    }
}