using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    [SerializeField] Vector3 angles;
    Transform cached;

    void Start()
    {
        cached = transform;
    }

    void Update()
    {
        cached.localRotation *= Quaternion.Euler(angles * Time.deltaTime);
    }
}
