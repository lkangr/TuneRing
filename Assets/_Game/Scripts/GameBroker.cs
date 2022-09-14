using UnityEngine;

public class GameBroker : Singleton<GameBroker>
{
    private GameManager _gameManager;
    public GameManager GameManager { get { if (!_gameManager) _gameManager = FindObjectOfType<GameManager>(); return _gameManager; } }

    private ViewManager _viewManager;
    public ViewManager ViewManager { get { if (!_viewManager) _viewManager = FindObjectOfType<ViewManager>(); return _viewManager; } }

    #region view
    public static void ShowView(string view)
    {
        Ins.ViewManager.ShowView(view);
    }
    #endregion

    #region game
    public static void PlayGame()
    {
        Ins.GameManager.Play();
    }

    public static void SetBG(Sprite image = null)
    {
        Ins.GameManager.bg.SetBG(image);
    }
    #endregion
}
