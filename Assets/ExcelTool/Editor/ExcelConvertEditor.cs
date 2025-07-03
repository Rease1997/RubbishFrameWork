using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Data;
using Excel;
using System.Text;
/*
 * Author:W
 * Excel配置表转换工具
 * 1）转Json
 * 2）转XML
 * 3）转Lua
 * 4) 转CSV
 * 5）转C#解析实体类
 */
namespace W.GameFrameWork.ExcelTool
{
	public class ExcelConvertEditor : EditorWindow
	{
		private static ExcelConvertEditor excelEditorWIn;

		/// <summary>
		/// Excel配置表路径
		/// </summary>
		private static string excelPath;
		private static string jsonPath;
		private static string cSharpPath;


		[MenuItem("Excel配置表/转换")]
		private static void ShowExcelEditorWin()
		{
			excelEditorWIn = EditorWindow.GetWindow<ExcelConvertEditor>();
			excelEditorWIn.position = new Rect(Screen.width/2,Screen.height/2,400,200);
			excelEditorWIn.titleContent = new GUIContent("Excel转换工具");
			excelEditorWIn.Show();
	
			//AssetDatabase.Refresh();
		}

		void Awake()
		{
			excelPath = PlayerPrefs.GetString("excelPath", "");

			jsonPath = PathUtil.GetJsonDir();
			if (!Directory.Exists(jsonPath))
				Directory.CreateDirectory(jsonPath);

			cSharpPath = Application.dataPath + "/../HotFix/HotFix/GameDataCScripts";
			if (!Directory.Exists(cSharpPath))
				Directory.CreateDirectory(cSharpPath);
		}
	

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			excelPath = Application.dataPath+ "/GameData/Data/Excel/";

			if (GUILayout.Button("转换Json", GUILayout.Width(150)))
			{				
				ConvertToJsons();
			}
			if (GUILayout.Button("生成C#实体解析类", GUILayout.Width(150)))
			{				
				GenerateCSharp();
			}

			EditorGUILayout.EndVertical();
		}
		

		/// <summary>
		/// 获取Excel表目录下的所有excel文件
		/// </summary>
		/// <returns></returns>
		private static string[] GetAllExcels()
		{
			if (string.IsNullOrEmpty(excelPath))
			{
				Debug.LogError("Excel配置表路径不能为空，请指定。");
				return null;
			}

			return Directory.GetFiles(excelPath);
		}


		/// <summary>
		/// 转换成Json数据
		/// </summary>
		private static void ConvertToJsons()
		{
			string[] allExcels = GetAllExcels();
			if (allExcels == null) return;

			for (int i = 0; i < allExcels.Length; i++)
			{				
				string filePath = allExcels[i].Replace(@"\",@"/"); //Assets\GameData\Data\Excel\ShopData.xlsx --> Assets/GameData/Data/Excel/ShopData.xlsx
				string fileName = new FileInfo(filePath).Name.Replace(".xlsx",""); //FileInfo.Name 获取到的是ShopData.xlsx  再通过Replace将 .xlsx 替换掉 变成ShopData
				string fileOutPath = jsonPath + "/"+fileName+".json";

				Debug.Log("fileName="+ fileName+"  filePath="+filePath+"  fileOutPath="+fileOutPath);
				ConvertToJson(fileName,filePath,fileOutPath);
			}
		}

		/// <summary>
		/// 将Excel文件转换成Json文件
		/// </summary>
		/// <param name="fileName">文件名</param>
		/// <param name="fileFromPath">excel文件路径</param>
		/// <param name="fileOutPath">json文件输出路径</param>
		private static void ConvertToJson(string fileName,string fileFromPath,string fileOutPath)
		{
			//读取Excel表中数据
			FileStream fileStream = File.OpenRead(fileFromPath);
			IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
			DataSet dataSet = excelDataReader.AsDataSet();

			//检查是否有表
			if (dataSet.Tables.Count < 1)
				return;

			DataTable sheet = dataSet.Tables[0];

			//检查表中是否有数据
			if (sheet.Rows.Count < 1)
				return;

			int rowCount = sheet.Rows.Count;
			int colCount = sheet.Columns.Count;

			//记录整张表的数据
			List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
			for (int i = 3; i < rowCount; i++)
			{
				Dictionary<string, object> rowData = new Dictionary<string, object>();
				for (int j = 0; j < colCount; j++)
				{
					//读取第1行数据作为表头字段
					string field = sheet.Rows[0][j].ToString()+":"+sheet.Rows[1][j].ToString();
					rowData[field] = sheet.Rows[i][j];
				}

				table.Add(rowData);
			}

			//生成Json字符串【自己写 NotDo】
			//string jsonStr = JsonConvert.SerializeObject(table,Formatting.Indented);
			string head = @"""data"": ";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{\n\t"+head+"\n[");
			stringBuilder.Append("\r\n");
			int dataCount = 0;
			
			foreach (Dictionary<string, object> dic in table)
			{
				dataCount++;

				stringBuilder.Append("\t{");
				stringBuilder.Append("\r\n");

				int fieldCount = 0;
				foreach (string key in dic.Keys)
				{
					fieldCount++;

					string[] keyArr = key.Split(':');
					string fieldName = keyArr[0];
					string fieldType = keyArr[1];

					//数组类型
					if (fieldType.EndsWith("_array"))
					{
						if (fieldType.StartsWith("string"))
						{
							string[] valueArr = dic[key].ToString().Split(';');
							StringBuilder stringBuilder1 = new StringBuilder();

							for (int i = 0; i < valueArr.Length; i++)
							{
								stringBuilder1.Append("\"");
								stringBuilder1.Append(valueArr[i]);
								stringBuilder1.Append("\"");

								if (i != valueArr.Length - 1)
									stringBuilder1.Append(",");
							}

							stringBuilder.Append("\t\t\"" + fieldName + "\" : [" + stringBuilder1.ToString() + "]");
						}
						else
						{
							string value = dic[key].ToString().Replace(';', ',');
							stringBuilder.Append("\t\t\"" + fieldName + "\" : [" + value + "]");

						}						

					} //基本数据类型：number/string
					else
					{
						if (fieldType.StartsWith("string"))
						{
							stringBuilder.Append(string.Format("\t\t\"{0}\" : \"{1}\"", fieldName, dic[key]));
						}
						else
						{
							stringBuilder.Append(string.Format("\t\t\"{0}\" : {1}", fieldName, dic[key]));
						}
					}

					if (fieldCount != dic.Keys.Count)
						stringBuilder.Append(",");

					stringBuilder.Append("\r\n");
				}


				stringBuilder.Append("\t}");
				if(dataCount!=table.Count)
					stringBuilder.Append(",");

				stringBuilder.Append("\r\n");
			}
		
			stringBuilder.Append("]\n\r}");


			//检查旧的json文件是否存在，存在则删除
			if (File.Exists(fileOutPath))
				File.Delete(fileOutPath);

			//写入文件
			FileStream fileStream1 = new FileStream(fileOutPath, FileMode.Create, FileAccess.Write);
			TextWriter textWriter = new StreamWriter(fileStream1,Encoding.UTF8);
			textWriter.Write(stringBuilder.ToString());
			textWriter.Close();
			textWriter.Dispose();

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 生成解析C#类
		/// </summary>
		private static void GenerateCSharp()
		{
			string[] allExcels = GetAllExcels();
			if (allExcels == null) return;

			for (int i = 0; i < allExcels.Length; i++)
			{
				string filePath = allExcels[i].Replace(@"\", @"/");
				string fileName = new FileInfo(filePath).Name.Replace(".xlsx", "");
				string fileOutPath = cSharpPath + "/" + fileName + ".cs";

				Debug.Log("fileName=" + fileName + "  filePath=" + filePath + "  fileOutPath=" + fileOutPath);
				GenerateCSharpFile(fileName, filePath, fileOutPath);
			}
		}

	    /// <summary>
		/// 生成C#实体类脚本文件
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="fileFromPath"></param>
		/// <param name="fileOutPath"></param>
		private static void GenerateCSharpFile(string fileName, string fileFromPath, string fileOutPath)
		{
			//读取Excel表中数据
			FileStream fileStream = File.OpenRead(fileFromPath);
			IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
			DataSet dataSet = excelDataReader.AsDataSet();

			//检查是否有表
			if (dataSet.Tables.Count < 1)
				return;

			DataTable sheet = dataSet.Tables[0];

			//检查表中是否有数据
			if (sheet.Rows.Count < 1)
				return;

			int rowCount = sheet.Rows.Count;
			int colCount = sheet.Columns.Count;


			//加载生成C#解析类需要的数据
			string className = fileName;
			List<Property> Properties = new List<Property>();
			for (int j = 0; j < colCount; j++)
			{
				Property property = new Property();
				property.Name = sheet.Rows[0][j].ToString();
				property.Type = sheet.Rows[1][j].ToString();
				property.Desc = sheet.Rows[2][j].ToString();

				Properties.Add(property);
			}

			//生成C#脚本文件
			StringBuilder stringBuilder = new StringBuilder();
			//引用空间
			stringBuilder.Append("using System.Collections;\r\n");
			stringBuilder.Append("using System.Collections.Generic;\r\n");
			stringBuilder.Append("using W.GameFrameWork.ExcelTool;\r\n");
			//类描述
			stringBuilder.Append("/*\r\n* Author:W\r\n* Excel表转换生成\r\n* ");
			stringBuilder.Append(className);
			stringBuilder.Append("\r\n*/");

			stringBuilder.Append("\r\n");

			//命名空间
			stringBuilder.Append("namespace HotFix\r\n{\r\n");
			//序列化属性
			stringBuilder.Append("\t[System.Serializable]\r\n");
			//类
			stringBuilder.Append("\tpublic class ");
			stringBuilder.Append(className+"ParSer");
			stringBuilder.Append("\r\n\t{\r\n");
			stringBuilder.Append("\t\tpublic List<" + className + "> data = new List<" + className + ">();\n");
			stringBuilder.Append("\t\tpublic List<" + className + "> Data\n");
			stringBuilder.Append("\t\t{\r\n");
			stringBuilder.Append("\t\t\tget\r\n");
			stringBuilder.Append("\t\t\t{\r\n");
			stringBuilder.Append("\t\t\t\treturn data;\r\n");
			stringBuilder.Append("\t\t\t} \n");
			stringBuilder.Append("\t\t}");
			stringBuilder.Append("\r\t} \n");

			//序列化属性
			stringBuilder.Append("\t[System.Serializable]\r\n");
			//类
			stringBuilder.Append("\tpublic class ");
			stringBuilder.Append(className);
			stringBuilder.Append(":ExcelItem");
			stringBuilder.Append("\r\n\t{\r\n");

			//属性生成
			for (int i = 0; i < Properties.Count; i++)
			{
				//属性描述
				stringBuilder.Append("\t/// <summary>\r\n");
				stringBuilder.Append("\t/// ");
				stringBuilder.Append(Properties[i].Desc);
				stringBuilder.Append("\r\n\t/// <summary>\r\n");
				//属性声明
				stringBuilder.Append("\tpublic ");
				string type = GetType(Properties[i].Type);
				stringBuilder.Append(type);
				stringBuilder.Append(" ");
				stringBuilder.Append(Properties[i].Name);
				stringBuilder.Append(";\r\n");
			}

			stringBuilder.Append("\r\n\t}");


			stringBuilder.Append("\r\n}");


			//检查是否存在旧的文件，存在则删除
			if (File.Exists(fileOutPath))
				File.Delete(fileOutPath);

			//写入文件
			FileStream fileStream1 = new FileStream(fileOutPath, FileMode.Create, FileAccess.Write);
			TextWriter textWriter = new StreamWriter(fileStream1, Encoding.UTF8);
			textWriter.Write(stringBuilder.ToString());
			textWriter.Close();
			textWriter.Dispose();

			AssetDatabase.Refresh();


		}

		/// <summary>
		/// 解析字段数据类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static string GetType(string type)
		{
			string value = null;
			if (type.EndsWith("_array"))
			{
				if (type.StartsWith("int"))
				{
					value = "int[]";
				}
				else if (type.StartsWith("float"))
				{
					value = "float[]";
				}
				else if (type.StartsWith("string"))
				{
					value = "string[]";
				}
			}
			else
			{
				if (type.StartsWith("int"))
				{
					value = "int";
				}
				else if (type.StartsWith("float"))
				{
					value = "float";
				}
				else if (type.StartsWith("string"))
				{
					value = "string";
				}
			}

			return value;
		}

		/// <summary>
		/// 属性字段描述
		/// </summary>	
		public class Property
		{
			/// <summary>
			/// 属性名称
			/// </summary>
			public string Name;
			/// <summary>
			/// 数据类型
			/// </summary>
			public string Type;
			/// <summary>
			/// 描述
			/// </summary>
			public string Desc;
		}
	}

}
