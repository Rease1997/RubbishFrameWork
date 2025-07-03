using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HotFix
{
    class TipsWindow:Window
    {
        Text titleText;
        Text sureText;
        Text cancelText;
        Text desText;
        Button sureBtn;
        Button cancelBtn;
        UnityAction sureAction;
        UnityAction cancelAction;
        public override void Awake(object param1 = null, object param2 = null, object param3 = null)
        {
            FindAllComponent();
            AddAllBtnsListener();
        }

        private void AddAllBtnsListener()
        {
            AddButtonClickListener(sureBtn, ()=> {
                if(sureAction != null)
                    sureAction.Invoke();
                UIManager.Instance.CloseWnd(this);
            });
            AddButtonClickListener(cancelBtn, ()=> {
                if (cancelAction != null)
                    cancelAction.Invoke();
                UIManager.Instance.CloseWnd(this);
            });
        }

        private void FindAllComponent()
        {
            titleText = m_Transform.Find("BG/Title").GetComponent<Text>();
            sureText = m_Transform.Find("BG/Btns/SureBtn/Text").GetComponent<Text>();
            cancelText = m_Transform.Find("BG/Btns/CancelBtn/Text").GetComponent<Text>();
            desText = m_Transform.Find("BG/Des").GetComponent<Text>();
            sureBtn = m_Transform.Find("BG/Btns/SureBtn").GetComponent<Button>();
            cancelBtn = m_Transform.Find("BG/Btns/CancelBtn").GetComponent<Button>();
        }

        public override void OnShow(object param1 = null, object param2 = null, object param3 = null)
        {
            string[] tipsData = param1 as string[];
            titleText.text = tipsData[0];
            desText.text = tipsData[1];
            sureBtn.gameObject.SetActive(!string.IsNullOrEmpty(tipsData[2]));
            cancelBtn.gameObject.SetActive(!string.IsNullOrEmpty(tipsData[3]));
            sureText.text = tipsData[2];
            cancelText.text = tipsData[3];
        }

        internal void SetAction(UnityAction sureAct = null, UnityAction cancelAct = null)
        {
            sureAction = sureAct;
            cancelAction = cancelAct;
        }
    }
}
