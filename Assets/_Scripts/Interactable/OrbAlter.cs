using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

//TODO: Refactor to "Sender" class
public class OrbAlter : Interactable {
    [SerializeField] private Transform displayPoint;
    [SerializeField] private MeshRenderer pillar;
    [SerializeField] private List<Togglable> togglables;    //The theory is that a list of togglables will be taken in that activate when an orb of a certain color interacts
    private GameObject activeDisplayOrb;
    
    //Uhhh yea this is pretty hardcoded for demo sake lmaoo
    public override void InteractAction(OrbThrownData data ) {
        base.InteractAction(data);
        if (activeDisplayOrb != null) {
            Destroy(activeDisplayOrb);
        }

        activeDisplayOrb = Instantiate(data.OrbObject, displayPoint.position, Quaternion.identity);
        Destroy(activeDisplayOrb.GetComponent<OrbThrow>());
        activeDisplayOrb.transform.DOScale(1f, 1f);
        activeDisplayOrb.SetActive(true);

        Color orbColor = activeDisplayOrb.GetComponentInChildren<MeshRenderer>().materials[0].GetColor("_Color");
        Material material = new Material(pillar.materials[0]);
        material.DOColor(orbColor, "_Circuit_Color", 1f);
        Material[] materials = { material };
        pillar.materials = materials;
        CheckTogglables(data);
    }

    private void CheckTogglables(OrbThrownData data) {
        foreach (Togglable togglable in togglables) {
            togglable.Toggle(data);
        }
    }
}
