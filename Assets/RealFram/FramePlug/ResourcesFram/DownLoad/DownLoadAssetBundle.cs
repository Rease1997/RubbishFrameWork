using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownLoadAssetBundle : DownLoadItem
{
    UnityWebRequest m_WebRequest;
    public DownLoadAssetBundle(string url,string path):base(url, path) { }
    /// <summary>
    /// 开始下载热更文件
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public override IEnumerator DownLoad(Action callback = null)
    {
        m_WebRequest = UnityWebRequest.Get(Url);
        StartDownLoad = true;
        m_WebRequest.timeout = 30;
        yield return m_WebRequest.SendWebRequest();
        StartDownLoad = false;
        if (m_WebRequest.isNetworkError)
        {
            Debug.LogError("Download Error" + m_WebRequest.error);
        }
        else
        {
            byte[] bytes = m_WebRequest.downloadHandler.data;
            FileTools.CreatFile(SaveFilePath, bytes);
            callback?.Invoke();
        }
    }
    public override void Destory()
    {
        if (m_WebRequest != null)
        {
            m_WebRequest.Dispose();
            m_WebRequest = null;
        }
    }

    public override long GetCurLength()
    {
        if (m_WebRequest != null)
        {
            return (long)m_WebRequest.downloadedBytes;
        }
        return 0;
    }

    public override long GetLength()
    {
        return 0;
    }

    public override float GetProcess()
    {
        if (m_WebRequest != null)
            return (long)m_WebRequest.downloadProgress;
        return 0;
    }
}
