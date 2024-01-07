using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float m_Speed = 5f;
    public Transform m_Target = null;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * m_Speed);
    }

    public void Init(Transform target)
    {
        m_Target = target;
        transform.LookAt(m_Target);
    }
}
