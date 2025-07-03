using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Author:W
 * 路径工具
 */
namespace W.GameFrameWork.ExcelTool
{
	public class PathUtil 
	{
		/// <summary>
		/// 获取CSV配置文件存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetCSVDir(string fileName = null)
		{
			string perPath = Application.persistentDataPath + "/CsvFiles";
			string streamPath = Application.streamingAssetsPath + "/CsvFiles";

#if UNITY_EDITOR
			return streamPath;
#else
			CheckFileVersion(perPath,streamPath,fileName);
 		    return perPath;			
#endif
		}

		/// <summary>
		/// 获取xml配置文件存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetXmlDir(string fileName = null)
		{
			string perPath = Application.persistentDataPath + "/XmlFiles";
			string streamPath = Application.streamingAssetsPath + "/XmlFiles";

#if UNITY_EDITOR
			return streamPath;
#else
			CheckFileVersion(perPath,streamPath,fileName);
 		    return perPath;			
#endif

		}

		/// <summary>
		/// 获取json配置文件存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetJsonDir(string fileName = null)
		{
			return Application.dataPath + "/GameData/Data/Json";
		}

		/// <summary>
		/// 获取lua配置文件存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetLuaDir(string fileName = null)
		{
			string perPath = Application.persistentDataPath + "/LuaFiles";
			string streamPath = Application.streamingAssetsPath + "/LuaFiles";

#if UNITY_EDITOR
			return streamPath;
#else
			CheckFileVersion(perPath,streamPath,fileName);
 		    return perPath;			
#endif
		}

		/// <summary>
		/// 获取sql数据库文件存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetSqlDir(string fileName = null)
		{
			string perPath = Application.persistentDataPath + "/SQLFile";
			string streamPath = Application.streamingAssetsPath + "/SQLFile";

#if UNITY_EDITOR
			return streamPath;
#else
            CheckFileVersion(perPath,streamPath,fileName);
 		    return perPath;			
#endif
		}


		/// <summary>
		/// 获取配置表生成的对应C#解析实体类存储目录
		/// </summary>
		/// <returns></returns>
		public static string GetGenerateCSharpDir()
		{
			return Application.dataPath + "/../HotFix/HotFix/GameDatas";
		}

		/// <summary>
		/// 【移动端】
		/// 检查资源沙盒目录下是否有该配置表文件，如果没有则把StreamingAssets文件夹下Copy一份过去；
		/// 如果有，继续检查沙盒下的文件是否比StreamingAsset文件夹下的旧，如果是旧版的，则也需要把
		/// StreamingAssets文件夹下Copy一份过去；
		/// 【目的：Copy一份到沙盒文件目录下（可读写）的原因，是针对支持热更的项目；如果是单机的项目，无需Copy
		/// 直接读取StreamingAsset文件夹下（只可读，另外值得注意的是，Android端只能使用www来读取）配置表即可】
		/// </summary>
		/// <param name="persisPath"></param>
		/// <param name="streamPath"></param>
		public static void CheckFileVersion(string persisPath, string streamPath,string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

            if (!System.IO.Directory.Exists(persisPath))
                System.IO.Directory.CreateDirectory(persisPath);

            persisPath += "/" + fileName;
			streamPath += "/" + fileName;

			//Debug.Log("wlq====>streamPath:" + streamPath);

			if (!System.IO.File.Exists(persisPath) || (System.IO.File.GetLastWriteTimeUtc(streamPath) > System.IO.File.GetLastWriteTimeUtc(persisPath)))
			{
				if (Application.platform == RuntimePlatform.Android)
				{
					// Android平台下的StreamingAsset文件下资源需要www访问	
					WWW www = new WWW(streamPath);
					while (!www.isDone) {; }

					if (String.IsNullOrEmpty(www.error))
					{
						System.IO.File.WriteAllBytes(persisPath, www.bytes);
					}
					else
					{
						Debug.LogError("wlq====>www.error:"+www.error);
					}

					//Debug.Log("wlq====>copy streamPath:" + streamPath + " to " + persisPath);
				}
				else
				{
					// Mac, Windows, Iphone					
					if (System.IO.File.Exists(streamPath))
					{
						System.IO.File.Copy(streamPath, persisPath, true);
					}
				}

			}
		}
	}
}

