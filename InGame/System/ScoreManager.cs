using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace NumberPlace
{
    public partial class StatusManager
    {
        [InitializeStatic]
        public static int Score { get; private set; } = 0;

        [InitializeStatic]
        public static Action<int> OnScoreChanged;

        public static void AddScore(Cell cell)
        {
            var logger = cell.GetComponentInChildren<IScore>();    
            if (logger == null) return;

            if(logger?.IsSafe() == true)
            {
                var score = logger.GetScore();
                Score += score;
            }
        }
    }

    public interface IScore : IMonoBehaviour
    {
        int GetScore();
    }

}