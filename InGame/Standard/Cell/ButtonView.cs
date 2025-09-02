using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using MantenseiLib.UI;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NumberPlace.UI;

namespace NumberPlace
{
    public class ButtonView : HubChild<Cell>, ISelectHandler, IPointerClickHandler
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        public Button Button { get; private set; }

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        public Image Image { get; private set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            CellManager.Instance.ResetCurrentCell();
            EventSystem.current.SetSelectedGameObject(Button.gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            HighLight(HUB);
        }

        public static void HighLight(Cell source)
        {
            foreach (var cell in CellManager.Instance.Cells)
            {
                cell.GetView().Image.color = ColorManager.SystemWhite;

                var memo = cell.GetComponentInChildren<MemoView>();
                memo?.ResetHighLight();
                if(source.IsCorrect())
                    memo?.HighLight(source.GetValue());

                if (cell.GetValue() == source.GetValue())
                {
                    if(source.IsCorrect() && cell.IsCorrect())
                        cell.GetView().Image.color = ColorManager.White;
                }

                if (
                    cell.Row() == source.Row()
                    || cell.Column() == source.Column()
                    //|| cell.Group() == source.Group()
                    )
                {
                    cell.GetView().Image.color = ColorManager.White;
                }
            }
        }  
        
        public static void HighLight(int internalNum)
        {
            foreach (var cell in CellManager.Instance.Cells)
            {
                cell.GetView().Image.color = ColorManager.SystemWhite;

                var memo = cell.GetComponentInChildren<MemoView>();
                memo?.ResetHighLight();

                memo?.HighLight(internalNum);

                if (cell.GetValue() == internalNum)
                {
                    if(cell.IsCorrect())
                        cell.GetView().Image.color = new Color(0.55f, 0.55f, 0.55f);
                }

            }
        }
    }

    public static partial class CellExtension
    {
        public static ButtonView GetView(this Cell cell) => cell.GetComponentInChildren<ButtonView>();
    }
}
