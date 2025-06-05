using System.Linq;
using UnityEngine;

public class BondManager : MonoBehaviour
{
    public static BondManager Instance { get; private set; }

    public GameObject bondPrefab;
    public float maxBondDistance = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void CreateBond(Atom atom1, Atom atom2, int bondOrder = 1)
    {
        // Проверяем, не существует ли уже связи
        if (atom1.bonds.Any(b => b.StartAtom == atom2 || b.EndAtom == atom2))
            return;

        // Проверяем расстояние
        float distance = Vector3.Distance(atom1.transform.position, atom2.transform.position);
        if (distance > maxBondDistance) return;

        // Проверяем валентность
        if (!atom1.CanFormBondWith(atom2, bondOrder))
        {
            Debug.LogWarning($"Cannot form bond between {atom1.Symbol} and {atom2.Symbol} with order {bondOrder}");
            return;
        }

        // Создаем связь
        GameObject bondObj = Instantiate(bondPrefab);
        Bond bond = bondObj.GetComponent<Bond>();
        bond.Initialize(atom1, atom2, bondOrder);

        atom1.AddBond(bond);
        atom2.AddBond(bond);

        // Обновляем визуализацию валентностей
        atom1.UpdateVisual();
        atom2.UpdateVisual();
    }

    public void ChangeBondOrder(Bond bond)
    {
        if (bond != null)
        {
            bond.CycleBondOrder();
        }
    }

    public void DeleteBond(Bond bond)
    {
        if (bond != null)
        {
            bond.BreakBond();
        }
    }


}