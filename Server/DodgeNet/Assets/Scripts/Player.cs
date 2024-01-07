using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float m_Speed = 7f;
    Vector3 m_StartPos = new Vector3(0, 2f, 0f);
    [SerializeField] FXParticle m_ptcBomb = null;
    [SerializeField] GameDlg m_GameDlg = null;
    public string m_userId = "";
    public Player m_player = null;

    private void Update()
    {
        if (GameMgr.Inst.BattleFSM.IsGameState())
        {
            if(CSocketIoMgr.IsMine(m_userId))
                Move();

        }
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 pos = new Vector3(x, 0f, z);
        pos.Normalize();

        transform.Translate(pos * Time.deltaTime * m_Speed);

        float xPos = Mathf.Clamp(transform.position.x, -8f, 8f);
        float zPos = Mathf.Clamp(transform.position.z, -8f, 8f);
        transform.position = new Vector3(xPos, 2f, zPos);
    }

    public void Init()
    {
        m_GameDlg = GameMgr.Inst.m_gameScene.m_HudUI.m_GameDlg;
        transform.position = m_StartPos;
        m_ptcBomb.Stop();
        m_GameDlg.FreshHP();
        m_player = this;
    }

    public void SetPlayerId(string id)
    {
        m_userId = id;

        if (CSocketIoMgr.IsMine(m_userId))
        {
            GameMgr.Inst.m_gameScene.m_GameUI.m_MyPlayer = m_player;
            MoveNet.Inst.StartSendMoveLoop();
        }
    }

    void Damaged()
    {
        GameMgr.Inst.GameInfo.AttackPlayer();
        m_ptcBomb.Play();
        m_GameDlg.FreshHP();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damaged();
            Destroy(collision.gameObject);
        }
    }
}
