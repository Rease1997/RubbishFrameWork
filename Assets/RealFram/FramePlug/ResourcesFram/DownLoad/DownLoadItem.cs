using System.Collections;
using System;
using System.IO;
public abstract class DownLoadItem
{
    /// <summary>
    /// 网络资源Url路径
    /// </summary>
    public string Url { get; protected set; }

    /// <summary>
    /// 资源下载路径，不包含文件
    /// </summary>
    public string SavePath { get; protected set; }
    /// <summary>
    /// 文件名，不包含后缀
    /// </summary>
    public string FileNameWithoutExt { get; protected set; }
    /// <summary>
    /// 文件的后缀
    /// </summary>
    public string FileExt { get; protected set; }
    /// <summary>
    /// 文件名包含后缀
    /// </summary>
    public string FileName { get; protected set; }
    /// <summary>
    /// 下载文件全路径，路径+文件名+后缀
    /// </summary>
    public string SaveFilePath { get; protected set; }
    /// <summary>
    /// 原文件大小
    /// </summary>
    public long FileLength { get; protected set; }
    /// <summary>
    /// 当前下载的大小
    /// </summary>
    public long CurLength { get; protected set; }
    /// <summary>
    /// 是否开始下载
    /// </summary>
    public bool StartDownLoad { get; protected set; }
    public DownLoadItem(string url, string path)
    {
        Url = url;
        SavePath = path;
        StartDownLoad = false;
        FileNameWithoutExt = Path.GetFileNameWithoutExtension(Url);
        FileExt = Path.GetExtension(Url);
        FileName = string.Format("{0}{1}", FileNameWithoutExt, FileExt);
        SaveFilePath = string.Format("{0}/{1}", SavePath, FileName);
    }

    public virtual IEnumerator DownLoad(Action callback = null)
    {
        yield return null;
    }
    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public abstract float GetProcess();
    /// <summary>
    /// 获取当前下载的文件大小
    /// </summary>
    /// <returns></returns>
    public abstract long GetCurLength();
    /// <summary>
    /// 获取下载的文件大小
    /// </summary>
    /// <returns></returns>
    public abstract long GetLength();
    public abstract void Destory();
}
