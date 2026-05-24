using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private bool estaEnSuelo = true;
    [SerializeField] private float jumpTrampolin = 40f;
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float multiplicadorSaltoBajo = 2f;

    //ANIMACION
    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    //ANIMACION
    [SerializeField] private float deadzoneAnimacion = 0.1f;

    //ANIMACION
    [SerializeField] private float tiempoParaFidget = 5f;

    //ANIMACION
    private float temporizadorQuieto = 0f;

    //ANIMACION
    private const string PARAM_CAMINANDO = "Caminando";

    //ANIMACION
    private const string PARAM_EN_SUELO = "EnSuelo";

    //ANIMACION
    private const string PARAM_VELOCIDAD_Y = "VelocidadY";

    //ANIMACION
    private const string PARAM_SALTAR = "Saltar";

    //ANIMACION
    private const string PARAM_FIDGET = "Fidget";

    //ROTACION MODELO
    [Header("Rotacion modelo")]
    [SerializeField] private Transform modeloVisual;
    [SerializeField] private float velocidadRotacionModelo = 10f;
    [SerializeField] private float offsetRotacionModelo = 0f;

    //SUELOS VFX
    [Header("Suelos / VFX pasos")]
    [SerializeField] private Transform puntoPieIzquierdo;
    [SerializeField] private Transform puntoPieDerecho;

    //SUELOS VFX
    private Suelo sueloActual;

    //SUELOS VFX
    private TipoSuelo tipoSueloActual;

    //SUELOS VFX
    private ParticleSystem efectoPasosIzquierdo;

    //SUELOS VFX
    private ParticleSystem efectoPasosDerecho;

    private TutorialManager tutorialManager;

    private Rigidbody rb;

    private float inputX;
    private float inputZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        tutorialManager = FindFirstObjectByType<TutorialManager>();

        //ANIMACION
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        //ANIMACION
        if (animator != null)
        {
            animator.SetBool(PARAM_EN_SUELO, estaEnSuelo);
        }

        //ROTACION MODELO
        if (modeloVisual == null)
        {
            Transform encontrado = transform.Find("Pinguino");

            if (encontrado != null)
            {
                modeloVisual = encontrado;
            }
        }

        //SUELOS VFX
        if (puntoPieIzquierdo == null)
        {
            puntoPieIzquierdo = transform;
        }

        //SUELOS VFX
        if (puntoPieDerecho == null)
        {
            puntoPieDerecho = transform;
        }
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        //ANIMACION
        ActualizarAnimacionMovimiento();

        //ANIMACION
        ActualizarAnimacionFidget();

        //ROTACION MODELO
        ActualizarRotacionModelo();

        //SUELOS VFX
        ActualizarEfectoPasos();

        if (Input.GetButtonDown("Saltar") && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            estaEnSuelo = false;

            //SUELOS VFX
            DetenerEfectoPasos();

            //ANIMACION
            temporizadorQuieto = 0f;

            //ANIMACION
            if (animator != null)
            {
                animator.ResetTrigger(PARAM_FIDGET);
                animator.SetBool(PARAM_EN_SUELO, false);
                animator.SetTrigger(PARAM_SALTAR);
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 direccion = transform.forward * inputZ + transform.right * inputX;

        // Evita que en diagonal vaya más rápido
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        Vector3 velocidad = direccion * speed;
        velocidad.y = rb.linearVelocity.y;
        rb.linearVelocity = velocidad;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Saltar"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorSaltoBajo - 1) * Time.fixedDeltaTime;
        }

        //ANIMACION
        ActualizarAnimacionAire();
    }

    //ANIMACION
    private void ActualizarAnimacionMovimiento()
    {
        if (animator == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);
        bool estaCaminando = inputMovimiento.magnitude > deadzoneAnimacion;

        animator.SetBool(PARAM_CAMINANDO, estaCaminando);
    }

    //ANIMACION
    private void ActualizarAnimacionFidget()
    {
        if (animator == null || rb == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        bool estaQuieto = inputMovimiento.magnitude <= deadzoneAnimacion &&
                          estaEnSuelo &&
                          Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        if (estaQuieto)
        {
            temporizadorQuieto += Time.deltaTime;

            if (temporizadorQuieto >= tiempoParaFidget)
            {
                animator.ResetTrigger(PARAM_FIDGET);
                animator.SetTrigger(PARAM_FIDGET);

                temporizadorQuieto = 0f;
            }
        }
        else
        {
            temporizadorQuieto = 0f;
            animator.ResetTrigger(PARAM_FIDGET);
        }
    }

    //ANIMACION
    private void ActualizarAnimacionAire()
    {
        if (animator == null || rb == null)
            return;

        animator.SetBool(PARAM_EN_SUELO, estaEnSuelo);
        animator.SetFloat(PARAM_VELOCIDAD_Y, rb.linearVelocity.y);
    }

    //ROTACION MODELO
    private void ActualizarRotacionModelo()
    {
        if (modeloVisual == null)
            return;

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        if (inputMovimiento.magnitude <= deadzoneAnimacion)
            return;

        float anguloObjetivo = Mathf.Atan2(inputX, inputZ) * Mathf.Rad2Deg;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, anguloObjetivo + offsetRotacionModelo, 0f);

        modeloVisual.localRotation = Quaternion.Slerp(
            modeloVisual.localRotation,
            rotacionObjetivo,
            velocidadRotacionModelo * Time.deltaTime
        );
    }

    //SUELOS VFX
    private void ActualizarEfectoPasos()
    {
        if (!estaEnSuelo)
        {
            DetenerEfectoPasos();
            return;
        }

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);
        bool estaCaminando = inputMovimiento.magnitude > deadzoneAnimacion;

        if (!estaCaminando)
        {
            DetenerEfectoPasos();
            return;
        }

        ReproducirEfecto(efectoPasosIzquierdo);
        ReproducirEfecto(efectoPasosDerecho);
    }

    //SUELOS VFX
    private void CambiarTipoSuelo(Suelo nuevoSuelo)
    {
        if (nuevoSuelo == null || nuevoSuelo.tipo == null)
            return;

        sueloActual = nuevoSuelo;

        if (tipoSueloActual == nuevoSuelo.tipo)
            return;

        tipoSueloActual = nuevoSuelo.tipo;

        DestruirEfectosPasos();

        if (tipoSueloActual.efectoVisualCaminar != null)
        {
            efectoPasosIzquierdo = CrearEfectoPasos(puntoPieIzquierdo);
            efectoPasosDerecho = CrearEfectoPasos(puntoPieDerecho);

            Debug.Log("Suelo actual: " + tipoSueloActual.nombre);
        }
    }


    //SUELOS VFX
    private ParticleSystem CrearEfectoPasos(Transform punto)
    {
        if (punto == null || tipoSueloActual == null || tipoSueloActual.efectoVisualCaminar == null)
            return null;

        GameObject nuevoObjetoEfecto = Instantiate(
            tipoSueloActual.efectoVisualCaminar,
            punto,
            false
        );

        nuevoObjetoEfecto.transform.localPosition = Vector3.zero;

        ParticleSystem particulas = nuevoObjetoEfecto.GetComponent<ParticleSystem>();

        if (particulas == null)
        {
            particulas = nuevoObjetoEfecto.GetComponentInChildren<ParticleSystem>();
        }

        if (particulas != null)
        {
            particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        return particulas;
    }

    //SUELOS VFX
    private void DestruirEfectosPasos()
    {
        if (efectoPasosIzquierdo != null)
        {
            Destroy(efectoPasosIzquierdo.gameObject);
            efectoPasosIzquierdo = null;
        }

        if (efectoPasosDerecho != null)
        {
            Destroy(efectoPasosDerecho.gameObject);
            efectoPasosDerecho = null;
        }
    }

    //SUELOS VFX
    private void ReproducirEfecto(ParticleSystem efecto)
    {
        if (efecto != null && !efecto.isPlaying)
        {
            efecto.Play();
        }
    }

    //SUELOS VFX
    private void DetenerEfecto(ParticleSystem efecto)
    {
        if (efecto != null && efecto.isPlaying)
        {
            efecto.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    //SUELOS VFX
    private void DetenerEfectoPasos()
    {
        DetenerEfecto(efectoPasosIzquierdo);
        DetenerEfecto(efectoPasosDerecho);
    }

    //SUELOS VFX
    private bool IntentarDetectarSuelo(Collision collision)
    {
        bool contactoSuperior = false;

        for (int i = 0; i < collision.contactCount; i++) // esto es para asegurarnos de que el contacto es por debajo del personaje, ya que a veces puede colisionar con piedras u otros objetos y no queremos que eso cuente como suelo
        {
            ContactPoint contacto = collision.GetContact(i); // obtenemos cada punto de contacto

            if (contacto.normal.y > 0.5f)
            {
                contactoSuperior = true;
                break;
            }
        }

        if (!contactoSuperior)
            return false;

        Suelo suelo = collision.gameObject.GetComponent<Suelo>();

        if (suelo == null) // Si no se encuentra en el objeto directamente, se busca en los padres (sobre todo por las piedras q tienen varios colliders)
        {
            suelo = collision.gameObject.GetComponentInParent<Suelo>();
        }

        if (suelo == null || suelo.tipo == null)
            return false;

        estaEnSuelo = true;
        CambiarTipoSuelo(suelo);

        //ANIMACION
        if (animator != null)
        {
            animator.SetBool(PARAM_EN_SUELO, true);
        }

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //SUELOS VFX
        IntentarDetectarSuelo(collision);

        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpTrampolin, ForceMode.Impulse);

            MorsaAnimacion morsaAnimacion = collision.gameObject.GetComponentInParent<MorsaAnimacion>();

            if (morsaAnimacion != null)
            {
                morsaAnimacion.ReproducirRebote();
            }

            //ANIMACION
            estaEnSuelo = false;

            //SUELOS VFX
            DetenerEfectoPasos();

            //ANIMACION
            temporizadorQuieto = 0f;

            //ANIMACION
            if (animator != null)
            {
                animator.ResetTrigger(PARAM_FIDGET);
                animator.SetBool(PARAM_EN_SUELO, false);
                animator.SetTrigger(PARAM_SALTAR);
            }

            if (tutorialManager != null)
            {
                tutorialManager.NotificarReboteEnMorsa();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //SUELOS VFX
        Suelo sueloSalida = collision.gameObject.GetComponent<Suelo>();

        if (sueloSalida == null)
        {
            sueloSalida = collision.gameObject.GetComponentInParent<Suelo>();
        }

        if (sueloActual != null && sueloSalida == sueloActual)
        {
            sueloActual = null;
            DetenerEfectoPasos();
        }
    }
}