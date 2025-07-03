using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TestSSS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        Debug.LogError((int)time.TotalHours+":"+(int)time.TotalMinutes+":"+ (int)time.TotalSeconds);
    }
    /// <summary>
    /// 加载预制体
    /// </summary>
    void Load()
    {
        AssetBundle config = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAssets = config.LoadAsset<TextAsset>("AssetBundleConfig");
        MemoryStream str = new MemoryStream(textAssets.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig abconfig = (AssetBundleConfig)bf.Deserialize(str);
        str.Close();
        string path = "Assets/GameData/Prefabs/Model/Attack.prefab";
        uint crc = Crc32.GetCrc32(path);
        ABBase abBase = null;
        for (int i = 0; i < abconfig.ABList.Count; i++)
        {
            if (abconfig.ABList[i].Crc == crc)
            {
                abBase = abconfig.ABList[i];
            }
        }
        for (int i = 0; i < abBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);
        }
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
        Instantiate(assetBundle.LoadAsset<GameObject>(abBase.AsseetName));

    }
}
