using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    /// <summary>
    /// 加载界面
    /// </summary>
    class LoadingWindow : Window
    {
        /// <summary>
        /// 是否执行过弹出界面的方法
        /// </summary>
        bool isOpen = false;
        /// <summary>
        /// 加载界面挂载脚本 用来获取组件
        /// </summary>
        private LoadingPanel m_MainPanel;
        /// <summary>
        /// 加载界面关闭后要显示的界面
        /// </summary>
        private string m_TargetPanelName;

        private Action<Action> action = null;

        private object[] objs = null;

        public override void Awake(object param1 = null, object param2 = null, object param3 = null)
        {
            m_MainPanel = m_GameObject.GetComponent<LoadingPanel>();
            GameMapManager.Instance.LoadSceneOverCallBack = LoadOtherScene;
            m_TargetPanelName = (string)param1;
            action = (Action<Action>)param2;
            isOpen = false;
            objs = (object[])param3;
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
            Debug.Log("关闭loading界面");
            UIManager.Instance.CloseWnd(this);
            string tarPanelName = m_TargetPanelName;
            object[] data = new object[3];
            if (objs != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (objs[i] != null)
                    {
                        data[i] = objs[i];
                    }
                }
            }
            if (tarPanelName == FilesName.MAINPANEL)
            {
                UIManager.Instance.CloseAllWnd();
                UIManager.Instance.PopUpWnd(tarPanelName, false, false, data[0], data[1], data[2]);
            }
            else
                UIManager.Instance.PopUpWnd(tarPanelName, true, false, data[0], data[1], data[2]);
        }
    }
}
