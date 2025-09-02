using MantenseiLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using NumberPlace.Undo;

namespace NumberPlace.Standard
{
    public class StandardInputManager : SingletonMonoBehaviour<StandardInputManager>
    {
        BoardData BoardData => CellManager.Instance.BoardData;
        public Cell[] Cells => BoardData.Cells;
        public bool IsUsedUp(int internalNum) => BoardData.IsUsedUp(internalNum);
        public Cell GetCell(int row, int column) => BoardData.GetCell(row, column);
        Cell SelectedCell => CellManager.CurrentCell;

        // 元々のメモモード状態（Tabで切り替え）
        public bool MemoModeToggled { get; set; } = false;
        // 実際に使われるメモモード（Shift押下で一時的に上書きされる）
        public bool MemoMode { get; private set; }

        int blockSize = 3;
        int gridSize => (int)Mathf.Pow(blockSize, 2);

        //ほぼDirectInjector専用
        //非アクティブ状態でも通知できるようにイベント化
        event Action _onKeyMoved;
        public static event Action OnKeyMoved
        {
            add => Instance._onKeyMoved += value;
            remove => Instance._onKeyMoved -= value;
        }

        protected override void Update()
        {
            base.Update();

            if (BoardData == null) return;

            // 👇 Shift押下時は一時的にメモモードON
            // NumLockがOffだとShiftが暴発する！！！！！！１１１１１
            bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            MemoMode = isShift ? true : MemoModeToggled;

            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0)) return;

                if (SelectedCell == null)
                {
                    var first = Cells.OrderBy(x => x.ID()).FirstOrDefault();
                    EventSystem.current.SetSelectedGameObject(first.gameObject);

                    _onKeyMoved?.Invoke();
                }

            }

            ObserveShortCut();
        }

        void ObserveShortCut()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)
                || Input.GetKeyDown(KeyCode.Delete)) InjectNumber(0);
            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) InjectNumber(1);
            if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) InjectNumber(2);
            if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) InjectNumber(3);
            if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) InjectNumber(4);
            if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) InjectNumber(5);
            if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) InjectNumber(6);
            if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) InjectNumber(7);
            if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8)) InjectNumber(8);
            if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9)) InjectNumber(9);

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                MemoModeToggled = !MemoModeToggled;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.M))
                {
                    foreach (var cell in Cells)
                    {
                        if (cell.GetComponentInChildren<MemoView>() is MemoView memoView)
                        {
                            memoView.AutoWrite();
                        }
                    }
                }
            }

            var edge = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.PageUp)) edge = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.PageDown)) edge = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.Home)) edge = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.End)) edge = Vector2Int.right;

            MoveToEdge(edge);

            var x = Input.GetButtonDown("Horizontal") ? (int)Input.GetAxisRaw("Horizontal") : 0;
            var y = Input.GetButtonDown("Vertical") ? (int)Input.GetAxisRaw("Vertical") : 0;

            MoveCursor(new Vector2Int(x, -y));

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Redo();
                }
                else
                {
                    Undo();
                }

            }
        }

        void MoveToEdge(Vector2Int dir)
        {
            if (SelectedCell == null) return;
            if (dir == Vector2Int.zero) return;

            int row = SelectedCell.Row();
            int col = SelectedCell.Column();
            Cell target = SelectedCell;

            if (dir == Vector2Int.up) row = 0;
            else if (dir == Vector2Int.down) row = gridSize - 1;
            else if (dir == Vector2Int.left) col = 0;
            else if (dir == Vector2Int.right) col = gridSize - 1;

            target = GetCell(row, col);

            if (target != null)
            {
                var button = target.GetView().Button;
                EventSystem.current.SetSelectedGameObject(button.gameObject);

                _onKeyMoved?.Invoke();
            }
        }

        void MoveCursor(Vector2Int dir)
        {
            if (dir == Vector2.zero) return;
            if (SelectedCell == null) return;

            var row = SelectedCell.Row();
            var col = SelectedCell.Column();

            int startRow = row;
            int startCol = col;

            int steps = 1;
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                steps = blockSize;
            }

            Cell nearest = null;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Cells.Any(x => x.State() == CellState.Empty))
                {
                    while (true)
                    {
                        row = (row + dir.y + gridSize) % gridSize;
                        col = (col + dir.x + gridSize) % gridSize;

                        var next = GetCell(row, col);
                        if (next.State() == CellState.Empty)
                        {
                            nearest = next;
                            break;
                        }

                        if (next == SelectedCell) break;
                    }

                    if (nearest == null)
                    {
                        for (int i = 0; i < gridSize; i++)
                        {
                            row = (row + dir.y + gridSize) % gridSize;
                            col = (col + dir.x + gridSize) % gridSize;

                            var next = GetCell(row, col);
                            if (next.State() == CellState.Empty)
                            {
                                nearest = next;
                                break; // 最初に見つけたやつに即ジャンプ
                            }
                            else
                            {
                                var around = Cells.Where(x => x.ID() != SelectedCell.ID())
                                    .Where(x => x.State() == CellState.Empty)
                                    .Where(x => Vector2.Distance(x.Address(), next.Address()) <= i);

                                var picked = around.FirstOrDefault();

                                if (picked != null)
                                {
                                    nearest = picked;
                                    break;
                                }
                            }
                        }
                    }

                    if (nearest == null)
                        nearest = Cells.Where(x => x.State() == CellState.Empty)
                            .OrderBy(x => Vector2.Distance(x.Address(), SelectedCell.Address()))
                            .FirstOrDefault();
                }
            }
            else
            {
                for (int i = 0; i < steps; i++)
                {
                    row = (row + dir.y + gridSize) % gridSize;
                    col = (col + dir.x + gridSize) % gridSize;
                }

                nearest = GetCell(row, col);
            }

            if (nearest != null && nearest != SelectedCell)
            {
                var button = nearest.GetView().Button;
                EventSystem.current.SetSelectedGameObject(button.gameObject);

                _onKeyMoved?.Invoke();
            }
        }


        public void InjectNumber(int displayNum, bool memoMode = false)
        {
            int internalNum = displayNum - 1;

            if (SelectedCell == null) return;
            if (SelectedCell.IsCorrect()) return;
            if (IsUsedUp(internalNum)) return;

            ICommand command;

            if (internalNum < 0)
            {
                command = new DeleteCommand(SelectedCell);
            }
            else
            {
                if (MemoMode || memoMode)
                {
                    command = new MemoCommand(SelectedCell, internalNum);
                }
                else
                {
                    command = new SetNumberCommand(SelectedCell, internalNum);
                }
            }

            if (command != null)
            {
                UndoManager.Instance.ExecuteCommand(command);
            }

            ReLoadFocus();

            if (IsUsedUp(internalNum))
            {
                foreach (var listener in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<INumberUsedUpListener>())
                {
                    listener?.Safe()?.OnNumberUsedUp(internalNum);
                }

                if (Cells.All(x => x.IsCorrect()))
                    GameEventManager.NotifyEvent(GameEvent.StageClear);
            }
        }

        //ボタンに奪われたフォーカスを奪取、UIも更新
        void ReLoadFocus()
        {
            var eventSystem = EventSystem.current;

            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(SelectedCell.GetView().Button.gameObject);
        }

        public void Undo()
        {
            UndoManager.Instance.Undo();
            ReLoadFocus();
        }

        public void Redo()
        {
            UndoManager.Instance.Redo();
            ReLoadFocus();
        }
    }

    public interface INumberUsedUpListener
    {
        void OnNumberUsedUp(int internalNum);
    }
}
