using System.Collections;
using UnityEngine;

public class PezPickupTest : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Pez pez;
    [SerializeField] private Animator burbujaAnimator;
    [SerializeField] private Animator pezAnimator;

    [Header("Animaciones")]
    [SerializeField] private string triggerExplotarBurbuja = "Explotar";
    [SerializeField] private string triggerRecolectarPez = "Recolectar";
    [SerializeField] private float duracionAntesDeRecoger = 0.35f;

    private bool recogido = false;
    private SphereCollider triggerCollider;
    private float radioBase = 0f;

    private PecesTestManager manager;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider != null)
        {
            radioBase = triggerCollider.radius;
        }

        if (pez == null)
        {
            pez = GetComponentInParent<Pez>();
        }

        Transform raiz = pez != null ? pez.transform : transform.root;

        if (burbujaAnimator == null)
        {
            Transform burbuja = BuscarHijoPorNombre(raiz, "Burbuja");

            if (burbuja != null)
            {
                burbujaAnimator = burbuja.GetComponent<Animator>();
            }
        }

        if (pezAnimator == null)
        {
            Transform pezVisual = BuscarHijoPorNombre(raiz, "Pez");

            if (pezVisual != null)
            {
                pezAnimator = pezVisual.GetComponent<Animator>();
            }
        }

        manager = FindFirstObjectByType<PecesTestManager>();
    }

    private void OnEnable()
    {
        recogido = false;

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;

            if (radioBase <= 0f)
            {
                radioBase = triggerCollider.radius;
            }
        }

        ReiniciarAnimator(burbujaAnimator, triggerExplotarBurbuja);
        ReiniciarAnimator(pezAnimator, triggerRecolectarPez);
    }

    public void SetMultiplicadorRecogida(float multiplicador)
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
        }

        if (triggerCollider == null)
            return;

        if (radioBase <= 0f)
        {
            radioBase = triggerCollider.radius;
        }

        triggerCollider.radius = radioBase * multiplicador;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogido)
            return;

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
            return;

        StartCoroutine(SecuenciaRecogerPez());
    }

    private IEnumerator SecuenciaRecogerPez()
    {
        recogido = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (burbujaAnimator != null)
        {
            burbujaAnimator.ResetTrigger(triggerExplotarBurbuja);
            burbujaAnimator.SetTrigger(triggerExplotarBurbuja);
        }

        if (pezAnimator != null)
        {
            pezAnimator.ResetTrigger(triggerRecolectarPez);
            pezAnimator.SetTrigger(triggerRecolectarPez);
        }

        yield return new WaitForSeconds(duracionAntesDeRecoger);

        if (manager == null)
        {
            manager = FindFirstObjectByType<PecesTestManager>();
        }

        if (pez == null)
        {
            pez = GetComponentInParent<Pez>();
        }

        if (pez != null && manager != null)
        {
            manager.ProcesarRecogida(pez);
        }
    }

    private void ReiniciarAnimator(Animator animator, string trigger)
    {
        if (animator == null)
            return;

        animator.ResetTrigger(trigger);
        animator.Rebind();
        animator.Update(0f);
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