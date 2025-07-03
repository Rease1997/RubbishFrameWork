//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;


public class ResourceObj
{
    /// <summary>
    /// 路径对应CRC
    /// </summary>
    public uint m_Crc = 0;
    /// <summary>
    /// 存ResourceItem
    /// </summary>
    public ResourceItem m_ResItem = null;
    /// <summary>
    /// 实例化出来的GameObject
    /// </summary>
    public GameObject m_CloneObj = null;
    /// <summary>
    /// 是否跳场景清除
    /// </summary>
    public bool m_bClear = true;
    /// <summary>
    /// 存储GUID
    /// </summary>
    public long m_Guid = 0;
    /// <summary>
    /// 是否已经放回对象池
    /// </summary>
    public bool m_Already = false;
    /// <summary>
    /// 是否放到场景下面
    /// </summary>
    public bool m_SetSceneParent = false;
    /// <summary>
    /// 实例化加载完成回调
    /// </summary>
    public OnAsyncResFinish m_DealFinish = null;
    /// <summary>
    /// 异步参数
    /// </summary>
    public object m_Param1, m_Param2, m_Param3 = null;
    /// <summary>
    /// 离线数据
    /// </summary>
    public OfflineData m_offlineData = null;
    public void Reset()
    {
        m_Crc = 0;
        m_CloneObj = null;
        m_bClear = true;
        m_Guid = 0;
        m_ResItem = null;
        m_Already = false;
        m_SetSceneParent = false;
        m_DealFinish = null;
        m_Param1 = m_Param2 = m_Param3 = null;
        m_offlineData = null;
    }
}

/////特效图片大小：2的n次方幂，尺寸（像素）不超过512*512
/// <summary>
/// 加载资源的优先级
/// </summary>
public enum LoadResPriority
{
    /// <summary>
    /// 一般优先级
    /// </summary>
    RES_MIDDLE = 0,
    /// <summary>
    /// 低优先级
    /// </summary>
    RES_SLOW,
    /// <summary>
    /// 最高优先级
    /// </summary>
    RES_HIGHT,
    /// <summary>
    /// 优先级数量
    /// </summary>
    RES_NUM,
}

/// <summary>
/// 资源加载回调（针对的是不实例化在场景中的资源）
/// </summary>
/// <param name="path"></param>
/// <param name="obj"></param>
/// <param name="param"></param>
public delegate void OnAsyncResFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);

/// <summary>
/// 实例化对象加载完成回调
/// </summary>
/// <param name="path"></param>
/// <param name="resObj"></param>
/// <param name="param1"></param>
/// <param name="param2"></param>
/// <param name="param3"></param>
public delegate void OnAsyncObjFinish(string path, ResourceObj resObj, object param1 = null, object param2 = null, object param3 = null);


/// <summary>
/// 资源队列
/// </summary>
public class AsyncLoadResParam
{
    public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
    public uint m_Crc;
    public string m_Path;
    /// <summary>
    /// 是否是图片
    /// </summary>
    public bool m_Sprite = false;
    public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

    public void Reset()
    {
        m_CallBackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_Sprite = false;
        m_Priority = LoadResPriority.RES_SLOW;
    }
}

/// <summary>
/// 回调数据
/// </summary>
public class AsyncCallBack
{
    /// <summary>
    /// 加载完成的回调（针对ObjectManager）
    /// </summary>
    public OnAsyncObjFinish m_DealObjFinish = null;
    /// <summary>
    /// 针对ObjectManager对应的中间
    /// </summary>
    public ResourceObj m_ResObj = null;

    /// <summary>
    /// 加载完成的回调
    /// </summary>
    public OnAsyncResFinish m_DealFinish = null;
    /// <summary>
    /// 回调参数
    /// </summary>
    public object m_Param1 = null, m_Param2 = null, m_Param3 = null;


    public void Reset()
    {
        m_DealObjFinish = null;
        m_DealFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
        m_ResObj = null;
    }
}
public class ResourceManager : Singleton<ResourceManager>
{
    /// <summary>
    /// 判断是否热更新
    /// </summary>
    public bool m_LoadFormAssetBundle = true;
    /// <summary>
    /// 最大缓存个数
    /// </summary>
    protected const int MAXCACHECOUNT = 500;
    /// <summary>
    /// 缓存引用计数为零的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
    /// </summary>
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    /// <summary>
    /// 缓存使用的资源列表
    /// </summary>
    public Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();
    /// <summary>
    /// 中间类，回调类的对象池
    /// </summary>
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = ObjectManager.Instance.GetOrCreatClassPool<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);

    /// <summary>
    /// Mono脚本
    /// </summary>
    protected MonoBehaviour m_Startmono;
    /// <summary>
    /// 正在异步加载的资源列表
    /// </summary>
    protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
    /// <summary>
    /// 正在异步加载的Dic
    /// </summary>
    protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
    /// <summary>
    /// 唯一guid
    /// </summary>
    private long m_Guid;

    /// <summary>
    /// 最长连续加载资源的时间，单位微秒
    /// </summary>
    private const long MAXLOADRESTIME = 200000;
    /// <summary>
    /// 初始化调用开启携程
    /// </summary>
    /// <param name="mono"></param>
    /// 
    internal void ClearAllObject()
    {
        m_NoRefrenceAssetMapList.Clear();
        AssetDic.Clear();
        m_LoadingAssetDic.Clear();
        m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
    }
    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        m_Startmono = mono;
        m_Startmono.StartCoroutine(AsyncLoadCor());
    }

    /// <summary>
    /// 同步资源加载，外部直接调用，仅加载不需要实例化的资源，例如Texture、音频等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResources<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCaCheResourcesItem(crc);
        if (item != null)
            return item.m_Obj as T;
        T obj = null;
#if UNITY_EDITOR
        if (!m_LoadFormAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = (T)item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }
        CacheResource(path, ref item, crc, obj);
        return obj;

    }
#if UNITY_EDITOR
    /// <summary>
    /// 从编辑器加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    protected T LoadAssetByEditor<T>(string path) where T : Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }

#endif
    /// <summary>
    /// 缓存加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int addrefcount = 1)
    {
        //缓存太多，清除最早没有使用的资源
        WashOut();
        if (item == null)
        {
            Debug.LogError("ResourcesItem is null,path:" + path);
        }
        if (obj == null)
        {
            Debug.LogError("ResourcesItem Fail:" + path);
        }
        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.m_LaseUseTime = Time.realtimeSinceStartup;
        item.RefCount += addrefcount;
        if (AssetDic.TryGetValue(item.m_Crc, out ResourceItem oldItem))
        {
            AssetDic[item.m_Crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_Crc, item);
        }
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroyCache"></param>
    protected void DestoryResourceItem(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }
        if (!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }
        if (!AssetDic.Remove(item.m_Crc))
            return;
        m_NoRefrenceAssetMapList.Remove(item);
        //释放assetbundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        //清空资源对应的对象池
        ObjectManager.Instance.ClearPoolObject(item.m_Crc);
        if (item.m_Obj != null)
        {
            item.m_Obj = null;
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
        }
    }
    /// <summary>
    /// 缓存太多清除最早没使用的资源
    /// </summary>
    protected void WashOut()
    {
        //当大于缓存个数时 ，进行一般释放
        while (m_NoRefrenceAssetMapList.Size() >= MAXCACHECOUNT)
        {
            for (int i = 0; i < MAXCACHECOUNT / 2; i++)
            {
                ResourceItem item = m_NoRefrenceAssetMapList.Back();
                DestoryResourceItem(item, true);
            }
        }

    }
    /// <summary>
    /// 从资源池获取缓存资源
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="addrefcount"></param>
    /// <returns></returns>
    ResourceItem GetCaCheResourcesItem(uint crc, int addrefcount = 1)
    {
        if (AssetDic.TryGetValue(crc, out ResourceItem item))
        {
            if (item != null)
            {
                item.RefCount += addrefcount;
                item.m_LaseUseTime = Time.realtimeSinceStartup;
            }
        }
        return item;
    }

    /// <summary>
    /// 不需要实例化资源的卸载，清除对象
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destoryObj"></param>
    /// <returns></returns>
    public bool RealaseResource(Object obj, bool destoryObj = false)
    {
        if (obj == null)
        {
            return false;
        }
        ResourceItem item = null;
        foreach (ResourceItem res in AssetDic.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
            {
                item = res;
            }
        }
        if (item == null)
        {
            Debug.LogError("AssetDic里面不存在该资源" + obj.name + " 可能释放了很多次");
            return false;
        }
        item.RefCount--;
        DestoryResourceItem(item, destoryObj);
        return true;
    }
    /// <summary>
    /// 不需要实例化资源的卸载，根据路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool RealaseResource(string path, bool destoryObj = false)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        uint crc = Crc32.GetCrc32(path);
        if (!AssetDic.TryGetValue(crc, out ResourceItem item) || null == item)
        {
            Debug.LogError("AssetDic里面不存在该资源" + path + " 可能释放了很多次");
        }
        item.RefCount--;
        DestoryResourceItem(item, destoryObj);
        return true;
    }

    /// <summary>
    /// 根据ResourceObj卸载资源
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="destoryCache"></param>
    public bool RealaseResource(ResourceObj resObj, bool destoryObj = false)
    {
        if (resObj == null)
        {
            return false;
        }
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(resObj.m_Crc, out item) || item == null)
        {
            Debug.LogError("AssetDic里面不存在该资源：" + resObj.m_CloneObj.name + "可能释放了很多次");
        }
        GameObject.Destroy(resObj.m_CloneObj);
        item.RefCount--;
        DestoryResourceItem(item, destoryObj);
        return true;
    }
    /// <summary>
    /// 预加载资源
    /// </summary>
    public void PreLoadRes(string path, bool isSprite = false)
    {
        if (string.IsNullOrEmpty(path))
            return;
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCaCheResourcesItem(crc, 0);
        if (item != null)
            return;
        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFormAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item != null && item.m_Obj != null)
            {
                obj = item.m_Obj;
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                if (isSprite)
                    obj = LoadAssetByEditor<Sprite>(path);
                else
                    obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    if (isSprite)
                        obj = item.m_AssetBundle.LoadAsset<Sprite>(item.m_AssetName);
                    else
                        obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResource(path, ref item, crc, obj);
        //跳场景不清空内存
        item.m_Clear = false;
        RealaseResource(obj, false);
    }

    /// <summary>
    /// 新图集加载方法
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Sprite LoadSpriteBySpriteAtlas(string path, string name)
    {
        Debug.Log(path + "   " + name);
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCaCheResourcesItem(crc);
        if (item != null)
        {
            var array = item.m_AssetBundle.LoadAssetWithSubAssets<Sprite>(path);
            foreach (var spr in array)
            {
                if (spr.name == name)
                    return spr;
            }
        }
        AssetBundle ab = null;
        Sprite sprite = null;
#if UNITY_EDITOR
        if (!m_LoadFormAssetBundle)
        {
            if (sprite == null)
            {
                Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var spr in sprites)
                {
                    if (spr.GetType() == typeof(Sprite))
                    {
                        if (spr.name == name)
                        {
                            sprite = (Sprite)spr;
                            return sprite;
                        }
                    }
                }
            }
        }
#endif
        if (ab == null)
        {
            item = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                ab = item.m_AssetBundle;
                foreach (var spr in ab.LoadAssetWithSubAssets<Sprite>(path))
                {
                    if (spr.name == name)
                        sprite = spr;
                }
            }
        }
        CacheResource(path, ref item, crc, ab);
        return sprite;
    }

    /// <summary>
    /// 异步加载资源（仅仅是不需要实例化加载的资源(Texture、音频等等)）
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="dealFinish">加载完成回调</param>
    /// <param name="priority">该加载资源的优先级</param>
    /// <param name="isSprite">是否是图片</param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="crc">该资源的crc</param>
    public void AsyncLoadResource(string path, OnAsyncResFinish dealFinish, LoadResPriority priority, bool isSprite = false,
        object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = Crc32.GetCrc32(path);
        }
        ResourceItem item = GetCaCheResourcesItem(crc);
        if (item != null)
        {
            dealFinish?.Invoke(path, item.m_Obj, param1, param2, param3);
            return;
        }
        //判断是否在加载中
        if (!m_LoadingAssetDic.TryGetValue(crc, out AsyncLoadResParam para) || para == null)
        {
            para = m_AsyncLoadResParamPool.Spawn(true);
            para.m_Crc = crc;
            para.m_Path = path;
            para.m_Sprite = isSprite;
            para.m_Priority = priority;
            m_LoadingAssetDic.Add(crc, para);
            m_LoadingAssetList[(int)priority].Add(para);
        }
        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_DealFinish = dealFinish;
        callBack.m_Param1 = param1;
        callBack.m_Param2 = param2;
        callBack.m_Param3 = param3;
        para.m_CallBackList.Add(callBack);
    }

    /// <summary>
    /// 异步加载（预制体）
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <param name="onLoadResourceObjFinish"></param>
    /// <param name="priority"></param>
    public void AsyncLoadResource(string path, ResourceObj resObj, OnAsyncObjFinish dealfinish, LoadResPriority priority)
    {
        ResourceItem item = GetCaCheResourcesItem(resObj.m_Crc);
        if (item != null)
        {
            resObj.m_ResItem = item;
            dealfinish?.Invoke(path, resObj);
            return;
        }
        //判断是否在加载中
        if (!m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out AsyncLoadResParam para) || para == null)
        {
            para = m_AsyncLoadResParamPool.Spawn(true);
            para.m_Crc = resObj.m_Crc;
            para.m_Path = path;
            para.m_Priority = priority;
            m_LoadingAssetDic.Add(resObj.m_Crc, para);
            m_LoadingAssetList[(int)priority].Add(para);
        }
        //往回调列表里面加回调
        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_DealObjFinish = dealfinish;
        callBack.m_ResObj = resObj;
        para.m_CallBackList.Add(callBack);
    }
    /// <summary>
    /// 异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        List<AsyncCallBack> callBackList = null;
        //上一次yield的时间
        long lastYiledTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool haveYield = false;
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                if (m_LoadingAssetList[(int)LoadResPriority.RES_HIGHT].Count > 0)
                {
                    i = (int)LoadResPriority.RES_HIGHT;
                }
                else if (m_LoadingAssetList[(int)LoadResPriority.RES_MIDDLE].Count > 0)
                {
                    i = (int)LoadResPriority.RES_MIDDLE;
                }
                List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                if (loadingList.Count <= 0)
                    continue;

                AsyncLoadResParam loagingItem = loadingList[0];//每次的第一个元素永远不相同
                loadingList.RemoveAt(0);
                callBackList = loagingItem.m_CallBackList;
                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!m_LoadFormAssetBundle)
                {
                    if (loagingItem.m_Sprite)
                        obj = LoadAssetByEditor<Sprite>(loagingItem.m_Path);
                    else
                        obj = LoadAssetByEditor<Object>(loagingItem.m_Path);
                    yield return new WaitForSeconds(1f);
                    item = AssetBundleManager.Instance.FindResourceItem(loagingItem.m_Crc);
                    if (item == null)
                    {
                        item = new ResourceItem
                        {
                            m_Crc = loagingItem.m_Crc
                        };
                    }
                }
#endif
                if (obj == null)
                {
                    item = GetCaCheResourcesItem(loagingItem.m_Crc);
                    if (item == null)
                        item = AssetBundleManager.Instance.LoadResourcesAssetBundle(loagingItem.m_Crc);//1.不存在这个资源，2.没有存入资源
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loagingItem.m_Sprite)
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        else
                            abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        yield return abRequest;//等待加载一步异步资源完成
                        if (abRequest.isDone)
                            obj = abRequest.asset;
                        lastYiledTime = System.DateTime.Now.Ticks;
                    }
                }
                CacheResource(loagingItem.m_Path, ref item, loagingItem.m_Crc, obj, callBackList.Count);
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsyncCallBack callBack = callBackList[j];
                    if (callBack != null && callBack.m_DealObjFinish != null && callBack.m_ResObj != null)
                    {
                        ResourceObj tempResObj = callBack.m_ResObj;
                        if (obj != null && tempResObj.m_CloneObj == null)
                        {
                            if (tempResObj.m_SetSceneParent)
                            {
                                tempResObj.m_CloneObj = GameObject.Instantiate(obj, ObjectManager.Instance.SceneTrs) as GameObject;
                            }
                            else
                            {
                                tempResObj.m_CloneObj = GameObject.Instantiate(obj) as GameObject;
                            }
                        }
                        tempResObj.m_ResItem = item;
                        tempResObj.m_DealFinish(loagingItem.m_Path, tempResObj.m_CloneObj, tempResObj.m_Param1, tempResObj.m_Param2, tempResObj.m_Param3);
                        callBack.m_DealFinish = null;
                        tempResObj = null;
                    }

                    if (callBack != null && callBack.m_DealFinish != null)
                    {
                        //callBack.m_ResObj.m_DealFinish?.Invoke(loagingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
                        callBack.m_DealFinish(loagingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
                        callBack.m_DealFinish = null;
                    }
                    callBack.Reset();
                    m_AsyncCallBackPool.Recycle(callBack);

                }
                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loagingItem.m_Crc);
                loagingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loagingItem);
                if (System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYiledTime = System.DateTime.Now.Ticks;
                    haveYield = true;
                }
            }
            if (!haveYield || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
            {
                lastYiledTime = System.DateTime.Now.Ticks;
                yield return null;//等待一帧跳过
            }
        }
    }
    /// <summary>
    /// 根据路径获取ResourceObj
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <returns></returns>
    public ResourceObj LoadResource(string path, ResourceObj resObj)
    {
        if (resObj == null)
        {
            return null;
        }
        uint crc = resObj.m_Crc == 0 ? Crc32.GetCrc32(path) : resObj.m_Crc;
        ResourceItem item = GetCaCheResourcesItem(crc);
        if (item != null)
        {
            resObj.m_ResItem = item;
            return resObj;
        }
        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFormAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item != null && item.m_Obj != null)
            {
                obj = item.m_Obj as Object;
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as Object;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResource(path, ref item, crc, obj);
        resObj.m_ResItem = item;
        item.m_Clear = resObj.m_bClear;
        return resObj;
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        foreach (ResourceItem item in AssetDic.Values)
        {
            if (item.m_Clear)
            {
                DestoryResourceItem(item, true);
            }
        }
    }

    /// <summary>
    /// 创建唯一ID
    /// </summary>
    /// <returns></returns>
    public long CreatGuid()
    {
        return m_Guid++;
    }

    /// <summary>
    /// 取消异步加载资源
    /// </summary>
    /// <param name="resObj"></param>
    /// <returns></returns>
    public bool CancleLoad(ResourceObj resObj)
    {
        AsyncLoadResParam para = null;
        if (m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out para) && m_LoadingAssetList[(int)para.m_Priority].Contains(para))
        {
            for (int i = para.m_CallBackList.Count; i >= 0; i--)
            {
                AsyncCallBack tempCallBack = para.m_CallBackList[i];
                if (tempCallBack != null && resObj == tempCallBack.m_ResObj)
                {
                    tempCallBack.Reset();
                    m_AsyncCallBackPool.Recycle(tempCallBack);
                    para.m_CallBackList.Remove(tempCallBack);
                }
            }
            if (para.m_CallBackList.Count < 0)
            {
                para.Reset();
                m_LoadingAssetList[(int)para.m_Priority].Remove(para);
                m_AsyncLoadResParamPool.Recycle(para);
                m_LoadingAssetDic.Remove(resObj.m_Crc);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 根据ResourceObj增加引用计数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int IncreaseResourceRef(ResourceObj resObj, int count = 1)
    {
        return resObj != null ? IncreaseResourceRef(resObj.m_Crc, count) : 0;
    }

    /// <summary>
    /// 根据crc增加引用计数
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int IncreaseResourceRef(uint crc = 0, int count = 1)
    {
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
        {
            return 0;
        }
        item.RefCount += count;
        item.m_LaseUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }

    /// <summary>
    /// 根据ResourceObj减小引用计数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int DecreaseResourcesRef(ResourceObj resObj, int count = 1)
    {
        return resObj != null ? DecreaseResourcesRef(resObj.m_Crc, count) : 0;
    }

    /// <summary>
    /// 根据crc引用计数
    /// </summary>
    /// <param name="m_Crc"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private int DecreaseResourcesRef(uint crc, int count)
    {
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
        {
            return 0;
        }
        item.RefCount -= count;
        return item.RefCount;
    }

}
/// <summary>
/// 双向链表结构节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinedListNode<T> where T : class, new()
{
    /// <summary>
    /// 前一个节点
    /// </summary>
    public DoubleLinedListNode<T> prev = null;
    /// <summary>
    /// 后一个节点
    /// </summary>
    public DoubleLinedListNode<T> next = null;
    /// <summary>
    /// 当前节点
    /// </summary>
    public T t = null;
}
/// <summary>
/// 双链表结构
/// </summary>
public class DoubleLinedList<T> where T : class, new()
{
    /// <summary>
    /// 表头
    /// </summary>
    public DoubleLinedListNode<T> Head = null;
    /// <summary>
    /// 表尾
    /// </summary>
    public DoubleLinedListNode<T> Tail = null;
    /// <summary>
    /// 双向链表结构类对象池
    /// </summary>
    protected ClassObjectPool<DoubleLinedListNode<T>> m_DoubleLinkNodePool = new ClassObjectPool<DoubleLinedListNode<T>>(500);
    /// <summary>
    /// 个数
    /// </summary>
    protected int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }
    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinedListNode<T> AddToHeader(T t)
    {
        DoubleLinedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeader(pList);
    }
    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinedListNode<T> AddToHeader(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }
        m_Count++;
        return Head;
    }
    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinedListNode<T> AddTail(T t)
    {
        DoubleLinedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddTail(pList);
    }
    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinedListNode<T> AddTail(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.next = null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }
        m_Count++;
        return Tail;
    }
    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return;
        }
        if (pNode == Head)
        {
            Head = pNode.next;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.next = pNode.prev = null;
        pNode.t = null;
        m_DoubleLinkNodePool.Recycle(pNode);
        m_Count--;
    }
    /// <summary>
    /// 把某个节点移动到头部
    /// </summary>
    /// <param name="pNode"></param>
    public void MoveToHead(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }
        if (pNode.prev == null && pNode.next == null)
        {
            return;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}
/// <summary>
/// 外部调用的接口管理
/// </summary>
/// <typeparam name="T"></typeparam>
public class CMapList<T> where T : class, new()
{
    DoubleLinedList<T> m_DLink = new DoubleLinedList<T>();
    Dictionary<T, DoubleLinedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinedListNode<T>>();
    ~CMapList()
    {
        Clear();
    }
    /// <summary>
    /// 清空列表
    /// </summary>
    public void Clear()
    {
        while (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }
    /// <summary>
    /// 删除某个节点
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        if (m_FindMap.TryGetValue(t, out DoubleLinedListNode<T> node) || node == null)
            return;
        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }
    /// <summary>
    /// 插入一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t)
    {
        if (m_FindMap.TryGetValue(t, out DoubleLinedListNode<T> node) && node != null)
        {
            m_DLink.AddToHeader(node);
            return;
        }
        m_DLink.AddToHeader(t);
        m_FindMap.Add(t, m_DLink.Head);
    }
    /// <summary>
    /// 从表尾弹出一个节点
    /// </summary>
    public void Pop()
    {
        if (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }
    /// <summary>
    /// 获取到尾部节点
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.t;
    }
    /// <summary>
    /// 返回节点个数
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }
    /// <summary>
    /// 查找是否存在该节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T t)
    {
        if (!m_FindMap.TryGetValue(t, out DoubleLinedListNode<T> node) || node == null)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 查找节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinedListNode<T> FindNode(T t)
    {
        if (!m_FindMap.TryGetValue(t, out DoubleLinedListNode<T> node) || node == null)
        {
            return null;
        }
        return node;
    }
    /// <summary>
    /// 刷新某个节点，把结点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Reflesh(T t)
    {
        if (!m_FindMap.TryGetValue(t, out DoubleLinedListNode<T> node) || node == null)
        {
            return false;
        }
        m_DLink.MoveToHead(node);
        return true;
    }
}