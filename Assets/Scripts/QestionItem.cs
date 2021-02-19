using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QestionItem : MonoBehaviour
{
    public Text title;
    public Image image;

    public void SetUp(string _title)
    {
        if(_title==" ")
        {
            image.enabled = false;
        }
        title.gameObject.SetActive(false);
        title.text = _title;
    }

    public void ShowText()
    {
        transform.DOScale(transform.localScale * 1.3f, 0.2f).OnComplete(()=> { transform.DOScale(transform.localScale / 1.3f, 0.2f); });
        title.gameObject.SetActive(true);
    }

}
