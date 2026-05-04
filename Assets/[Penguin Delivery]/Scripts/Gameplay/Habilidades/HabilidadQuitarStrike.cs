using UnityEngine;

// ====================
// HABILIDAD QUITAR STRIKE
// ====================
public class HabilidadQuitarStrike : HabilidadBase
{
    [Header("Efecto")]
    [SerializeField] private StrikeManager strikeManager;

    private bool usada = false;

    public override bool EstaUsada()
    {
        return usada;
    }

    public override void Tick(HabilidadesManager manager)
    {
    }

    protected override bool Activar(HabilidadesManager manager)
    {
        if (usada)
            return false;

        if (strikeManager == null)
            strikeManager = FindFirstObjectByType<StrikeManager>();

        if (strikeManager == null)
            return false;

        bool haQuitado = strikeManager.RemoveStrike();

        if (!haQuitado)
        {
            if (manager != null)
                manager.MostrarMensaje("No tienes strikes");

            return false;
        }

        usada = true;

        if (manager != null)
            manager.MostrarMensaje("Has eliminado un strike");

        return true;
    }
}