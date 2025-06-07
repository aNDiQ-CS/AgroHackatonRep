using System.Collections.Generic;
using UnityEngine;

public class AtomCreator : MonoBehaviour
{
    public enum AtomType { Carbon, Hydrogen, Oxygen, Nitrogen, Custom }

    [System.Serializable]
    public class AtomPrefab
    {
        public AtomType type;
        public GameObject prefab;
        public Color color;
        public string symbol;
        public int valenceElectrons;
    }

    public List<AtomPrefab> atomPrefabs = new List<AtomPrefab>();
    public AtomType currentAtomType = AtomType.Carbon;
    public bool autoConnect = true;
    public float atomSpacing = 0.8f;
    public LayerMask ignoreUI;

    private Atom _selectedAtom;
    private Atom _hoveredAtom;

    void Update()
    {
        HandleAtomCreation();
        HandleHover();
    }

    private void HandleAtomCreation()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log(hit.transform);
            // ���� �������� �� ������������ ���� - �������� ���
            Atom clickedAtom = hit.collider.GetComponent<Atom>();
            if (clickedAtom != null)
            {
                SelectAtom(clickedAtom);
                return;
            }

            // ������� ����� ����
            CreateAtomAtPosition(hit.point);
        }
    }

    private void CreateAtomAtPosition(Vector3 position)
    {
        position = new Vector3(position.x, position.y, 0);
        AtomPrefab prefabInfo = atomPrefabs.Find(p => p.type == currentAtomType);
        if (prefabInfo == null) return;

        GameObject atomObj = Instantiate(prefabInfo.prefab, position, Quaternion.identity);
        Atom newAtom = atomObj.GetComponent<Atom>();

        // ����������� ����
        newAtom.Initialize(prefabInfo.symbol, prefabInfo.valenceElectrons, prefabInfo.color);

        // ������������� ��������� � ��������� ������
        if (autoConnect && _selectedAtom != null && _selectedAtom.CanBond && newAtom.CanBond)
        {
            CreateBond(_selectedAtom, newAtom);
        }

        MoleculeManager.Instance.RegisterAtom(newAtom);        
        // �������� ����� ����
        SelectAtom(newAtom);
    }

    private void CreateBond(Atom atom1, Atom atom2)
    {
        // ��������� ����������
        float distance = Vector3.Distance(atom1.transform.position, atom2.transform.position);
        if (distance > atomSpacing * 1.5f) return;

        // ������� �����
        GameObject bondObj = new GameObject("Bond");
        bondObj.transform.position = (atom1.transform.position + atom2.transform.position) / 2;

        Bond bond = bondObj.AddComponent<Bond>();
        LineRenderer lr = bondObj.AddComponent<LineRenderer>();

        // ��������� LineRenderer
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Standard"));
        lr.material.color = Color.black;
        lr.positionCount = 2;
        lr.SetPosition(0, atom1.transform.position);
        lr.SetPosition(1, atom2.transform.position);

        bond.Initialize(atom1, atom2);
        atom1.AddBond(bond);
        atom2.AddBond(bond);
        MoleculeManager.Instance.RegisterBond(bond);
    }

    private void SelectAtom(Atom atom)
    {
        // ������� ��������� � ����������� �����
        if (_selectedAtom != null)
            _selectedAtom.SetHighlight(false);

        // �������� ����� ����
        _selectedAtom = atom;
        _selectedAtom.SetHighlight(true);
    }

    private void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Atom atom = hit.collider.GetComponent<Atom>();
            if (atom != null && atom != _hoveredAtom)
            {
                if (_hoveredAtom != null && _hoveredAtom != _selectedAtom)
                    _hoveredAtom.SetHighlight(false);

                _hoveredAtom = atom;
                if (_selectedAtom != atom)
                    atom.SetHighlight(true);
            }
        }
        else if (_hoveredAtom != null)
        {
            if (_hoveredAtom != _selectedAtom)
                _hoveredAtom.SetHighlight(false);
            _hoveredAtom = null;
        }
    }

    // ������ ��� UI
    public void SetAtomType(int typeIndex)
    {
        currentAtomType = (AtomType)typeIndex;
    }

    public void ToggleAutoConnect(bool enabled)
    {
        autoConnect = enabled;
    }
}