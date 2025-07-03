using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// 资源文件的名称
    /// </summary>
    protected string m_ABConfigABName = "assetbundleconfig";
    /// <summary>
    /// 资源关系依赖配置表，可以根据crc来找对应资源模块
    /// </summary>
    protected Dictionary<uint, ResourceItem> m_ResourcesItemDic = new Dictionary<uint, ResourceItem>();
    /// <summary>
    /// 储存预加载的AB包key为crc
    /// </summary>
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    /// <summary>
    /// AssetBundleItem类对象池(扩展资源数量)
    /// </summary>
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreatClassPool<AssetBundleItem>(500);

    /// <summary>
    /// AB包加载路径
    /// </summary>
    public string ABLoadPath
    {
        get
        {
#if UNITY_ANDROID
            return Application.persistentDataPath + "/Origin/";
#else
            return Application.streamingAssetsPath + "/";//---初始框架使用
#endif

        }
    }
  public static string AesKey  = ConStr.AesKey;
    /// <summary>
    /// 加载ab配置表
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
#if UNITY_EDITOR
        if (!ResourceManager.Instance.m_LoadFormAssetBundle)
            return false;
#endif
        m_ResourcesItemDic.Clear();
        string configPath = ABLoadPath + m_ABConfigABName;

        string hotPath = HotPatchManager.Instance.ComputeABPath(m_ABConfigABName);
        configPath = string.IsNullOrEmpty(hotPath) ? configPath : hotPath;

        byte[] bytes = AES.AESFileByteDecrypt(configPath, AesKey);

        //原来AB从文件夹加载，先从加密后的AB加载
        //AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
        AssetBundle configAB = AssetBundle.LoadFromMemory(bytes);
        
        TextAsset textAsset = configAB.LoadAsset<TextAsset>(m_ABConfigABName);
        if (textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }
        AssetBundleConfig config = null;
        using (MemoryStream stream = new MemoryStream(textAsset.bytes))
        {
            BinaryFormatter bf = new BinaryFormatter();
            config = (AssetBundleConfig)bf.Deserialize(stream);
        }
        for (int i = 0; i < config.ABList.Count; i++)
        {
            ABBase abBase = config.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = abBase.Crc;
            item.m_AssetName = abBase.AsseetName;
            item.m_ABName = abBase.ABName;
            item.m_DependAssetBundle = abBase.ABDependce;
            if (m_ResourcesItemDic.ContainsKey(item.m_Crc))
            {
                Debug.LogError("重复的CRC 资源名：" + item.m_AssetName + " ab包名：" + item.m_ABName);
            }
            else
            {
                m_ResourcesItemDic.Add(item.m_Crc, item);
            }
        }
        //foreach (var item in m_ResourcesItemDic)
        //{
        //    Debug.LogError(item.Value.m_ABName);
        //}
        return true;
    }

    /// <summary>
    /// 根据路径的crc加载中间类ResourcesItem（外部调用）
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourcesAssetBundle(uint crc)
    {
        if (!m_ResourcesItemDic.TryGetValue(crc, out ResourceItem item) || item == null)
        {
            Debug.LogError(string.Format("LoadResourceAssetBundle error: can not find crc {0} in AssetBundleConfig", crc.ToString()));
            return item;
        }
        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        if (item.m_DependAssetBundle != null)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                LoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }
        return item;
    }
    /// <summary>
    /// 加载单个assetbundle根据名字
    /// </summary>
    /// <param name="name">AB包名</param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        uint crc = Crc32.GetCrc32(name);
        if (!m_AssetBundleItemDic.TryGetValue(crc, out AssetBundleItem item))
        {
            ////修改全路径
            //string fullPath = ABLoadPath + name;

            string hotABPath = HotPatchManager.Instance.ComputeABPath(name);
            string fullPath = string.IsNullOrEmpty(hotABPath) ? ABLoadPath + name : hotABPath;

            byte[] bytes = AES.AESFileByteDecrypt(fullPath, AesKey);
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(bytes);

            //AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error:" + fullPath);
            }
            item = m_AssetBundleItemPool.Spawn(true);
            item.assetBundle = assetBundle;
            item.RefCount++;
            m_AssetBundleItemDic.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }
        return item.assetBundle;
    }
    /// <summary>
    /// 卸载回收AB包根据名字
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null)
        {
            return;
        }
        if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                UnLoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }
        UnLoadAssetBundle(item.m_ABName);
    }
    /// <summary>
    /// 根据该资源所在的AssetBundle的名字卸载
    /// </summary>
    /// <param name="name"></param>
    private void UnLoadAssetBundle(string name)
    {
        uint crc = Crc32.GetCrc32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out AssetBundleItem item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle != null)
            {
                item.assetBundle.Unload(true);
                item.Rest();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }
    /// <summary>
    /// 根据crc查找ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        m_ResourcesItemDic.TryGetValue(crc, out ResourceItem item);
        return item;
    }
}
/// <summary>
/// 记录AB包以及对应AB包的计数引用
/// 类对象池创建
/// </summary>
public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount;

    /// <summary>
    /// 回收时调用，为类对象池提供清空的方法
    /// </summary>
    public void Rest()
    {
        assetBundle = null;
        RefCount = 0;
    }
}

/// <summary>
/// 资源中间表（过度）
/// </summary>
public class ResourceItem
{
    /// <summary>
    /// 资源路径的CRC
    /// </summary>
    public uint m_Crc = 0;
    /// <summary>
    /// 该资源的文件名
    /// </summary>
    public string m_AssetName = string.Empty;
    /// <summary>
    /// 该资源所在的AssetBundle
    /// </summary>
    public string m_ABName = string.Empty;
    /// <summary>
    /// 该资源所依赖的AssetBundle
    /// </summary>
    public List<string> m_DependAssetBundle = null;
    /// <summary>
    /// 该资源加载完的AB包（单独处理）
    /// </summary>
    public AssetBundle m_AssetBundle = null;

    //-----------------以下主要针对资源----------------------
    /// <summary>
    /// 加载出来的资源对象
    /// </summary>
    public Object m_Obj = null;
    /// <summary>
    /// 资源唯一标识
    /// </summary>
    public int m_Guid = 0;
    /// <summary>
    /// 资源最后所使用时间
    /// </summary>
    public float m_LaseUseTime = 0.0f;
    /// <summary>
    /// 资源引用计数
    /// </summary>
    public int m_RefCount = 0;
    /// <summary>
    /// 是否跳场景清掉
    /// </summary>
    public bool m_Clear = true;

    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;
            if (m_RefCount < 0)
            {
                Debug.LogError("refcount<0" + m_RefCount + "," + (m_Obj != null ? m_Obj.name : "name is null"));
            }
        }
    }
}