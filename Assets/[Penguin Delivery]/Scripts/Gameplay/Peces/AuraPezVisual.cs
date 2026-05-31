using System.Collections;
using UnityEngine;

public class AuraPezVisual : MonoBehaviour
{
    [Header("Render")]
    [SerializeField] private Renderer auraRenderer;

    [Header("Fade")]
    [SerializeField] private float opacidadNormal = 0.75f;
    [SerializeField] private float duracionDesvanecer = 0.45f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private Material materialInstanciado;
    private Coroutine rutinaFade;
    private Color colorActual = Color.white;

    private void Awake()
    {
        if (auraRenderer == null)
            auraRenderer = GetComponentInChildren<Renderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (auraRenderer != null)
        {
            materialInstanciado = auraRenderer.material;
        }
    }

    private void OnEnable()
    {
        ReiniciarAura();
    }

    public void ConfigurarColor(Color nuevoColor)
    {
        colorActual = nuevoColor;
        colorActual.a = opacidadNormal;

        AplicarColor(colorActual);
    }

    public void ReiniciarAura()
    {
        if (rutinaFade != null)
        {
            StopCoroutine(rutinaFade);
            rutinaFade = null;
        }

        gameObject.SetActive(true);

        Color color = colorActual;
        color.a = opacidadNormal;
        AplicarColor(color);
    }

    public void Desvanecer()
    {
        Desvanecer(duracionDesvanecer);
    }

    public void Desvanecer(float duracion)
    {
        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        rutinaFade = StartCoroutine(DesvanecerCoroutine(duracion));
    }

    private IEnumerator DesvanecerCoroutine(float duracion)
    {
        if (duracion <= 0f)
            duracion = 0.1f;

        Color inicio = colorActual;
        inicio.a = opacidadNormal;

        Color fin = colorActual;
        fin.a = 0f;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;
            Color color = Color.Lerp(inicio, fin, t);

            AplicarColor(color);

            yield return null;
        }

        AplicarColor(fin);

        if (animator != null)
            animator.enabled = false;

        gameObject.SetActive(false);
        rutinaFade = null;
    }

    private void AplicarColor(Color color)
    {
        if (materialInstanciado == null)
            return;

        if (materialInstanciado.HasProperty("_BaseColor"))
        {
            materialInstanciado.SetColor("_BaseColor", color);
        }

        if (materialInstanciado.HasProperty("_Color"))
        {
            materialInstanciado.SetColor("_Color", color);
        }

        if (materialInstanciado.HasProperty("_EmissionColor"))
        {
            Color emision = color * 1.5f;
            emision.a = color.a;
            materialInstanciado.SetColor("_EmissionColor", emision);
        }
    }
}