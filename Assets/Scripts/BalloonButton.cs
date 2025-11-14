using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 處理氣球按鈕點擊事件的腳本
/// 每個氣球物件都應該附加此腳本
/// </summary>
public class BalloonButton : MonoBehaviour, IPointerClickHandler
{
	[Header("氣球設定")]
	[Tooltip("此氣球代表的選項數值")]
	[SerializeField] private int optionValue;
	
	[Tooltip("氣球物件本身（通常是此 GameObject）")]
	[SerializeField] private GameObject balloonObject;
	
	[Tooltip("QuizUIManager 的參考（可在 Inspector 中設定，或自動尋找）")]
	[SerializeField] private QuizUIManager quizUI;
	
	void Awake()
	{
		// 如果 balloonObject 未設定，預設為此 GameObject
		if (balloonObject == null)
		{
			balloonObject = gameObject;
		}
		
		// 如果 quizUI 未設定，自動尋找場景中的 QuizUIManager
		if (quizUI == null)
		{
			quizUI = FindObjectOfType<QuizUIManager>();
		}
	}
	
	/// <summary>
	/// 當玩家點擊氣球時觸發（IPointerClickHandler 介面）
	/// </summary>
	/// <param name="eventData">點擊事件資料</param>
	public void OnPointerClick(PointerEventData eventData)
	{
		// 如果 QuizUIManager 不存在，無法處理
		if (quizUI == null)
		{
			Debug.LogWarning("BalloonButton: QuizUIManager 未找到！");
			return;
		}
		
		// 隱藏氣球
		if (balloonObject != null)
		{
			balloonObject.SetActive(false);
		}
		
		// 通知 QuizUIManager 處理氣球點擊
		quizUI.HandleBalloonClicked(this, optionValue);
	}
	
	/// <summary>
	/// 設定選項數值（可在執行時動態設定）
	/// </summary>
	/// <param name="value">選項數值</param>
	public void SetOptionValue(int value)
	{
		optionValue = value;
	}
	
	/// <summary>
	/// 取得選項數值
	/// </summary>
	/// <returns>選項數值</returns>
	public int GetOptionValue()
	{
		return optionValue;
	}
	
	/// <summary>
	/// 設定 QuizUIManager 參考
	/// </summary>
	/// <param name="manager">QuizUIManager 實例</param>
	public void SetQuizUI(QuizUIManager manager)
	{
		quizUI = manager;
	}
}
