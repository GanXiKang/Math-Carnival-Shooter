using UnityEngine;

[CreateAssetMenu(fileName = "Math", menuName = "Scriptable Objects/Math")]
public class Math : ScriptableObject
{
    public string questionText;
    public int correctAnswer;
    public int[] options;
    public string level;
}
