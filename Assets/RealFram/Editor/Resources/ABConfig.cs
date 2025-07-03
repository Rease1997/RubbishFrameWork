using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ABConfig",menuName ="CreatABConfig",order =0)]
public class ABConfig : ScriptableObject
{
    /// <summary>
    /// 单个文件所在文件夹路径，会遍历这个文件夹下面所有prefab，所有的prefab的名字不能重复，必须保证名字的唯一性
    /// </summary>
    public List<string> m_AllPrefabPath = new List<string>();
    /// <summary>
    /// 文件路径
    /// </summary>
    public List<FileDirName> m_AllFileDirAB = new List<FileDirName>();

    [System.Serializable]
    public struct FileDirName
    {
        public string ABName;
        public string Path;
    }
}
