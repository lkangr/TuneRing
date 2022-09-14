using UnityEngine;
using UnityEngine.UI;

public class UIPlayButton : MonoBehaviour
{
    public Button button;

    private void Reset()
    {
        if (!button) button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnPlayButtonClick);
    }

    private void OnPlayButtonClick()
    {
        GameBroker.PlayGame();
    }
}
