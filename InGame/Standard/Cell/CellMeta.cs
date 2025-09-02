using MantenseiLib;
using UnityEngine;

namespace NumberPlace
{
    public class CellMeta : HubChild<Cell>, ICellInfoReceiver
    {
        public object Value { get; set; } = -1;
        public int ID { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Group { get; set; }
        public CellState State { get; set; }

        public void ReceiveInfo(object value)
        {
            this.PassCellInfo2Children(value);
        }
    }

    public static partial class CellExtension
    {
        public static int GetValue(this Cell cell) => (int)cell.Meta.Value;
        public static int ID(this Cell cell) => cell.Meta.ID;
        public static Vector2Int Address(this Cell cell) => new Vector2Int(cell.Column(), cell.Row());    
        public static int Row(this Cell cell) => cell.Meta.Row;
        public static int Column(this Cell cell) => cell.Meta.Column;
        public static int Group(this Cell cell) => cell.Meta.Group;
        public static CellState State(this Cell cell) => cell.Meta.State;
        public static bool IsCorrect(this Cell cell) => cell.State() == CellState.Correct || cell.State() == CellState.Opened;
    }

    public interface IIntValue
    {
        int ToInt();
    }

    public enum CellState
    {
        Empty,
        Opened,
        Correct,
        Incorrect,
    }
}

