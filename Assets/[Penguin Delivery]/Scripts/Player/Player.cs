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
    [SerializeField] private Transform puntoEfectosPasos;

    //SUELOS VFX
    private Suelo sueloActual;

    //SUELOS VFX
    private TipoSuelo tipoSueloActual;

    //SUELOS VFX
    private ParticleSystem efectoPasosActual;

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
        if (puntoEfectosPasos == null)
        {
            puntoEfectosPasos = transform;
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

        if (efectoPasosActual != null && !efectoPasosActual.isPlaying)
        {
            efectoPasosActual.Play();
        }
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

        if (efectoPasosActual != null)
        {
            Destroy(efectoPasosActual.gameObject);
            efectoPasosActual = null;
        }

        if (tipoSueloActual.efectoVisualCaminar != null)
        {
            GameObject nuevoObjetoEfecto = Instantiate(
                tipoSueloActual.efectoVisualCaminar,
                puntoEfectosPasos.position,
                Quaternion.identity,
                puntoEfectosPasos
            );

            nuevoObjetoEfecto.transform.localPosition = Vector3.zero;

            efectoPasosActual = nuevoObjetoEfecto.GetComponent<ParticleSystem>();

            if (efectoPasosActual == null)
            {
                efectoPasosActual = nuevoObjetoEfecto.GetComponentInChildren<ParticleSystem>();
            }

            if (efectoPasosActual != null)
            {
                efectoPasosActual.Stop();
            }

            Debug.Log("Suelo actual: " + tipoSueloActual.nombre);
        }
    }

    //SUELOS VFX
    private void DetenerEfectoPasos()
    {
        if (efectoPasosActual != null && efectoPasosActual.isPlaying)
        {
            efectoPasosActual.Stop();
        }
    }

    //SUELOS VFX
    private bool IntentarDetectarSuelo(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Suelo suelo))
        {
            estaEnSuelo = true;
            CambiarTipoSuelo(suelo);

            //ANIMACION
            if (animator != null)
            {
                animator.SetBool(PARAM_EN_SUELO, true);
            }

            return true;
        }

        return false;
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
        if (sueloActual != null && collision.gameObject == sueloActual.gameObject)
        {
            sueloActual = null;
        }
    }
}