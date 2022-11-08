using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HitCircle : MonoBehaviour
{
    public GameObject sliderFollowCircle;
    public SpriteRenderer numberInCircle;

    public Sprite missSprite;

    public List<Sprite> listNumDis;

    private float lifeTime = 1f;
    [NonSerialized] public bool inLifeTime = false;

    private bool autoMode;
    private  int layer;

    private Color[] colorBar = { new Color(1f, 0.6f, 0.9f), new Color(0.6f, 0.7f, 1f), new Color(0.6f, 1f, 0.7f), new Color(1f, 0.9f, 0.6f) };

    #region set param
    private float appearDuration = 0.8f;
    #endregion

    public void SetPositionAndLayer(float posX, float posY, int color, int numDis, int layer, bool autoMode)
    {
        var sr = GetComponent<SpriteRenderer>();

        this.autoMode = autoMode;
        this.layer = layer;

        numberInCircle.sprite = listNumDis[numDis - 1];

        sr.color = colorBar[color];

        GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, -posY);

        sr.sortingOrder = layer;
        sliderFollowCircle.GetComponent<SpriteRenderer>().sortingOrder = layer;
        numberInCircle.sortingOrder = layer;
        GetComponent<SpriteMask>().frontSortingOrder = layer - 1;

        lifeTime = 1f;
        inLifeTime = true;

        sr.DOFade(1f, appearDuration);
        sliderFollowCircle.GetComponent<SpriteRenderer>().DOFade(1f, appearDuration);
        numberInCircle.DOFade(1f, appearDuration);

        sliderFollowCircle.transform.DOScale(new Vector3(0.48f, 0.48f, 1), 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(sliderFollowCircle));
    }

    private void FixedUpdate()
    {
        if (inLifeTime)
        {
            lifeTime -= Time.fixedDeltaTime;

            if (autoMode && lifeTime <= 0)
            {
                Hit();
                gameObject.name = "...";
                GameBroker.CursorTo(-layer + 1);
            }
            else if (lifeTime < -0.3f)
            {
                inLifeTime = false; 
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                GetComponent<SpriteRenderer>().sprite = missSprite;
                DOVirtual.DelayedCall(0.5f, () => Destroy(gameObject));

                //Sequence s = DOTween.Sequence();
                //s.Append(transform.DOMoveY(transform.position.y + 0.5f, 0.5f).OnComplete(() => Destroy(gameObject)));
                //s.Join(GetComponent<SpriteRenderer>().DOFade(0, 0.5f));
            }
        }
    }

    public void Hit()
    {
        if (inLifeTime)
        {
            if (lifeTime >= 0.3f)
            {
                transform.DOShakePosition(0.1f, 10);
            }
            else if (lifeTime < 0.3f)
            {
                inLifeTime = false;
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                GetComponent<AudioSource>().Play();

                numberInCircle.gameObject.SetActive(false);
                GetComponent<SpriteMask>().enabled = false;

                Sequence ss = DOTween.Sequence();
                ss.Append(transform.DOScale(1.2f, 0.1f)); //.OnComplete(() => Destroy(gameObject));
                ss.Join(GetComponent<SpriteRenderer>().DOFade(0f, 0.1f));
                ss.AppendCallback(() => Destroy(gameObject));
            }
        }
    }
}

public class CircleDetail
{
    public float posX;
    public float posY;
    public float time;
    public int color = 0;
    public int numDis = 1;

    public CircleDetail(float x, float y, float t)
    {
        posX = x;
        posY = y;
        time = t;
    }
}