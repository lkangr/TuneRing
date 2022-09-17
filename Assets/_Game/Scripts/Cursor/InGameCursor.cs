using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCursor : MonoBehaviour
{
    public GameObject clickCirclePrefab;

    private bool autoMode = false;

    public bool waitToNextCircle = false;

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!autoMode)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 10);

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
            {
                RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
                if (rayHit)
                {
                    var circle = rayHit.collider.gameObject.GetComponent<HitCircle>();
                    if (circle) circle.Hit();
                }

                var click = Instantiate(clickCirclePrefab);

                click.transform.position = new Vector3(mousePosition.x, mousePosition.y, 5);

                click.transform.DOScale(0.01f, 0.5f).OnComplete(() => Destroy(click));
            }
        }
        else if (waitToNextCircle)
        {
            if (GameBroker.Ins.GameManager.hitCircleContainer.childCount > 0)
            {
                waitToNextCircle = false;
                To(GameBroker.Ins.GameManager.hitCircleContainer.GetChild(0).position, 1f);
            }
        }
    }

    #region auto mode
    public void EnterAutoMode()
    {
        autoMode = true;
        Cursor.visible = true;
        transform.position = new Vector3(0, 0, 10);
    }

    public void ExitAutoMode()
    {
        if (autoMode)
        {
            autoMode = false;
            Cursor.visible = false;
        }
    }

    public void To(Vector3 pos, float duration)
    {
        transform.DOMove(pos, duration);
    }
    #endregion
}
