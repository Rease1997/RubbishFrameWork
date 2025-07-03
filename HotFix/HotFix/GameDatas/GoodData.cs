using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix
{
    class GoodData
	{
		/*
		"ID" : 1001,
		"Name" : "装备1",
		"Des" : ""装备1"的详细物品描述",
		"Type" : 1,
		"ImgPath" : "Assets/GameData/ScriptAtlas/Atlas/UI1.0/Icon.png",
		"ImgName" : "00430"
		*/
		public int ID;//物品ID
		public string Name;//名称
		public string Des;//介绍
		public int Type;//类型
		public string ImgPath;//所在图集路径
		public string ImgName;//图集中当前这个图片的名字

        public GoodData(int iD, string name, string des, int type, string imgPath, string imgName)
        {
            ID = iD;
            Name = name;
            Des = des;
            Type = type;
            ImgPath = imgPath;
            ImgName = imgName;
        }
    }
}
