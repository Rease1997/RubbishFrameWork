using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class SkillData : ExcelBase
{
    [XmlElement("AllSkillList")]
    public List<SkillBase> AllSkillList { get; set; }
}
public enum SkillType
{
    NormalSkill = 1, AttackSkill, SuperSkill
}
public enum HitType
{
    ScaleEnemy, SelectedEnemy, SelectedFriend, ScaleFriend, Self
}
public enum AttRangeType
{
    None, Fan,Circle
}
[System.Serializable]
public class SkillBase
{
    [XmlAttribute("Id")]
    public int Id { get; set; }

    [XmlAttribute("SkillName")]
    public string SkillName { get; set; }
    [XmlAttribute("SkillIcon")]
    public string SkillIcon { get; set; }

    [XmlAttribute("SkillType")]
    public SkillType SkillType { get; set; }

    [XmlAttribute("Skillpriority")]
    public int Skillpriority { get; set; }

    [XmlAttribute("Skillenergy")]
    public int Skillenergy { get; set; }

    [XmlAttribute("AttDis")]
    public float AttDis { get; set; }
    [XmlAttribute("HitType")]
    public HitType HitType { get; set; }
    [XmlAttribute("AttRangeType")]
    public AttRangeType AttRangeType { get; set; }
    [XmlAttribute("DamageNum")]
    public int DamageNum { get; set; }

    [XmlAttribute("AttRange")]
    public float AttRange { get; set; }

    [XmlAttribute("Angle")]
    public int Angle { get; set; }

    [XmlAttribute("BpNeed")]
    public int BpNeed { get; set; }

    [XmlAttribute("Guard")]
    public float Guard { get; set; }

    [XmlAttribute("Para")]
    public bool Para { get; set; }

    [XmlAttribute("Para1")]
    public float Para1 { get; set; }

    [XmlAttribute("Para2")]
    public float Para2 { get; set; }

    [XmlAttribute("Para3")]
    public float Para3 { get; set; }

    [XmlAttribute("Para4")]
    public float Para4 { get; set; }

    [XmlAttribute("Para5")]
    public float Para5 { get; set; }

    [XmlAttribute("SkillHitCount")]
    public int SkillHitCount { get; set; }
    [XmlAttribute("Damagecoefficient1")]
    public int Damagecoefficient1{ get; set; }
    [XmlAttribute("Damagecoefficient2")]
    public int Damagecoefficient2 { get; set; }
    [XmlAttribute("Damagecoefficient3")]
    public int Damagecoefficient3 { get; set; }
    [XmlElement("DamegeTypeList")]
    public List<string> DamegeTypeList { get; set; }
    [XmlElement("HitedEffect")]
    public List<int> HitedEffect { get; set; }
    [XmlElement("BuffList")]
    public List<string> BuffList { get; set; }
    [XmlAttribute("Desc")]
    public int Desc { get; set; }

}