using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hamspon : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void Show()
    {
        if (target) target.SetActive(true);
    }

    public void Hide()
    {
        if (target) target.SetActive(false);
    }
}
