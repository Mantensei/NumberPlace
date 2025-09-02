using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;

namespace NumberPlace.UI
{
    public class Answer_Magic : MonoBehaviour, ISwipeMenuListener
    {
        public void OnPointerUp()
        {
            var cell = CellManager.CurrentCell;

            if(cell?.IsSafe() == true)
            {
                var numberView = CellManager.CurrentCell.GetComponentInChildren<NumberView>();
                numberView.Open();
            }
        }
    } 
}
