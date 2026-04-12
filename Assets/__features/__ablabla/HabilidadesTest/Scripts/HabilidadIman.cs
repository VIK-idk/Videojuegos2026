using UnityEngine;

// ====================
// HABILIDAD IMAN
// ====================
public class HabilidadIman : HabilidadBase
{
    [Header("Efecto")]
    [SerializeField] private PecesTestManager pecesManager;
    [SerializeField] private float multiplicadorRecogida = 2f;

    protected override bool Activar(HabilidadesManager manager)
    {
        if (pecesManager == null)
            pecesManager = FindFirstObjectByType<PecesTestManager>();

        if (pecesManager == null)
            return false;

        pecesManager.SetMultiplicadorRecogida(multiplicadorRecogida);
        EmpezarEfecto();

        if (manager != null)
            manager.MostrarMensaje("Has activado el iman");

        return true;
    }

    protected override void AlTerminarEfecto(HabilidadesManager manager)
    {
        if (pecesManager == null)
            pecesManager = FindFirstObjectByType<PecesTestManager>();

        if (pecesManager != null)
            pecesManager.SetMultiplicadorRecogida(1f);
    }
}