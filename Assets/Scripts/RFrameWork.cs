using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RFrameWork : UnitySingleton<RFrameWork>
{
    public bool useHotFix = true;
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        ResourceManager.Instance.m_LoadFormAssetBundle = useHotFix;
        ResourceManager.Instance.Init(this);
        //AssetBundleManager.Instance.LoadAssetBundleConfig();
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
        GameMapManager.Instance.Init(this);
        HotPatchManager.Instance.Init(this);
        InitUiManager();
        RegisterUI();
    }
    ResourceItem item;
    GameObject obj;
   // Start is called before the first frame update
   void Start()
    {

        UIManager.Instance.PopUpWnd(ConStr.HOTFIXPANEL, true, true);

        //Debug.LogError(ConfigManager.Instance.FindData<BuffData>("Assets/GameData/Data/Binary/BuffData.bytes").FinBuffById(1).Name);
        //AudioSource source = GetComponent<AudioSource>();
        //AudioClip clip= ResourceManager.Instance.LoadResourrces<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
        //source.clip = clip;
        //source.Play();
        //ResourceManager.Instance.PreLoadRes("Assets/GameData/Prefabs/Model/Attack.prefab", false);
        //ResourceManager.Instance.AsyncLoadResource();
    }

    public void QuitTheGame()
    {
        Application.Quit();
        #region
        //        try
        //        {
        //#if UNITY_ANDROID
        //            if (mJavaObject == null)
        //            {
        //                Debug.LogError("AndroidJavaObject is null...");
        //                return;
        //            }
        //            else
        //            {
        //                mJavaObject.Call("ExitUnity");
        //            }

        //#elif UNITY_IPHONE
        //            IOSManager.isBack = true;
        //            IOSManager.ExitUnityToOtherUI();
        //            DestroyImmediate(gameObject);
        //#endif
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError("error:" + e.Message);
        //        }
        #endregion
    }
    public void OpenCommonConfirm(string title, string str, UnityEngine.Events.UnityAction confirmAction = null, UnityEngine.Events.UnityAction cancleAction = null)
    {
        GameObject commonObj = Instantiate(Resources.Load<GameObject>("CommonConfirm"));
        commonObj.transform.SetParent(UIManager.Instance.m_WndRoot, false);
        CommonConfirm commonItem = commonObj.GetComponent<CommonConfirm>();
        commonItem.Show(title, str, confirmAction, cancleAction);
    }

    private void InitUiManager()
    {
        RectTransform uiRoot = transform.Find("UIRoot") as RectTransform;
        RectTransform winRoot = transform.Find("UIRoot/WndRoot") as RectTransform;
        Camera uiCamera = transform.Find("UIRoot/UICamera").GetComponent<Camera>();
        EventSystem uiEvent = transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>();
        UIManager.Instance.Init(uiRoot, winRoot, uiCamera, uiEvent);
    }

    void RegisterUI()
    {
        UIManager.Instance.Register<MenuWindow>(ConStr.MENUPANEL);
        UIManager.Instance.Register<LoadingWindow>(ConStr.LOADINGPANEL);
        UIManager.Instance.Register<HotFixUI>(ConStr.HOTFIXPANEL);
        UIManager.Instance.Register<CityWindow>(ConStr.CITYPANEL);
    }

    public IEnumerator StartGame(Image image, Text text)
    {
        image.fillAmount = 0;
        yield return null;
        text.text = "加载本地数据.....";
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        image.fillAmount = 0.1f;
        yield return null;
        text.text = "加载DLL.....";
        ILRuntimeManager.Instance.Init(this);
        image.fillAmount = 0.2f;
        yield return null;
        text.text = "加载数据表......";
        //LoadConfiger();
        image.fillAmount = 0.3f;
        yield return null;
        text.text = "加载配置......";
        image.fillAmount = 0.4f;
        yield return null;
        text.text = "初始化地图......";
        GameMapManager.Instance.Init(this);
        image.fillAmount = 0.5f;
        yield return null;
        text.text = "初始化地图......";
        image.fillAmount = 0.6f;
        yield return null;
        image.fillAmount = 0.8f;
        yield return null;
        image.fillAmount = 1f;
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        { 
        }
        UIManager.Instance.OnUpdate();
    }
}
