using UnityEngine;

public class CameraPivotController : MonoBehaviour
{
    [SerializeField] private float sensibilidadPorDefecto = 550f;
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;
    private bool ignorarPrimerFrame = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float anguloInicial = transform.localEulerAngles.x;

        if (anguloInicial > 180f)
            anguloInicial -= 360f;

        xRotation = anguloInicial;
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

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}