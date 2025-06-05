using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoleculeManager : MonoBehaviour
{
    public static MoleculeManager Instance { get; private set; }

    public List<Atom> Atoms { get; } = new List<Atom>();
    public List<Bond> Bonds { get; } = new List<Bond>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void RegisterAtom(Atom atom)
    {
        if (!Atoms.Contains(atom))
            Atoms.Add(atom);
        //DebugAtom();
    }

    public void UnregisterAtom(Atom atom)
    {
        if (Atoms.Contains(atom))
            Atoms.Remove(atom);
        //DebugAtom();
    }

    public void RegisterBond(Bond bond)
    {
        if (!Bonds.Contains(bond))
            Bonds.Add(bond);
        //DebugBond();
    }

    public void UnregisterBond(Bond bond)
    {
        if (Bonds.Contains(bond))
            Bonds.Remove(bond);
        //DebugBond();
    }

    public string GenerateSmiles()
    {
        // Фильтруем только активные атомы
        List<Atom> allAtoms = Atoms.Where(a => a != null && a.gameObject.activeInHierarchy).ToList();
        return new SmilesGenerator().GenerateSmiles(allAtoms);
    }

    // Обновление всех визуальных элементов
    public void UpdateMoleculeVisuals()
    {
        foreach (Bond bond in Bonds)
            bond.UpdateVisual();
    }

    private void DebugAtom()
    {
        for (int i = 0; i < Atoms.Count; i++)
        {
            Debug.Log(Atoms[i]);
        }
    }

    private void DebugBond()
    {
        for (int i = 0; i < Bonds.Count;i++) 
        {
            Debug.Log(Bonds[i]);    
        }
    }
}