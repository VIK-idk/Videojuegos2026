using UnityEngine;

[System.Serializable]
public class Habilidad
{
    [SerializeField] private string nombre;

    [TextArea(2, 5)]
    [SerializeField] private string descripcion;

    [SerializeField] private bool comprado;
    [SerializeField] private int precio;
    [SerializeField] private Sprite icono;

    // Getters y setters
    public string Nombre
    {
        get { return nombre; }
        set { nombre = value; }
    }

    public string Descripcion
    {
        get { return descripcion; }
        set { descripcion = value; }
    }

    public bool Comprado
    {
        get { return comprado; }
        set { comprado = value; }
    }

    public int Precio
    {
        get { return precio; }
        set { precio = value; }
    }

    public Sprite Icono
    {
        get { return icono; }
        set { icono = value; }
    }
}