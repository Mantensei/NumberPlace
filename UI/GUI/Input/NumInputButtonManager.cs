using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace.Standard
{
    public class NumInputButtonManager : MonoBehaviour, IBoardGenerateHandler
    {
        List<NumInputButton> _buttons = new();

        public static readonly int bigSize = 64;
        public static readonly int smallSize = 32;
        public bool MemoMode
        {
            get => StandardInputManager.Instance.MemoMode;
            //private set => StandardInputManager.Instance.MemoModeToggled = value;
        }

        void Update()
        {
            foreach (var button in _buttons)
            {
                button.Text.fontSize = MemoMode ? smallSize : bigSize;
            }
        }


        public void HandleBoardInfo(BoardData data)
        {
            _buttons.AddRange(NumInputButton.GenerateButtons(transform, data.Size));
        }

        public void SetInteractable(int num, bool interactable)
        {
            foreach (var button in _buttons)
                button.GetComponent<UnityEngine.UI.Button>().interactable = interactable;
        }



    }
}
