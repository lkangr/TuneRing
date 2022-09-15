using DG.Tweening;
using System;
using UnityEngine;

public class HitCircle : MonoBehaviour
{
    public GameObject sliderFollowCircle;

    private float lifeTime = 1f;
    [NonSerialized] public bool inLifeTime = false;

    public void SetPositionAndLayer(float posX, float posY, int layer)
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(posX + 64, posY - 48);

        GetComponent<SpriteRenderer>().sortingOrder = layer;
        sliderFollowCircle.GetComponent<SpriteRenderer>().sortingOrder = layer;
        lifeTime = 1f;
        inLifeTime = true;

        sliderFollowCircle.transform.DOScale(new Vector3(0.48f, 0.48f, 1), 1f).OnComplete(() => Destroy(sliderFollowCircle.gameObject));
    }

    private void FixedUpdate()
    {
        if (inLifeTime)
        {
            lifeTime -= Time.fixedDeltaTime;
            if (lifeTime <= 0)
            {
                inLifeTime = false;
                Sequence s = DOTween.Sequence();
                s.Append(transform.DOMoveY(transform.position.y + 0.5f, 0.5f).OnComplete(() => Destroy(gameObject)));
                s.Join(GetComponent<SpriteRenderer>().DOFade(0, 0.5f));
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