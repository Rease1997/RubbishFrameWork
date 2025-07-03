using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效
/// </summary>
public class EffectOfflineData : OfflineData
{
    public ParticleSystem[] m_Particale;
    public TrailRenderer[] m_TrailRe;
    public override void ResetProp()
    {
        base.ResetProp();
        foreach (ParticleSystem particle in m_Particale)
        {
            particle.Clear(true);
            particle.Play();
        }
        foreach (TrailRenderer trail in m_TrailRe)
        {
            trail.Clear();
        }
    }
    public override void BindData()
    {
        base.BindData();
        m_Particale = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        m_TrailRe = gameObject.GetComponentsInChildren<TrailRenderer>(true);
    }
    
}
