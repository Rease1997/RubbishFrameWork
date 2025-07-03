using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class HotFixUI : Window
{
    private HotFixPanel m_Panel;
    private float m_SumTime = 0;
    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        m_SumTime = 0;
        m_Panel = m_GameObject.GetComponent<HotFixPanel>();
        m_Panel.Image.value = 0;
        m_Panel.SpeedText.text = string.Format("{0:F}M/S", 0);
        HotPatchManager.Instance.SeverInfoError += ServerInfoError;
        HotPatchManager.Instance.ItemError += ItemError;
    }

    public override void OnShow(object param1 = null, object param2 = null, object param3 = null)
    {
        //#if UNITY_EDITOR
        //        StartOnFinish();
        //#else
        //if (HotPatchManager.Instance.ComputeUnPackFile())
        //{
        //    m_Panel.SliderTopTex.text = "解压中...";
        //    HotPatchManager.Instance.StartUnPackFile(() =>
        //    {
        //        m_SumTime = 0;
        //        HotFix();
        //    });
        //}
        //else
        //{
        //    HotFix();
        //}
        //#endif
        StartOnFinish();
    }

    /// <summary>
    /// 检查热更
    /// </summary>
    private void HotFix()
    {
        //当前无网络连接
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //提示网络错误，检测网格连接是否正常
            RFrameWork.Instance.OpenCommonConfirm("网络连接失败", "网络连接失败，请检查网络连接是否正常？", () => { Tools.ExitGame(); }, null);
        }
        else
        {
            CheckVersion();
        }
    }
    /// <summary>
    /// 检查版本
    /// </summary>
    private void CheckVersion()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                RFrameWork.Instance.OpenCommonConfirm("热更确定", string.Format("发现新版本，有{1:F}M大小热更包，是否确定下载？",
                    HotPatchManager.Instance.CurVersion, HotPatchManager.Instance.LoadSumSize / 1024.0f),
                    OnClickStartDownLoad, OnClickCanelDownLoad);
            }
            else
            {
                StartOnFinish();
            }
        });
    }
    /// <summary>
    /// 下载完成回调，或者没有下载的东西直接进入游戏
    /// </summary>
    private void StartOnFinish()
    {
        RFrameWork.Instance.StartCoroutine(OnFinish());
    }

    IEnumerator OnFinish()
    {
        yield return RFrameWork.Instance.StartCoroutine(RFrameWork.Instance.StartGame(m_Panel.Image.image, m_Panel.SliderTopTex));
        UIManager.Instance.CloseWnd(this);
    }
    public override void OnUpdate()
    {
        if (HotPatchManager.Instance.StartUnPack)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.Image.value = HotPatchManager.Instance.GetUnpackProgress();
            float speed = (HotPatchManager.Instance.AlreadyUnPackSize / 1024.0f) / m_SumTime;
            m_Panel.SpeedText.text = string.Format("{0:F}M/S", speed);
        }

        if (HotPatchManager.Instance.StartDownload)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.Image.value = HotPatchManager.Instance.GetProgress();
            float speed = (HotPatchManager.Instance.GetLoadSize() / 1024.0f) / m_SumTime;
            m_Panel.SpeedText.text = string.Format("{0:F}M/S", speed);
        }
    }

    private void OnClickCanelDownLoad()
    {
        RFrameWork.Instance.QuitTheGame();
    }

    private void OnClickStartDownLoad()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            Debug.LogError("当前版本为移动端");
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                RFrameWork.Instance.OpenCommonConfirm("下载确认", "当前是用的是手机流量，是否继续下载？", StartDownLoad, OnClickCanelDownLoad);
            }
            else
            {
                StartDownLoad();
            }
        }
        else
        {
            StartDownLoad();
        }
    }
    /// <summary>
    /// 正式开始下载
    /// </summary>
    private void StartDownLoad()
    {
        m_Panel.SliderTopTex.text = "下载中...";
        m_Panel.HotContentTex.text = HotPatchManager.Instance.CurrentPatches.Des;
        RFrameWork.Instance.StartCoroutine(HotPatchManager.Instance.StartDownAB(StartOnFinish));
    }

    private void ItemError(string obj)
    {
        RFrameWork.Instance.OpenCommonConfirm("资源下载失败", "{0}等资源失败，请重新尝试下载！", AnewDownload, Tools.ExitGame);
    }

    private void AnewDownload()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                StartDownLoad();
            }
            else
            {
                StartOnFinish();
            }
        });
    }

    private void ServerInfoError()
    {
        RFrameWork.Instance.OpenCommonConfirm("服务器列表获取失败", "服务器列表获取失败，请检查网络连接是否正常？尝试重新下载！", CheckVersion, Tools.ExitGame);
    }
    public override void OnClose()
    {
        HotPatchManager.Instance.SeverInfoError -= ServerInfoError;
        HotPatchManager.Instance.ItemError -= ItemError;
        GetData();
    }

    private async void GetData()
    {
        GameMapManager.Instance.LoadScene(ConStr.MAINSCENE, ConStr.MAINPANEL);
    }
}
