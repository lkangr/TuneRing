using UnityEngine;
using UnityEngine.UI;

public class UIProcessSongButton : MonoBehaviour
{
    public Button button;

    private void Reset()
    {
        if (!button) button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        GameBroker.ProcessSong();
    }
}
