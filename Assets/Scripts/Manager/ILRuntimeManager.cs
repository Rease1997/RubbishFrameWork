using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    AppDomain m_AppDomain;
    public AppDomain ILRunAppDomain => m_AppDomain;
    MemoryStream ms;
    MemoryStream ps;
    public static string DLLPATH = "Assets/GameData/Data/HotFix/HotFix.dll.bytes";
    public static string PDBPATH = "Assets/GameData/Data/HotFix/HotFix.pdb.bytes";
    MonoBehaviour mono;
    public void Init(MonoBehaviour mono)
    {
        this.mono = mono;
        LoadHotFixAssembly();
    }
    byte[] pdbdll;
    private void LoadHotFixAssembly()
    {
        //真个工程只有一个ILRuntime的AppDomain
        m_AppDomain = new AppDomain();
        //读取热更新的资源dll
        TextAsset dllText = ResourceManager.Instance.LoadResources<TextAsset>(DLLPATH);
        //PBD文件，调试数据可，日志报错
        ms = new MemoryStream(dllText.bytes);

        //TextAsset pdbText = ResourceManager.Instance.LoadResources<TextAsset>(PDBPATH);
        //ps = new MemoryStream(pdbText.bytes);
       
        mono.StartCoroutine(LoadPDB());
        
        //载入热更库
        m_AppDomain.LoadAssembly(ms, ps, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        //m_AppDomain.LoadAssembly(ms);
        InitialzeILRuntime();
    }
    /// <summary>
    /// 记载PDB
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadPDB()
    {
        UnityWebRequest pdbrequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/HotFix.pdb");
        yield return pdbrequest.SendWebRequest();
        if (!string.IsNullOrEmpty(pdbrequest.error))
            Debug.LogError(pdbrequest.error);

        pdbdll = pdbrequest.downloadHandler.data;//拿到dll文件的2进制数据
        ps = new MemoryStream(pdbdll);
        pdbrequest.Dispose();//关闭网络请求
    }

    /// <summary>
    /// 进行热更代码注册
    /// </summary>
    private void InitialzeILRuntime()
    {
        m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
        //带返回值的委托的话需要用RegisterFunctionDelegate,返回类型为最后一个
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String,UnityEngine.Object,System.Object,System.Object>();

        //UGUI Button点击事件转换
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((System.Action)action)();
            });
        });
        //UGUI toggle点击事件
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<bool>((a) =>
            {
                ((System.Action<bool>)action)(a);
            });
        });

        //注册事件
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<OnAsyncResFinish>((act) =>
        {
            return new OnAsyncResFinish ((path,obj,param1,param2,oaram3) =>
            {
                ((System.Action<string,Object,object,object,object>)act)(path, obj, param1, param2, oaram3);
            });
        });

        //注册协程适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        //注册Mono适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        //Vector3 适配器
        m_AppDomain.RegisterValueTypeBinder(typeof(Vector3),new Vector3Binder());
        //注册Window适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new HotFix.WindowAdapter());

        m_AppDomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        m_AppDomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());

        SetupCLRAddCompontent();
        SetUpCLRGetCompontent();
        SetUpCLRDebug();
       
        m_AppDomain.DebugService.StartDebugService(56000);
        //绑定最后执行
        ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
        Debug.LogError("有东西");
    }

    unsafe void SetUpCLRGetCompontent()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "GetCompontent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, GetCompontent);
            }
        }
    }

    private unsafe StackObject* GetCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();

        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                foreach (var clrInstance in clrInstances)
                {
                    if (clrInstance.ILInstance != null)
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe void SetupCLRAddCompontent()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, AddCompontent);
            }
        }
    }

    private unsafe StackObject* AddCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
        {
            throw new System.NullReferenceException();
        }
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType)//CLRType表示这个类型是Unity工程里的类型   //ILType表示是热更dll里面的类型
            {
                //Unity主工程的类，不需要做处理
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //创建出来MonoTest
                var ilInstance = new ILTypeInstance(type as ILType, false);
                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddCompontent出来的有效实例，所以要替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;

                //补掉Awake
                clrInstance.Awake();
            }
            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }


    //绑定debug
    unsafe void SetUpCLRDebug()
    {
        var mi = typeof(Debug).GetMethod("Log", new System.Type[] { typeof(object) });
        ILRunAppDomain.RegisterCLRMethodRedirection(mi, Log_11);
    }
    //编写重定向方法对于刚接触ILRuntime的朋友可能比较困难，比较简单的方式是通过CLR绑定生成绑定代码，然后在这个基础上改，比如下面这个代码是从UnityEngine_Debug_Binding里面复制来改的
    //如何使用CLR绑定请看相关教程和文档
    //ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj
    unsafe static StackObject* Log_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
        StackObject* ptr_of_this_method;
        //这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
        StackObject* __ret = ILIntepreter.Minus(__esp, 1);
        //取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
        ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

        //这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
        object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
        //所有非基础类型都得调用Free来释放托管堆栈
        __intp.Free(ptr_of_this_method);

        //在真实调用Debug.Log前，我们先获取DLL内的堆栈
        var stacktrace = __domain.DebugService.GetStackTrace(__intp);

        //我们在输出信息后面加上DLL堆栈
        UnityEngine.Debug.Log(message + "\n" + stacktrace);

        return __ret;
    }

}
