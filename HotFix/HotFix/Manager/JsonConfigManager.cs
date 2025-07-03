using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix
{
    class JsonConfigManager
    {
        private static List<GoodData> allGoodDatas = null;

        private static void AnalyzeJson(string jsonName,Action<JsonData> callback)
        {
            string jsonPath = "Assets/GameData/Data/Json/" + jsonName + ".json";
            Debug.Log("json文件路径：" + jsonPath);
            string jsonStr = ResourceManager.Instance.LoadResources<TextAsset>(jsonPath).ToString();
            Debug.Log("json文件内容：" + jsonStr);
            JsonData temp = JsonMapper.ToObject(jsonStr);
            callback(temp);
        }

        public static List<GoodData> GetGoodData()
        {
            if(allGoodDatas == null)
            {
                allGoodDatas = new List<GoodData>();
                AnalyzeJson("ShopData", (JsonData temp) =>
                {
                    foreach (JsonData item in temp["data"])
                    {
                        Debug.Log("AllGoodData itemStr:" + item.ToJson());
                        GoodData data = new GoodData(int.Parse(item["ID"].ToString()),
                                                    item["Name"].ToString(),
                                                    item["Des"].ToString(),
                                                    int.Parse(item["Type"].ToString()),
                                                    item["ImgPath"].ToString(),
                                                    item["ImgName"].ToString());
                        allGoodDatas.Add(data);
                        Debug.Log("AllGoodDatas.count:" + allGoodDatas.Count);
                    }
                });
            }
            return allGoodDatas;
        } 
    }
}
