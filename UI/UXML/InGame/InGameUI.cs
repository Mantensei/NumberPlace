using MantenseiLib;
using UnityEngine;
using UnityEngine.UIElements;

namespace NumberPlace.UI
{
    public class InGameUI : BaseMonoBehaviour
    {
        [GetComponent] public UIDocument UIDocument { get; private set; }

        public VisualElement InfoPanel { get; private set; }
        public VisualElement InputPad { get; private set; }
        public VisualElement CanvasPlaceholder { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitializeUIElements();
        }

        public void InitializeUIElements()
        {
            var root = UIDocument.rootVisualElement;
            InfoPanel = root.Q<VisualElement>("info-panel");
            InputPad = root.Q<VisualElement>("input-panel");
            CanvasPlaceholder = root.Q<VisualElement>("canvas-placeholder");
        }
    }
}
