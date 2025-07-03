using System.Collections.Generic;
using System.Xml.Serialization;

[System.Serializable]
public class ServerInfo 
{
    /// <summary>
    /// 版本信息
    /// 有可能有其他渠道的版本包
    /// </summary>
    [XmlElement("GameVersion")]
    public VersionInfo[] GameVersion;
}
/// <summary>
/// 当前游戏版本对应的所有补丁
/// 可以拓展包含其他渠道的信息
/// </summary>
[System.Serializable]
public class VersionInfo
{
    /// <summary>
    /// 当前游戏版本号
    /// </summary>
     [XmlAttribute]
    public string Version;
    /// <summary>
    /// 总的热更补丁
    /// </summary>
    [XmlElement("Pathces")]
    public Pathces[] pathces;
}
/// <summary>
/// 所有补丁列表
/// </summary>
[System.Serializable]
public class Pathces
{
    /// <summary>
    /// 当前热更版本热更次数
    /// </summary>
    [XmlAttribute]
    public int Version;
    /// <summary>
    /// 热更描述
    /// </summary>
    [XmlAttribute]
    public string  Des;
    /// <summary>
    /// 当前热更补丁包所包含的文件
    /// </summary>
    [XmlElement]
    public List<Patch> Files;
}
/// <summary>
/// 单个热更文件信息
/// </summary>
[System.Serializable]
public class Patch
{
    /// <summary>
    /// 当前热更文件名称
    /// </summary>
    [XmlAttribute]
    public string Name;

    /// <summary>
    /// 当前热更文件需要下载的地址
    /// </summary>
    [XmlAttribute]
    public string Url;

    /// <summary>
    /// 当前热更文件对应的平台
    /// </summary>
    [XmlAttribute]
    public string Platform;

    /// <summary>
    /// 当前热更文件的Md5
    /// </summary>
    [XmlAttribute]
    public string Md5;

    /// <summary>
    /// 当前热更文件大小
    /// </summary>
    [XmlAttribute]
    public float Size;
}