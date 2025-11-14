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
    [SerializeField] private QuizUIManager quizUI;
    private GameObject balloonObject;

	void Awake()
	{
		balloonObject = gameObject;
	}
	
	/// <summary>
	/// 當玩家點擊氣球時觸發（IPointerClickHandler 介面）
	/// </summary>
	/// <param name="eventData">點擊事件資料</param>
	public void OnPointerClick(PointerEventData eventData)
	{
		// 隱藏氣球
		balloonObject.SetActive(false);
		// 通知 QuizUIManager 處理氣球點擊
		quizUI.HandleBalloonClicked(this, optionValue);
    }

	public void SetOptionValue(int value)
	{
		optionValue = value;
	}

	public int GetOptionValue()
	{
		return optionValue;
	}
	
	public void SetQuizUI(QuizUIManager manager)
	{
		quizUI = manager;
	}
}
