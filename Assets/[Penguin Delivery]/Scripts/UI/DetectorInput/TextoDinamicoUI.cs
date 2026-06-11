using UnityEngine;
using UnityEngine.UI;

public class TextoDinamicoUI : MonoBehaviour
{
    // Ahora pide un componente Image en lugar de Text
    public Image miImagen; 

    [Header("Imágenes a mostrar")]
    public Sprite imagenTeclado; // Arrastra aquí tu icono de la tecla (Ej: E)
    public Sprite imagenMando;   // Arrastra aquí tu icono del mando (Ej: Cuadrado)

    private bool estadoAnteriorMando;

    void Start()
    {
        estadoAnteriorMando = InputDetector.usandoMando;
        ActualizarImagen();
    }

    void Update()
    {
        if (estadoAnteriorMando != InputDetector.usandoMando)
        {
            estadoAnteriorMando = InputDetector.usandoMando;
            ActualizarImagen();
        }
    }

    void ActualizarImagen()
    {
        // Cambia el sprite de la imagen dependiendo del input
        miImagen.sprite = InputDetector.usandoMando ? imagenMando : imagenTeclado;
    }
}