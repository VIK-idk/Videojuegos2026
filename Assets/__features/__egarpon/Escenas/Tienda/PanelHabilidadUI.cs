using UnityEngine;
using UnityEngine.UI;

public class PanelHabilidadUI : MonoBehaviour
{
    [SerializeField] private Image iconoHabilidad;
    [SerializeField] private Button botonComprar;

    [SerializeField] private Color colorComprado = Color.gray;

    private bool comprado = false; 

    // click -> Awake
    private void Awake()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }
    }

    private void Comprar()
    {
        if (comprado)
        {
            return;
        }

        comprado = true;

        Debug.Log("gracais por el pene guapeton.");

        if (iconoHabilidad != null)
        {
            iconoHabilidad.color = colorComprado;
        }
    }
}