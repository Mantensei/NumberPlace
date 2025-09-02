using System;
using UnityEngine;
using NumberPlace.Standard;
using System.Linq;

namespace NumberPlace.Events
{
    // セル状態変更の通知データ
    public struct CellStateChangeData
    {
        public Cell Cell { get; }
        public CellState CurrentState { get; }
        public int CurrentValue { get; }

        public CellStateChangeData(Cell cell, CellState currState, int currValue)
        {
            Cell = cell;
            CurrentState = currState;
            CurrentValue = currValue;
        }

        // 便利なプロパティ
        public bool IsError => CurrentState == CellState.Incorrect;
    }

    // リスナーインターフェイス（これだけ実装すればOK）
    public interface ICellStateChangeListener
    {
        void OnCellStateChanged(CellStateChangeData data);
    }    
}