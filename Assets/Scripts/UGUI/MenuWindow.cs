using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : Window
{
    MenuPanel m_MenuPanel;
    private string m_TargetPanelName;
    public List<string> Paths;
    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        m_MenuPanel = m_GameObject.GetComponent<MenuPanel>();
        m_TargetPanelName = (string)param1;
        m_MenuPanel.Image1.sprite = ResourceManager.Instance.LoadResources<Sprite>("Assets/GameData/UI/ID_368.bmp");
        Paths = new List<string>() { "Assets/GameData/UI/ID_368.bmp", "Assets/GameData/UI/ID_401.bmp", "Assets/GameData/UI/shadow_tentaclyorb.png" };
        AddButtonClickListener(m_MenuPanel.starBtn, OnClickStart);
    }

    private void OnClickStart()
    {
        int index = UnityEngine.Random.Range(0, Paths.Count);
        string path = Paths[index];
        ChangeImageSprite(path, m_MenuPanel.Image1);
    }
}
