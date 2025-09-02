using MantenseiLib;
using NumberPlace.Standard;
using NumberPlace.Undo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NumberPlace.UI
{
    public class Redo_Button : BaseMonoBehaviour, IPointerClickHandler
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        Image Image;

        [SerializeField]
        Sprite Enabled_Img;

        [SerializeField]
        Sprite Disabled_Img;

        public void OnPointerClick(PointerEventData eventData)
        {
            StandardInputManager.Instance.Redo(); 
        }
        protected override void Update()
        {
            base.Update();

            if (UndoManager.Instance.CanRedo)
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