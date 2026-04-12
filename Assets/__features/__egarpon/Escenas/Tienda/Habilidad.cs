using UnityEngine;

[System.Serializable]
public class Habilidad
{
    [SerializeField] private string id;
    [SerializeField] private string nombre;

    [TextArea(2, 5)]
    [SerializeField] private string descripcion;

    [SerializeField] private int precio;
    [SerializeField] private Sprite icono;

    public string Id
    {
        get { return id; }
    }

    public string Nombre
    {
        get { return nombre; }
    }

    public string Descripcion
    {
        get { return descripcion; }
    }

    public int Precio
    {
        get { return precio; }
    }

    public Sprite Icono
    {
        get { return icono; }
    }
}