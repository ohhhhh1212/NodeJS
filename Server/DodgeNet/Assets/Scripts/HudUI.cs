using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudUI : MonoBehaviour
{
    public TitleDlg m_TitleDlg = null;
    public GameDlg m_GameDlg = null;
    public ResultDlg m_ResultDlg = null;

    public void Init()
    {
        //m_TitleDlg.Init();
        m_ResultDlg.Init();
    }

    public void OnReadyState()
    {
        //m_TitleDlg.gameObject.SetActive(false);
        m_GameDlg.gameObject.SetActive(true);
        m_ResultDlg.gameObject.SetActive(false);
        m_GameDlg.StartCount();
    }

    public void OnGameState()
    {
        m_GameDlg.OnGameState();
    }

    public void OnResultState()
    {
        m_GameDlg.gameObject.SetActive(false);
        m_ResultDlg.gameObject.SetActive(true);
        m_ResultDlg.PrintResult();
    }
}
