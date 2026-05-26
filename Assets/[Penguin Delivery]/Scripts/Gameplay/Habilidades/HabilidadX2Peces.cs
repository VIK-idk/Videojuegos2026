using UnityEngine;

// ====================
// HABILIDAD X2 PECES
// ====================
public class HabilidadX2Peces : HabilidadBase
{
    [SerializeField] private int pecesPorRecogida = 2;

    public int GetCantidadPeces()
    {
        if (activa)
            return pecesPorRecogida;

        return 1;
    }

    protected override bool Activar(HabilidadesManager manager)
    {
        EmpezarEfecto();

        if (manager != null)
        {
            manager.ReproducirVFXHabilidad(this, false);
            manager.MostrarMensaje("Has activado x2 de peces");
        }

        return true;
    }

    protected override void AlTerminarEfecto(HabilidadesManager manager)
    {
        if (manager != null)
        {
            manager.DetenerVFXHabilidad(this);
        }
    }
}