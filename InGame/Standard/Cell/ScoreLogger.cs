using MantenseiLib;
using NumberPlace.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace.Standard
{
    public class ScoreLogger : HubChild<Cell>, IScore, ICellStateChangeListener, IBoardGenerateHandler
    {
        public int BaseScore { get; private set; } = 100;
        public int Score { get; private set; } = 100;
        public bool available { get; set; } = false;

        public int GetScore() 
        {
            return Score;
        }

        public void MultiplyScore(float multiplier)
        {
            Score = Mathf.RoundToInt(BaseScore * multiplier);
        }

        public void HandleBoardInfo(BoardData data)
        {
            available = true;
        }

        public void OnCellStateChanged(CellStateChangeData data)
        {
            if(!available) return;

            if(data.Cell == HUB)
            {
                if (data.Cell.State() == CellState.Correct && HUB.State() != CellState.Opened)
                {
                    StatusManager.AddScore(HUB);
                }
            }
        }
    }

}