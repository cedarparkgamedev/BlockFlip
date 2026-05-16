using System.Collections.Generic;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{

    public static ScoreManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI comboText;

    private int score;
    private int combo;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        UpdateUI();
    }

    public void OnRowsCleared(int rowCount)
    {
        if (rowCount <= 0)
        {
            combo = 0;
            UpdateUI();
            return;
        }

        combo++;

        int baseScore = 100 * rowCount;
        int multiLineBonus = rowCount > 1 ? rowCount * 100 : 0;
        int comboBonus = combo * 50;

        Debug.Log("score : "+score+" baseScore "+baseScore+" multiLineBonus : "+multiLineBonus+" comboBonus : "+comboBonus);

        score += baseScore + multiLineBonus + comboBonus;

        UpdateUI();
    }

    private void UpdateUI()
    {
        scoreText.text = score.ToString("N0");
        comboText.text = $"x{combo}";
    }
}