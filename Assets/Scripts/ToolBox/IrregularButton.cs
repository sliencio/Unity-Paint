/// <summary>
/// 根据alpha 设置多边形按钮
/// Author：LMN
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class IrregularButton : MonoBehaviour {
	private float m_fEventAlphaThreshold = 0.1f;
    void Awake () {
        // 设置阈值
        Image image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = m_fEventAlphaThreshold;
    }
}