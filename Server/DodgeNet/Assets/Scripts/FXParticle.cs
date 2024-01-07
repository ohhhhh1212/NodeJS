using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXParticle : MonoBehaviour
{
	public bool m_OnlyDeactivate = false;
	private Coroutine m_kCoroutine = null;

	private bool m_bLoop = true;

	// Start is called before the first frame update
	void OnEnable()
	{
		m_kCoroutine = StartCoroutine("EnumFunc_CheckAlive", 0.5f);
	}


    public void Play()
    {
		Stop();
		Invoke("Callback_StartFX", 0.1f);
	}


	void Callback_StartFX()
    {
		this.gameObject.SetActive(true);
	}


	public void Stop()
    {
		m_bLoop = false;
		this.gameObject.SetActive(false);
	}


    IEnumerator EnumFunc_CheckAlive(float fDealy)
	{
		//yield return new WaitForSeconds(fDealy);
		m_bLoop = true;

		while (m_bLoop)
		{
			yield return new WaitForSeconds(fDealy);
			if (!GetComponent<ParticleSystem>().IsAlive(true))
			{
				if (m_OnlyDeactivate)
					this.gameObject.SetActive(false);
				else
					GameObject.Destroy(this.gameObject);

				m_bLoop = false;
				break;
			}
		}
	}
}
