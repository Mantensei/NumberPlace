using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using UnityEngine.UI;
using TMPro;
using NumberPlace.UI;

namespace NumberPlace.Standard
{
    public class NumInputButton : BaseMonoBehaviour, INumberUsedUpListener
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        public Button Button { get; private set; }
        
        [GetComponent]
        public Image Image { get; private set; }

        [field:SerializeField]
        public TextMeshProUGUI Text { get; private set; }

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        SwipeMenu SwipeMenu { get; set; }

        static GameObject _occupieObject;
        public static GameObject OccupieObject
        {
            get => _occupieObject;
            set
            {
                _occupieObject = value;

                if (_occupieObject != null)
                    SwipeMenu.allowSwipeMode = false;
                else
                    SwipeMenu.allowSwipeMode = true;

                SwipeMenu.OnSwipeAction = () =>
                {
                    _occupieObject?.SetActive(true);
                    _occupieObject?.SetActive(false);
                    _occupieObject = null;
                };
            }
        }

        public int DisplayNum { get; private set; }

        public bool CancelMode;

        protected override void Start()
        {
            base.Start();

            Text.text = DisplayNum.ToString();
        }

        public static NumInputButton[] GenerateButtons(Transform parent, int length)
        {
            List<NumInputButton> buttons = new List<NumInputButton>(length);
            var prefab = ResourceManager.GetResource<NumInputButton>();

            for (int i = 0; i < length; i++)
            {
                var button = Instantiate(prefab, parent);
                button.DisplayNum = i + 1;
                button.gameObject.name = $"NumInputButton_{button.DisplayNum}";
                buttons.Add(button);
            }
            return buttons.ToArray();
        }

        public void OnNumberUsedUp(int internalNum)
        {
            if(internalNum +1 == DisplayNum)
            {
                Button.interactable = false;
                Image.enabled = false;
                Text.enabled = false;
            }
        }
    }
}