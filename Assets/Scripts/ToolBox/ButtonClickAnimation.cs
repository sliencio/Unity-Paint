using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

	public Vector3 m_ScaleSize = new Vector3 (0.95f, 0.95f, 0.95f);
	private float m_fScale = 1.0f;
	/// <summary>
	/// 点击按下
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerDown (PointerEventData eventData) {
		transform.localScale = new Vector3(m_fScale,m_fScale,m_fScale);
		m_fScale = transform.localScale.x;
		transform.DOScale (m_ScaleSize * m_fScale, 0.2f);
	}
	/// <summary>
	/// 点击抬起
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerUp (PointerEventData eventData) {
		transform.DOScale (Vector3.one * m_fScale, 0.2f);
	}
	/// <summary>
	/// 点击退出
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerExit (PointerEventData eventData) { }

	/// <summary>
	/// 设置缩放
	/// </summary>
	/// <param name="scale"></param>
	public void SetScale(float scale){
		m_fScale = scale;
	}
}