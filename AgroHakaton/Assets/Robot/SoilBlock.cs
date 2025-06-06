using UnityEngine;

public class SoilBlock : MonoBehaviour
{
    public bool isWatered;
    private Material defaultMat;
    public Material wateredMat;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        defaultMat = rend.material;
    }

    public void Water()
    {
        isWatered = true;
        rend.material = wateredMat;
    }
}