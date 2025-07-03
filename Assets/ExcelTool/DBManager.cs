using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
/*
 * Author:W
 * 配置表管理器
 */
namespace W.GameFrameWork.ExcelTool
{
	public class DBManager
	{
		private static DBManager _instance;

		public static DBManager Instance
		{
			get
			{
				if (_instance == null)
					_instance = new DBManager();

				return _instance;
			}
		}

		/// <summary>
		/// 项目中所有加载的json配置表
		/// </summary>
		private Dictionary<string, List<ExcelItem>> jsonTableDict = new Dictionary<string, List<ExcelItem>>();

		public DBManager()
		{
			if (jsonTableDict == null)
				jsonTableDict = new Dictionary<string, List<ExcelItem>>();

			jsonTableDict.Clear();
		}

        #region Json表的加载、卸载以及表数据的获取
        /// <summary>
        /// 加载一Json文件中所有数据
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadJsonTable<T>(string fileName) where T:ExcelItem
		{
			if (jsonTableDict.ContainsKey(fileName))
			{
				Debug.Log("Json表：" + fileName + "已经加载，无需重复加载！");
				return;
			}

			string fileFullName = fileName + ".json";
			string filePath = PathUtil.GetJsonDir(fileFullName) + "/"+ fileFullName;
			List<T> excelItems =  JsonHelper.ReadJsonFile<T>(filePath);
			List<ExcelItem> commonItems = new List<ExcelItem>();
			for (int i = 0; i < excelItems.Count; i++)
			{
				commonItems.Add((ExcelItem)excelItems[i]);
			}

			jsonTableDict.Add(fileName,commonItems);

			//Debug.Log("wlq====>json表加载完毕");
		}

		/// <summary>
		/// 获取某张Json表的所有数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public List<T> GetJsonTable<T>(string fileName) where T : ExcelItem
		{
			List<ExcelItem> commonItems = null;
			List<T> resItems = null;
			if (jsonTableDict.TryGetValue(fileName, out commonItems))
			{
				resItems = new List<T>();

				for (int i = 0; i < commonItems.Count; i++)
				{
					resItems.Add((T)commonItems[i]);
				}
			}

			if (resItems == null)
				Debug.LogError("Json表 " + fileName + "未加载，不能获取该表的所有数据");

			return resItems;
		}


		/// <summary>
		/// 获取某张Json表中某行数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public T GetJsonTableRowData<T>(string fileName, string id) where T : ExcelItem
		{
			T resItem = null;

			List<ExcelItem> commonItems = null;
			if (jsonTableDict.TryGetValue(fileName, out commonItems))
			{
				ExcelItem excelItem = commonItems.Find(t => t.ID == id);

				if (excelItem != null)
				{
					resItem = (T)excelItem;
				}
			}

			if (resItem == null)
				Debug.LogError("Json表 " + fileName + " ID=" + id + " 不存在 原因表未加载或者没有该ID的行数据");

			return resItem;
		}

		/// <summary>
		/// 卸载某张Json表
		/// </summary>
		/// <param name="fileName"></param>
		public void UnLoadJsonTable(string fileName)
		{
			if (jsonTableDict.ContainsKey(fileName))
			{
				jsonTableDict.Remove(fileName);
			}
		}
        #endregion
    }
}

