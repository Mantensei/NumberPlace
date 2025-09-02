using MantenseiLib;
using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NumberPlace.UI
{
    public class Memo_Button : BaseMonoBehaviour, IPointerClickHandler
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        Image Image;

        [SerializeField]
        Sprite Enabled_Img;

        [SerializeField]
        Sprite Disabled_Img;

        bool memoMode
        {
            get => StandardInputManager.Instance.MemoMode;
            set => StandardInputManager.Instance.MemoModeToggled = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            memoMode = !memoMode;
        }

        protected override void Update()
        {
            base.Update();

            if (memoMode)
            {
                Image.sprite = Enabled_Img;
            }
            else
            {
                Image.sprite = Disabled_Img;
            }
        }
    }

}