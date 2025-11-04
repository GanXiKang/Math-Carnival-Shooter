using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class QuizUIManager : MonoBehaviour
{
	// Preferred TMP references
#if TMP_PRESENT
	[SerializeField] private TMP_Text questionTextTMP;
	[SerializeField] private TMP_Text resultTextTMP;
#endif
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
			var label = GetButtonLabel(btn);
			if (label != null)
			{
				label.text = currentQuestion.options != null && currentQuestion.options.Length > index
					? currentQuestion.options[index].ToString()
					: "?";
			}

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
	static void SetText(
#if TMP_PRESENT
		TMP_Text tmp,
#else
		object tmp,
#endif
		Text ui, string value)
	{
#if TMP_PRESENT
		if (tmp != null) { tmp.text = value; return; }
#endif
		if (ui != null) { ui.text = value; }
	}

	// Tries to find a text label on the button for displaying option text
	static Text GetButtonLabel(Button btn)
	{
		if (btn == null) return null;
		// Prefer TMP if available
#if TMP_PRESENT
		var tmp = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
		if (tmp != null)
		{
			// Mirror TMP text into a hidden UI.Text if needed is not necessary; just set TMP
			tmp.enableAutoSizing = true;
			return null; // We will handle TMP separately by setting directly where needed
		}
#endif
		return btn.GetComponentInChildren<Text>(true);
	}
}


