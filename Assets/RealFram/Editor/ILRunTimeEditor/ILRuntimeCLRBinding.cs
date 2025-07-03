#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
   [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream("Assets/GameData/Data/HotFix/HotFix.dll.bytes", System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/RealFram/Generated");
        }

        AssetDatabase.Refresh();
        Debug.Log("生成成功");
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        domain.RegisterCrossBindingAdaptor(new HotFix.WindowAdapter());
        domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
    }
}
#endif
