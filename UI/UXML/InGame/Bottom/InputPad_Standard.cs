using UnityEngine.UIElements;
using System.Collections.Generic;

namespace NumberPlace.UI
{
    public class InputPad_Standard
    {
        public IReadOnlyDictionary<int, Button> NumberButtons => _numberButtons;
        public Button ClearButton { get; }
        public Toggle MemoToggle { get; }
        public Button UndoButton { get; }
        public Button RedoButton { get; }

        private readonly Dictionary<int, Button> _numberButtons = new();

        public InputPad_Standard(VisualElement root)
        {
            for (int i = 1; i <= 9; i++)
            {
                var btn = root.Q<Button>($"btn-{i}");
                if (btn != null) _numberButtons[i] = btn;
            }

            ClearButton = root.Q<Button>("btn-clear");
            MemoToggle = root.Q<Toggle>("memo-toggle");
            UndoButton = root.Q<Button>("undo-button");
            RedoButton = root.Q<Button>("redo-button");
        }
    }
}
