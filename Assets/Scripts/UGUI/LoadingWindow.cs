using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWindow : Window
{

    private LoadingPanel m_MainPanel;
    private string m_TargetPanelName;

    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        m_MainPanel = m_GameObject.GetComponent<LoadingPanel>();
        m_TargetPanelName = (string)param1;
    }

    public override void OnUpdate()
    {
        if (m_MainPanel == null)
            return;

        m_MainPanel.m_Slider.value = GameMapManager.LoadingProgress / 100.0f;
        m_MainPanel.m_Text.text = string.Format("{0}%", GameMapManager.LoadingProgress);
        if (GameMapManager.LoadingProgress >= 100)
        {
            LoadOtherScene();
        }
    }

    /// <summary>
    /// 加载对应场景第一个UI
    /// </summary>
    public void LoadOtherScene()
    {
        //根据场景名字打开对应场景第一个界面
        string tarPanelName = m_TargetPanelName;
        Debug.LogError(tarPanelName);
        UIManager.Instance.PopUpWnd(tarPanelName, resource: false);
        if (m_TargetPanelName == "MainPanel")
        {
            //UIManager.Instance.PopUpWnd(ConStr.BATTLE, false);
        }
        UIManager.Instance.CloseWnd(ConStr.LOADINGPANEL);
    }
}
