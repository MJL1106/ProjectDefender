using System;
using UnityEngine;

public class ProjectileSpiderNest : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetupSpider()
    {
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
