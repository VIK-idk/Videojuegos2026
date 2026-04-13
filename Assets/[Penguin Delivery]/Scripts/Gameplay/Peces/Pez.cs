using UnityEngine;

// ====================
// PEZ
// Guarda el color del pez, su id y actualiza su aspecto
// ====================
public class Pez : MonoBehaviour
{
    // ====================
    // DATOS
    // ====================
    [Header("Datos")]
    [SerializeField] private ColorPez colorPez;
    [SerializeField] private int colorId;

    // ====================
    // RENDERS
    // ====================
    [Header("Renders")]
    [SerializeField] private Renderer renderExterior;
    [SerializeField] private Renderer renderInterior;

    // ====================
    // COLORES
    // ====================
    [Header("Colores visuales")]
    [SerializeField] private Color colorExterior = new Color(0.3f, 0.6f, 1f, 0.35f);
    [SerializeField] private Color colorRosa = new Color(1f, 0.4f, 0.8f, 1f);
    [SerializeField] private Color colorAmarillo = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color colorVerde = new Color(0.3f, 1f, 0.4f, 1f);

    // ====================
    // PROPIEDADES
    // ====================
    public ColorPez GetColorPez()
    {
        return colorPez;
    }

    public int GetColorId()
    {
        return colorId;
    }

    // ====================
    // CONFIGURAR
    // ====================
    public void ConfigurarPez(ColorPez nuevoColor)
    {
        colorPez = nuevoColor;
        colorId = (int)nuevoColor;
        ActualizarVisual();
    }

    // ====================
    // VISUAL
    // ====================
    public void ActualizarVisual()
    {
        if (renderExterior != null)
        {
            renderExterior.material.color = colorExterior;
        }

        if (renderInterior != null)
        {
            if (colorPez == ColorPez.Rosa)
            {
                renderInterior.material.color = colorRosa;
            }
            else if (colorPez == ColorPez.Amarillo)
            {
                renderInterior.material.color = colorAmarillo;
            }
            else
            {
                renderInterior.material.color = colorVerde;
            }
        }
    }
}