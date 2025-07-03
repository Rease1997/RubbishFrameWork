using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/*
 * Author:W
 * Json文件解析
 */
namespace W.GameFrameWork.ExcelTool
{
	public class JsonHelper
	{
		/// <summary>
        /// 读取Json文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
		public static List<T> ReadJsonFile<T>(string filePath)
		{
			FileStream fileStream = File.OpenRead(filePath);
			StreamReader streamReader = new StreamReader(fileStream);
			List<T> objList = new List<T>();

			string jsonStr = streamReader.ReadToEnd();
			//objList = JsonConvert.DeserializeObject<List<T>>(jsonStr);

			streamReader.Close();
			streamReader.Dispose();
			fileStream.Close();
			fileStream.Dispose();

			return objList;
		}
	}
}

