using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace
{
    public class CellView : MonoBehaviour, ICellInfoReceiver
    {
        public void ReceiveInfo(object value)
        {
            this.PassCellInfo2Children(value);
        }
    }
}