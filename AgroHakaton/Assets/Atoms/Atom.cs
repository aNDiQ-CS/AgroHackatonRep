using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Atom : MonoBehaviour
{
    [Header("Valence Visualization")]
    public GameObject valenceIndicatorPrefab;
    public float indicatorOffset = 0.5f;

    private List<GameObject> _valenceIndicators = new List<GameObject>();

    public string Symbol { get; private set; }
    public int ValenceElectrons { get; private set; }
    public int CurrentBonds => bonds.Count;
    public bool CanBond => CurrentBonds < ValenceElectrons;

    public List<Bond> bonds = new List<Bond>();

    private Renderer _renderer;
    private Color _baseColor;
    private MaterialPropertyBlock _propBlock;
    private bool _isHighlighted;
    private bool _isSelected;

    public void Initialize(string symbol, int valence, Color color)
    {
        Symbol = symbol;
        ValenceElectrons = valence;
        _baseColor = color;

        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        // Инициализация MaterialPropertyBlock
        _renderer.GetPropertyBlock(_propBlock);        
        _propBlock.SetColor("_BaseColor", _baseColor);
        _propBlock.SetColor("_HighlightColor", Color.yellow);
        _renderer.SetPropertyBlock(_propBlock);

        // Добавляем коллайдер, если отсутствует
        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.4f;
        }
    }

    public void SetHighlight(bool highlighted)
    {
        _isHighlighted = highlighted;
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        
        if (_renderer == null) return;
        //Debug.Log(_isSelected);
        //_renderer.GetPropertyBlock(_propBlock);

        if (_isSelected)
        {
            _renderer.material.color = Color.yellow;
            //_propBlock.SetColor("_BaseColor", Color.yellow);
        }
        else if (_isHighlighted)
        {
            // Слегка затемненный желтый для наведения
            _renderer.material.color = new Color(0.8f, 0.8f, 0f);
            //_propBlock.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0f));
        }
        else
        {
            _renderer.material.color = _baseColor;
            _propBlock.SetColor("_BaseColor", _baseColor);
        }
        UpdateValenceVisualization();
        //_renderer.SetPropertyBlock(_propBlock);
    }

    public void AddBond(Bond bond)
    {
        if (!bonds.Contains(bond))
            bonds.Add(bond);
    }

    public void RemoveBond(Bond bond)
    {
        if (bonds.Contains(bond))
            bonds.Remove(bond);
    }

    private void OnDestroy()
    {
        // Удаляем все связи при уничтожении атома
        foreach (Bond bond in bonds.ToList())
        {
            if (bond != null) bond.BreakBond();
        }
        ClearValenceIndicators();
    }

    public int GetAvailableBondSlots()
    {
        return ValenceElectrons - CurrentBonds;
    }

    public bool CanFormBondWith(Atom other, int bondOrder)
    {
        // Для водородов только одинарные связи
        if (Symbol == "H" && bondOrder > 1) return false;
        if (other.Symbol == "H" && bondOrder > 1) return false;

        // Проверка доступных слотов
        return GetAvailableBondSlots() >= bondOrder &&
               other.GetAvailableBondSlots() >= bondOrder;
    }

    private void UpdateValenceVisualization()
    {
        // Удаляем старые индикаторы
        ClearValenceIndicators();

        // Рассчитываем позиции для индикаторов
        int availableSlots = GetAvailableBondSlots();

        // Для атомов без свободных валентностей не показываем индикаторы
        if (availableSlots <= 0) return;

        // Распределяем индикаторы равномерно вокруг атома
        float angleStep = 360f / availableSlots;
        for (int i = 0; i < availableSlots; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = transform.position + new Vector3(
                Mathf.Cos(angle) * indicatorOffset,
                Mathf.Sin(angle) * indicatorOffset,
                0
            );

            GameObject indicator = Instantiate(
                valenceIndicatorPrefab,
                position,
                Quaternion.identity,
                transform
            );

            _valenceIndicators.Add(indicator);
        }
    }

    private void ClearValenceIndicators()
    {
        foreach (GameObject indicator in _valenceIndicators)
        {
            Destroy(indicator);
        }
        _valenceIndicators.Clear();
    }
}