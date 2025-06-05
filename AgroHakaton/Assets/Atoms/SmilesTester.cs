using UnityEngine;

public class SmilesTester : MonoBehaviour
{
    public MoleculeManager moleculeManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            string smiles = moleculeManager.GenerateSmiles();
            Debug.Log($"Generated SMILES: {smiles}");
        }
    }
}