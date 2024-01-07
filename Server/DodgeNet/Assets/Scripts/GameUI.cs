using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] Turret[] m_Turrets = null;
    [SerializeField] Transform m_BulletParent = null;
    [SerializeField] Transform[] m_SpawnPoints = null;
    public GameObject m_PrePlayer = null;
    public Player m_MyPlayer = null;
    public List<Player> m_Players = null;

    public void OnReadyState()
    {
        DestroyBullet();
        Init();
    }

    public void OnGameState()
    {
        for (int i = 0; i < m_Turrets.Length; i++)
        {
            m_Turrets[i].Init();
        }
    }

    public void OnResultState()
    {
        for (int i = 0; i < m_Turrets.Length; i++)
        {
            m_Turrets[i].StopShoot();
        }
    }

    void DestroyBullet()
    {
        if (m_BulletParent.childCount == 0)
            return;

        for (int i = 0; i < m_BulletParent.childCount; i++)
        {
            Destroy(m_BulletParent.GetChild(i).gameObject);
        }
    }

    public void Init()
    {
        int count = CSocketIoMgr.MyRoom.PlayerCount();

        for (int i = 0; i < count; i++)
        {
            string id = CSocketIoMgr.MyRoom.players[i].Name();

            CreatePlayer(id);
        }
    }

    void CreatePlayer(string id)
    {
        GameObject go = Instantiate(m_PrePlayer);
        Player player = go.GetComponent<Player>();
        player.Init();
        player.SetPlayerId(id);

        m_Players.Add(player);
    }

    public void OnAck_PlayerMove(string id, Vector3 pos)
    {
        int count = CSocketIoMgr.MyRoom.PlayerCount();

        Debug.Log(count);

        for (int i = 0; i < count; i++)
        {
            SOPlayer player = CSocketIoMgr.MyRoom.players[i];
            if(player.Name() == id)
            {
                m_Players[i].transform.position = pos;
                Debug.Log(id + "의 위치가 변경됨");
                break;
            }
        }
    }

    private void OnDestroy()
    {
        DestroyBullet();
    }
}
