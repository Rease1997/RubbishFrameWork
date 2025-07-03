using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class BundleEditor
{
    /// <summary>
    /// 打的AB包放置的路径
    /// </summary>
    private static string m_BundleTargetPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();

    /// <summary>
    /// AB包配置表的二进制路径
    /// </summary>
    private static string ABBYTEPATH = RealConfig.GetRealFram().m_ABBytePath;


    /// <summary>
    /// 所有文件夹AB包字典
    /// key是ab包名，value是路径
    /// </summary>
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();

    /// <summary>
    /// 过滤的list如果文件夹下已经包含了就不需要打AB
    /// </summary>
    private static List<string> m_AllFileAB = new List<string>();

    /// <summary>
    /// 单个prefab的ab包
    /// key：AB包，value：该AB包所有的依赖项
    /// </summary>
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();

    /// <summary>
    /// 储存所有有效路径,不需要动态加载的
    /// </summary>
    private static List<string> m_ConfigFile = new List<string>();
    [MenuItem("Tools/测试/测试加密")]
    public static void TestEnc()
    {
        AES.AESFileEncrypt(Application.dataPath+ "/Scripts/Test/TestSSS.cs", "2311");
    }
    [MenuItem("Tools/测试/测试解密")] 
    public static void TestDec()
    {
        AES.AESFileDecrypt(Application.dataPath + "/Scripts/Test/TestSSS.cs", "2311");
        AssetDatabase.Refresh();
    }

    #region 热更部分变量
    private static string m_VersionMd5Path = Application.dataPath + "/../Version/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    /// <summary>
    /// 存储AB包名及对应的Md5信息
    /// </summary>
    private static Dictionary<string, ABMD5Base> m_PackageMd5 = new Dictionary<string, ABMD5Base>();
    private static string hotABPath = "http://172.20.10.6:8080/AssetBundle/";
    /// <summary>
    /// 热更包路径
    /// </summary>
    private static string m_HotPath = Application.dataPath + "/../Hot/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    /// <summary>
    /// 
    /// </summary>
    public static string AesKey { get; private set; } = ConStr.AesKey;
    #endregion
    /// <summary>
    /// 打所有资源AB包
    /// </summary>
    [MenuItem("Tools/打包")]
    public static void NormalBuild()
    {
        Build();
    }
    /// <summary>
    /// 打包
    /// </summary>
    /// <param name="hotfix"></param>
    /// <param name="abmd5Path"></param>
    /// <param name="hotCount"></param>
    public static void Build(bool hotfix = false, string abmd5Path = "", string hotCount = "1")
    {
        DataEditor.AllXmlToBindary();
        m_ConfigFile.Clear();
        m_AllFileAB.Clear();
        m_AllFileDir.Clear();
        m_AllPrefabDir.Clear();
        ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>("Assets/ABConfig.asset");
        //循环遍历所有需要打包的文件夹
        foreach (ABConfig.FileDirName fileDir in aBConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFile.Add(fileDir.Path);
            }
        }
        //获取指定路径下的所有预制体，API返回的是资源路径的GUID
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", aBConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
            m_ConfigFile.Add(path);
            if (!ContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                m_AllFileAB.Add(path);
                allDependPath.Add(path);
                for (int j = 0; j < allDepend.Length; j++)
                {
                    if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs") && !allDepend[j].EndsWith(".prefab"))
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字的Prefab!名字：" + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }
        BuildAssetBundle();
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名", "名字：" + oldABNames[i], i * 1.0f / oldABNames.Length);
        }
        if (hotfix)
            ReadMd5Com(abmd5Path, hotCount);
        else
            WriteABMD5();
        AssetDatabase.SaveAssets();//编辑器保存
        AssetDatabase.Refresh();//编辑器刷新
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 读取MD5文件
    /// </summary>
    /// <param name="abmd5Path"></param>
    /// <param name="hotCount"></param>
    private static void ReadMd5Com(string abmd5Path, string hotCount)
    {
        m_PackageMd5.Clear();
        using(FileStream fs=new FileStream(abmd5Path, FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            ABMD5 abmd5 = bf.Deserialize(fs) as ABMD5;
            foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
            {
                m_PackageMd5.Add(abmd5Base.Name, abmd5Base);
            }
        }
        CopyABAndGeneratXml(SaveChangeABName(), hotCount);
    }

    private static void CopyABAndGeneratXml(List<string> changeList, string hotCount)
    {
        if (!Directory.Exists(m_HotPath))
            Directory.CreateDirectory(m_HotPath);
        DeleteAllFile(m_HotPath);
        foreach (string abName in changeList)
        {
            if (!abName.EndsWith(".manifest"))
                File.Copy(m_BundleTargetPath + "/" + abName, m_HotPath + "/" + abName);
        }
        //生成服务器Patch
        DirectoryInfo directory = new DirectoryInfo(m_HotPath);
        FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
        Pathces pathces = new Pathces();
        pathces.Version = int.Parse(hotCount);
        pathces.Files = new List<Patch>();
        for (int i = 0; i < files.Length; i++)
        {
            Patch patch = new Patch()
            {
                Md5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName),
                Name = files[i].Name,
                Size = files[i].Length / 1024.0f,
                Platform = EditorUserBuildSettings.activeBuildTarget.ToString(),
                Url = string.Format("{0}{1}/{2}/{3}", hotABPath, PlayerSettings.bundleVersion, hotCount, files[i].Name),
            };
            pathces.Files.Add(patch);
        }
        BinarySerializeOpt.Xmlserialize(m_HotPath + "/Patch.xml", pathces);
    }
    /// <summary>
    /// 删除指定路径下所有文件
    /// </summary>
    /// <param name="path"></param>
    private static void DeleteAllFile(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(m_HotPath);
        FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i].FullName);
        }
    }

    /// <summary>
    /// 保存改变的AB包名称
    /// </summary>
    /// <returns></returns>
    private static List<string> SaveChangeABName()
    {
        List<string> changeList = new List<string>();
        DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith(".manifest"))
            {
                string name = files[i].Name;
                string md5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName);
                if (!m_PackageMd5.ContainsKey(name))//如果字典中不包含，表示我们新打的AB
                    changeList.Add(name);
                else
                {
                    if(m_PackageMd5.TryGetValue(name,out ABMD5Base abmd5Base))
                    {
                        //如果MD5不一致，则表示该AB包被改变了
                        if (md5 != abmd5Base.MD5)
                            changeList.Add(name);
                    }
                }
            }
        }
        return changeList;
    }

    /// <summary>
    /// 写入MD5文件
    /// </summary>
    static void WriteABMD5()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        ABMD5 md5 = new ABMD5();
        md5.ABMD5List = new List<ABMD5Base>();
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".mate") && !files[i].Name.EndsWith(".manifest"))
            {
                ABMD5Base md5Base = new ABMD5Base
                {
                    Name = files[i].Name,
                    MD5=MD5Manager.Instance.BuildFileMd5(files[i].FullName),
                    Size=files[i].Length/1024f,
                };
                md5.ABMD5List.Add(md5Base);
            }
        }
        string ABMD5Path = Application.dataPath + "/Resources/ABMD5.bytes";//第一次AB  写入ABMD5.bytes   第二次AB
        BinarySerializeOpt.BinarySerlize(ABMD5Path, md5);
        if (!Directory.Exists(m_VersionMd5Path))
            Directory.CreateDirectory(m_VersionMd5Path) ;
        string targetPath = m_VersionMd5Path + "/ABMD5_" + PlayerSettings.bundleVersion + ".bytes";//存储不同平台的MD5
        if (File.Exists(targetPath))
            File.Delete(targetPath);
        File.Copy(ABMD5Path, targetPath);
    }
    /// <summary>
    /// 是否包含在已经有的AB包里，用来做AB包冗余剔除
    /// </summary>
    /// <param name="path">预制体路径</param>
    /// <returns></returns>
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i], "")[0] == '/')))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 设置AB包名称
    /// </summary>
    /// <param name="name">AB包名称</param>
    /// <param name="path">该包所在路径</param>
    static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件：" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }
    static void SetABName(string name,List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }
    /// <summary>
    /// 打AB包
    /// </summary>
    private static void BuildAssetBundle()
    {
        string[] allBunndles = AssetDatabase.GetAllAssetBundleNames();
        //key为全路径，value为包名，作用：用于写入AB配置文件时候，读取字典中的数据
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();

        for (int i = 0; i < allBunndles.Length; i++)
        {
            //根据AB包名获取路径
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBunndles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                if (allBundlePath[j].EndsWith(".cs"))
                    continue;
                resPathDic.Add(allBundlePath[j], allBunndles[i]);
            }
        }
        if (!Directory.Exists(m_BundleTargetPath))
        {
            Directory.CreateDirectory(m_BundleTargetPath);
        }
        DeleteAB();
        //生成自己的配置表
        WriteData(resPathDic);
        //打包
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("AssetBundle打包失败！");
        }
        else
        {
            Debug.LogError("AssetBundle打包完毕！");
        }
        //新增
        DelegateManifest();
        //加密AB包
        EncryptAB();
    }
    [MenuItem("Tools/加密AB包")]
    public static void EncryptAB()
    {
        AesEncryptOrDecrpty(true);
        Debug.LogError("加密完成");
    }
    [MenuItem("Tools/解密AB包")]
    public static void DecrptyAB()
    {
        AesEncryptOrDecrpty(false);
        Debug.LogError("解密完成");
    }
    private static void AesEncryptOrDecrpty(bool isEn)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith("meta") && !files[i].Name.EndsWith("manifest"))
            {
                if (isEn)
                {
                    AES.AESFileEncrypt(files[i].FullName, AesKey);
                }
                else
                {
                    AES.AESFileByteDecrypt(files[i].FullName, AesKey);
                }
            }
        }
    }
    /// <summary>
    /// 删除生成的 .manifest文件
    /// </summary>
    private static void DelegateManifest()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".manifest"))
            {
                File.Delete(files[i].FullName);
            }
        }
    }

    /// <summary>
    /// 生成自己的AB包配置文件
    /// </summary>
    /// <param name="resPathDic"></param>
    private static void WriteData(Dictionary<string, string> resPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<ABBase>();
        foreach (string path in resPathDic.Keys)
        {
            if (!ValidPath(path))
                continue;
            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = Crc32.GetCrc32(path);

            abBase.ABName = resPathDic[path];
            abBase.AsseetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.ABDependce = new List<string>();
            string[] resDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDependce.Length; i++)
            {
                string tempPath = resDependce[i];
                if (tempPath.Equals(path) || path.EndsWith(".cs"))//如果依赖项是本身或者脚本文件
                    continue;
                if (resPathDic.TryGetValue(tempPath, out string abName))
                {
                    if (abName.Equals(resPathDic[path]))
                        continue;
                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }
                }
            }
            config.ABList.Add(abBase);
        }

        //写入xml
        string xmlPath = Application.dataPath + "/AssetbundleConfig.xml";
        if (File.Exists(xmlPath)) File.Delete(xmlPath);
        using(FileStream fileStream=new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using(StreamWriter sw=new StreamWriter(fileStream, System.Text.Encoding.UTF8))
            {
                XmlSerializer xs = new XmlSerializer(config.GetType());
                xs.Serialize(sw, config);
            }
        }
        //写入二进制--->优化，省内存
        foreach (ABBase abBase in config.ABList)
        {
            abBase.Path = "";
        }
        using(FileStream fs=new FileStream(ABBYTEPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(0);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, config);
        }
        AssetDatabase.Refresh();
        SetABName("assetbundleconfig", ABBYTEPATH);
    }
    /// <summary>
    /// 判断有效路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFile.Count; i++)
        {
            if (path.Contains(m_ConfigFile[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 删除之前的AB包
    /// </summary>
    private static void DeleteAB()
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (ContainABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta") ||
                files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
            {
                continue;
            }
            else
            {
                Debug.LogError("此AB包已经被删或者改名了：" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
                if (File.Exists(files[i].FullName + ".manifest"))
                {
                    File.Delete(files[i].FullName + ".manifest");
                }
            }
        }
    }

    /// <summary>
    /// 遍历文件夹里的文件名与设置的所有AB包进行检查更新判断
    /// </summary>
    /// <param name="name"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    private static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name.Equals(strs[i]))
            {
                return true;
            }
        }
        return false;
    }
}