using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 可重用的答題氣球點擊腳本：
/// - 點擊後隱藏/刪除氣球本體
/// - 在氣球位置顯示「答對/答錯」回饋圖片，1 秒後自動消失
/// - 可同時用於多個氣球
/// 支援 UI（IPointerClickHandler）與 3D/2D 物件（OnMouseDown，有 Collider）
/// </summary>
public class AnswerBalloon : MonoBehaviour, IPointerClickHandler
{
	[Header("設定")]
	[SerializeField] private bool isCorrect = false; // 此氣球是否為正確答案
	[SerializeField] private bool destroyBalloonOnClick = true; // 點擊後是否摧毀氣球（否則 SetActive(false)）
	[SerializeField] private float feedbackDurationSeconds = 1f; // 回饋顯示時長

	[Header("回饋 Prefab（必填其一）")]
	[SerializeField] private GameObject correctFeedbackPrefab; // 答對顯示的預置物（建議為含 Image 的 UI 物件）
	[SerializeField] private GameObject wrongFeedbackPrefab;   // 答錯顯示的預置物

	[Header("UI 放置（可選）")]
	[SerializeField] private Canvas uiCanvas;                  // 若使用 UI 回饋，指定目標 Canvas（建議 Screen Space - Overlay 或 Camera）
	[SerializeField] private Transform feedbackParentOverride; // 指定回饋物件的父物件（未指定則使用 Canvas 或氣球父物件）

	// UI 氣球（Button/Image）使用此事件
	public void OnPointerClick(PointerEventData eventData)
	{
		HandleClick();
	}

	// 3D/2D 物件（需有 Collider/Collider2D）使用此事件
	void OnMouseDown()
	{
		HandleClick();
	}

	void HandleClick()
	{
		ShowFeedbackAtBalloon();

		if (destroyBalloonOnClick)
		{
			Destroy(gameObject);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	void ShowFeedbackAtBalloon()
	{
		GameObject prefab = isCorrect ? correctFeedbackPrefab : wrongFeedbackPrefab;
		if (prefab == null) return;

		// 如果是 UI Canvas，將回饋物件轉為 UI 並放置在相同畫面位置
		if (uiCanvas != null && prefab.GetComponent<RectTransform>() != null)
		{
			Transform parent = feedbackParentOverride != null ? feedbackParentOverride : uiCanvas.transform;
			var go = Instantiate(prefab, parent);
			PositionUIFeedbackAtThis(go);
			Destroy(go, Mathf.Max(0.01f, feedbackDurationSeconds));
			return;
		}

		// 非 UI：以世界座標生成
		{
			Transform parent = feedbackParentOverride != null ? feedbackParentOverride : transform.parent;
			var go = Instantiate(prefab, transform.position, Quaternion.identity, parent);
			Destroy(go, Mathf.Max(0.01f, feedbackDurationSeconds));
		}
	}

	void PositionUIFeedbackAtThis(GameObject feedbackGO)
	{
		var rt = feedbackGO.GetComponent<RectTransform>();
		if (rt == null || uiCanvas == null) return;

		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
			uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera,
			transform.position
		);

		RectTransform canvasRect = uiCanvas.transform as RectTransform;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvasRect,
			screenPoint,
			uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera,
			out Vector2 localPoint))
		{
			rt.anchoredPosition = localPoint;
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
			// 可依需求調整尺寸或錨點
		}
	}

	// 對外方法：在生成後設定是否為正確答案
	public void Configure(bool correct)
	{
		isCorrect = correct;
	}
}


