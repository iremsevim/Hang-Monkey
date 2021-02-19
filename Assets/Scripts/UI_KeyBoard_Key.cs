using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_KeyBoard_Key : MonoBehaviour
{
    public Text title;
    public System.Action OnClicked;
    public bool IsClicked;
    public Image image;

   

    public void SetUp(string _title,System.Action _OnClicked)
    {
        title.text = _title;
        OnClicked = _OnClicked;
    }
    public void Clicked()
    {
        if (IsClicked) return;
        if (!GameManager.instance.runtimeData.isGameStarted) return;
        "btnclick".PlayAudio();
        image.color = Color.gray;
        OnClicked?.Invoke();
        IsClicked = true;
    }

    
}
