using UnityEngine;
using UnityEngine.UI;

public class BackGround : MonoBehaviour
{
    public Image bg;

    [Header("-------------------")]
    public Sprite defaultSprite;

    private RectTransform canvas;

    private void Reset()
    {
        if (!bg) bg = GetComponent<Image>();
    }

    private void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("MasterCanvas").GetComponent<RectTransform>();
    }

    public void SetBG(Sprite image = null)
    {
        bg.sprite = image ? image : defaultSprite;

        var imageSize = bg.sprite.rect.size;
        var scale = Mathf.Max(canvas.sizeDelta.x / imageSize.x, canvas.sizeDelta.y / imageSize.y);
        bg.GetComponent<RectTransform>().sizeDelta = imageSize * scale;
    }
}
