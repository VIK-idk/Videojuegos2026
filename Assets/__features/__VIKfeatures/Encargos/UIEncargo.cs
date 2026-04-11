using UnityEngine;
using TMPro;
using System.Collections;

public class UIEncargo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoPeces;
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private CanvasGroup canvasGroup;

    public void ActualizarUI(Encargo encargo, float tiempo,
        int a, int r, int v)
    {
        string texto = "";

        if (encargo.pecesAmarillos > 0)
            texto += "Peces amarillos " + a + "/" + encargo.pecesAmarillos + "\n";

        if (encargo.pecesRosas > 0)
            texto += "Peces rosas " + r + "/" + encargo.pecesRosas + "\n";

        if (encargo.pecesVerdes > 0)
            texto += "Peces verdes " + v + "/" + encargo.pecesVerdes + "\n";

        textoPeces.text = texto;
        textoTiempo.text = "Tiempo: " + tiempo.ToString("F1");
    }

    public void Mostrar()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0, 1));
    }

    public void Ocultar()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1, 0));
    }

    public void OcultarInstantaneo()
    {
        canvasGroup.alpha = 0;
    }

    IEnumerator Fade(float inicio, float fin)
    {
        float t = 0;
        float duracion = 0.5f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(inicio, fin, t / duracion);
            yield return null;
        }

        canvasGroup.alpha = fin;
    }
}