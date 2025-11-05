using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUIManager : MonoBehaviour
{
	// Preferred TMP references
	[SerializeField] private TMP_Text questionTextTMP;
	[SerializeField] private TMP_Text resultTextTMP;
	// Fallback to built-in UI Text (in case TMP isn't used/available)
	[SerializeField] private Text questionTextUI;
	[SerializeField] private Text resultTextUI;

	// Answer buttons (size 4)
	[SerializeField] private Button[] optionButtons;

	// Difficulty level key used by QuestionGenerator (e.g., "Elementary", "JuniorHigh", ...)
	[SerializeField] private string level = "Elementary";

	// Delay between answer selection and next question
	[SerializeField] private float nextQuestionDelaySeconds = 1.5f;

	private MathQuestion currentQuestion;
	private bool isLocked;

	void Awake()
	{
		// Optional: prefer portrait orientation on mobile
		Screen.orientation = ScreenOrientation.Portrait;
	}

	void Start()
	{
		GenerateAndDisplayQuestion();
	}

	// Generates a new question and updates UI
	void GenerateAndDisplayQuestion()
	{
		isLocked = false;
		currentQuestion = QuestionGenerator.GenerateQuestion(level);
		SetText(questionTextTMP, questionTextUI, currentQuestion.questionText);
		SetText(resultTextTMP, resultTextUI, "");

		if (optionButtons == null || optionButtons.Length < 4) return;

		for (int i = 0; i < optionButtons.Length; i++)
		{
			int index = i; // capture for closure
			var btn = optionButtons[i];
			if (btn == null) continue;

			btn.onClick.RemoveAllListeners();
			// Set label text on either TMP or UI.Text if present
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
		SetText(resultTextTMP, resultTextUI, correct ? "✅ Correct!" : "❌ Wrong!");

		// Disable buttons until next question
		SetButtonsInteractable(false);
		StartCoroutine(LoadNextQuestionAfterDelay(nextQuestionDelaySeconds));
	}

	IEnumerator LoadNextQuestionAfterDelay(float seconds)
	{
		yield return new WaitForSeconds(seconds);
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

	// Utility: Set text on TMP if available, otherwise on UI.Text
	static void SetText(TMP_Text tmp, Text ui, string value)
	{
		if (tmp != null) { tmp.text = value; return; }
		if (ui != null) { ui.text = value; }
	}
}



