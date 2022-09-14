using UnityEngine;

public class BaseView : MonoBehaviour
{
    public void BaseShow()
    {
        gameObject.SetActive(true);

        OnShow();
    }

    public void BaseHide()
    {
        gameObject.SetActive(false);

        OnHide();
    }

    protected virtual void OnShow() { }

    protected virtual void OnHide() { }
}
