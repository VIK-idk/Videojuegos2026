using UnityEngine;
using Ablabla.Habilidades;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private bool estaEnSuelo = true;
    [SerializeField] private float jumpTrampolin = 40f;
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float multiplicadorSaltoBajo = 2f;

    [Header("Recoleccion")]
    [SerializeField] private SphereCollider zonaRecoleccion;
    [SerializeField] private float radioRecoleccionNormal = 1.5f;
    [SerializeField] private float radioRecoleccionIman = 4f;
    [SerializeField] private int puntosPorOrbe = 15;

    private Rigidbody rb;
    private GameManager gm;
    private Jugador_Habilidades habilidades;

    private float inputX;
    private float inputZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gm = FindFirstObjectByType<GameManager>();
        habilidades = GetComponent<Jugador_Habilidades>();

        rb.freezeRotation = true;

        if (zonaRecoleccion != null)
        {
            zonaRecoleccion.isTrigger = true;
            zonaRecoleccion.radius = radioRecoleccionNormal;
        }
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        ActualizarRadioRecoleccion();

        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            estaEnSuelo = false;
        }
    }

    void FixedUpdate()
    {
        Vector3 direccion = transform.forward * inputZ + transform.right * inputX;
        Vector3 velocidad = direccion * speed;
        velocidad.y = rb.linearVelocity.y;

        rb.linearVelocity = velocidad;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorSaltoBajo - 1) * Time.fixedDeltaTime;
        }
    }

    private void ActualizarRadioRecoleccion()
    {
        if (zonaRecoleccion == null)
        {
            return;
        }

        if (habilidades != null && habilidades.EstaActivaLaHabilidad(Tipo_Habilidades.Iman))
        {
            zonaRecoleccion.radius = radioRecoleccionIman;
        }
        else
        {
            zonaRecoleccion.radius = radioRecoleccionNormal;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            estaEnSuelo = true;
        }

        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpTrampolin, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orbe"))
        {
            int puntosGanados = puntosPorOrbe;

            if (habilidades != null && habilidades.EstaActivaLaHabilidad(Tipo_Habilidades.DoblePez))
            {
                puntosGanados *= 2;
            }

            Debug.Log("Orbe recogido. Puntos ganados: " + puntosGanados);

            gm.SumarPuntos(puntosGanados);
            Destroy(other.gameObject);
        }
    }
}