using UnityEngine;
using MantenseiLib;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace NumberPlace
{
    public class Cell : HubChild<IBoard>, ICellInfoReceiver
    {
        public IBoard Board => HUB;

        [GetComponent(HierarchyRelation.Children | HierarchyRelation.Self)]
        public CellView View { get; private set; }

        [GetComponent(HierarchyRelation.Children | HierarchyRelation.Self)]
        public CellMeta Meta { get; private set; }

        public void ReceiveInfo(object value) => this.PassCellInfo2Children(value);

        public static Cell CurrentCell => CellManager.CurrentCell;

        public bool IsCurrent() => CurrentCell == this;

        public Cell[] GetAroundCells()
        {
            var board = CellManager.Instance.BoardData;
            //if (board == null) return new Cell[0];

            return
                board.GetBlock(this)
                .Concat(CellManager.Instance.BoardData.GetColumn(this))
                .Concat(CellManager.Instance.BoardData.GetRow(this))
                .Where(x => x != this)
                .ToArray()
                ;
        }
    }

    public interface ICellInfoReceiver : IMonoBehaviour
    {
        void ReceiveInfo(object value);
    }

    public static class CellInfoReceiverExtension
    {
        public static void PassCellInfo2Children(this ICellInfoReceiver receiver, object value)
        {
            foreach (Transform child in receiver.transform)
            {
                foreach (var childReceiver in child.GetComponents<ICellInfoReceiver>())
                {
                    childReceiver.ReceiveInfo(value);
                }
            }
        }
    }
}