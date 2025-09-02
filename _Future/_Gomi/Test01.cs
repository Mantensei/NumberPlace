using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MantenseiLib; // 忘れずに！

public class Test_01 : MonoBehaviour
{
    void Start()
    {
        int[] array = Enumerable.Range(1, 5).ToArray();

        Debug.Log("--- スタイル別 ---");
        Debug.Log(array.JoinToString() + "　　:デフォルト（カンマ区切り）");
        Debug.Log(array.JoinToString(JoinFormat.CommaWrapped) + "　　:括弧つきカンマ");
        Debug.Log(array.JoinToString(JoinFormat.Space) + "　　:スペース区切り");
        Debug.Log(array.JoinToString(JoinFormat.None) + "　　:区切りなし");
        Debug.Log(array.JoinToString(JoinFormat.Line) + "　　:改行");

        Debug.Log("--- Linqの途中でも呼べる！ ---");
        var chain = new []{ 60, 81, 36, 81, 25, 2 }
                          .JoinLog(x => $"{(char)(x + 12354)}", JoinFormat.None)
                          .JoinLog()
                          .Where(x => x % 2 == 1)
                          .JoinLog()
                          .ToList();
    }
}