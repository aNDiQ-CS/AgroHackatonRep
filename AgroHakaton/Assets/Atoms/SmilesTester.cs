using UnityEngine;
using TMPro;

public class SmilesTester : MonoBehaviour
{
    public MoleculeManager moleculeManager;
    public string rightSmiles;
    public TMP_Text textMeshPro;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            string smiles = moleculeManager.GenerateSmiles();
            Debug.Log($"Generated SMILES: {smiles}");
        }
    }

    public void CheckSmiles()
    {
        string smiles = moleculeManager.GenerateSmiles();
        if (smiles == rightSmiles)
        {
            textMeshPro.text = "✓";
            textMeshPro.color = Color.green;
        }
        else
        {
            textMeshPro.text = "X";
            textMeshPro.color = Color.red;
        }
    }
}