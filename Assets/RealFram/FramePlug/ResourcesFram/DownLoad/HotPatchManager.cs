using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class HotPatchManager : Singleton<HotPatchManager>
{
    MonoBehaviour mon;
    private string xmlUrl = "http://172.20.10.6:8080/ServerInfo.xml";
    /// <summary>
    /// 下载的文件服务器XML保存路径
    /// persistentDataPath目录可以在真实手机上临时保存一些数据，在真实手机上是不能够用Application.streamingAssetsPath目录和datapath的
    /// 如果想临时保存一下数据，只能够用Application.persistentDataPath 
    /// </summary>
    private string SeverXmlPath => Application.persistentDataPath + "/SeverInfo.xml";

    /// <summary>
    /// 本地热更新文件信息配置表
    /// </summary>
    private string LocalXmlPath => Application.persistentDataPath + "/LocalInfo.xml";
   
    /// <summary>
    /// 当前版本
    /// </summary>
   public string CurVersion;
    /// <summary>
    /// 当前包名
    /// </summary>
   public string CurPackName;
    /// <summary>
    /// 服务器热更信息、本地热更信息
    /// </summary>
    private ServerInfo m_SeverInfo, m_LocalInfo;
    /// <summary>
    /// 对比服务器热更配置文件保存的热更信息
    /// </summary>
    private VersionInfo m_GameVersion;
    /// <summary>
    /// 当前热更包
    /// </summary>
    public Pathces CurrentPatches { get; private set; }
    /// <summary>
    /// 服务器下载出错回调
    /// </summary>
    public Action SeverInfoError { get; set; }
    /// <summary>
    /// 文件下载出错回调
    /// </summary>
    public Action<string> ItemError { get; set; }
    /// <summary>
    /// 所有热更信息
    /// </summary>
    public Dictionary<string, Patch> m_HotFixDic = new Dictionary<string, Patch>();

    /// <summary>
    /// 所有需要下载的资源包
    /// </summary>
    private List<Patch> m_DownLoadList = new List<Patch>();
    /// <summary>
    /// 所有需要下载资源的字典
    /// </summary>
    private Dictionary<string, Patch> m_DownLoadDic = new Dictionary<string, Patch>();
    /// <summary>
    /// 服务器上的资源名对应的MD5，用于下载后的MD5校验
    /// </summary>
    private Dictionary<string, string> m_DownLoadMD5Dic = new Dictionary<string, string>();
    /// <summary>
    /// 解压路径
    /// </summary>
    private string UnPackPath => Application.persistentDataPath + "/Origin";
    /// <summary>
    /// 下载的AB包保存路径
    /// </summary>
    private string DownLoadPath => Application.persistentDataPath + "/DownLoad";
    /// <summary>
    /// 需要下载资源的总大小 KB
    /// </summary>
    public float LoadSumSize { get; internal set; } = 0;
    /// <summary>
    /// 存储已经下载的资源
    /// </summary>
    public List<Patch> AlreadyDownList { get; private set; } = new List<Patch>();
    /// <summary>
    /// 
    /// </summary>
    public bool StartDownload { get; private set; } = false;
    /// <summary>
    /// 当前正在下载在的资源
    /// </summary>
    private DownLoadAssetBundle m_CurDownLoad = null;
    /// <summary>
	/// 需要下载资源的总个数
	/// </summary>
	public int LoadFileCount { get; set; } = 0;

    /// <summary>
    /// 尝试下载次数
    /// </summary>
    public int TryDownCount { get; private set; } = 0;

    /// <summary>
    /// 最大尝试下载次数
    /// </summary>
    public const int DOWNLOADCOUNT = 4;
    /// <summary>
    /// 下载完成回调
    /// </summary>
    public Action LoadOver;
    /// <summary>
    /// 所有需要解压的文件
    /// </summary>
    private List<string> m_UnPackedList = new List<string>();
    /// <summary>
    /// 原包记录的Md5
    /// </summary>
    private Dictionary<string, ABMD5Base> m_PackedMd5 = new Dictionary<string, ABMD5Base>();
    /// <summary>
    /// 是否开始解压
    /// </summary>
    public bool StartUnPack = false;
    /// <summary>
    /// 解压文件总大小
    /// </summary>
    public float UnPackSumSize { get; set; } = 0;
    /// <summary>
    /// 已解压大小
    /// </summary>
    public float AlreadyUnPackSize { get; set; } = 0;
    /// <summary>
    /// 初始化热更框架
    /// </summary>
    /// <param name="m"></param>
    public void Init(MonoBehaviour m)
    {
        mon = m;
        //CheckVersion();//测试
        ReadMD5();
    }
    private void ReadMD5()
    {
        m_PackedMd5.Clear();
        TextAsset md5 = Resources.Load<TextAsset>("ABMD5");
        if (md5 == null)
        {
            Debug.LogError("未读取到本地MD5");
            return;
        }
        using (MemoryStream ms = new MemoryStream(md5.bytes))
        {
            BinaryFormatter bf = new BinaryFormatter();
            ABMD5 abmd5 = bf.Deserialize(ms) as ABMD5;
            foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
            {
                m_PackedMd5.Add(abmd5Base.Name, abmd5Base);
            }
        }
    }
    /// <summary>
    /// 1.检查版本是否需要热更
    /// </summary>
    /// <param name="hotCallBack">回调函数：同志是否热更，UI如何显示，因为下载需要协程所以用回调</param>
    public void CheckVersion(Action<bool> hotCallBack = null)
    {
        //1.读取打包时的版本配置表
        ReadVersion();
        mon.StartCoroutine(ReadXml(() => DownFinish(hotCallBack)));
    }
    /// <summary>
    /// 下载更新完成
    /// </summary>
    /// <param name="hotCallBack"></param>
    private void DownFinish(Action<bool> callBack)
    {
        if (m_SeverInfo == null)
        {
            SeverInfoError?.Invoke();
            return;
        }
        foreach (VersionInfo version in m_SeverInfo.GameVersion)
        {
            if (version.Version.Equals(CurVersion))
            {
                m_GameVersion = version;
                break;
            }
        }
        GetHotAB();
        if (CheckLocalAndServerPatch())
        {
            if (File.Exists(SeverXmlPath))
            {
                if (File.Exists(LocalXmlPath))
                {
                    File.Delete(LocalXmlPath);
                }
                File.Move(SeverXmlPath, LocalXmlPath);
            }
        }
        ComputeDownLoad();
        LoadFileCount = m_DownLoadList.Count;
        LoadSumSize = m_DownLoadList.Sum(x => x.Size);
        callBack?.Invoke(LoadFileCount > 0);
    }
    /// <summary>
    /// 计算AB包路径
    /// AssetBundleManager-->LoadAssetBundleConfig调用
    /// LoadAssetBundle调用
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string ComputeABPath(string name)
    {
        m_HotFixDic.TryGetValue(name, out Patch patch);
        if (patch != null)
            return DownLoadPath + "/" + name;
        return "";
    }

    /// <summary>
    /// 外部调用，开始下载热更文件
    /// </summary>
    /// <param name="callBack"></param>
    /// <param name="allPatch"></param>
    /// <returns></returns>
    public IEnumerator StartDownAB(Action callBack,List<Patch> allPatch=null)
    {
        AlreadyDownList.Clear();
        StartDownload = true;
        if (allPatch == null)
            allPatch = m_DownLoadList;
        if (!Directory.Exists(DownLoadPath))
            Directory.CreateDirectory(DownLoadPath);
        List<DownLoadAssetBundle> downLoadAssetBundle = new List<DownLoadAssetBundle>();
        foreach (Patch patch in allPatch)
        {
            downLoadAssetBundle.Add(new DownLoadAssetBundle(patch.Url, DownLoadPath));
        }
        foreach (DownLoadAssetBundle downLoad in downLoadAssetBundle)
        {
            m_CurDownLoad = downLoad;
            yield return mon.StartCoroutine(downLoad.DownLoad());//下载一个hotab
            Patch patch = FindPatchByGamePath(downLoad.FileName);
            if (patch != null)
                AlreadyDownList.Add(patch);
            downLoad.Destory();
        }
        yield return null;
        //MD5码校验，如果校验没通过，自动下载没通过的文件，重复下载计数，达到一定次数后，反馈某某文件下载失败
        VerifyMD5(downLoadAssetBundle, callBack);
    }
    /// <summary>
    /// 根据名称查找对象的热更Patch
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private Patch FindPatchByGamePath(string name)
    {
        m_DownLoadDic.TryGetValue(name, out Patch patch);
        return patch;
    }

    /// <summary>
    /// 计算下载的资源
    /// </summary>
    private void ComputeDownLoad()
    {
        m_DownLoadList.Clear();
        m_DownLoadDic.Clear();
        m_DownLoadMD5Dic.Clear();

        if (m_GameVersion != null && m_GameVersion.pathces != null && m_GameVersion.pathces.Length > 0)
        {
            CurrentPatches = m_GameVersion.pathces[m_GameVersion.pathces.Length - 1];//拿到最后一个更新补丁包
            if (CurrentPatches.Files != null && CurrentPatches.Files.Count > 0)//判断补丁包中文件个数是否大于0
            {
                foreach (Patch patch in CurrentPatches.Files)//获取到每个需要下载的文件
                {
                    AddDownLoadList(patch);
                }
            }
        }
    }

    private void AddDownLoadList(Patch patch)
    {
        string filePath = DownLoadPath + "/" + patch.Name;
        if (File.Exists(filePath)){
            string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
            if (patch.Md5 != md5)
            {
                m_DownLoadList.Add(patch);
                m_DownLoadDic.Add(patch.Name, patch);
                m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
            }
        }
        else
        {
            m_DownLoadList.Add(patch);
            m_DownLoadDic.Add(patch.Name, patch);
            m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
        }
    }

    /// <summary>
    /// 检查本地热更信息与服务器热更信息比较
    /// </summary>
    /// <returns></returns>
    private bool CheckLocalAndServerPatch()
    {
        if (!File.Exists(LocalXmlPath))//判断是否有上次一热更信息文件
            return true;
        m_LocalInfo = BinarySerializeOpt.XmlDeserialize(LocalXmlPath, typeof(ServerInfo)) as ServerInfo;
        VersionInfo localGameVersion = null;
        if (m_LocalInfo != null)
        {
            foreach (VersionInfo version in m_LocalInfo.GameVersion)
            {
                if (version.Version.Equals(CurVersion))
                {
                    localGameVersion = version;

                    break;
                }
            }
        }
        //本地上一次热更信息不为空  && 本次热更包信息不为空 && 上一次热更包信息不为空 && 上一次热更包数组长度大于0  &&  当前热更版本的热更次数 ！=上一次热更版本的热更次数
        if (localGameVersion != null && m_GameVersion.pathces != null && localGameVersion.pathces != null && localGameVersion.pathces.Length > 0
            && m_GameVersion.pathces[m_GameVersion.pathces.Length - 1].Version != localGameVersion.pathces[localGameVersion.pathces.Length - 1].Version)
            return true;
        return false;
    }

    /// <summary>
    /// 获取所有热更包信息
    /// </summary>
    private void GetHotAB()
    {
        if (m_GameVersion != null && m_GameVersion.pathces != null && m_GameVersion.pathces.Length > 0)
        {
            Pathces lastPatches = m_GameVersion.pathces[m_GameVersion.pathces.Length - 1];
            if (lastPatches != null && lastPatches.Files != null)
            {
                foreach (Patch patch in lastPatches.Files)
                {
                    m_HotFixDic.Add(patch.Name, patch);
                }
            }
        }
    }

    /// <summary>
    /// 读取打包时的版本配置表
    /// 在Resources目录下的
    /// </summary>
    private void ReadVersion()
    {
        TextAsset versionText = Resources.Load<TextAsset>("Version");
        if (versionText == null)
        {
            Debug.LogError("未读取到版本配置文件");
            return;
        }
        string[] all = versionText.text.Split('\n');
        if (all.Length > 0)
        {
            string[] infolist = all[0].Split(';');
            if (infolist.Length >= 2)
            {
                CurVersion = infolist[0].Split('|')[1];
                CurPackName = infolist[1].Split('|')[1];
            }
        }
    }
    /// <summary>
    /// 下载服务器XML并读取
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    IEnumerator ReadXml(Action callback=null)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(xmlUrl);
        webRequest.timeout = 30;//设置下载超时时间
        yield return webRequest.SendWebRequest();//等待下载完成

        //如果下载超时
        if (webRequest.isNetworkError)
        {
            Debug.Log("Download Error" + webRequest.error);
        }
        else
        {
            FileTools.CreatFile(SeverXmlPath, webRequest.downloadHandler.data);
            if (File.Exists(SeverXmlPath))
            {
                m_SeverInfo = BinarySerializeOpt.XmlDeserialize(SeverXmlPath, typeof(ServerInfo)) as ServerInfo;
            }
            else
            {
                Debug.LogError("热更配置读取出错！");
            }
        }

        callback?.Invoke();
    }
    /// <summary>
    /// 已下载文件MD5效验
    /// </summary>
    /// <param name="downLoadAssetBundles">已下载的文件列表</param>
    /// <param name="callBack">回调</param>
    private void VerifyMD5(List<DownLoadAssetBundle> downLoadAssets, Action callBack)
    {
        //出错列表：表示下载的文件不完整
        List<Patch> downLoadList = new List<Patch>();
        foreach (DownLoadAssetBundle downLoad in downLoadAssets)
        {
            if (m_DownLoadMD5Dic.TryGetValue(downLoad.FileName, out string md5))
            {
                if (MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath) != md5)
                {
                    Debug.Log(string.Format("此文件{0}MD5效验失败，即将重新下载", downLoad.FileName));
                    Patch patch = FindPatchByGamePath(downLoad.FileName);
                    if (patch != null)
                    {
                        downLoadList.Add(patch);
                    }
                }
            }
        }

        if (downLoadList.Count <= 0)
        {
            downLoadList.Clear();
            if (callBack != null)
            {
                StartDownload = false;
                callBack();
            }
            LoadOver?.Invoke();
        }
        else
        {
            if (TryDownCount >= DOWNLOADCOUNT)
            {
                string allName = "";
                StartDownload = false;
                foreach (Patch patch in downLoadList)
                {
                    allName += patch.Name + ";";
                }
                Debug.LogError("资源重复下载4次MD5效验都失败，请检查资源" + allName);
                ItemError?.Invoke(allName);
            }
            else
            {
                TryDownCount++;
                m_DownLoadMD5Dic.Clear();
                foreach (Patch patch in downLoadList)
                {
                    m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
                }
                mon.StartCoroutine(StartDownAB(callBack, downLoadList));
            }
        }
    }
    /// <summary>
    /// 计算需要解压的文件
    /// </summary>
    /// <returns></returns>
    public bool ComputeUnPackFile()
    {
#if UNITY_ANDROID
        if (!Directory.Exists(UnPackPath))
            Directory.CreateDirectory(UnPackPath);
        m_UnPackedList.Clear();
        foreach (string fileName in m_PackedMd5.Keys)
        {
            string filePath = UnPackPath + "/" + fileName;
            if (File.Exists(filePath))
            {
                string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
                if (m_PackedMd5[fileName].MD5 != md5)
                    m_UnPackedList.Add(fileName);

            }
            else
                m_UnPackedList.Add(fileName);
        }
        foreach (string fileName in m_UnPackedList)
        {
            if (m_PackedMd5.ContainsKey(fileName))
                UnPackSumSize += m_PackedMd5[fileName].Size;
        }
        return m_UnPackedList.Count > 0;
#else
        return false;
#endif
    }


    /// <summary>
    /// 获取解压进度
    /// </summary>
    /// <returns></returns>
    public float GetUnpackProgress()
    {
        return AlreadyUnPackSize / UnPackSumSize;
    }

    /// <summary>
    /// 开始解压文件
    /// </summary>
    /// <param name="callBack"></param>
    public void StartUnPackFile(Action callBack)
    {
        StartUnPack = true;
        mon.StartCoroutine(UnPackToPersistentDataPath(callBack));
    }

    /// <summary>
    /// 开始解压包里的资源至本地
    /// </summary>
    /// <param name="callBack"></param>
    /// <returns></returns>
    IEnumerator UnPackToPersistentDataPath(Action callBack)
    {
        foreach (string fileName in m_UnPackedList)
        {

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileName);

            unityWebRequest.timeout = 30;
            yield return unityWebRequest.SendWebRequest();
            if (unityWebRequest.isNetworkError)
                //if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
                Debug.LogError("UnPack Error" + unityWebRequest.error);
            else
            {
                byte[] bytes = unityWebRequest.downloadHandler.data;
                FileTools.CreatFile(UnPackPath + "/" + fileName, bytes);
            }
            if (m_PackedMd5.ContainsKey(fileName))
                AlreadyUnPackSize += m_PackedMd5[fileName].Size;
            unityWebRequest.Dispose();
        }
        callBack?.Invoke();
        StartUnPack = false;
    }

 

    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public float GetProgress()
    {
        return GetLoadSize() / LoadSumSize;
    }
    /// <summary>
    /// 获取已下载文件总大小
    /// </summary>
    /// <returns></returns>
    public float GetLoadSize()
    {
        float alreadySize = AlreadyDownList.Sum(x => x.Size);
        float curAlreadSize = 0;
        if (m_CurDownLoad != null)
        {
            Patch patch = FindPatchByGamePath(m_CurDownLoad.FileName);
            if (patch != null && !AlreadyDownList.Contains(patch))
            {
                curAlreadSize = m_CurDownLoad.GetProcess() * patch.Size;
            }
        }
        return alreadySize + curAlreadSize;
    }
}
public static class FileTools
{
    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bytes"></param>
    public static void CreatFile(string filePath,byte[] bytes)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        FileInfo file = new FileInfo(filePath);
        using(Stream st = file.Create())
        {
            st.Write(bytes, 0, bytes.Length);
        }
    }
}