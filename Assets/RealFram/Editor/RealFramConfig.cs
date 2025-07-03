using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "RealFramConfig", menuName = "RealFramConfig", order = 0)]
public class RealFramConfig : ScriptableObject
{
    /// <summary>
    /// 打包时生成AB包配置表的二进制路径
    /// </summary>
    public string m_ABBytePath;
    /// <summary>
    /// xml文件夹路径
    /// </summary>
    public string m_XmlPath;
    /// <summary>
    /// 二进制文件夹路径
    /// </summary>
    public string m_BinaryPath;
    /// <summary>
    /// AB加密的密匙
    /// </summary>
    public string AesKey;
}
[CustomEditor(typeof(RealFramConfig))]
public class RealFramConfigInspector : Editor
{
    public SerializedProperty m_ABBytePath;
    public SerializedProperty m_XmlPath;
    public SerializedProperty m_BinaryPath;
    public SerializedProperty AesKey;

    private void OnEnable()
    {
        m_ABBytePath = serializedObject.FindProperty("m_ABBytePath");
        m_XmlPath = serializedObject.FindProperty("m_XmlPath");
        m_BinaryPath = serializedObject.FindProperty("m_BinaryPath");
        AesKey = serializedObject.FindProperty("AesKey");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_ABBytePath, new GUIContent("ab包二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_XmlPath, new GUIContent("Xml路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_BinaryPath, new GUIContent("二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(AesKey, new GUIContent("AesKey"));
        GUILayout.Space(5);
        serializedObject.ApplyModifiedProperties();//随时刷新
    }
}
public class RealConfig
{
    private const string RealFramPath = "Assets/RealFram/Editor/RealFramConfig.asset";

    public static  RealFramConfig GetRealFram()
    {
        RealFramConfig realConfig = AssetDatabase.LoadAssetAtPath<RealFramConfig>(RealFramPath);
        return realConfig;
    }
}