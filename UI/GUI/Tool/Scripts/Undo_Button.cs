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
    public class Undo_Button : BaseMonoBehaviour, IPointerClickHandler
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        Image Image;

        [SerializeField]
        Sprite Enabled_Img;

        [SerializeField]
        Sprite Disabled_Img;

        public void OnPointerClick(PointerEventData eventData)
        {
            StandardInputManager.Instance.Undo();
        }

        protected override void Update()
        {
            base.Update();

            if (UndoManager.Instance.CanUndo)
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