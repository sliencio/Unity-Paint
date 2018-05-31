using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
public class MainControl : MonoBehaviour {
	//触摸反馈
	private UIEventFilter m_UIEventFileter = null;
	//画板
	private Transform m_PaintPanel = null;
	//最后一个坐标点
	private Vector3 m_LastPos = Vector3.zero;
	//材质
	public Material m_LineMat = null;
	//一个点的坐标列表
	private List<Vector3> m_PointList = new List<Vector3>();
	//新建线段的gameObject列表
	private List<GameObject> m_LineHistoryObjList = new List<GameObject>();

	//当前LineRender 
	private LineRenderer m_CurrentLienRenderer = null;
	//当前颜色
	private Color m_CurrentLineColor = Color.red;
	//当前背景
	private Sprite m_CurrentBgSp = null;
	//线段的容器
	private Transform m_LineObjContent = null;
	//当前划线的宽度
	private int m_nCurrentLineWidth = 2;
	//按钮的状态
	private bool bIsTouchDown = false;
	//当前fingerId 
	private int m_nCurrentFingerId = -1;
	//最下距离才进行添加点
	private float m_fMinDrawDis = 0.01f;
	//初始深度
	private float m_fMinDeep = 0;

	//后退按钮
	private Button m_BackBtn = null;

	//左侧按钮对应功能面板字典
	private Dictionary<GameObject, GameObject> m_LeftFuntionBtnPanelDict = new Dictionary<GameObject, GameObject>();
	//左侧功能按钮开关组
	private ToggleGroup m_LeftFunctionBtnToggleGroup = null;
	//背景面板
	private GameObject m_BgPanel = null;
	//背景容器
	private Transform m_BgContent = null;
	//背景数据
	private JSONNode m_BgListData = null;
	//预制体对象
	private GameObject m_BgItemPrefab = null;
	private List<GameObject> m_BgItemList = new List<GameObject>();


	//保存图片面板
	private GameObject m_SaveImagePanel = null;
	//保存图片的路径
	private string m_strSaveImagePath = string.Empty;
	//背景容器
	private Transform m_SaveImageContent = null;
	private List<GameObject> m_SaveImageItemList = new List<GameObject>();
	//保存图片的开关组
	private ToggleGroup m_SaveImageToggleGroup = null;


	//颜色面板
	private GameObject m_ColorPanel = null;
	//预制体对象
	private GameObject m_ColorItemPrefab = null;
	private JSONNode m_ColorListData = null;
	private List<GameObject> m_ColorItemList = new List<GameObject>();

	//画笔大小控制器
	private Slider m_PaintWidthSlider = null;

	// Use this for initialization
	void Start () {
		m_LineObjContent = GameObject.Find("PaintObjs").transform;
		m_strSaveImagePath = Application.persistentDataPath + "/ScreenShot/";
		if (!Directory.Exists(m_strSaveImagePath)) {
			Directory.CreateDirectory(m_strSaveImagePath);
		}
		m_BgItemPrefab = Resources.Load("Prefabs/BgItemPrefab", typeof(GameObject)) as GameObject;
		m_ColorItemPrefab = Resources.Load("Prefabs/ColorItem", typeof(GameObject)) as GameObject;
		TextAsset colorAsset = Resources.Load("Json/Color", typeof(TextAsset)) as TextAsset;
		m_ColorListData = JSONNode.Parse(colorAsset.text);
		TextAsset bgAsset = Resources.Load("Json/Bg", typeof(TextAsset)) as TextAsset;
		m_BgListData = JSONNode.Parse(bgAsset.text);


		m_PaintPanel = transform.Find("paintPanel");
		m_CurrentBgSp = m_PaintPanel.GetComponent<Image>().sprite;
		m_UIEventFileter = m_PaintPanel.GetComponent<UIEventFilter>();
		m_ColorPanel = transform.Find("ColorContent").gameObject;
		m_BgPanel = transform.Find("BgList").gameObject;
		if (null != m_BgPanel) {
			m_BgContent = m_BgPanel.transform.Find("content");
		}
		m_SaveImagePanel = transform.Find("SaveContent").gameObject;
		if (null != m_SaveImagePanel) {
			m_SaveImageContent = m_SaveImagePanel.transform.Find("content");
			m_SaveImageToggleGroup = m_SaveImageContent.GetComponent<ToggleGroup>();
		}
		//左侧面板
		Transform leftFunctionPanel = transform.Find("LeftFunctionPanel");
		if (null != leftFunctionPanel) {
			m_LeftFunctionBtnToggleGroup = leftFunctionPanel.GetComponent<ToggleGroup>();
			GameObject colorListBtn = leftFunctionPanel.Find("ColorListBtn").gameObject;
			m_LeftFuntionBtnPanelDict.Add(colorListBtn, m_ColorPanel);

			GameObject bgListBtn = leftFunctionPanel.Find("bgListBtn").gameObject;
			m_LeftFuntionBtnPanelDict.Add(bgListBtn, m_BgPanel);

			GameObject saveImgListBtn = leftFunctionPanel.Find("saveImglistBtn").gameObject;
			m_LeftFuntionBtnPanelDict.Add(saveImgListBtn, m_SaveImagePanel);

			m_PaintWidthSlider = leftFunctionPanel.Find("Slider").GetComponent<Slider>();
			m_PaintWidthSlider.onValueChanged.AddListener(PaintWidthChanged);
			m_PaintWidthSlider.value = m_nCurrentLineWidth;

			//注册回调
			foreach (GameObject funtionBtn in m_LeftFuntionBtnPanelDict.Keys) {
				funtionBtn.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool isOn) {
					FunctionBtnClick(funtionBtn, isOn);
				});
			}
		}

		//右侧面板
		Transform rightFunctionPanel = transform.Find("RightFuntionBtnPanel");
		if (null != rightFunctionPanel) {
			//删除按钮
			rightFunctionPanel.Find("delBtn").GetComponent<Button>().onClick.AddListener(delegate () {
				string delName = CheckDeleteOpetion();
				//删除操作
				if (delName != string.Empty) {
					PaintTips.GetInstance().Show(TipsType.DELETE);
				}
				//清除操作
				else {
					PaintTips.GetInstance().Show(TipsType.CLEAN);
				}

			});
			//返回按钮
			m_BackBtn = rightFunctionPanel.Find("backBtn").GetComponent<Button>();
			m_BackBtn.onClick.AddListener(delegate () {
				if (m_LineHistoryObjList.Count > 0) {
					m_BackBtn.interactable = true;
					Destroy(m_LineHistoryObjList[m_LineHistoryObjList.Count - 1]);
					m_LineHistoryObjList.RemoveAt(m_LineHistoryObjList.Count - 1);
				}
				else {
					m_BackBtn.interactable = false;
				}

			});
			//保存按钮
			rightFunctionPanel.Find("saveBtn").GetComponent<Button>().onClick.AddListener(delegate () {
				PaintTips.GetInstance().Show(TipsType.SAVE);
				m_LeftFunctionBtnToggleGroup.SetAllTogglesOff();
			});
		}


		//初始化颜色列表
		InitColorList();
		//初始化背景列表
		InitBgList();
		//初始化保存图片列表
		InitSaveImageList();
	}


	// Update is called once per frame
	void Update () {
		if (m_UIEventFileter.getIsOnTouchOver()) {
#if UNITY_EDITOR
			//按下左键
			if (Input.GetMouseButtonDown(0)) {
				TouchBegain();
			}

			//左键抬起
			if (Input.GetMouseButtonUp(0)) {
				m_BackBtn.interactable = ( m_LineHistoryObjList.Count > 0 );
				m_PointList.Clear();
				bIsTouchDown = false;
			}
#endif

#if UNITY_IOS
			if (m_nCurrentFingerId == -1 && Input.touchCount > 0) {
				m_nCurrentFingerId = Input.touches[0].fingerId;
				TouchBegain();
				//print("----touchBegain------");
			}
#endif
			//左键移动
			if (bIsTouchDown) {
				Vector3 mousePos = GetPosition();
				if (Vector3.SqrMagnitude(mousePos - m_LastPos) > m_fMinDrawDis && m_CurrentLienRenderer != null) {
					//print("----draw------");
					m_PointList.Add(mousePos);
					m_CurrentLienRenderer.positionCount = m_PointList.Count;
					m_CurrentLienRenderer.SetPositions(m_PointList.ToArray());
					m_LastPos = mousePos;
				}
			}
		}
		else {
#if UNITY_IOS
			if (Input.touchCount == 0) {
				m_BackBtn.interactable = ( m_LineHistoryObjList.Count > 0 );
				m_nCurrentFingerId = -1;
				m_PointList.Clear();
				bIsTouchDown = false;
				//print("----touchEnded------");
			}
#endif
		}
	}

	/// <summary>
	/// 开始按下
	/// </summary>
	void TouchBegain () {
		//设置所有的关闭
		m_LeftFunctionBtnToggleGroup.SetAllTogglesOff();
		//创建线段
		CreateLineRenderer();
		//标示开始划线
		bIsTouchDown = true;
		//设置线段显示
		m_LineObjContent.gameObject.SetActive(true);
		//重新设置背景
		m_PaintPanel.GetComponent<Image>().sprite = m_CurrentBgSp;
		//初始化第一个点
		Vector3 firstPos = GetPosition();
		m_PointList.Add(firstPos);
		m_LastPos = firstPos;
	}

	/// <summary>
	/// 可用时调用
	/// </summary>
	private void OnEnable () {
		PaintTips.PaintTipsResponse += PaintTipsResponse;
	}

	/// <summary>
	/// 不可用调用
	/// </summary>
	private void OnDisable () {
		PaintTips.PaintTipsResponse -= PaintTipsResponse;
	}

	/// <summary>
	/// 弹出框回调
	/// </summary>
	void PaintTipsResponse () {
		TipsType type = PaintTips.GetInstance().PaintTipsType;
		switch (type) {
			case TipsType.DELETE:
				string delName = CheckDeleteOpetion();
				//进行删除操作
				if (delName != string.Empty) {
					print("name:" + delName);
					string imagFullPath = m_strSaveImagePath + delName;
					if (File.Exists(imagFullPath)) {
						File.Delete(imagFullPath);
					}
					InitSaveImageList();
					//删除后重新设置背景
					m_PaintPanel.GetComponent<Image>().sprite = m_CurrentBgSp;
				}
				break;
			case TipsType.SAVE:
				string texName = PaintTips.GetInstance().GetSaveName();
				//判断名称是否为空
				if (texName == string.Empty) {
					CommonMesTips.GetInstance().AlertTips("输入的名称不能为空！");
					return;
				}
				//检测是否该文件是否已经存在
				string tempPath = m_strSaveImagePath + texName + ".png";
				//不能进行保存
				if (File.Exists(tempPath)) {
					PaintTips.GetInstance().Show(TipsType.COVER);
					return;
				}
				else {
					StartCoroutine(CaptureByRect(new Rect(130, 0, Screen.width - 260, Screen.height), tempPath));
				}
				break;
			//进行清空操作
			case TipsType.CLEAN:
				m_BackBtn.interactable = false;
				for (int i = 0; i < m_LineHistoryObjList.Count; i++) {
					Destroy(m_LineHistoryObjList[i]);
				}
				m_LineHistoryObjList.RemoveRange(0, m_LineHistoryObjList.Count);
				break;
			case TipsType.COVER:
				string texName2 = PaintTips.GetInstance().GetSaveName();
				//检测是否该文件是否已经存在
				string tempPath2 = m_strSaveImagePath + texName2 + ".png";
				StartCoroutine(CaptureByRect(new Rect(130, 0, 1074, 750), tempPath2));
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 判断是否为删除操作，否：为清除操作
	/// </summary>
	/// <returns><c>true</c>, if delete opetion was checked, <c>false</c> otherwise.</returns>
	string CheckDeleteOpetion () {
		//将要删除的名称
		string delName = string.Empty;
		//1.删除操作：当左侧面板弹出，并且当前选中的是保存图片的面板，进行删除操作
		if (m_LeftFunctionBtnToggleGroup.AnyTogglesOn() && m_SaveImageToggleGroup.AnyTogglesOn()) {
			Toggle to = m_LeftFunctionBtnToggleGroup.ActiveToggles().FirstOrDefault();
			//判断图片列表的按钮是否选中
			if (to.name == "saveImglistBtn") {
				//拿到将要删除的元素
				GameObject willDelGo = m_SaveImageToggleGroup.ActiveToggles().FirstOrDefault().gameObject;
				delName = willDelGo.name;
			}
		}
		return delName;
	}



	/// <summary>
	/// 获取坐标
	/// </summary>
	/// <returns>The position.</returns>
	Vector3 GetPosition () {
		Vector3 retPos = Vector3.zero;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		bool isCollider = Physics.Raycast(ray, out hit);
		if (isCollider && hit.collider.gameObject.tag == "Plant") {
			retPos = new Vector3(hit.point.x, hit.point.y, 0);
		}
		return retPos;
	}

	/// <summary>
	/// 创建LineRenderer
	/// </summary>
	void CreateLineRenderer () {
		GameObject lineGo = new GameObject();
		lineGo.AddComponent<LineRenderer>();
		LineRenderer line = lineGo.GetComponent<LineRenderer>();
		SetLineRendererProperty(line);
		m_CurrentLienRenderer = line;
		m_fMinDeep -= 0.01f;
		lineGo.transform.position = new Vector3(0, 0, m_fMinDeep);
		lineGo.transform.SetParent(m_LineObjContent);
		m_LineHistoryObjList.Add(lineGo);
	}

	/// <summary>
	/// 设置LineRendere的属性
	/// </summary>
	/// <param name="lineRenderer">Line renderer.</param>
	void SetLineRendererProperty (LineRenderer lineRenderer) {
		lineRenderer.startWidth = m_nCurrentLineWidth;
		lineRenderer.endWidth = m_nCurrentLineWidth;
		lineRenderer.startColor = m_CurrentLineColor;
		lineRenderer.endColor = m_CurrentLineColor;
		lineRenderer.numCornerVertices = 5;
		lineRenderer.numCornerVertices = 5;
		lineRenderer.material = m_LineMat;
	}

	/// <summary>
	/// 颜色面板回调
	/// </summary>
	/// <param name="go">Go.</param>
	/// <param name="isOn">If set to <c>true</c> isOn.</param>
	void FunctionBtnClick (GameObject go, bool isOn) {
		foreach (GameObject functionBtn in m_LeftFuntionBtnPanelDict.Keys) {
			GameObject targetPanel = m_LeftFuntionBtnPanelDict[functionBtn];
			if (functionBtn != go) {
				targetPanel.SetActive(false);
			}
			else {
				targetPanel.SetActive(isOn);
				//全部重置为不选中状态
				if (go.name == "saveImglistBtn") {
					m_SaveImageToggleGroup.SetAllTogglesOff();
				}
			}
		}
	}


	/// <summary>
	/// 具体的颜色按钮按下
	/// </summary>
	/// <param name="colorGo">Color go.</param>
	void ColorItemClick (GameObject colorGo) {
		if (m_ColorItemList.Contains(colorGo)) {
			int index = m_ColorItemList.IndexOf(colorGo);
			if (index < m_ColorListData.Count) {
				m_CurrentLineColor = colorGo.GetComponent<Image>().color;
			}
		}
	}

	/// <summary>
	/// 具体的颜色按钮按下
	/// </summary>
	/// <param name="bgGo">Background go.</param>
	void BgItemClick (GameObject bgGo) {
		if (m_BgItemList.Contains(bgGo)) {
			int index = m_BgItemList.IndexOf(bgGo);
			if (index < m_BgListData.Count) {
				Sprite willSetSp = bgGo.GetComponent<Image>().sprite;
				m_PaintPanel.GetComponent<Image>().sprite = willSetSp;
				m_CurrentBgSp = willSetSp;
			}
		}
	}


	/// <summary>
	/// 画笔宽度改变
	/// </summary>
	void PaintWidthChanged (float wid) {
		m_nCurrentLineWidth = (int)wid;
		m_PaintWidthSlider.handleRect.Find("Text").GetComponent<Text>().text = m_nCurrentLineWidth.ToString();
	}

	/// <summary>
	/// 初始化颜色列表
	/// </summary>
	void InitColorList () {
		//开关组
		ToggleGroup colorToggleGroup = m_ColorPanel.GetComponent<ToggleGroup>();
		if (null != m_ColorListData) {
			for (int i = 0; i < m_ColorListData.Count; i++) {
				GameObject colorItem = Instantiate(m_ColorItemPrefab);
				colorItem.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool isOn) {
					if (isOn) {
						ColorItemClick(colorItem);
					}
				});
				colorItem.GetComponent<Toggle>().group = colorToggleGroup;
				colorItem.transform.SetParent(m_ColorPanel.transform);
				colorItem.transform.localScale = Vector3.one;
				colorItem.transform.localPosition = Vector3.zero;
				JSONNode tempColor = JSONNode.Parse(m_ColorListData[i]["rgb"]);
				float r = float.Parse(tempColor[0]);
				float g = float.Parse(tempColor[1]);
				float b = float.Parse(tempColor[2]);
				colorItem.GetComponent<Image>().color = new Color(r / 255, g / 255, b / 255, 1);
				m_ColorItemList.Add(colorItem);
				if (i == 0) {
					colorItem.GetComponent<Toggle>().isOn = true;
				}
			}
		}
	}

	/// <summary>
	/// 初始化背景列表
	/// </summary>
	void InitBgList () {
		ToggleGroup bgToggleGroup = m_BgContent.GetComponent<ToggleGroup>();
		for (int i = 0; i < m_BgListData.Count; i++) {
			string bgImageName = m_BgListData[i]["icon"];
			GameObject bgItem = Instantiate(m_BgItemPrefab);
			bgItem.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool isOn) {
				if (isOn) {
					BgItemClick(bgItem);
				}
			});
			bgItem.GetComponent<Toggle>().group = bgToggleGroup;
			bgItem.transform.SetParent(m_BgContent);
			bgItem.transform.localScale = Vector3.one;
			bgItem.transform.localPosition = Vector3.zero;
			bgItem.GetComponent<Image>().sprite = Resources.Load("UI/" + bgImageName, typeof(Sprite)) as Sprite;
			m_BgItemList.Add(bgItem);
			if (i == 0) {
				bgItem.GetComponent<Toggle>().isOn = true;
			}
		}
	}

	/// <summary>
	/// 初始化保存图片列表
	/// </summary>
	void InitSaveImageList () {
		for (int i = 0; i < m_SaveImageContent.childCount; i++) {
			Destroy(m_SaveImageContent.GetChild(i).gameObject);
		}

		Dictionary<string, Sprite> saveImageSprite = new Dictionary<string, Sprite>();
		if (Directory.Exists(m_strSaveImagePath)) {
			DirectoryInfo directory = new DirectoryInfo(m_strSaveImagePath);
			FileInfo[] files = directory.GetFiles("*.png");
			for (int i = 0; i < files.Length; i++) {
				string fileName = files[i].Name;
				if (fileName == ".png") {
					continue;
				}
				Texture2D tex = new Texture2D(1024, 512);
				tex.LoadImage(GetImageByte(m_strSaveImagePath + files[i].Name));
				Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5F, 0.5F));
				saveImageSprite.Add(files[i].Name, sp);
			}
		}
		foreach (string key in saveImageSprite.Keys) {
			GameObject saveImageItem = Instantiate(m_BgItemPrefab);
			saveImageItem.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool isOn) {
				if (isOn) {
					//加载画图
					m_PaintPanel.GetComponent<Image>().sprite = saveImageSprite[key];
					//隐藏划线
					m_LineObjContent.gameObject.SetActive(false);
				}
			});
			saveImageItem.GetComponent<Toggle>().group = m_SaveImageToggleGroup;
			saveImageItem.transform.SetParent(m_SaveImageContent);
			saveImageItem.transform.localScale = Vector3.one;
			saveImageItem.transform.localPosition = Vector3.zero;
			saveImageItem.GetComponent<Image>().sprite = saveImageSprite[key];
			saveImageItem.name = key;
			m_SaveImageItemList.Add(saveImageItem);
		}
	}
	/// <summary>
	/// 根据图片路径返回图片的字节流byte[]  
	/// </summary>
	/// <returns>返回字节流</returns>
	/// <param name="imagePath">图片路径.</param>
	private static byte[] GetImageByte (string imagePath) {
		FileStream files = new FileStream(imagePath, FileMode.Open);
		byte[] imgByte = new byte[files.Length];
		files.Read(imgByte, 0, imgByte.Length);
		files.Close();
		return imgByte;
	}


	/// <summary>  
	/// 根据一个Rect类型来截取指定范围的屏幕  
	/// 左下角为(0,0)  
	/// </summary>  
	/// <param name="rect">M rect.</param>  
	/// <param name="fileName">M file name.</param>  
	private IEnumerator CaptureByRect (Rect rect, string fileName) {
		//等待渲染线程结束  
		yield return new WaitForEndOfFrame();
		//初始化Texture2D  
		Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
		//读取屏幕像素信息并存储为纹理数据  
		tex.ReadPixels(rect, 0, 0);
		//应用  
		tex.Apply();
		//将图片信息编码为字节信息  
		byte[] bytes = tex.EncodeToPNG();
		//保存  
		File.WriteAllBytes(fileName, bytes);
		InitSaveImageList();
	}
}