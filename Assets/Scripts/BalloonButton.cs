using UnityEngine;
using UnityEngine.EventSystems;

public class BalloonButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private QuizUIManager quizUI;
    private GameObject balloonObject;
    private int optionValue;

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

