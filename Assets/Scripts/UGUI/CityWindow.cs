using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityWindow : Window
{
    CityPanel cityPanel;
    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        cityPanel = m_GameObject.GetComponent<CityPanel>();
        AddButtonClickListener(cityPanel.button, ChangeScene);
    }

    private void ChangeScene()
    {
        GameMapManager.Instance.LoadScene(ConStr.EMPTYSCENE, ConStr.MENUPANEL);
    }
}
