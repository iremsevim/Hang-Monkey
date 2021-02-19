using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CategorySelectButton : MonoBehaviour
{

    public Text title;
    public System.Action OnClicked;

    public void SetUp(string _title,System.Action _OnClicked)
    {
        title.text = _title;
        OnClicked = _OnClicked;
    }
    public void ClickMe()
    {
        OnClicked?.Invoke();
        "btnclick".PlayAudio();
    }
}
