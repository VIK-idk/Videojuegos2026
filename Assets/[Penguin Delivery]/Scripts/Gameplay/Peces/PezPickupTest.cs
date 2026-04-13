using UnityEngine;

// ====================
// PICKUP TEST
// ====================
public class PezPickupTest : MonoBehaviour
{
    private bool recogido = false;
    private SphereCollider triggerCollider;
    private float radioBase = 0f;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider != null)
            radioBase = triggerCollider.radius;
    }

    private void OnEnable()
    {
        recogido = false;

        if (triggerCollider == null)
            triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider != null && radioBase <= 0f)
            radioBase = triggerCollider.radius;
    }

    public void SetMultiplicadorRecogida(float multiplicador)
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider == null)
            return;

        if (radioBase <= 0f)
            radioBase = triggerCollider.radius;

        triggerCollider.radius = radioBase * multiplicador;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogido)
            return;

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
            return;

        Pez pez = GetComponentInParent<Pez>();
        PecesTestManager manager = FindFirstObjectByType<PecesTestManager>();

        if (pez == null || manager == null)
            return;

        recogido = true;
        manager.ProcesarRecogida(pez);
    }
}