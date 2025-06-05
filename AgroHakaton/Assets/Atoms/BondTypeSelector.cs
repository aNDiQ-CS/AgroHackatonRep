using UnityEngine;
using UnityEngine.UI;

public class BondTypeSelector : MonoBehaviour
{
    public Button singleBondButton;
    public Button doubleBondButton;
    public Button tripleBondButton;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    private BondCreator _bondCreator;

    private void Start()
    {
        _bondCreator = FindObjectOfType<BondCreator>();
        if (_bondCreator == null)
        {
            Debug.LogError("BondCreator not found in scene!");
            return;
        }

        // Установка обработчиков
        singleBondButton.onClick.AddListener(() => SetBondType(1));
        doubleBondButton.onClick.AddListener(() => SetBondType(2));
        tripleBondButton.onClick.AddListener(() => SetBondType(3));

        // Выбор одинарной связи по умолчанию
        SetBondType(1);
    }

    private void SetBondType(int bondType)
    {
        _bondCreator.SetCurrentBondOrder(bondType);
        UpdateButtonColors(bondType);
    }

    private void UpdateButtonColors(int selectedType)
    {
        singleBondButton.image.color = selectedType == 1 ? selectedColor : normalColor;
        doubleBondButton.image.color = selectedType == 2 ? selectedColor : normalColor;
        tripleBondButton.image.color = selectedType == 3 ? selectedColor : normalColor;
    }
}