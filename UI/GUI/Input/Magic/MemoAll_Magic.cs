using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace.UI
{
    public class MemoAll_Magic : MonoBehaviour, ISwipeMenuListener
    {
        public void OnPointerUp()
        {
            foreach(var cell in CellManager.Instance.Cells)
            {
                Memo_Magic.Memo(cell);
            }
        }
    }

}