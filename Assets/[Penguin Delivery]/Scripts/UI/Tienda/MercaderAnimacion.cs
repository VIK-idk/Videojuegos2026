using System.Collections;
using UnityEngine;

public class MercaderAnimacion : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Duraciones")]
    [SerializeField] private float duracionDespedida = 0.45f;
    [SerializeField] private float duracionEnojado = 3f;

    private Coroutine rutinaEnojado;

    private const string TRIGGER_LLEGADA = "Llegada";
    private const string TRIGGER_ALEGRE = "Alegre";
    private const string TRIGGER_ENOJADO = "Enojado";
    private const string TRIGGER_DESPEDIDA = "Despedida";
    private const string BOOL_MANTENER_ENOJADO = "MantenerEnojado";

    private bool enojadoActivo = false;
    private float tiempoEnojadoRestante = 0f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        enojadoActivo = false;
        tiempoEnojadoRestante = 0f;

        if (animator != null)
        {
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
            animator.ResetTrigger(TRIGGER_LLEGADA);
            animator.ResetTrigger(TRIGGER_ALEGRE);
            animator.ResetTrigger(TRIGGER_ENOJADO);
            animator.ResetTrigger(TRIGGER_DESPEDIDA);
        }
    }

    public void ReproducirLlegada()
    {
        if (animator == null)
            return;

        DetenerEnojado();

        animator.SetTrigger(TRIGGER_LLEGADA);
    }

    public void PedirAlegre()
    {
        if (animator == null)
            return;

        DetenerEnojado();

        animator.SetTrigger(TRIGGER_ALEGRE);
    }

    public void PedirEnojado()
    {
        if (animator == null)
            return;

        tiempoEnojadoRestante = duracionEnojado;
        animator.SetBool(BOOL_MANTENER_ENOJADO, true);

        if (enojadoActivo)
        {
            // Si ya está enojado, NO reinicia la animación.
            // Solo vuelve a poner el contador en 3 segundos.
            return;
        }

        animator.SetTrigger(TRIGGER_ENOJADO);
        rutinaEnojado = StartCoroutine(EnojadoTemporal());
    }

    private IEnumerator EnojadoTemporal()
    {
        enojadoActivo = true;

        while (tiempoEnojadoRestante > 0f)
        {
            tiempoEnojadoRestante -= Time.unscaledDeltaTime;
            yield return null;
        }

        enojadoActivo = false;
        rutinaEnojado = null;

        if (animator != null)
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
    }

    public IEnumerator ReproducirDespedida()
    {
        if (animator == null)
            yield break;

        DetenerEnojado();

        animator.SetTrigger(TRIGGER_DESPEDIDA);

        yield return new WaitForSecondsRealtime(duracionDespedida);
    }

    private void DetenerEnojado()
    {
        if (rutinaEnojado != null)
        {
            StopCoroutine(rutinaEnojado);
            rutinaEnojado = null;
        }

        enojadoActivo = false;
        tiempoEnojadoRestante = 0f;

        if (animator != null)
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
    }
}