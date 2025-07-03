using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class AssetBundleConfig 
{
    [XmlElement("ABList")]
    public List<ABBase> ABList { get; set; }

}
[System.Serializable] 
public class ABBase
{
    [XmlAttribute("Path")]
    public string Path { get; set; }
    /// <summary>
    /// 类似于MD5码，MD5相对Crc更加精确，不过这里用CRC做唯一标识也够用
    /// </summary>
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }
    [XmlAttribute("ABName")]
    public string ABName { get; set; }
    [XmlAttribute("AsseetName")]
    public string AsseetName { get; set; }
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }
}
