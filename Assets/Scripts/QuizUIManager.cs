using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUIManager : MonoBehaviour
{
	// 優先使用 TMP 文字元件
	[SerializeField] private TMP_Text questionTextTMP;
	[SerializeField] private TMP_Text resultTextTMP;
	// 備用：使用內建 UI Text（若未使用或無法使用 TMP）
	[SerializeField] private Text questionTextUI;
	[SerializeField] private Text resultTextUI;

	// 答案按鈕（共 4 個）
	[SerializeField] private Button[] optionButtons;

	// 難度等級字串（傳入 QuestionGenerator，例如 "Elementary"、"JuniorHigh"...）
	[SerializeField] private string level = "Elementary";

	// 作答後切換到下一題的延遲時間
	[SerializeField] private float nextQuestionDelaySeconds = 1.5f;

	// 升級所需的連續答對數（目前：小學 → 國中）
	[SerializeField] private int correctToLevelUp = 10;

	private MathQuestion currentQuestion;
	private bool isLocked;
	private int correctInCurrentLevel;

	void Awake()
	{
		// 選用：在行動裝置上偏好直向
		Screen.orientation = ScreenOrientation.Portrait;
	}

	void Start()
	{
		GenerateAndDisplayQuestion();
	}

	// 產生新題目並更新介面
	void GenerateAndDisplayQuestion()
	{
		isLocked = false;
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
	}

	void OnOptionSelected(int optionIndex)
	{
		if (isLocked) return;
		isLocked = true;

		int chosen = SafeGetOption(optionIndex);
		bool correct = chosen == currentQuestion.correctAnswer;
		SetText(resultTextTMP, resultTextUI, correct ? "✅ 答對了！" : "❌ 答錯了！");

		bool didLevelUp = false;
		if (correct)
		{
			correctInCurrentLevel++;
			if (level == "Elementary" && correctInCurrentLevel >= correctToLevelUp)
			{
				// 升級到國中並重置計數
				level = "JuniorHigh";
				correctInCurrentLevel = 0;
				didLevelUp = true;
			}
		}

		// 題目切換前先停用按鈕
		SetButtonsInteractable(false);
		if (didLevelUp)
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
		SetText(resultTextTMP, resultTextUI, "⬆️ 升級！歡迎來到國中！");
		yield return new WaitForSeconds(2f);
		GenerateAndDisplayQuestion();
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

	// 工具方法：優先設定 TMP，否則設定 UI.Text
	static void SetText(TMP_Text tmp, Text ui, string value)
	{
		if (tmp != null) { tmp.text = value; return; }
		if (ui != null) { ui.text = value; }
	}
}



