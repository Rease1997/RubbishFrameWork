using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonConfirm : BaseItem
{
    private Text m_Title;
    public Text Title
    {
        get
        {
            if (m_Title == null)
                m_Title = transform.FindBehaviour<Text>("Title");
            return m_Title;
        }
    }
    private Text m_Des;
    public Text Des
    {
        get
        {
            if (m_Des == null)
                m_Des = transform.FindBehaviour<Text>("Des");
            return m_Des;
        }
    }

    private Button m_ConfirmBtn;
    public Button ConfirmBtn
    {
        get
        {
            if (m_ConfirmBtn == null)
                m_ConfirmBtn = transform.FindBehaviour<Button>("ConfirmButton");
            return m_ConfirmBtn;
        }
    }
    private Button m_CancleBtn;
    public Button CancleBtn
    {
        get
        {
            if (m_CancleBtn == null)
                m_CancleBtn = transform.FindBehaviour<Button>("CancleButton");
            return m_CancleBtn;
        }
    }
    public void Show(string title,string str,UnityEngine.Events.UnityAction confirmAction ,UnityEngine.Events.UnityAction cancleAction)
    {
        Title.text = title;
        Des.text = str;
        AddButtonClickListener(ConfirmBtn, () =>
        {
            confirmAction();
            Destroy(gameObject);
        });
        AddButtonClickListener(CancleBtn, () =>
        {
            cancleAction();
            Destroy(gameObject);
        });
    }
}
