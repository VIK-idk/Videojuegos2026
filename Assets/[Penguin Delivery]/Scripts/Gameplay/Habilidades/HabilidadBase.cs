using UnityEngine;

// ====================
// HABILIDAD BASE
// ====================
public abstract class HabilidadBase : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] protected string titulo = "Habilidad";
    [SerializeField] protected string textoTecla = "[Pulsa]";
    [SerializeField] protected bool adquirida = false;
    [SerializeField] protected string idCompra = "";

    [Header("Tiempos")]
    [SerializeField] protected float duracion = 8f;
    [SerializeField] protected float cooldown = 10f;

    protected bool activa = false;
    protected float tiempoActivoRestante = 0f;
    protected float tiempoCooldownRestante = 0f;

    public string GetTitulo()
    {
        return titulo;
    }

    public string GetTextoTecla()
    {
        return textoTecla;
    }

    public bool EstaAdquirida()
    {
        return adquirida;
    }

    public bool EstaActiva()
    {
        return activa;
    }

    public bool EstaEnCooldown()
    {
        return tiempoCooldownRestante > 0f;
    }

    public virtual bool EstaUsada()
    {
        return false;
    }

    public float GetTiempoVisible()
    {
        if (activa)
            return tiempoActivoRestante;

        return tiempoCooldownRestante;
    }
    protected virtual void Awake()
    {
        if (idCompra == "x2_peces")
            adquirida = SesionPartida.habilidadX2Comprada;

        if (idCompra == "iman")
            adquirida = SesionPartida.habilidadImanComprada;

        if (idCompra == "quitar_strike")
            adquirida = SesionPartida.habilidadQuitarStrikeComprada;
    }
    public virtual void Tick(HabilidadesManager manager)
    {
        if (activa)
        {
            tiempoActivoRestante -= Time.deltaTime;

            if (tiempoActivoRestante <= 0f)
            {
                tiempoActivoRestante = 0f;
                activa = false;

                AlTerminarEfecto(manager);

                if (cooldown > 0f)
                {
                    tiempoCooldownRestante = cooldown;
                }
            }
        }
        else if (tiempoCooldownRestante > 0f)
        {
            tiempoCooldownRestante -= Time.deltaTime;

            if (tiempoCooldownRestante < 0f)
                tiempoCooldownRestante = 0f;
        }
    }

    public bool IntentarActivar(HabilidadesManager manager)
    {
        if (!adquirida)
            return false;

        if (activa)
            return false;

        if (EstaEnCooldown())
            return false;

        if (EstaUsada())
            return false;

        if (manager != null && manager.HayOtraHabilidadActiva(this))
            return false;

        return Activar(manager);
    }

    protected void EmpezarEfecto()
    {
        activa = true;
        tiempoActivoRestante = duracion;
    }

    protected abstract bool Activar(HabilidadesManager manager);

    protected virtual void AlTerminarEfecto(HabilidadesManager manager)
    {
    }
}