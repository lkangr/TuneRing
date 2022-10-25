using DG.Tweening;
using System;
using UnityEngine;

public class HitCircle : MonoBehaviour
{
    public GameObject sliderFollowCircle;

    public Sprite missSprite;

    private float lifeTime = 1f;
    [NonSerialized] public bool inLifeTime = false;

    private bool autoMode;
    private  int layer;

    public void SetPositionAndLayer(float posX, float posY, int layer, bool autoMode)
    {
        this.autoMode = autoMode;
        this.layer = layer;

        GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);

        GetComponent<SpriteRenderer>().sortingOrder = layer;
        sliderFollowCircle.GetComponent<SpriteRenderer>().sortingOrder = layer;
        lifeTime = 1f;
        inLifeTime = true;

        sliderFollowCircle.transform.DOScale(new Vector3(0.48f, 0.48f, 1), 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(sliderFollowCircle.gameObject));
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

    public CircleDetail(float x, float y, float t)
    {
        posX = x;
        posY = y;
        time = t;
    }
}