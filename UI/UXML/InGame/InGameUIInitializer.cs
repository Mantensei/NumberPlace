using UnityEngine;
using UnityEngine.UIElements;
using MantenseiLib;

namespace NumberPlace.UI
{
    public class InGameUIInitializer : BaseMonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)] 
        private InGameUI inGameUI;

        [Header("UXML")]
        [SerializeField] private VisualTreeAsset gameUITemplate;

        [SerializeField] UIPositionType Position;

        VisualElement GetElement()
        {
            inGameUI.InitializeUIElements();

            switch (Position)
            {
                case UIPositionType.Top:
                    return inGameUI.InfoPanel;
                case UIPositionType.Bottom:
                    return inGameUI.InputPad;

                default:
                    return null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            var parent = GetElement();
            if (parent == null) return;

            parent.Clear();
            parent.Add(gameUITemplate.Instantiate());
        }

        public enum UIPositionType
        {
            None,
            Top,
            Bottom,
            Left,
            Right,
            Center,
        }
    }
}
