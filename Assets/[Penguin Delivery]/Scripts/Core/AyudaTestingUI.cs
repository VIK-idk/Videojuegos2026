using UnityEngine;
using UnityEngine.UI;

public class AyudaTestingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelAtajos;
    [SerializeField] private Text textoAviso;

    [Header("Input")]
    [SerializeField] private KeyCode teclaMostrarOcultar = KeyCode.O;

    // ====================
    // SOLO GUARDA EL ESTADO ENTRE ESCENAS
    // ====================
    private static bool visibleGuardado = false;

    private const string AVISO_MOSTRAR = "Pulsa O para ver los atajos de testing";
    private const string AVISO_OCULTAR = "Pulsa O para ocultar los atajos";

    private void Start()
    {
        AplicarEstadoGuardado();
    }

    private void Update()
    {
        if (Input.GetKeyDown(teclaMostrarOcultar))
        {
            visibleGuardado = !visibleGuardado;
            AplicarEstadoGuardado();
        }
    }

    private void AplicarEstadoGuardado()
    {
        if (panelAtajos != null)
            panelAtajos.SetActive(visibleGuardado);

        if (textoAviso != null)
        {
            if (visibleGuardado)
                textoAviso.text = AVISO_OCULTAR;
            else
                textoAviso.text = AVISO_MOSTRAR;
        }
    }
}