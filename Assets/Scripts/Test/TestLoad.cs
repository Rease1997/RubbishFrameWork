using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class TestLoad : MonoBehaviour
{
    public Transform RecyclePoolTrs;
    /// <summary>
    /// 场景节点
    /// </summary>
    public Transform SceneTrs;
    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(RecyclePoolTrs, SceneTrs);
    }
    // Start is called before the first frame update
    //ResourceItem item;
    //GameObject go;

    //ResourceItem item1, item2;
    public Image image1, image2, image3;
    //public AudioSource Source;
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    //protected CMapList<ObjectManager> m_NoRefrenceAssetMapList1 = new CMapList<ObjectManager>();

    //异步加载图片路径
    string path1 = "Assets/GameData/UI/ID_368.bmp";
    string path2 = "Assets/GameData/UI/ID_401.bmp";
    string path3 = "Assets/GameData/UI/shadow_tentaclyorb.png";

    void Start()
    {
        ResMsg();
        //ObjMsg();
    }
    GameObject Prefab;
    private void ObjMsg()
    {
        Prefab=ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Model/Attack.prefab");
    }
    private void ResMsg()
    {
        //#region 调用ResourceManager内的LoadResource方法加载图片  加载音乐  预加载
        //ResourceManager.Instance.PreLoadRes("Assets/GameData/UI/ID_401.bmp", true); //预加载

        //Sprite sp = ResourceManager.Instance.LoadResources<Sprite>("Assets/GameData/UI/ID_401.bmp");
        //image1.sprite = sp;

        //AudioClip clip = ResourceManager.Instance.LoadResourrces<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
        //Source.clip = clip;
        //Source.Play();
        //#endregion

        //#region 测试链表
        ////string im1path = "Assets/GameData/UI/ID_368.bmp";
        ////string im2path = "Assets/GameData/UI/ID_401.bmp";
        ////uint crc1 = Crc32.GetCrc32(im1path);
        ////uint crc2 = Crc32.GetCrc32(im2path);
        ////item1 = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc1);
        ////item2 = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc2);
        //#endregion

        //#region 加载资源
        ////Load();
        //#endregion

        //#region 先加载出资源 再按下A键卸载资源
        ////uint crc = Crc32.GetCrc32("Assets/GameData/Prefabs/Model/Attack.prefab"); //卸载资源
        ////item = AssetBundleManager.Instance.LoadResourcesAssetBundle(crc);
        ////go = Instantiate(item.m_AssetBundle.LoadAsset<GameObject>(item.m_AssetName));
        //#endregion

        //#region 异步加载
        ResourceManager.Instance.AsyncLoadResource(path1, OnLoadSpriteFinish, LoadResPriority.RES_SLOW, true, image1, false); //2
        ResourceManager.Instance.AsyncLoadResource(path2, OnLoadSpriteFinish, LoadResPriority.RES_HIGHT, true, image2, false); //1
        ResourceManager.Instance.AsyncLoadResource(path3, OnLoadSpriteFinish, LoadResPriority.RES_MIDDLE, true, image3, false); //3
        //#endregion
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ObjectManager.Instance.RealaseObject(Prefab,-1,destoryCache:true,recycleParent:true);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            ObjMsg();    
        }
        ////点击A卸载所加载的资源
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    Destroy(go);
        //    AssetBundleManager.Instance.ReleaseAsset(item);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    ResourceManager.Instance.RealaseResource("Assets/GameData/UI/ID_401.bmp", true); //资源卸载
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    Sprite sp = ResourceManager.Instance.LoadResourrces<Sprite>("Assets/GameData/UI/ID_401.bmp");
        //    image1.sprite = sp;
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    image1.sprite = item1.m_AssetBundle.LoadAsset<Sprite>(item1.m_AssetName);
        //    m_NoRefrenceAssetMapList.InsertToHead(item1);
        //}
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    image2.sprite = item2.m_AssetBundle.LoadAsset<Sprite>(item2.m_AssetName);
        //    m_NoRefrenceAssetMapList.InsertToHead(item2);
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    //DoubleLinkedListNode<ResourceItem> tempItem = m_NoRefrenceAssetMapList.Find(item2);
        //    //Debug.LogError(tempItem.t.m_AssetName);
        //}
    }
    void Load()
    {
        AssetBundle config = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = config.LoadAsset<TextAsset>("AssetbundleConfig");
        MemoryStream str = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig abconfig = (AssetBundleConfig)bf.Deserialize(str);
        str.Close();
        string path = "Assets/GameData/Prefabs/Model/Attack.prefab";
        uint crc = Crc32.GetCrc32(path);
        ABBase abBase = null;
        for (int i = 0; i < abconfig.ABList.Count; i++)
        {
            if (abconfig.ABList[i].Crc == crc)
            {
                abBase = abconfig.ABList[i];
            }
        }
        for (int i = 0; i < abBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);
        }
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
        Instantiate(assetBundle.LoadAsset<GameObject>(abBase.AsseetName));
    }

    /// <summary>
    /// 图片加载完成
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    void OnLoadSpriteFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            Image image = param1 as Image;
            bool setNativeSize = (bool)param2;
            if (image.sprite != null)
            {
                image.sprite = null;
            }
            image.sprite = sp;
            if (setNativeSize)
            {
                image.SetNativeSize();
            }
        }
    }
}
