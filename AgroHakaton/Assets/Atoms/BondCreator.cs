using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BondCreator : MonoBehaviour
{
    [Header("Bond Settings")]
    public int currentBondOrder = 1;

    public enum AtomType { Carbon, Hydrogen, Oxygen, Nitrogen }

    public AtomType currentAtomType = AtomType.Carbon;
    public bool autoCreateBonds = true;
    public float atomSpacing = 0.8f;

    private Atom _selectedAtom;
    private Atom _hoveredAtom;
    private bool _isDeleteMode;

    void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            _isDeleteMode = !_isDeleteMode;
            Debug.Log($"Delete mode: {_isDeleteMode}");
        }
    }

    public void SetCurrentBondOrder(int order)
    {
        currentBondOrder = Mathf.Clamp(order, 1, 3);
        Debug.Log($"Bond type set to: {GetBondTypeName(currentBondOrder)}");
    }

    private string GetBondTypeName(int order)
    {
        return order switch
        {
            1 => "Single",
            2 => "Double",
            3 => "Triple",
            _ => "Unknown"
        };
    }

    

    private bool CanAtomsFormBond(Atom a1, Atom a2, int bondOrder)
    {
        // Проверяем, достаточно ли у атомов свободных валентностей для связи
        int requiredSlots = bondOrder;
        return a1.CurrentBonds + requiredSlots <= a1.ValenceElectrons &&
               a2.CurrentBonds + requiredSlots <= a2.ValenceElectrons;
    }

    private void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (_isDeleteMode)
            {
                DeleteElement(hit.collider.gameObject);
                return;
            }

            Atom clickedAtom = hit.collider.GetComponent<Atom>();
            if (clickedAtom != null)
            {
                HandleAtomClick(clickedAtom);
            }
            else
            {
                Bond clickedBond = hit.collider.GetComponent<Bond>();
                if (clickedBond != null)
                {
                    BondManager.Instance.ChangeBondOrder(clickedBond);
                }
                else
                {
                    CreateAtomAtPosition(hit.point);
                }
            }
        }
        else
        {
            // Создаем атом на позиции курсора
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            CreateAtomAtPosition(worldPos);
        }
    }

    private void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Bond clickedBond = hit.collider.GetComponent<Bond>();
            if (clickedBond != null)
            {
                BondManager.Instance.DeleteBond(clickedBond);
            }
        }
    }

    private void HandleAtomClick(Atom clickedAtom)
    {
        if (_selectedAtom == null)
        {
            SelectAtom(clickedAtom);
        }
        else if (_selectedAtom == clickedAtom)
        {
            DeselectCurrentAtom();
        }
        else
        {
            if (_selectedAtom.CanBond && clickedAtom.CanBond)
            {
                // Проверяем валентность для двойных/тройных связей
                if (CanAtomsFormBond(_selectedAtom, clickedAtom, currentBondOrder))
                {
                    BondManager.Instance.CreateBond(_selectedAtom, clickedAtom, currentBondOrder);
                }
                else
                {
                    Debug.LogWarning("Atoms cannot form this bond type due to valence restrictions!");
                }
            }
            DeselectCurrentAtom();
        }
    }

    private void CreateAtomAtPosition(Vector3 position)
    {
        // Получаем префаб из базы данных
        GameObject atomPrefab = AtomDatabase.Instance.GetPrefab(currentAtomType);
        if (atomPrefab == null)
        {
            Debug.LogError($"Prefab for {currentAtomType} not found!");
            return;
        }

        // Создаем атом
        GameObject atomObj = Instantiate(atomPrefab, position, Quaternion.identity);
        Atom newAtom = atomObj.GetComponent<Atom>();

        // Инициализируем атом
        AtomDatabase.AtomDefinition def = AtomDatabase.Instance.GetDefinition(currentAtomType);
        newAtom.Initialize(def.symbol, def.valenceElectrons, def.color);

        // Автоматическое создание связи
        if (autoCreateBonds && _selectedAtom != null &&
            _selectedAtom.CanBond && newAtom.CanBond)
        {
            BondManager.Instance.CreateBond(_selectedAtom, newAtom);
        }

        // Выделяем новый атом
        SelectAtom(newAtom);
    }

    private void SelectAtom(Atom atom)
    {
        // Снимаем выделение с предыдущего атома
        if (_selectedAtom != null)
        {
            _selectedAtom.SetSelected(false);
        }

        // Устанавливаем выделение на новый атом
        _selectedAtom = atom;
        _selectedAtom.SetSelected(true);

        // Снимаем подсветку, если она была на этом атоме
        if (_hoveredAtom == _selectedAtom)
        {
            _hoveredAtom.SetHighlight(false);
            _hoveredAtom = null;
        }
    }

    private void DeselectCurrentAtom()
    {
        if (_selectedAtom != null)
        {
            _selectedAtom.SetSelected(false);
            _selectedAtom = null;
        }
    }

    private void DeleteElement(GameObject element)
    {
        Atom atom = element.GetComponent<Atom>();
        if (atom != null)
        {
            // Очищаем ссылки перед удалением
            if (_selectedAtom == atom)
            {
                DeselectCurrentAtom();
            }

            if (_hoveredAtom == atom)
            {
                _hoveredAtom.SetHighlight(false);
                _hoveredAtom = null;
            }

            Destroy(atom.gameObject);
            return;
        }

        Bond bond = element.GetComponent<Bond>();
        if (bond != null)
        {
            bond.BreakBond();
        }
    }

    private void HandleHover()
    {
        // Очищаем ссылки на уничтоженные объекты
        if (_hoveredAtom != null && _hoveredAtom.gameObject == null)
        {
            _hoveredAtom = null;
        }

        if (_selectedAtom != null && _selectedAtom.gameObject == null)
        {
            _selectedAtom = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit);
        Atom newHoveredAtom = hitSomething ? hit.collider.GetComponent<Atom>() : null;

        // Пропускаем выбранный атом
        if (newHoveredAtom == _selectedAtom)
        {
            newHoveredAtom = null;
        }

        // Снимаем подсветку, если объект под курсором изменился
        if (_hoveredAtom != newHoveredAtom)
        {
            if (_hoveredAtom != null)
            {
                _hoveredAtom.SetHighlight(false);
            }

            _hoveredAtom = newHoveredAtom;

            if (_hoveredAtom != null)
            {
                _hoveredAtom.SetHighlight(true);
            }
        }
    }
}   