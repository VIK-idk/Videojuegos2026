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
    // RENDER DEL PEZ
    // ====================
    [Header("Render del pez")]
    [SerializeField] private Renderer renderInterior;

    // ====================
    // COLORES
    // ====================
    [Header("Colores visuales")]
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
    // UNITY
    // ====================
    private void Awake()
    {
        if (renderInterior == null)
        {
            BuscarRenderInterior();
        }

        ActualizarVisual();
    }

    private void OnValidate()
    {
        if (renderInterior == null)
        {
            BuscarRenderInterior();
        }
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
        if (renderInterior == null)
        {
            BuscarRenderInterior();
        }

        if (renderInterior == null)
            return;

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

    // ====================
    // BUSCAR RENDER
    // ====================
    private void BuscarRenderInterior()
    {
        Transform pezHijo = BuscarHijoPorNombre(transform, "Pez");

        if (pezHijo != null)
        {
            renderInterior = pezHijo.GetComponent<Renderer>();

            if (renderInterior == null)
            {
                renderInterior = pezHijo.GetComponentInChildren<Renderer>();
            }
        }

        if (renderInterior == null)
        {
            renderInterior = GetComponentInChildren<Renderer>();
        }
    }

    private Transform BuscarHijoPorNombre(Transform padre, string nombre)
    {
        if (padre == null)
            return null;

        if (padre.name == nombre)
            return padre;

        for (int i = 0; i < padre.childCount; i++)
        {
            Transform resultado = BuscarHijoPorNombre(padre.GetChild(i), nombre);

            if (resultado != null)
                return resultado;
        }

        return null;
    }
}