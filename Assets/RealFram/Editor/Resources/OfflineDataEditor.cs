using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class OfflineDataEditor 
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreatOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + objects[i] + "......", 1.0f / objects.Length * i);
            CreateOfflineData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void CreateOfflineData(GameObject obj)
    {
        OfflineData offlineData = obj.GetComponent<OfflineData>();
        if (offlineData == null)
        {
            offlineData = obj.AddComponent<OfflineData>();
        }
        offlineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.LogError("修改了" + obj.name + " prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreatUIData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + objects[i] + "......", 1.0f / objects.Length * i);
            CreateUIData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void CreateUIData(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");
        UIOfflineData uiData = obj.GetComponent<UIOfflineData>();
        if (uiData == null)
        {
            uiData = obj.AddComponent<UIOfflineData>();
        }
        uiData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.LogError("修改了" + obj.name + "UI prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/还原UI预制体数据")]
    public static  void ResUIPrefab()
    {
        GameObject obj = Selection.activeGameObject;
        UIOfflineData uiData = obj.GetComponent<UIOfflineData>();
        uiData.ResetProp();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/还原所有预制体数据")]
    public static void ResAllUIPrefab()
    {
        GameObject[] obj = Selection.gameObjects;
        for (int i = 0; i < obj.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + obj[i] + "......", 1.0f / obj.Length * i);
            UIOfflineData uiData = obj[i].GetComponent<UIOfflineData>();
            if (uiData != null)
                uiData.ResetProp();
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("离线数据/生成所有UI prefab离线数据")]
    public static void AllCreatUIData()
    {
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/UGUI" });
        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("添加离线数据", "正在扫描路径:" + prefabPath + "......", 1.0f / allStr.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null)
                continue;
            CreateUIData(obj);
        }

        Debug.Log("UI离线数据全部生成完毕");
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/生成特效离线数据")]
    public static void AssetCreateEffectData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在扫描路径:" + objects[i] + "......", 1.0f / objects.Length * i);
            CreateEffectData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/生成所有特效 prefab离线数据")]
    public static void AllCreateEffectData()
    {
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/Effect" });
        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在扫描路径:" + prefabPath + "......", 1.0f / prefabPath.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null)
                continue;
            CreateEffectData(obj);
        }
        Debug.LogError("特效离线数据全部生成完毕！");
        EditorUtility.ClearProgressBar();
    }

    private static void CreateEffectData(GameObject obj)
    {
        EffectOfflineData effectData = obj.GetComponent<EffectOfflineData>();
        if (effectData == null)
        {
            effectData = obj.AddComponent<EffectOfflineData>();
        }
        effectData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.name + "特效 prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/还原特效预制体数据")]
    public static void ResEffectPrefab()
    {
        GameObject obj = Selection.activeGameObject;
        EffectOfflineData effData = obj.GetComponent<EffectOfflineData>();
        effData.ResetProp();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/还原所有特效预制体数据")]
    public static void ResAllEffectPrefab()
    {
        GameObject[] obj = Selection.gameObjects;
        for (int i = 0; i < obj.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + obj[i] + "......", 1.0f / obj.Length * i);
            EffectOfflineData effData = obj[i].GetComponent<EffectOfflineData>();
            if (effData != null)
                effData.ResetProp();
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
}
