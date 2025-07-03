using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix
{
    class MonthCardWindow : Window
    {
        Button monthCardBtn;
        Button seasonCardBtn;
        public override void Awake(object param1 = null, object param2 = null, object param3 = null)
        {
            FindAllComponent();
            AddAllBtnsListener();
        }

        private void AddAllBtnsListener()
        {
            AddButtonClickListener(monthCardBtn, BuyMonthCard);
            AddButtonClickListener(seasonCardBtn, BuySeasonCard);
        }

        /// <summary>
        /// 购买季卡的方法
        /// </summary>
        private void BuySeasonCard()
        {
            string[] tipsData = new string[] { "购买", "是否花费250金币购买季卡\n(购买即得50000点券)", "确认", "取消" };
            var tips = UIManager.Instance.PopUpWnd(FilesName.TIPSPANEL, true, false, tipsData) as TipsWindow;
            tips.SetAction(BuySeasonCardSure);
        }

        /// <summary>
        /// 购买月卡的方法
        /// </summary>
        private void BuyMonthCard()
        {
            string[] tipsData = new string[] { "购买", "是否花费100金币购买月卡\n(购买即得10000点券)", "确认", "取消" };

            var tips = UIManager.Instance.PopUpWnd(FilesName.TIPSPANEL, true, false, tipsData) as TipsWindow;
            tips.SetAction(BuyMonthCardSure);
        }

        void BuyMonthCardSure()
        {
            //购买月卡的逻辑
            Debug.Log("购买月卡");
            UserInfoManager.money -= 100;
            UserInfoManager.coupon += 10000;
            var mainWindow = UIManager.Instance.GetWndByName(FilesName.MAINPANEL) as MainWindow;
            mainWindow.InitData();
            monthCardBtn.interactable = false;
        }


        void BuySeasonCardSure()
        {
            //购买季卡的逻辑
            Debug.Log("购买季卡");
            UserInfoManager.money -= 250;
            UserInfoManager.coupon += 50000;
            var mainWindow = UIManager.Instance.GetWndByName(FilesName.MAINPANEL) as MainWindow;
            mainWindow.InitData();
            seasonCardBtn.interactable = false;
        }

        private void FindAllComponent()
        {
            monthCardBtn = m_Transform.Find("MonthCardBtn").GetComponent<Button>();
            seasonCardBtn = m_Transform.Find("SeasonCardBtn").GetComponent<Button>();
        }

        public override void OnShow(object param1 = null, object param2 = null, object param3 = null)
        {

        }
    }
}
