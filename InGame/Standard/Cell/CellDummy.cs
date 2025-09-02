using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace
{
    public class CellDummy : MonoBehaviour, ICellInfoReceiver
    {
        public void ReceiveInfo(object value)
        {
            Debug.Log(value);
            this.PassCellInfo2Children(value);
        }
    }
}
