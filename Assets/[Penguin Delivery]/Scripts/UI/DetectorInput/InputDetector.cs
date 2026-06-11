using UnityEngine;

public class InputDetector : MonoBehaviour
{
    public static bool usandoMando = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            usandoMando = false;
        }
        
        if (Input.anyKeyDown)
        {
            if (EsBotonDeMando())
                usandoMando = true;
            else
                usandoMando = false;
        }
    }

    private bool EsBotonDeMando()
    {
        for (int i = 330; i <= 349; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
                return true;
        }
        return false;
    }
}