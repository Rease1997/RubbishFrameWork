#region 模块信息
// ****************************************************************
// Copyright (C) 2017 The company name
//
// 文件名(File Name):             HotFixPanel.cs
// 作者(Author):                  #AuthorName#
// 创建时间(CreateTime):           #CreateTime#
// 修改者列表(modifier):
// 模块描述(Module description):
// ****************************************************************
#endregion

using UnityEngine;
using UnityEngine.UI;

public class HotFixPanel : MonoBehaviour
{
    private Slider m_Image;
    public Slider Image
    {
        get
        {
            if (m_Image == null)
                m_Image = transform.FindBehaviour<Slider>("Background");
            return m_Image;
        }
    }

    private Text m_Tex;
    public Text SpeedText
    {
        get
        {
            if (m_Tex == null)
                m_Tex = transform.FindBehaviour<Text>("SpeedText");
            return m_Tex;
        }
    }

    private Text m_SliderTopTex;
    public Text SliderTopTex
    {
        get
        {
            if (m_SliderTopTex == null)
                m_SliderTopTex = transform.FindBehaviour<Text>("SliderTopText");
            return m_SliderTopTex;
        }
    }

    private Text m_HotContentTex;
    public Text HotContentTex
    {
        get
        {
            if (m_HotContentTex == null)
                m_HotContentTex = transform.FindBehaviour<Text>("Text");
            return m_HotContentTex;
        }
    }

}
