using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace NumberPlace.Standard
{
    public partial class PuzzleGenerator
    {
        public PuzzleCell[] GetRow(int row) => _cloneBoard.Where(c => c.Y == row).ToArray();
        public PuzzleCell[] GetRow(PuzzleCell cell) => GetRow(cell.Y).Remove(cell).ToArray();
        public PuzzleCell[] GetColumn(int column) => _cloneBoard.Where(c => c.X == column).ToArray();
        public PuzzleCell[] GetColumn(PuzzleCell cell) => GetColumn(cell.X).Remove(cell).ToArray();
        public PuzzleCell[] GetBlock(int block) => _cloneBoard.Where(c => c.Block == block).ToArray();
        public PuzzleCell[] GetBlock(PuzzleCell cell) => GetBlock(cell.Block).Remove(cell).ToArray();
        
        public int[] GetRowNum(PuzzleCell cell) => GetRow(cell).Visible().ToNum().ToArray();
        public int[] GetColumnNum(PuzzleCell cell) => GetColumn(cell).Visible().ToNum().ToArray();
        public int[] GetBlockNum(PuzzleCell cell) => GetBlock(cell).Visible().ToNum().ToArray();

        public PuzzleCell[][] GetRows() => NumRange.Select(i => GetRow(i)).ToArray();
        public PuzzleCell[][] GetColumns() => NumRange.Select(i => GetColumn(i)).ToArray();
        public PuzzleCell[][] GetBlocks() => NumRange.Select(i => GetBlock(i)).ToArray();

        public PuzzleCell[] GetIndexOfGroup(int index, int address) => _cloneBoard.Where(x => x.IndexOfAddress(index) == address).ToArray();

        public PuzzleCell[] GetAroundCells(PuzzleCell cell)
        {
            var aroundCells = new List<PuzzleCell>();
            aroundCells.AddRange(GetRow(cell));
            aroundCells.AddRange(GetColumn(cell));
            aroundCells.AddRange(GetBlock(cell));
            return aroundCells.ToArray();
        }
        public int[] GetAroundNums(PuzzleCell cell) => GetAroundCells(cell).Visible().Select(c => c.Num).ToArray();
        public int[] NumRange => Enumerable.Range(0, Size).ToArray();

        public void UpdateCandidates(PuzzleCell cell)
        {
            cell.Candidates.Clear();
            if (cell.IsVisible)
            {
                cell.Candidates.Add(cell.Num);
                return;
            }

            var used = GetAroundNums(cell);
            cell.Candidates.UnionWith(NumRange);

            foreach (var i in NumRange)
            {
                if (used.Contains(i))
                {
                    cell.Candidates.Remove(i);
                }
            }
        }
        public void UpdateAllCandidates()
        {
            foreach (var cell in _cloneBoard)
                UpdateCandidates(cell);
        }
    }

    public static class PuzzleCellExtension
    {
        public static IEnumerable<PuzzleCell> Remove(this IEnumerable<PuzzleCell> cells, PuzzleCell cell)
            => cells.Where(c => c.Address != cell.Address);

        public static IEnumerable<PuzzleCell> Empty(this IEnumerable<PuzzleCell> cells)
            => cells.Where(c => c.State == PuzzleCellState.Empty);

        public static IEnumerable<PuzzleCell> Visible(this IEnumerable<PuzzleCell> cells)
            => cells.Where(c => c.IsVisible);
        public static bool ContainsNum(this IEnumerable<PuzzleCell> cells, int num)
            => cells.Any(c => c.Num == num);
        
        public static bool ContainsAddress(this IEnumerable<PuzzleCell> cells, PuzzleCell cell)
            => cells.Any(c => c.Address == cell.Address);

        public static bool EqualsAddress(this PuzzleCell a, PuzzleCell b)
            => a.Address == b.Address;

        public static IEnumerable<int> ToNum(this IEnumerable<PuzzleCell> cells)
            => cells.Select(c => c.Num);
        
        public static IEnumerable<Vector2Int> ToAddress(this IEnumerable<PuzzleCell> cells)
            => cells.Select(c => c.Address);

        public static HashSet<int> NumSet(this IEnumerable<PuzzleCell> cells)
            => cells.Select(c => c.Num).ToHashSet();
    }

}