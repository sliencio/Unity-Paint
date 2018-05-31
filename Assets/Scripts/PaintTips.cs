using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum TipsType {
	DELETE,
	SAVE,
	CLEAN,
	COVER
}

public class PaintTips : MonoBehaviour {
	private GameObject m_LineObjContent = null;
	private Transform m_UIRoot = null;
	public delegate void PaintTipsDelegate ();
	public static event PaintTipsDelegate PaintTipsResponse;
	private static PaintTips m_Instance = null;
	private bool m_bIsInit = false;
	public static PaintTips GetInstance () {
		return m_Instance;
	}

	//标题
	private Text m_Title = null;
	//输入框
	private InputField m_TextureInput = null;
	//保存图片的名称
	public string GetSaveName () {
		return m_TextureInput.text;
	}
	//当前的显示模式
	public TipsType PaintTipsType {
		set; get;
	}
	//内容框
	private Text m_Content = null;
	//提示框
	private Text m_Warming = null;
	// Use this for initialization
	void Start () {
		m_LineObjContent = GameObject.Find("PaintObjs");
		if (!m_bIsInit) {
			m_UIRoot = transform.Find("AlertWindow");
			Transform innerPanel = m_UIRoot.Find("innerPanel");
			if (null != innerPanel) {
				m_Title = innerPanel.Find("title").GetComponent<Text>();
				m_TextureInput = innerPanel.Find("InputField").GetComponent<InputField>();
				m_Content = innerPanel.Find("content").GetComponent<Text>();
				m_Warming = innerPanel.Find("warming").GetComponent<Text>();
				//确定取消按钮设置回调
				innerPanel.Find("sureBtn").GetComponent<Button>().onClick.AddListener(SureBtnClick);
				innerPanel.Find("cancelBtn").GetComponent<Button>().onClick.AddListener(CancelBtnclick);
			}
		}
		m_bIsInit = true;
	}
	private void Awake () {
		m_Instance = this;
	}

	// Update is called once per frame
	void Update () {

	}

	/// <summary>
	/// 确定按钮回调
	/// </summary>
	void SureBtnClick () {
		SetVisible(false);
		PaintTipsResponse();

	}

	/// <summary>
	/// 取消按钮回调
	/// </summary>
	void CancelBtnclick () {
		print("cancel");
		SetVisible(false);
	}


	/// <summary>
	/// 展示面板
	/// </summary>
	/// <returns>The show.</returns>
	/// <param name="type">Type.</param>
	public void Show (TipsType type) {
		if (!m_bIsInit) {
			Start();
		}
		PaintTipsType = type;
		SetVisible(true);
		m_Content.gameObject.SetActive(false);
		m_TextureInput.gameObject.SetActive(false);
		m_Warming.gameObject.SetActive(false);
		switch (type) {
			case TipsType.SAVE:
				m_Title.text = "保 存";
				m_TextureInput.gameObject.SetActive(true);
				break;
			case TipsType.DELETE:
				m_Title.text = "删 除";
				m_Content.gameObject.SetActive(true);
				m_Content.text = "删除后无法找回！";
				break;
			case TipsType.CLEAN:
				m_Title.text = "清 除";
				m_Content.gameObject.SetActive(true);
				m_Content.text = "确认清空";
				break;
			case TipsType.COVER:
				m_Title.text = "覆 盖";
				m_Content.gameObject.SetActive(true);
				m_Content.text = "是否覆盖";
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 设置提醒
	/// </summary>
	public void ShowWarming () {
		m_Warming.gameObject.SetActive(true);
	}

	/// <summary>
	/// 设置隐藏可见
	/// </summary>
	/// <param name="isVis">If set to <c>true</c> is vis.</param>
	public void SetVisible (bool isVis) {
		m_UIRoot.gameObject.SetActive(isVis);
		m_LineObjContent.SetActive(!isVis);
	}
}
