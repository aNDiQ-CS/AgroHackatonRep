using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUI : MonoBehaviour
{
    [SerializeField] private AtomDragger atomDragger;
    [SerializeField] private AtomCreator atomCreator;
    [SerializeField] private BondCreator bondCreator;

    public void AtomDraggerClick()
    {
        atomDragger.enabled = true;
        atomCreator.enabled = false;
        bondCreator.enabled = false;
    }

    public void AtomCreatorClick()
    {
        atomDragger.enabled = false;
        atomCreator.enabled = true;
        bondCreator.enabled = false;
    }

    public void BondCreatorClick()
    {
        atomDragger.enabled = false;
        atomCreator.enabled = false;
        bondCreator.enabled = true;
    }
}
