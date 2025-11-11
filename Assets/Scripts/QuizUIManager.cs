using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUIManager : MonoBehaviour
{
	// 優先使用 TMP 文字元件
	[SerializeField] private TMP_Text questionTextTMP;
	[SerializeField] private TMP_Text resultTextTMP;
	[SerializeField] private TMP_Text levelProgressTMP;
	// 備用：使用內建 UI Text（若未使用或無法使用 TMP）
	[SerializeField] private Text questionTextUI;
	[SerializeField] private Text resultTextUI;
	[SerializeField] private Text levelProgressUI;

	// 答案按鈕（共 4 個）
	[SerializeField] private Button[] optionButtons;

	// 星星顯示（右上角）：容器與星星預置物（動態生成）
	[SerializeField] private RectTransform starsContainer; // 放在畫面右上角的容器（已錨點到右上）
	[SerializeField] private GameObject starPrefab; // 星星預置物（建議為 UI Image 或含 Image 的物件）
	[SerializeField] private int maxStars = 5; // 星星上限

	// 生命值系統管理器
	[SerializeField] private GameUIManager gameUIManager;

	// 難度等級字串（傳入 QuestionGenerator，例如 "Elementary"、"JuniorHigh"...）
	[SerializeField] private string level = "Elementary";

	// 作答後切換到下一題的延遲時間
	[SerializeField] private float nextQuestionDelaySeconds = 1.5f;

	// 升級所需的連續答對數（目前：小學 → 國中）
	[SerializeField] private int correctToLevelUp = 10;

	private MathQuestion currentQuestion;
	private bool isLocked;
	private int correctInCurrentLevel;
	private bool gameCompleted;
	private bool gameOver; // 遊戲結束標記（生命值歸零）

	// 等級順序（可擴充）
	private readonly string[] levelOrder = new string[] { "Elementary", "JuniorHigh", "HighSchool", "University", "PhD" };

	void Awake()
	{
		// 選用：在行動裝置上偏好直向
		Screen.orientation = ScreenOrientation.Portrait;
	}

	void Start()
	{
		// 訂閱 Game Over 事件
		if (gameUIManager != null)
		{
			gameUIManager.OnGameOver += OnGameOver;
		}
		
		GenerateAndDisplayQuestion();
		UpdateStarsUI(); // 初始顯示（Level 1 -> 1 顆星）
	}
	
	void OnDestroy()
	{
		// 取消訂閱事件
		if (gameUIManager != null)
		{
			gameUIManager.OnGameOver -= OnGameOver;
		}
	}

	// 產生新題目並更新介面
	void GenerateAndDisplayQuestion()
	{
		isLocked = false;
		
		// 檢查遊戲是否結束（生命值歸零或完成所有等級）
		if (gameOver || gameCompleted)
		{
			// 遊戲結束後不再生成新題目，保持按鈕停用
			SetText(questionTextTMP, questionTextUI, "");
			SetButtonsInteractable(false);
			UpdateLevelProgressUI();
			return;
		}
		currentQuestion = QuestionGenerator.GenerateQuestion(level);
		SetText(questionTextTMP, questionTextUI, currentQuestion.questionText);
		SetText(resultTextTMP, resultTextUI, "");

		if (optionButtons == null || optionButtons.Length < 4) return;

		for (int i = 0; i < optionButtons.Length; i++)
		{
			int index = i; // 捕捉閉包用
			var btn = optionButtons[i];
			if (btn == null) continue;

			btn.onClick.RemoveAllListeners();
			// 將選項文字設到 TMP 或 UI.Text（若存在）
			TMP_Text tmpLabel = btn.GetComponentInChildren<TMP_Text>(true);
			Text uiLabel = btn.GetComponentInChildren<Text>(true);
			string optionText = (currentQuestion.options != null && currentQuestion.options.Length > index)
				? currentQuestion.options[index].ToString()
				: "?";
			SetText(tmpLabel, uiLabel, optionText);

			btn.interactable = true;
			btn.onClick.AddListener(() => OnOptionSelected(index));
		}

		UpdateLevelProgressUI();
	}

	void OnOptionSelected(int optionIndex)
	{
		if (isLocked || gameOver || gameCompleted) return;
		isLocked = true;

		int chosen = SafeGetOption(optionIndex);
		bool correct = chosen == currentQuestion.correctAnswer;
		SetText(resultTextTMP, resultTextUI, correct ? "答對了！" : "答錯了！");

		bool didLevelUp = false;
		bool didComplete = false;
		bool isGameOver = false;
		
		if (correct)
		{
			correctInCurrentLevel++;
			int required = GetRequiredForLevel(level);
			if (correctInCurrentLevel >= required)
			{
				string next = GetNextLevel(level);
				if (!string.IsNullOrEmpty(next))
				{
					// 晉升到下一等級
					level = next;
					correctInCurrentLevel = 0;
					didLevelUp = true;
					SetText(resultTextTMP, resultTextUI, $"Level Up！");
					UpdateStarsUI(); // 等級變更時更新星星顯示（+1，最多 5）
				}
				else
				{
					// 已是最後等級（PhD）且達成需求 → 完成遊戲
					didComplete = true;
					gameCompleted = true;
					SetText(resultTextTMP, resultTextUI, "Finish！");
				}
			}
		}
		else
		{
			// 答錯：失去一點生命值
			if (gameUIManager != null)
			{
				isGameOver = gameUIManager.LoseLife();
				if (isGameOver)
				{
					gameOver = true;
					SetText(resultTextTMP, resultTextUI, "Game Over！");
				}
			}
		}

		// 題目切換前先停用按鈕
		SetButtonsInteractable(false);
		UpdateLevelProgressUI();
		
		if (isGameOver || didComplete)
		{
			// 遊戲結束或完成後不再出題
			return;
		}
		else if (didLevelUp)
		{
			StartCoroutine(ShowLevelUpThenNext());
		}
		else
		{
			StartCoroutine(LoadNextQuestionAfterDelay(nextQuestionDelaySeconds));
		}
	}

	IEnumerator LoadNextQuestionAfterDelay(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		GenerateAndDisplayQuestion();
	}

	// 顯示升級訊息 2 秒後進入下一題
	IEnumerator ShowLevelUpThenNext()
	{
		// 訊息已在 OnOptionSelected 設定
		yield return new WaitForSeconds(2f);
		GenerateAndDisplayQuestion();
	}

	// 顯示等級進度，例如："JuniorHigh: 3/8"
	void UpdateLevelProgressUI()
	{
		int required = GetRequiredForLevel(level);
		string progressText = $"{level}: {correctInCurrentLevel}/{required}";
		SetText(levelProgressTMP, levelProgressUI, progressText);
	}

	int GetRequiredForLevel(string lvl)
	{
		switch (lvl)
		{
			case "Elementary": return 10;
			case "JuniorHigh": return 8;
			case "HighSchool": return 6;
			case "University": return 4;
			case "PhD": return 2;
			default: return 1;
		}
	}

	string GetNextLevel(string current)
	{
		for (int i = 0; i < levelOrder.Length; i++)
		{
			if (levelOrder[i] == current)
			{
				if (i + 1 < levelOrder.Length) return levelOrder[i + 1];
				return null;
			}
		}
		return null;
	}

	int SafeGetOption(int idx)
	{
		if (currentQuestion == null || currentQuestion.options == null) return int.MinValue;
		if (idx < 0 || idx >= currentQuestion.options.Length) return int.MinValue;
		return currentQuestion.options[idx];
	}

	void SetButtonsInteractable(bool value)
	{
		if (optionButtons == null) return;
		for (int i = 0; i < optionButtons.Length; i++)
		{
			if (optionButtons[i] != null) optionButtons[i].interactable = value;
		}
	}

	/// <summary>
	/// Game Over 事件處理
	/// </summary>
	void OnGameOver()
	{
		gameOver = true;
		SetButtonsInteractable(false);
	}
	
	/// <summary>
	/// 重新開始 quiz（由 GameUIManager 的 Retry 按鈕呼叫）
	/// </summary>
	public void RestartQuiz()
	{
		// 重置遊戲狀態
		gameOver = false;
		gameCompleted = false;
		correctInCurrentLevel = 0;
		level = "Elementary";
		isLocked = false;
		
		// 清除結果文字
		SetText(resultTextTMP, resultTextUI, "");
		
		// 重新生成題目
		GenerateAndDisplayQuestion();
		UpdateStarsUI(); // 重置後回到 1 顆星
	}
	
	// 工具方法：優先設定 TMP，否則設定 UI.Text
	static void SetText(TMP_Text tmp, Text ui, string value)
	{
		if (tmp != null) { tmp.text = value; return; }
		if (ui != null) { ui.text = value; }
	}

	// 計算目前等級對應的星星數（Level1=1顆，Level2=2顆，...，最多5顆）
	int GetStarCountForCurrentLevel()
	{
		int index = GetLevelIndex(level); // Elementary=0, JuniorHigh=1, ...
		int stars = Mathf.Clamp(index + 1, 1, maxStars);
		return stars;
	}

	// 取得等級在 levelOrder 的索引，找不到時回傳 0（預設 Elementary）
	int GetLevelIndex(string lvl)
	{
		for (int i = 0; i < levelOrder.Length; i++)
		{
			if (levelOrder[i] == lvl) return i;
		}
		return 0;
	}

	// 依目前等級動態更新星星顯示（清空後再生成）
	void UpdateStarsUI()
	{
		if (starsContainer == null || starPrefab == null) return;

		// 移除舊的星星
		for (int i = starsContainer.childCount - 1; i >= 0; i--)
		{
			var child = starsContainer.GetChild(i);
			Destroy(child.gameObject);
		}

		int starCount = GetStarCountForCurrentLevel();

		// 確保有水平排版（維持等距間隔），若沒有則自動加上
		var hlg = starsContainer.GetComponent<HorizontalLayoutGroup>();
		if (hlg == null)
		{
			hlg = starsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
			hlg.childAlignment = TextAnchor.UpperRight;
			hlg.childControlHeight = true;
			hlg.childControlWidth = true;
			hlg.childForceExpandHeight = false;
			hlg.childForceExpandWidth = false;
			hlg.spacing = 8f; // 星星之間的水平間距
			hlg.reverseArrangement = true; // 讓新增的星星從右往左排列（右上角）
		}

		// 生成星星
		for (int i = 0; i < starCount; i++)
		{
			var star = Instantiate(starPrefab, starsContainer);
			var rt = star.GetComponent<RectTransform>();
			if (rt != null)
			{
				rt.anchorMin = new Vector2(1f, 1f);
				rt.anchorMax = new Vector2(1f, 1f);
				rt.pivot = new Vector2(1f, 1f);
				rt.anchoredPosition = Vector2.zero;
				rt.localScale = Vector3.one;
			}
		}
	}
}



