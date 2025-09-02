using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NumberPlace.Standard;
using MantenseiLib;
using TMPro;

namespace NumberPlace.UI
{
    public class NumInjector : HubChild<NumInputButton>, ISwipeMenuListener
    {
        [GetComponent(HierarchyRelation.Children)]
        TextMeshProUGUI Text;

        public static readonly int bigSize = 64;
        public static readonly int smallSize = 32;
        static bool MemoMode => StandardInputManager.Instance.MemoMode;

        protected override void Start()
        {
            base.Start();
            Text.text = HUB.DisplayNum.ToString();
        }

        public void OnPointerUp()
        {
            StandardInputManager.Instance.InjectNumber(HUB.DisplayNum);
        }
    }
}
