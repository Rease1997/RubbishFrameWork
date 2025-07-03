using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

[System.Serializable]
public class MD5Manager : Singleton<MD5Manager>
{
    /// <summary>
    /// 存储MD5
    /// </summary>
    /// <param name="savePath">保存文件的路径</param>
    public void SaveMd5(string savePath)
    {
        string md5 = BuildFileMd5(savePath);
        string name = savePath + "_Md5.dat" ;
        if (File.Exists(name))
            File.Delete(name);
        using(StreamWriter sw=new StreamWriter(name, false, System.Text.Encoding.UTF8))
        {
            if (sw != null)
            {
                sw.Write(md5);
                //标示文件的写入只要写入一个字符接写入文件中，而不用等到该文件流关闭后一次性写入
                sw.Flush();
            }
        }
    }
    /// <summary>
    /// 根据路径获取Md5文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetMd5(string path)
    {
        string name = path + "_md5.dat";
        try
        {
            using(StreamReader sr=new StreamReader(name, System.Text.Encoding.UTF8))
            {
                string content = sr.ReadToEnd();
                return content;
            }
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 生成Md5码
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string BuildFileMd5(string filePath)
    {
        string fileMd5 = "";
        try
        {
            using(FileStream fs = File.OpenRead(filePath))
            {
                MD5 md5 = MD5.Create();
                byte[] fileMd5Bytes = md5.ComputeHash(fs);
                fileMd5 = FormmatMd5(fileMd5Bytes);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        return fileMd5;
    }
    /// <summary>
    /// 将字节数组转换为String
    /// </summary>
    /// <param name="fileMd5Bytes"></param>
    /// <returns></returns>
    private string FormmatMd5(byte[] fileMd5Bytes)
    {
        //转换字符串中的—,并将字符转转换为大写
        return BitConverter.ToString(fileMd5Bytes).Replace("-", "").ToLower();
    }
}
