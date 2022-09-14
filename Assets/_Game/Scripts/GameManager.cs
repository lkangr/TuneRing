using DG.Tweening;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Cursor")]
    public Transform cursor;

    [Header("Background")]
    public BackGround bg;
    public Image maskIngame;
    public Sprite imageTemp;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Map")]
    public DefaultAsset mapFile;

    [Header("Hit Circle")]
    public Transform hitCircleContainer;
    public GameObject hitCirclePrefabs;

    [Header("------------------------------------")]
    [SerializeField] private Vector2 resolution;

    private Camera MainCamera;
    private Vector2 screenScale;

    private List<CircleDetail> listCircle = new List<CircleDetail>();
    private int currCircle;

    private bool preIngame = false;
    private float timePreIngame;
    private bool inGame = false;

    void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Cursor.visible = false;

        var canvas = GameObject.FindGameObjectWithTag("MasterCanvas").GetComponent<RectTransform>();
        screenScale = new Vector2(canvas.sizeDelta.x / resolution.x, canvas.sizeDelta.y / resolution.y);

        MainView.Show();
    }

    void Update()
    {
        var mousePosition = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        cursor.position = new Vector3(mousePosition.x, mousePosition.y, 10);

        if (inGame)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                EndGame();
            }

            if (!audioSource.isPlaying)
            {
                EndGame();
            }
        }
    }

    public void Play()
    {
        InGameView.Show();

        listCircle.Clear();
        currCircle = 0;

        var map = new StreamReader(AssetDatabase.GetAssetPath(mapFile)).ReadToEnd().Split("\n");
        foreach (var line in map)
        {
            var data = line.Split(",");
            listCircle.Add(new CircleDetail(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]) / 1000));
        }

        preIngame = true;
        timePreIngame = -2f;
        DOVirtual.DelayedCall(2.5f, () => maskIngame.DOFade(0.8f, 1f).OnComplete(() =>
        {
            DOTween.To(() => timePreIngame, x => timePreIngame = x, 0f, 2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                preIngame = false;
                audioSource.Play();
                inGame = true;
            });
        }));
    }

    private void EndGame()
    {
        audioSource.Stop();

        inGame = false;

        maskIngame.color = new Color(0, 0, 0, 0);

        MainView.Show();
    }

    private void FixedUpdate()
    {
        if (listCircle.Count > currCircle)
        {
            var circle = listCircle[currCircle];
            if ((preIngame && timePreIngame >= circle.time - 1f) ||
                (inGame && audioSource.time >= circle.time - 1f))
            {
                var obj = Instantiate(hitCirclePrefabs, hitCircleContainer).GetComponent<HitCircle>();
                obj.SetPositionAndLayer(circle.posX * screenScale.x, -circle.posY * screenScale.y, -currCircle);
                currCircle++;
            }
        }
    }
}
