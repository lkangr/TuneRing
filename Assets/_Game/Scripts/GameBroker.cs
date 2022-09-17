using UnityEngine;

public class GameBroker : Singleton<GameBroker>
{
    private GameManager _gameManager;
    public GameManager GameManager { get { if (!_gameManager) _gameManager = FindObjectOfType<GameManager>(); return _gameManager; } }

    private ViewManager _viewManager;
    public ViewManager ViewManager { get { if (!_viewManager) _viewManager = FindObjectOfType<ViewManager>(); return _viewManager; } }

    private InGameCursor _igCursor;
    public InGameCursor IgCursor { get { if (!_igCursor) _igCursor = FindObjectOfType<InGameCursor>(); return _igCursor; } }

    #region view
    public static void ShowView(string view)
    {
        Ins.ViewManager.ShowView(view);
    }
    #endregion

    #region game
    public static void PlayGame(bool autoMode)
    {
        Ins.GameManager.Play(autoMode);
        if (autoMode) Ins.IgCursor.EnterAutoMode();
    }

    public static void EndGame()
    {
        Ins.GameManager.EndGame();
        Ins.IgCursor.ExitAutoMode();
    }

    public static void SetBG(Sprite image = null)
    {
        Ins.GameManager.bg.SetBG(image);
    }

    public static void CursorTo(int idx)
    {
        if (idx >= Ins.GameManager.listCircle.Count) return;

        if (idx == 0)
        {
            Ins.IgCursor.To(Ins.GameManager.hitCircleContainer.GetChild(0).position, 1f);
        }
        else
        {
            var hitCircle = Ins.GameManager.hitCircleContainer.Find("HitCircle(Clone)");

            if (hitCircle)
            {
                var dur = Ins.GameManager.listCircle[idx].time - Ins.GameManager.audioSource.time;
                Ins.IgCursor.To(hitCircle.position, dur);
            }
            else
            {
                Ins.IgCursor.waitToNextCircle = true;
            }
        }
    }
    #endregion
}
