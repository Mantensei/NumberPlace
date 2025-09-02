using System;
using System.Collections.Generic;
using UnityEngine;
using NumberPlace.Standard;
using MantenseiLib;
using UnityEngine.EventSystems;
using System.Linq;

namespace NumberPlace.Undo
{
    // Undoマネージャー
    public class UndoManager : SingletonMonoBehaviour<UndoManager>
    {
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();

        [SerializeField] private int maxUndoSteps = 100;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoStack.Push(command);

            // Redo履歴をクリア
            redoStack.Clear();

            // 最大数を超えた場合は古いものを削除
            if (undoStack.Count > maxUndoSteps)
            {
                var temp = new Stack<ICommand>();
                for (int i = 0; i < maxUndoSteps - 1; i++)
                {
                    temp.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (temp.Count > 0)
                {
                    undoStack.Push(temp.Pop());
                }
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
            }
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        // デバッグ用
        public int UndoCount => undoStack.Count;
        public int RedoCount => redoStack.Count;
    }

    // コマンドの基礎インターフェース
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    // セル操作の基礎クラス
    public abstract class CellCommand : ICommand
    {
        protected Cell targetCell;
        protected int value;

        protected CellCommand(Cell cell, int val)
        {
            targetCell = cell;
            value = val;
        }

        public abstract void Execute();
        public abstract void Undo();

        protected void SendInstruction(InstructionType type, int num, InstructionOption option = InstructionOption.None)
        {
            targetCell.ReceiveInfo(new StandardRule_Instruction
            {
                num = num,
                type = type,
                option = option
            });
        }
    }

    // セルのメモ状態を記録するクラス
    public class CellMemoSnapshot
    {
        public Cell Cell { get; set; }
        public bool[] MemoState { get; set; }

        public CellMemoSnapshot(Cell cell)
        {
            Cell = cell;
            var memoView = cell.GetComponentInChildren<MemoView>();
            MemoState = memoView?.MemoState;
        }

        public void Restore()
        {
            var memoView = Cell.GetComponentInChildren<MemoView>();
            if (memoView != null && MemoState != null)
            {
                memoView.SetMemoState(MemoState);
            }
        }
    }

    // 数字設定のコマンド（周囲のメモ削除も含む）
    public class SetNumberCommand : CellCommand
    {
        private readonly int previousValue;
        private readonly CellState previousState;
        private readonly List<CellMemoSnapshot> affectedMemoSnapshots;

        public SetNumberCommand(Cell cell, int value) : base(cell, value)
        {
            // 実行前の状態を記録
            var numberView = cell.GetView().GetComponent<NumberView>();
            previousValue = numberView?.Value ?? -1;
            previousState = cell.State();

            // 周囲のセルのメモ状態を事前に記録
            affectedMemoSnapshots = new List<CellMemoSnapshot>();
            RecordAffectedMemos();
        }

        private void RecordAffectedMemos()
        {
            // 実際に正解になる場合のみ、周囲のメモ状態を記録
            if (value >= 0 && value == targetCell.GetValue())
            {
                var aroundCells = targetCell.GetAroundCells();
                foreach (var cell in aroundCells)
                {
                    var memoView = cell.GetComponentInChildren<MemoView>();
                    if (memoView != null && memoView.MemoState != null && memoView.MemoState.Length > value && memoView.MemoState[value])
                    {
                        // このセルのメモが影響を受ける場合のみ記録
                        affectedMemoSnapshots.Add(new CellMemoSnapshot(cell));
                    }
                }
            }
        }

        public override void Execute()
        {
            SendInstruction(InstructionType.SetNum, value);
        }

        public override void Undo()
        {
            // まず数字を元に戻す
            if (previousValue >= 0)
            {
                SendInstruction(InstructionType.SetNum, previousValue);
            }
            else
            {
                // 空セルに戻す
                SendInstruction(InstructionType.SetNum, -1, InstructionOption.Empty);
            }

            // 影響を受けたメモを復元
            foreach (var snapshot in affectedMemoSnapshots)
            {
                snapshot.Restore();
            }
        }
    }

    // メモ変更のコマンド
    public class MemoCommand : CellCommand
    {
        private readonly bool[] previousMemoState;

        public MemoCommand(Cell cell, int memoNum) : base(cell, memoNum)
        {
            // 実行前のメモ状態を記録
            var memoView = cell.GetComponentInChildren<MemoView>();
            previousMemoState = memoView?.MemoState;
        }

        public override void Execute()
        {
            SendInstruction(InstructionType.Memo, value);
        }

        public override void Undo()
        {
            if (previousMemoState != null)
            {
                var memoView = targetCell.GetComponentInChildren<MemoView>();
                memoView?.SetMemoState(previousMemoState);
            }
        }
    }

    // セル状態のスナップショット
    public class CellSnapshot
    {
        public int NumberValue { get; set; } = -1;
        public CellState State { get; set; } = CellState.Empty;
        public bool[] MemoState { get; set; }

        public static CellSnapshot CreateFrom(Cell cell)
        {
            var snapshot = new CellSnapshot();

            var numberView = cell.GetComponentInChildren<NumberView>();
            if (numberView != null)
            {
                snapshot.NumberValue = numberView.Value;
                snapshot.State = cell.State();
            }

            var memoView = cell.GetComponentInChildren<MemoView>();
            if (memoView != null)
            {
                snapshot.MemoState = memoView.MemoState;
            }

            return snapshot;
        }

        public void RestoreToCell(Cell cell)
        {
            // 数値の復元
            if (NumberValue >= 0)
            {
                cell.ReceiveInfo(new StandardRule_Instruction
                {
                    num = NumberValue,
                    type = InstructionType.SetNum
                });
            }
            else
            {
                cell.ReceiveInfo(new StandardRule_Instruction
                {
                    num = -1,
                    type = InstructionType.SetNum,
                    option = InstructionOption.Empty
                });
            }

            // メモの復元
            if (MemoState != null)
            {
                var memoView = cell.GetComponentInChildren<MemoView>();
                memoView?.SetMemoState(MemoState);
            }
        }
    }

    // より汎用的なセルコマンド
    public class GenericCellCommand : ICommand
    {
        private readonly Cell targetCell;
        private readonly StandardRule_Instruction instruction;
        private readonly CellSnapshot previousSnapshot;

        public GenericCellCommand(Cell cell, StandardRule_Instruction instr)
        {
            targetCell = cell;
            instruction = instr;
            previousSnapshot = CellSnapshot.CreateFrom(cell);
        }

        public void Execute()
        {
            targetCell.ReceiveInfo(instruction);
        }

        public void Undo()
        {
            previousSnapshot.RestoreToCell(targetCell);
        }
    }

    // 複数操作をまとめるコマンド（メモ全設定など）
    public class CompositeCommand : ICommand
    {
        private List<ICommand> commands = new List<ICommand>();

        public void AddCommand(ICommand command)
        {
            commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            // 逆順でUndo を実行
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                commands[i].Undo();
            }
        }
    }

    // ファクトリクラス：適切なコマンドを生成
    public static class CommandFactory
    {
        public static ICommand CreateCommand(Cell cell, StandardRule_Instruction instruction)
        {
            // 特定のコマンドを使いたい場合
            switch (instruction.type)
            {
                case InstructionType.SetNum:
                    return new SetNumberCommand(cell, instruction.num);
                case InstructionType.Memo:
                    return new MemoCommand(cell, instruction.num);
                default:
                    return new GenericCellCommand(cell, instruction);
            }
        }
    }

    // 削除コマンド（セルを空にする）
    public class DeleteCommand : ICommand
    {
        private readonly Cell targetCell;
        private readonly CellSnapshot previousSnapshot;

        public DeleteCommand(Cell cell)
        {
            targetCell = cell;
            previousSnapshot = CellSnapshot.CreateFrom(cell);
        }

        public void Execute()
        {
            targetCell.ReceiveInfo(new StandardRule_Instruction
            {
                num = -1,
                type = InstructionType.None,
                option = InstructionOption.Empty
            });
        }

        public void Undo()
        {
            previousSnapshot.RestoreToCell(targetCell);
        }
    }
}