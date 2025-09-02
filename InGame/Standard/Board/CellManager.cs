using MantenseiLib;
using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NumberPlace
{
    public class CellManager : SingletonMonoBehaviour<CellManager>
    {
        List<Cell> _cells = new List<Cell>();
        public Cell[] Cells => _cells.ToArray();

        public BoardData BoardData
        {
            get 
            {
                if (generator == null || generator.IsRunning) return null;
                return new BoardData(Cells, Blocksize); 
            }
        }

        public string GeneratingStatus => generator?.ProgressStatus;
        public bool Generating => generator?.IsRunning == true;
        RandomPuzzleGenerator generator;
        public int[] solvedGrid => generator.SolvedGrid.Flatten().ToArray();
        public int[] puzzleGrid => generator.PuzzleGrid.Flatten().ToArray();
        public int Blocksize => generator.BlockSize;
        public int Size => generator.Size;

        private Cell _lastValidCell;
        public static Cell CurrentCell => Instance._lastValidCell;
        
        //ÉVÅ[ÉìëJà⁄Ç≈îjä¸Ç≥ÇπÇΩÇ¢Ç©ÇÁstaticÇÕÇ‚ÇﬂÇÈ
        event Action<Cell> _onCurrentCellChanged;
        public static event Action<Cell> OnCurrentCellChanged
        {
            add => Instance._onCurrentCellChanged += value;
            remove => Instance._onCurrentCellChanged -= value;
        }

        public void ResetCurrentCell()
        {
            if (_lastValidCell != null)
            {
                _lastValidCell = null;
                _onCurrentCellChanged?.Invoke(_lastValidCell);
            }
        }

        void ObserveCurrentCell()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            var cell = selected?.GetComponentInParent<Cell>();

            if (cell != null)
            {
                var tmp = _lastValidCell;

                _lastValidCell = cell;

                if (tmp != _lastValidCell)
                    _onCurrentCellChanged?.Invoke(cell);
            }
        }

        protected override void Update()
        {
            base.Update();
            ObserveCurrentCell();
        }


        public void GenerateBoard(int blockSize, int hint, Transform parent)
        {
            generator = new RandomPuzzleGenerator(blockSize)
            {
                targetHints = hint,
            };

            StartCoroutine(generator.GenerateMinimalPuzzleCoroutine(() =>
            {
                for (int i = 0; i < solvedGrid.Length; i++)
                {
                    var cell = Instantiate(ResourceManager.GetResource<Cell>(), parent);

                    cell.ReceiveInfo(new StandardRule_Initialize_Instruction
                    {
                        num = solvedGrid[i] - 1,
                        id = i,
                        blockSize = blockSize,
                        option = puzzleGrid[i] == 0 ? InstructionOption.Empty : InstructionOption.Open,
                    });

                    _cells.Add(cell);
                }

                GameEventManager.ForeachSafetyFindObjects<IBoardGenerateHandler>(x => x.HandleBoardInfo(BoardData));
            }
            ));
        }
    }

    public interface IBoardGenerateHandler
    {
        public void HandleBoardInfo(BoardData data);
    }

    public class BoardData
    {
        public Cell[] Cells { get; }
        public int[] SolvedGrid { get; }
        public int[] PuzzleGrid { get; }
        public int BlockSize { get; }
        public int Size => BlockSize * BlockSize;

        public BoardData(Cell[] cells, int blockSize)
        {
            Cells = cells;
            BlockSize = blockSize;
        }

        Cell[] FilteredCells(Cell cell) => Cells;
        public Cell[] GetRow(Cell cell) => FilteredCells(cell).Where(c => cell.Row() == c.Row()).ToArray();
        public int[] GetRowNum(Cell cell) => GetRow(cell).Select(c => c.GetValue()).ToArray();
        public Cell[] GetColumn(Cell cell) => FilteredCells(cell).Where(c => cell.Column() == c.Column()).ToArray();
        public int[] GetColumnNum(Cell cell) => GetColumn(cell).Select(c => c.GetValue()).ToArray();
        public Cell[] GetBlock(Cell cell) => FilteredCells(cell).Where(c => cell.Group() == c.Group()).ToArray();
        public int[] GetBlockNum(Cell cell) => GetBlock(cell).Select(c => c.GetValue()).ToArray();
        public Cell GetCell(int row, int column) => Cells.FirstOrDefault(c => c.Row() == row && c.Column() == column);

        public bool IsUsedUp(int internalNum)
        {
            return Cells.Count(x => x.GetValue() == internalNum && x.IsCorrect()) == Size;
        }
    }
}