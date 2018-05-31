using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CommonMesTips : MonoBehaviour {
	public GameObject m_AlertTipsItemPrefab;
	private int m_nInitItemNum = 15;
	private static CommonMesTips m_sInstance = null;

	//存储未使用的弹出条
	private List<GameObject> m_UnuseAlertTipsList = new List<GameObject>();
	// Use this for initialization
	void Start () {
		Create(m_nInitItemNum);
	}

	private void Awake () {
		m_sInstance = this;
	}

	public static CommonMesTips GetInstance () {
		return m_sInstance;
	}

	void Create (int count) {
		for (int i = 0; i < count; i++) {
			GameObject go = Instantiate(m_AlertTipsItemPrefab) as GameObject;
			go.transform.SetParent(transform);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			go.GetComponent<DOTweenAnimation>().onComplete.AddListener(delegate () {
				TipsCompleteAni(go);
			});
			go.SetActive(false);
			m_UnuseAlertTipsList.Add(go);
		}
	}


	void TipsCompleteAni (GameObject go) {
		go.SetActive(false);
		m_UnuseAlertTipsList.Add(go);
	}
	GameObject GetAlert () {
		GameObject retGo = null;
		if (m_UnuseAlertTipsList.Count == 0) {
			Create(5);
		}
		retGo = m_UnuseAlertTipsList[0];
		m_UnuseAlertTipsList.Remove(retGo);
		return retGo;
	}

	public void AlertTips (string mes) {
		GameObject tipsItem = GetAlert();
		tipsItem.GetComponentInChildren<Text>().text = mes;
		tipsItem.SetActive(true);
	}
}
