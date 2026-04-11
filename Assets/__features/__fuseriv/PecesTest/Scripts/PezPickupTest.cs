using UnityEngine;

// ====================
// PICKUP TEST
// ====================
public class PezPickupTest : MonoBehaviour
{
    private bool recogido = false;

    private void OnEnable()
    {
        recogido = false;
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

        if (pez == null)
            return;

        if (manager == null)
            return;

        recogido = true;
        manager.ProcesarRecogida(pez);
    }
}