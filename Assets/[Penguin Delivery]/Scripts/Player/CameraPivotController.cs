using UnityEngine;

public class CameraPivotController : MonoBehaviour
{
    [Header("Sensibilidad")]
    [SerializeField] private float sensibilidadRaton = 550f;
    [SerializeField] private float sensibilidadMando = 200f;

    [Header("Deadzone mando")]
    [Range(0f, 0.5f)]
    [SerializeField] private float deadzoneMando = 0.15f;

    [SerializeField] private Transform playerBody;

    [Header("Limites")]
    [SerializeField] private float limiteVertical = 85f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool ignorarPrimerFrame = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float anguloInicialX = transform.localEulerAngles.x;
        if (anguloInicialX > 180f)
            anguloInicialX -= 360f;

        xRotation = anguloInicialX;

        if (playerBody != null)
            yRotation = playerBody.eulerAngles.y;
    }

    private void Update()
    {
        if (ignorarPrimerFrame)
        {
            ignorarPrimerFrame = false;
            return;
        }

        // 🔹 RATÓN
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.deltaTime;

        // 🔹 MANDO
        float padX = ApplyDeadzone(Input.GetAxis("RightStickX")) * sensibilidadMando * Time.deltaTime;
        float padY = ApplyDeadzone(Input.GetAxis("RightStickY")) * sensibilidadMando * Time.deltaTime;

        float inputX = mouseX + padX;
        float inputY = mouseY + padY;

        yRotation += inputX;
        xRotation -= inputY;

        xRotation = Mathf.Clamp(xRotation, -limiteVertical, limiteVertical);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    private float ApplyDeadzone(float value)
    {
        if (Mathf.Abs(value) < deadzoneMando)
            return 0f;

        // reescala para que no pierdas sensibilidad fuera de la zona muerta
        return Mathf.Sign(value) * ((Mathf.Abs(value) - deadzoneMando) / (1f - deadzoneMando));
    }
}