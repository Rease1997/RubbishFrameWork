using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// 所有资源信息类
/// </summary>
[System.Serializable]
public class ABMD5
{
    [XmlElement("ABMD5List")]
    public List<ABMD5Base> ABMD5List { get; set; }
}
[System.Serializable]
public class ABMD5Base
{
    /// <summary>
    /// 资源名称（AB包名）
    /// </summary>
    [XmlAttribute("Name")]
    public string Name { get; set; }
    /// <summary>
    /// 资源MD5
    /// </summary>
    [XmlAttribute("MD5")]
    public string MD5 { get; set; }
    /// <summary>
    /// 资源大小
    /// </summary>
    [XmlAttribute("Size")]
    public float Size { get; set; }
}