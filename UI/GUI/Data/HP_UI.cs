using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NumberPlace.Events;

namespace NumberPlace.UI
{
    public class HP_UI : MonoBehaviour, ICellStateChangeListener
    {
        [field: SerializeField]
        public TextMeshProUGUI Title_Txt { get; private set; }

        [field: SerializeField]
        public TextMeshProUGUI Content_Txt { get; private set; }

        public int HP => StatusManager.HP;

        public void OnCellStateChanged(CellStateChangeData data)
        {
            if(data.CurrentState == CellState.Incorrect)
            {
                StatusManager.RemoveHP();
                UpdateText();
            }
        }

        void Update()
        {
            UpdateText();
        }

        void UpdateText()
        {
            Content_Txt.text = $"[{HP}]";
        }
    } 
}
