using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 管理遊戲生命值系統與 Game Over UI
/// </summary>
public class GameUIManager : MonoBehaviour
{
	[Header("生命值顯示")]
	[SerializeField] private Image[] heartImages; // 3 個心形圖示（按順序：左到右或上到下）
	
	[Header("Game Over 面板")]
	[SerializeField] private GameObject gameOverPanel; // Game Over 面板 GameObject
	[SerializeField] private Button retryButton; // 重新開始按鈕
	
	[Header("設定")]
	[SerializeField] private int maxLives = 3; // 最大生命值
	[SerializeField] private Sprite heartFull; // 完整的心形圖示
	
	private int currentLives;
	private bool isGameOver;
	
	// 事件：當生命值歸零時觸發
	public System.Action OnGameOver;
	
	// 公開屬性
	public int CurrentLives => currentLives;
	public bool IsGameOver => isGameOver;
	
	void Awake()
	{
		// 初始化生命值
		currentLives = maxLives;
		isGameOver = false;
		
		// 隱藏 Game Over 面板
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}
		
		// 設定 Retry 按鈕
		if (retryButton != null)
		{
			retryButton.onClick.RemoveAllListeners();
			retryButton.onClick.AddListener(OnRetryClicked);
		}
		
		// 更新生命值顯示
		UpdateLivesDisplay();
	}
	
	public bool LoseLife()
	{
		if (isGameOver) return true;
		
		currentLives--;
		currentLives = Mathf.Max(0, currentLives); // 確保不為負數
		
		UpdateLivesDisplay();
		
		// 檢查是否遊戲結束
		if (currentLives <= 0)
		{
			TriggerGameOver();
			return true;
		}
		
		return false;
	}
	
	void UpdateLivesDisplay()
	{
		if (heartImages == null || heartImages.Length == 0) return;
		
		for (int i = 0; i < heartImages.Length; i++)
		{
			if (heartImages[i] == null) continue;
			
			// 如果索引小於當前生命值，顯示完整心形，否則顯示空的心形
			if (i < currentLives)
			{
				// 顯示完整心形
				if (heartFull != null)
				{
					heartImages[i].sprite = heartFull;
				}
				heartImages[i].color = Color.white; // 完整不透明度
			}
			else
			{
				heartImages[i].color = new Color(1f, 1f, 1f, 0.25f); // 降低透明度
			}
		}
	}

	void TriggerGameOver()
	{
		if (isGameOver) return;
		
		isGameOver = true;
	    gameOverPanel.SetActive(true);
		// 觸發事件
		OnGameOver?.Invoke();
	}
	
	public void ResetGame()
	{
		currentLives = maxLives;
		isGameOver = false;
		
		if (gameOverPanel != null)
			gameOverPanel.SetActive(false);
		
		// 更新生命值顯示
		UpdateLivesDisplay();
	}
	
	void OnRetryClicked()
	{
		ResetGame();
		
		// 通知 QuizUIManager 重新開始
		QuizUIManager quizManager = FindObjectOfType<QuizUIManager>();
		if (quizManager != null)
		{
			quizManager.RestartQuiz();
		}
	}
}