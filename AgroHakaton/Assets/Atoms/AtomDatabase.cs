using System;
using System.Collections.Generic;
using UnityEngine;

public class AtomDatabase : MonoBehaviour
{
    public static AtomDatabase Instance { get; private set; }

    [Serializable]
    public class AtomDefinition
    {
        public BondCreator.AtomType type;
        public GameObject prefab;
        public Color color = Color.white;
        public string symbol = "C";
        public int valenceElectrons = 4;
    }

    public List<AtomDefinition> atomDefinitions = new List<AtomDefinition>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public GameObject GetPrefab(BondCreator.AtomType type)
    {
        AtomDefinition def = atomDefinitions.Find(d => d.type == type);
        return def?.prefab;
    }

    public AtomDefinition GetDefinition(BondCreator.AtomType type)
    {
        return atomDefinitions.Find(d => d.type == type);
    }
}
