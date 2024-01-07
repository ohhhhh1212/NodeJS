using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultDlg : MonoBehaviour
{
    [SerializeField] Button m_btnReStart = null;
    [SerializeField] Button m_btnExit = null;
    [SerializeField] Text m_txtTime = null;

    public void Init()
    {
        m_btnReStart.onClick.AddListener(OnClicked_ReStart);
        m_btnExit.onClick.AddListener(OnClicked_Exit);
        gameObject.SetActive(false);
    }

    void OnClicked_ReStart()
    {
        GameMgr.Inst.BattleFSM.SetReadyState();
    }

    void OnClicked_Exit()
    {
        GameMgr.Inst.BattleFSM.SetNoneState();
        SceneManager.LoadScene(0);
    }

    public void PrintResult()
    {
        int sec = GameMgr.Inst.GameInfo.Sec % 60;
        int min = GameMgr.Inst.GameInfo.Sec / 60;

        m_txtTime.text = string.Format("{0:00} : {1:00}", min, sec);
    }
}
