using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDlg : MonoBehaviour
{
    [SerializeField] Text m_txtTimer = null;
    [SerializeField] Text m_txtCount = null;
    [SerializeField] Slider m_sldHp = null;

    int sec = 0;
    int min = 0;

    public void StartCount()
    {
        m_txtCount.gameObject.SetActive(true);
        m_txtTimer.gameObject.SetActive(false);
        m_sldHp.gameObject.SetActive(false);

        StartCoroutine(Co_Count());
    }

    IEnumerator Co_Count()
    {
        m_txtCount.text = "3";
        yield return new WaitForSeconds(1f);
        m_txtCount.text = "2";
        yield return new WaitForSeconds(1f);
        m_txtCount.text = "1";
        yield return new WaitForSeconds(1f);
        m_txtCount.text = "Start!!";
        yield return new WaitForSeconds(0.7f);

        GameMgr.Inst.BattleFSM.SetGameState();
    }

    public void OnGameState()
    {
        m_txtTimer.gameObject.SetActive(true);
        m_sldHp.gameObject.SetActive(true);
        m_txtCount.gameObject.SetActive(false);
    }

    public void FreshHP()
    {
        m_sldHp.value = GameMgr.Inst.GameInfo.PlayerHP;
    }

    void Time()
    {
        sec = GameMgr.Inst.GameInfo.Sec % 60;
        min = GameMgr.Inst.GameInfo.Sec / 60;
        m_txtTimer.text = string.Format("{0:00} : {1:00}", min, sec);
    }

    private void Update()
    {
        Time();
    }
}
