using UnityEngine;

public class MainView : BaseView
{
    public static void Show() { GameBroker.ShowView("MainView"); }

    protected override void OnShow()
    {
        base.OnShow();

        GameBroker.SetBG();
    }
}
