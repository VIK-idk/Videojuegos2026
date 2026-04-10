using UnityEngine;
using UnityEngine.UI;

public class PanelHabilidadUI : MonoBehaviour
{
    [Header("Elementos del panel")]
    public Image iconoHabilidad;
    public Text textoNombre;
    public Text textoDescripcion;
    public Text textoPrecio;
    public Button botonComprar;

    [Header("Configuración")]
    public Color colorComprado = Color.gray;
    public int dineroJugador = 100;

    // Habilidad seleccionada actualmente
    private Habilidad habilidadActual;

    // Botón de la izquierda que seleccionó la habilidad
    private BotonHabilidadUI botonActual;

    private void Awake()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }
    }

    // Rellena el panel con los datos de la habilidad seleccionada
    public void MostrarHabilidad(Habilidad habilidadSeleccionada, BotonHabilidadUI botonSeleccionado)
    {
        if (habilidadSeleccionada == null)
        {
            return;
        }

        habilidadActual = habilidadSeleccionada;
        botonActual = botonSeleccionado;

        if (iconoHabilidad != null)
        {
            iconoHabilidad.sprite = habilidadActual.Icono;

            if (habilidadActual.Comprado)
            {
                iconoHabilidad.color = colorComprado;
            }
            else
            {
                iconoHabilidad.color = Color.white;
            }
        }

        if (textoNombre != null)
        {
            textoNombre.text = habilidadActual.Nombre;
        }

        if (textoDescripcion != null)
        {
            textoDescripcion.text = habilidadActual.Descripcion;
        }

        if (textoPrecio != null)
        {
            if (habilidadActual.Comprado)
            {
                textoPrecio.text = "Comprado";
            }
            else
            {
                textoPrecio.text = habilidadActual.Precio.ToString();
            }
        }
        
    }

    private void Comprar()
    {
        if (habilidadActual == null)
        {
            Debug.Log("No hay ninguna habilidad seleccionada");
            return;
        }

        if (habilidadActual.Comprado)
        {
            Debug.Log("Este item ya se ha comprado");
            return;
        }

        if (dineroJugador < habilidadActual.Precio)
        {
            Debug.Log("No tienes dinero suficiente");
            return;
        }

        // Restar dinero
        dineroJugador = dineroJugador - habilidadActual.Precio;

        // Marcar como comprada
        habilidadActual.Comprado = true;

        // Cambiar visual del panel
        if (iconoHabilidad != null)
        {
            iconoHabilidad.color = colorComprado;
        }

        if (textoPrecio != null)
        {
            textoPrecio.text = "Comprado";
        }

        // Actualizar el botón de la izquierda
        if (botonActual != null)
        {
            botonActual.ActualizarVisual();
        }

        Debug.Log("Compra realizada. Dinero restante: " + dineroJugador);
    }
}