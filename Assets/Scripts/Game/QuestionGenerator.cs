using System;
using System.Collections.Generic;
using UnityEngine;

public class MathQuestion
{
	public string questionText;
	public int correctAnswer;
	public int[] options;
	public string level;
}

public static class QuestionGenerator
{
	public static MathQuestion GenerateQuestion(string level)
	{
		level = level ?? "";
		switch (level)
		{
			case "Elementary":
				return GenerateElementary();
			case "JuniorHigh":
				return GenerateJuniorHigh();
			case "HighSchool":
				return GenerateHighSchool();
			case "University":
				return GenerateUniversity();
			case "PhD":
				return GeneratePhD();
			default:
				return GenerateElementary();
		}
	}

	static MathQuestion GenerateElementary()
	{
		int a = UnityEngine.Random.Range(1, 16);
		int b = UnityEngine.Random.Range(1, 16);
		int op = UnityEngine.Random.Range(0, 4); // 0:+, 1:-, 2:*, 3:/
		string symbol;
		int answer;
		switch (op)
		{
			case 0:
				symbol = "+";
				answer = a + b;
				break;
			case 1:
				symbol = "-";
				answer = a - b;
				break;
			case 2:
				symbol = "×";
				answer = a * b;
				break;
			default:
				symbol = "÷";
				// Ensure divisible
				b = UnityEngine.Random.Range(1, 13);
				answer = UnityEngine.Random.Range(1, 16);
				a = answer * b;
				break;
		}
		string q = $"{a} {symbol} {b} = ？";
		return BuildQuestion(q, answer, "Elementary");
	}

	static MathQuestion GenerateJuniorHigh()
	{
		// Patterns with parentheses and multiple operations
		int pattern = UnityEngine.Random.Range(0, 3);
		int a = UnityEngine.Random.Range(1, 21);
		int b = UnityEngine.Random.Range(1, 21);
		int c = UnityEngine.Random.Range(1, 11);
		int d = UnityEngine.Random.Range(1, 11);
		string q;
		int answer;
			switch (pattern)
		{
			case 0:
				// (a + b) * c - d
				q = $"({a} + {b}) × {c} - {d} = ？";
				answer = (a + b) * c - d;
				break;
			case 1:
				// a + (b * c) - d
				q = $"{a} + ({b} × {c}) - {d} = ？";
				answer = a + (b * c) - d;
				break;
			default:
				// (a - b) + (c × d)
				q = $"({a} - {b}) + ({c} × {d}) = ？";
				answer = (a - b) + (c * d);
				break;
		}
		return BuildQuestion(q, answer, "JuniorHigh");
	}

	static MathQuestion GenerateHighSchool()
	{
		int choice = UnityEngine.Random.Range(0, 3);
		string q;
		int answer;
		if (choice == 0)
		{
			// Power with small exponent
			int baseVal = UnityEngine.Random.Range(2, 8);
			int exp = UnityEngine.Random.Range(2, 4);
			answer = (int)Mathf.Pow(baseVal, exp);
			q = $"{baseVal}^{exp} = ？";
		}
		else if (choice == 1)
		{
			// Modulo
			int m = UnityEngine.Random.Range(2, 11);
			int n = UnityEngine.Random.Range(0, 101);
			answer = n % m;
			q = $"{n} % {m} = ？";
		}
        else
        {
            // Fraction division: (a/b) ÷ (c/d) -> (a*d)/(b*c)
            int b = UnityEngine.Random.Range(2, 10);
            int d = UnityEngine.Random.Range(2, 10);
            int a = UnityEngine.Random.Range(1, 10) * b; // 確保 a/b 是整數
            int c = UnityEngine.Random.Range(1, 10) * d; // 確保 c/d 是整數

            int left = a * d;
            int right = b * c;

            // 約分
            int gcd = GCD(Mathf.Abs(left), Mathf.Abs(right));
            left /= gcd;
            right /= gcd;

            // 如果要整數答案，確保 right 能整除 left
            while (left % right != 0)
            {
                a = UnityEngine.Random.Range(1, 10) * b;
                c = UnityEngine.Random.Range(1, 10) * d;
                left = a * d;
                right = b * c;
                gcd = GCD(Mathf.Abs(left), Mathf.Abs(right));
                left /= gcd;
                right /= gcd;
            }

            answer = left / right;
            q = $"({a}/{b}) ÷ ({c}/{d}) = ？";
        }
        return BuildQuestion(q, answer, "HighSchool");
	}

	static MathQuestion GenerateUniversity()
	{
		// Linear algebra-like: px + q = r
		int p = UnityEngine.Random.Range(1, 11);
		int x = UnityEngine.Random.Range(-10, 11);
		int q = UnityEngine.Random.Range(-20, 21);
		int r = p * x + q;
		string text = $"{p}x + {q} = {r}";
		int answer = x;
		return BuildQuestion(text, answer, "University");
	}

    static List<(string, int)> phdPool = new List<(string, int)>
    {
        ("f(n)=n^2-n+1\nf(5)=?", 21),
        ("5P3 = ?", 60),
        ("F(n)=F(n-1)+F(n-2)\nF(1)=1, F(2)=1F(6)=?", 8),
        ("a<b<c, a+b+c=9\ncount=?", 3),
        ("2,5,8,...,a_10=?", 29)
    };
    static void ResetPhDPool()
    {
        phdPool = new List<(string, int)>
	    {
		    ("f(n)=n^2-n+1，f(5)=?", 21),
		    ("5P3 = ?", 60),
		    ("F(1)=1,F(2)=1,F(n)=F(n-1)+F(n-2)，F(6)=?", 8),
		    ("a<b<c, a+b+c=9 → count=?", 3),
		    ("a_1=2,d=3 → a_10=?", 29)
	    };
	}
    static MathQuestion GeneratePhD()
	{
        if (phdPool.Count == 0)
            ResetPhDPool(); 

        int index = UnityEngine.Random.Range(0, phdPool.Count);
        var pick = phdPool[index];

        phdPool.RemoveAt(index);

        return BuildQuestion(pick.Item1, pick.Item2, "PhD");
    }

	static MathQuestion BuildQuestion(string text, int correct, string level)
	{
		var optionSet = GenerateOptions(correct);
		return new MathQuestion
		{
			questionText = text,
			correctAnswer = correct,
			options = optionSet,
			level = level
		};
	}

	static int[] GenerateOptions(int correct)
	{
		var set = new HashSet<int> { correct };
		int spread = Mathf.Max(3, Mathf.Abs(correct) / 5 + 3);
		while (set.Count < 4)
		{
			int delta = UnityEngine.Random.Range(-spread, spread + 1);
			if (delta == 0) delta = UnityEngine.Random.Range(1, spread + 1);
			int candidate = correct + delta;
			set.Add(candidate);
		}
		var arr = new List<int>(set);
		// Ensure exactly 4, set may exceed if duplicates prevented; trim if needed
		if (arr.Count > 4) arr.RemoveAt(1);
		// Shuffle
		for (int i = arr.Count - 1; i > 0; i--)
		{
			int j = UnityEngine.Random.Range(0, i + 1);
			int tmp = arr[i];
			arr[i] = arr[j];
			arr[j] = tmp;
		}
		return arr.ToArray();
	}

	static int GCD(int a, int b)
	{
		while (b != 0)
		{
			int t = b;
			b = a % b;
			a = t;
		}
		return Mathf.Abs(a);
	}
}