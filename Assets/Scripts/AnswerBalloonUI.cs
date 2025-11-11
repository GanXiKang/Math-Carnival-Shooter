using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 答題氣球（Button 或 Image）：
/// - 點擊後隱藏/刪除氣球
/// - 在原位置生成「答對/答錯」回饋圖片（預置物）
/// - 可設定 parent Canvas
/// - 可重用於多個氣球
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class AnswerBalloonUI : MonoBehaviour
{
	[Header("設定")]
	[SerializeField] private bool isCorrect = false;
	[SerializeField] private bool destroyBalloonOnClick = true;
	[SerializeField] private float feedbackDurationSeconds = 1f;

	[Header("回饋 Prefab")]
	[SerializeField] private GameObject correctFeedbackPrefab;
	[SerializeField] private GameObject wrongFeedbackPrefab;

	[Header("UI 父物件（可選）")]
	[SerializeField] private Canvas uiCanvas; // 若未指定，會嘗試尋找最近的父 Canvas

	private Button cachedButton;
	private RectTransform rectTransform;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		cachedButton = GetComponent<Button>();

		// 若物件上沒有 Button，但有 Image，則自動加上 Button 以接收 onClick
		if (cachedButton == null)
		{
			var img = GetComponent<Image>();
			if (img != null)
			{
				cachedButton = gameObject.AddComponent<Button>();
			}
		}

		// 若未指定 Canvas，找最近的父 Canvas
		if (uiCanvas == null)
		{
			uiCanvas = GetComponentInParent<Canvas>();
		}
	}

	void OnEnable()
	{
		if (cachedButton != null)
		{
			cachedButton.onClick.AddListener(OnClicked);
		}
	}

	void OnDisable()
	{
		if (cachedButton != null)
		{
			cachedButton.onClick.RemoveListener(OnClicked);
		}
	}

	void OnClicked()
	{
		SpawnFeedbackAtThis();

		if (destroyBalloonOnClick)
		{
			Destroy(gameObject);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	void SpawnFeedbackAtThis()
	{
		GameObject prefab = isCorrect ? correctFeedbackPrefab : wrongFeedbackPrefab;
		if (prefab == null) return;

		// 需要 RectTransform 的 UI 預置物
		var rtPrefab = prefab.GetComponent<RectTransform>();
		if (rtPrefab == null) return;

		Canvas targetCanvas = uiCanvas != null ? uiCanvas : GetComponentInParent<Canvas>();
		if (targetCanvas == null) return;

		// 轉換世界座標到 Canvas 的本地座標，放置在同一畫面位置
		var go = Instantiate(prefab, targetCanvas.transform);
		var rt = go.GetComponent<RectTransform>();
		if (rt != null)
		{
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
				targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
				rectTransform.position
			);

			RectTransform canvasRect = targetCanvas.transform as RectTransform;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvasRect,
				screenPoint,
				targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
				out Vector2 localPoint))
			{
				rt.anchoredPosition = localPoint;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
			}
		}

		Destroy(go, Mathf.Max(0.01f, feedbackDurationSeconds));
	}

	// 允許動態設定正確與否
	public void Configure(bool correct)
	{
		isCorrect = correct;
	}
}


