using UnityEngine;
using UnityEngine.UI;

// ====================
// SLOT HABILIDAD UI
// ====================
public class HabilidadSlotUI : MonoBehaviour
{
    [SerializeField] private Image fondo;
    [SerializeField] private Text textoTiempo;
    [SerializeField] private Text textoTitulo;
    [SerializeField] private Text textoTecla;

    [Header("Colores")]
    [SerializeField] private Color colorDisponible = new Color(0.65f, 0.45f, 0.15f, 0.95f);
    [SerializeField] private Color colorActiva = new Color(0.25f, 0.7f, 0.25f, 0.95f);
    [SerializeField] private Color colorCooldown = new Color(0.45f, 0.45f, 0.45f, 0.95f);
    [SerializeField] private Color colorBloqueada = new Color(0.35f, 0.35f, 0.35f, 0.95f);
    [SerializeField] private Color colorVacia = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    [SerializeField] private Color colorUsada = new Color(0.15f, 0.15f, 0.15f, 0.8f);

    private void AplicarEstado(string titulo, string tecla, bool mostrarTiempo, float tiempo, Color color)
    {
        if (fondo != null)
            fondo.color = color;

        if (textoTitulo != null)
            textoTitulo.text = titulo;

        if (textoTecla != null)
            textoTecla.text = tecla;

        if (textoTiempo != null)
        {
            textoTiempo.gameObject.SetActive(mostrarTiempo);

            if (mostrarTiempo)
                textoTiempo.text = Mathf.CeilToInt(Mathf.Max(tiempo, 0f)).ToString();
        }
    }

    public void MostrarVacio()
    {
        AplicarEstado("Slot de Habilidad Vacio", "", false, 0f, colorVacia);
    }

    public void MostrarDisponible(string titulo, string tecla)
    {
        AplicarEstado(titulo, tecla, false, 0f, colorDisponible);
    }

    public void MostrarActiva(string titulo, string tecla, float tiempo)
    {
        AplicarEstado(titulo, tecla, true, tiempo, colorActiva);
    }

    public void MostrarCooldown(string titulo, string tecla, float tiempo)
    {
        AplicarEstado(titulo, tecla, true, tiempo, colorCooldown);
    }

    public void MostrarBloqueada(string titulo, string tecla)
    {
        AplicarEstado(titulo, tecla, false, 0f, colorBloqueada);
    }

    public void MostrarUsada(string titulo)
    {
        AplicarEstado(titulo, "Usada", false, 0f, colorUsada);
    }
}