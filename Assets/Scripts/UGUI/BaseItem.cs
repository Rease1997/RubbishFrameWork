using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseItem : MonoBehaviour
{
    /// <summary>
    /// 添加button事件监听
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="action"></param>
    public void AddButtonClickListener(Button btn,UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(BtnPlaySound);
        }
    }
    /// <summary>
    /// Toggle事件监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="action"></param>
    public void AddToggleClickListener(Toggle toggle,UnityEngine.Events.UnityAction <bool> action)
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
            toggle.onValueChanged.AddListener(TogglePlaySound);
        }
    }
    /// <summary>
    /// 播放Toggle声音
    /// </summary>
    /// <param name="arg0"></param>
    private void TogglePlaySound(bool arg0)
    {
        
    }

    /// <summary>
    /// 播放Button声音
    /// </summary>
    private void BtnPlaySound()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
