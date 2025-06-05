using System.Linq;
using UnityEngine;

public class AtomDragger : MonoBehaviour
{
    private Atom _draggedAtom;
    private Vector3 _offset;
    private Plane _dragPlane;

    void Update()
    {
        HandleAtomDrag();
    }

    private void HandleAtomDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && _draggedAtom != null)
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _draggedAtom != null)
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Atom atom = hit.collider.GetComponent<Atom>();
            if (atom != null)
            {
                _draggedAtom = atom;
                _dragPlane = new Plane(Vector3.forward, _draggedAtom.transform.position);

                // Рассчитываем смещение
                float distance;
                _dragPlane.Raycast(ray, out distance);
                _offset = _draggedAtom.transform.position - ray.GetPoint(distance);
            }
        }
    }

    private void ContinueDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (_dragPlane.Raycast(ray, out distance))
        {
            Vector3 newPosition = ray.GetPoint(distance) + _offset;
            _draggedAtom.transform.position = newPosition;

            // Обновляем все связи
            foreach (Bond bond in _draggedAtom.bonds)
            {
                bond.UpdateVisual();
            }
        }
    }

    private void EndDrag()
    {
        // Проверяем возможность создания связи при отпускании
        CheckForBondCreation();
        _draggedAtom = null;
    }

    private void CheckForBondCreation()
    {
        // Находим ближайший атом
        Atom nearestAtom = null;
        float minDistance = float.MaxValue;

        foreach (Atom atom in FindObjectsOfType<Atom>())
        {
            if (atom == _draggedAtom) continue;

            float distance = Vector3.Distance(_draggedAtom.transform.position, atom.transform.position);
            if (distance < minDistance && distance < 1.2f) // Максимальное расстояние для связи
            {
                minDistance = distance;
                nearestAtom = atom;
            }
        }

        // Создаем связь, если возможно
        if (nearestAtom != null &&
            _draggedAtom.CanBond &&
            nearestAtom.CanBond &&
            !_draggedAtom.bonds.Any(b => b.StartAtom == nearestAtom || b.EndAtom == nearestAtom))
        {
            CreateBond(_draggedAtom, nearestAtom);
        }
    }

    private void CreateBond(Atom atom1, Atom atom2)
    {
        GameObject bondObj = new GameObject("Bond");
        bondObj.transform.position = (atom1.transform.position + atom2.transform.position) / 2;

        Bond bond = bondObj.AddComponent<Bond>();
        LineRenderer lr = bondObj.AddComponent<LineRenderer>();

        // Настройка LineRenderer
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
    }
}