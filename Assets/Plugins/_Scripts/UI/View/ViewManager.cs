using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewManager : MonoBehaviour
{
    public Image mask;

    public List<BaseView> viewData = new List<BaseView>();

    public void ShowView(string view)
    {
        mask.DOFade(1, 1f).OnComplete(() =>
        {
            foreach (var data in viewData)
            {
                if (data.name == view)
                {
                    if(!data.gameObject.activeSelf) data.BaseShow();
                }
                else
                {
                    if (data.gameObject.activeSelf) data.BaseHide();
                }
            }
            mask.DOFade(0, 1f);
        });
    }
}
