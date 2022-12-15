using TMPro;
using UnityEngine;

public class InGameView : BaseView
{
    public static void Show() { GameBroker.ShowView("InGameView"); }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;

    private void Start()
    {
        GameBroker.Ins.GameManager.onUpdateScore.AddListener(OnScoreUpdate);   
    }

    protected override void OnShow()
    {
        base.OnShow();

        GameBroker.SetBG(GameBroker.Ins.GameManager.imageTemp);
    }

    private void OnScoreUpdate(int score, int combo)
    {
        scoreText.text = score.ToString();
        comboText.text = combo.ToString();
        comboText.gameObject.SetActive(combo != 0);
    }
}