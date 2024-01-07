using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform m_Body = null;
    public Transform m_BulletPos = null;
    public GameObject m_PreBullet = null;
    public Transform m_BulletParent = null;
    public Transform m_Target = null;

    Coroutine m_CoShoot = null;

    public void Init()
    {
        m_CoShoot = StartCoroutine(Co_Shoot());
    }

    IEnumerator Co_Shoot()
    {
        while (GameMgr.Inst.BattleFSM.IsGameState())
        {
            yield return new WaitForSeconds(1f);
            CreateBullet();
        }
    }

    public void StopShoot()
    {
        StopCoroutine(m_CoShoot);
    }

    void CreateBullet()
    {
        GameObject go = Instantiate(m_PreBullet, m_BulletParent);
        go.transform.position = m_BulletPos.position;

        Bullet bullet = go.GetComponent<Bullet>();
        bullet.Init(m_Target);
    }

    private void Update()
    {
        m_Body.LookAt(m_Target);
    }
}
