public class InGameView : BaseView
{
    public static void Show() { GameBroker.ShowView("InGameView"); }

    protected override void OnShow()
    {
        base.OnShow();

        GameBroker.SetBG(GameBroker.Ins.GameManager.imageTemp);
    }
}
