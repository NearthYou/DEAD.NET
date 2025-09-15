using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using UnityEngine.EventSystems;

public class StructureFade : MonoBehaviour
{
    private Material curMaterial;
    [SerializeField] private Renderer rend;
    [SerializeField] Material cloakingMaterial;

    void Start()
    {
        if (rend != null)
            curMaterial = rend.material;
    }

    public void FadeIn()
    {
        if (rend == null)
            return;

        rend.material = cloakingMaterial;
    }

    public void FadeOut()
    {
        if (rend == null)
            return;

        rend.material = curMaterial;
    }
}