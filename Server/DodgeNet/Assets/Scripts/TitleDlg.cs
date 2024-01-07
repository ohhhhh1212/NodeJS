using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleDlg : MonoBehaviour
{
    [SerializeField] Button m_btnStart = null;

    public void Init()
    {
        gameObject.SetActive(true);
        m_btnStart.onClick.AddListener(OnClicked_Start);
    }

    void OnClicked_Start()
    {
        GameMgr.Inst.BattleFSM.SetReadyState();
        gameObject.SetActive(false);
    }

}
