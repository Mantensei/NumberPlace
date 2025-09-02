using MantenseiLib;
using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NumberPlace.UI
{
	public class Memo_Magic : BaseMonoBehaviour, ISwipeMenuListener
	{
        public bool IsDirectMode = false;
        Cell CurrentCell => CellManager.CurrentCell;

        protected override void Start()
        {
            base.Start();

            CellManager.OnCurrentCellChanged += (c) =>
            {
                if (IsDirectMode)
                {
                    if(CurrentCell?.IsSafe() == true)
                    {
                        Memo();
                    }
                }
            };
        }

        public static void Memo(Cell cell)
        {
            if (cell?.IsSafe() == true)
            {
                var memo = cell.GetComponentInChildren<MemoView>();
                if(!memo.AutoWrite()) return;

                var cells = CellManager.Instance.Cells;
                var score = cell.GetComponentInChildren<ScoreLogger>();
                int count = cells.Count();
                float writable = cells.Count(x => x.State() != CellState.Opened);
                float empty = cells.Count(x => !x.IsCorrect());

                float difficulty = writable / count;
                Debug.Log(difficulty);
                float rate = 1 - empty / writable / 2;
                score.MultiplyScore(rate);
            }
        }

        void Memo()
        {
            Memo(CurrentCell);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            IsDirectMode = false;
        }

        public void OnPointerUp()
        {
            IsDirectMode = true;
        }
    } 
}
