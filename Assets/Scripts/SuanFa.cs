using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuanFa : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int[] nums = new int[] { 3, 5, 11, 25, 100 };
        int target = 16;
        var ar = TwoSum(nums, target);
        if (ar.Length != 0)
        {
            string str = "";
            foreach (int i in ar)
            {
                str = str + " " + i;
            }
            Debug.Log("�������±�Ϊ" + str + "  ���Ժϳ�" + target);
        }
        else
        {
            Debug.Log("���������û���κ����������ۼӳ�" + target);
        }
    }
    public int[] TwoSum(int[] nums, int target)
    {
        int[] ar = new int[2];
        for (int i = 0; i < nums.Length; i++)
        {
            for (int j = i + 1; j < nums.Length; j++)
            {
                if (nums[i] + nums[j] == target)
                {
                    ar[0] = i;
                    ar[1] = j;
                    return ar; //����оͷ�������
                }
            }
        }
        return new int[0]; //����ǰ��ñ�������û��return ��˵��û�д�������� ����ֱ�ӷ���һ���յ�int[]
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
