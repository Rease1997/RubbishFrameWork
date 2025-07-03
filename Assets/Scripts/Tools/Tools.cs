using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 公共工具类
/// </summary>
public static class Tools
{
    public static void ExitGame()
    {
        RFrameWork.Instance.QuitTheGame();
    }
    /// <summary>                        
    /// thi Transform root给Transform添加扩展方法，扩展方法为在根节点查找name的子物体
    /// 查找子物体
    /// </summary>
    /// <param name="root"></需要查找物体的根节点>
    /// <param name="name"></需要查找子物体的名称>
    /// <returns></returns>
    public static Transform FindRecursively(this Transform root, string name)
    {

        if (root.name == name)
        {
            return root;
        }
        //遍历根节点下的所有子物体
        foreach (Transform child in root)
        {
            //递归查找子物体
            Transform t = FindRecursively(child, name);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }

    public static T FindBehaviour<T>(this Transform root, string name) where T : MonoBehaviour
    {
        Transform child = FindRecursively(root, name);

        if (child == null)
        {
            return null;
        }

        T temp = child.GetComponent<T>();
        if (temp == null)
        {
            Debug.LogError(name + " is not has component ");
        }

        return temp;
    }


    /// <summary>
    /// 字符串转整型;
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int Str2Int(string str)
    {
        if (IsNumber(str))
        {
            return string.IsNullOrEmpty(str) == true ? 0 : Convert.ToInt32(str);
        }
        return 0;

    }
    /// <summary>
    /// 常量A是否全是数字
    /// </summary>
    public static string A = "^[0-9]+$";
    /// <summary>
    /// 利用正则表达式来判断字符串是否全是数字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static bool IsNumber(string str)
    {
        Regex regex = new Regex(A);
        return regex.IsMatch(str) == true ? true : false;
    }
    /// <summary>
    /// 字符串转成float;
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float Str2Float(string str)
    {
        return string.IsNullOrEmpty(str) == true ? 0 : Convert.ToSingle(str);
    }
    /// <summary>
    /// 字符串转成bool;
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool Str2Boolean(string str)
    {
        if (str == "1" || str.ToLower() == "true" || str.ToLower() == "yes")
        {
            return true;
        }

        return false;
    }
}
