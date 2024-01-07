using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGRotate : MonoBehaviour
{
    [SerializeField] Transform Bg = null;
    [SerializeField] float speed = 1f;

    void Update()
    {
        Bg.Rotate(new Vector3(0f, 0f, speed) * Time.deltaTime);
    }

}
