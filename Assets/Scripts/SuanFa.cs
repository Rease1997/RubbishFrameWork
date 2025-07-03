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
            Debug.Log("数组中下标为" + str + "  可以合成" + target);
        }
        else
        {
            Debug.Log("这个数组中没有任何两个数能累加成" + target);
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
                    return ar; //如果有就返回数组
                }
            }
        }
        return new int[0]; //但凡前面得遍历里面没有return 就说明没有达成条件得 所以直接返回一个空的int[]
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
