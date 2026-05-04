using UnityEngine;

public class CameraPivotController : MonoBehaviour
{
    [SerializeField] private float sensibilidadPorDefecto = 550f;
    [SerializeField] private Transform playerBody;

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
        {
            yRotation = playerBody.eulerAngles.y;
        }
    }

    private void Update()
    {
        if (ignorarPrimerFrame)
        {
            ignorarPrimerFrame = false;
            return;
        }

        float sensibilidadActual = PlayerPrefs.GetFloat("Sensibilidad", sensibilidadPorDefecto);

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadActual * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadActual * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -limiteVertical, limiteVertical);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}