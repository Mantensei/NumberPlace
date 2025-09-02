using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using Random = UnityEngine.Random;
using MantenseiLib;

namespace NumberPlace.Standard
{
    public class PuzzleCell
    {
        public int Num { get; }
        public int Y { get; }
        public int X { get; }
        public Vector2Int Address => new Vector2Int(X, Y);
        public int Block { get; }

        public int IndexOfAddress(int index)
        {
            switch(index)
            {
                case 0: return Y;
                case 1: return X;
                case 2: return Block;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0, 1, or 2.");
            }
        }

        public PuzzleCellState State { get; set; }
        public bool IsVisible => State != PuzzleCellState.Empty;
        public HashSet<int> Candidates { get; } = new();
        public PuzzleCell InitializeCandidates(int size)
        {
            Candidates.Clear();
            for (int i = 0; i < size; i++) Candidates.Add(i);
            return this;
        }


        public PuzzleCell(int number, int row, int col, int block)
        {
            Y = row;
            X = col;
            Num = number;
            Block = block;
        }

        public PuzzleCell(Cell cell) : this(
            number: (int)cell.Meta.Value,
            row: cell.Row(),
            col: cell.Column(),
            block: cell.Group()
        )
        { }

        public override string ToString() => $"{Num + 1}{Address + Vector2Int.one}";
    }

    public enum IndexType
    {
        Row = 0,
        Column = 1,
        Block = 2
    }
    public enum PuzzleCellState
    {
        Open,
        Empty,
        Close,
    }

    public partial class PuzzleGenerator
    {
        PuzzleCell[] _cells;
        public PuzzleCell[] Cells => _cells;
        // 可視セル一覧（シャッフル済み）
        public IEnumerable<PuzzleCell> GetRandomVisibleCells()
            => _cells.Where(c => c.State == PuzzleCellState.Open).Shuffle();

        PuzzleCell[] _cloneBoard;

        PuzzleSolution[] _solutions;
        public int BlockSize { get; private set; }
        public int Size { get; private set; }

        private PuzzleGenerator(int blockSize)
        {
            BlockSize = blockSize;
            Size = (int)Mathf.Pow(blockSize, 2);
            _solutions = CollectAllSolutions();
        }

        public PuzzleGenerator(int[][] table, int blockSize) : this(blockSize)
        {
            List<PuzzleCell> cellList = new List<PuzzleCell>();

            for (int i = 0; i < table.Length; i++)
            {
                for (int j = 0; j < table[i].Length; j++)
                {
                    var block = (i / blockSize) * blockSize + (j / blockSize);
                    var cell = new PuzzleCell(table[i][j], i, j, block)
                        .InitializeCandidates((int)Mathf.Pow(blockSize, 2));
                    cellList.Add(cell);
                }
            }
            _cells = cellList.ToArray();
        }

        public PuzzleGenerator(Cell[] cells, int blockSize): this(blockSize)
        {
            this._cells = cells.Select(x =>
            {
                var cell = new PuzzleCell((int)x.Meta.Value, x.Row(), x.Column(), x.Group());
                switch (x.State())
                {
                    case CellState.Empty:
                        cell.State = PuzzleCellState.Empty;
                        break;
                    default:
                        cell.State = PuzzleCellState.Open;
                        break;
                }

                cell.InitializeCandidates((int)Mathf.Pow(blockSize, 2));
                return cell;
            }
            ).ToArray();
        }

        PuzzleSolution[] CollectAllSolutions()
        {
            var result = new List<PuzzleSolution>();
            var methods = typeof(PuzzleGenerator).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<NumberPlaceSolutionAttribute>();
                if (attr == null) continue;

                if (attr.Type == PuzzleSolutionType.Predicate)
                {
                    // デリゲートに変換（ラムダでthisを固定）
                    Func<PuzzleCell, bool> predicate = (PuzzleCell cell) =>
                    {
                        return (bool)method.Invoke(this, new object[] { cell });
                    };
                    result.Add(new PuzzleSolution(attr, method, predicate));
                }
                else if (attr.Type == PuzzleSolutionType.Func)
                {
                    // デリゲートに変換（ラムダでthisを固定）
                    Func<PuzzleCell[]> func = () =>
                    {
                        return (PuzzleCell[])method.Invoke(this, null);
                    };
                    result.Add(new PuzzleSolution(attr, method, func));
                }
            }
            return result.OrderBy(x => x.Difficulty).ToArray();
        }

        public void DigHoles()
        {
            foreach (var cell in GetRandomVisibleCells())
            {
                TryEmptyCell(cell);
            }

            foreach (var solution in _solutions)
            {
                TryInvokeFunc(solution);
            }
        }

        public void TryInvokeFunc(PuzzleSolution solution)
        {
            if (solution.SolutionType == PuzzleSolutionType.Func)
            {
                var cells = solution.ApplyFunc();
                //Debug.Log($"{solution}, {true}");
                if(cells == null || cells.Length == 0) return;
                Debug.Log($"{solution}, {true}");

                foreach (var cell in cells)
                {
                    cell.State = PuzzleCellState.Empty;
                }
            }
        }

        public bool TryEmptyCell(PuzzleCell cell)
        {
            CloneBoard();
            if (Solve(cell))
            {
                cell.State = PuzzleCellState.Empty;
                return true;
            }
            else
            {
                cell.State = PuzzleCellState.Close;
                return false;
            }
        }

        bool Solve(PuzzleCell cell)
        {
            foreach (var solution in _solutions.Where(x => x.SolutionType == PuzzleSolutionType.Predicate))
            {
                if (solution?.Apply(cell) == true)
                {
                    Debug.Log($"{solution}, {true}");
                    return true;
                }
            }

            return false;
        }


        void CloneBoard()
        {
            _cloneBoard = _cells.Select(c => new PuzzleCell(
                number: c.Num,
                row: c.Y,
                col: c.X,
                block: c.Block
            )
            { State = c.State }).ToArray();
        }
    }

    public class PuzzleSolution
    {
        public string Name => Method.Name;
        public NumberPlaceSolutionAttribute Attribute { get; }
        public PuzzleSolutionDifficulty Difficulty => Attribute.Level;
        public MethodInfo Method { get; }
        private Func<PuzzleCell, bool> _predicate;
        private Func<PuzzleCell[]> _func;
        public PuzzleSolutionType SolutionType => Attribute.Type;

        public PuzzleSolution(NumberPlaceSolutionAttribute attribute, MethodInfo method, Func<PuzzleCell, bool> predicate)
        {
            Attribute = attribute;
            Method = method;
            _predicate = predicate;
        }
        
        public PuzzleSolution(NumberPlaceSolutionAttribute attribute, MethodInfo method, Func<PuzzleCell[]> func)
        {
            Attribute = attribute;
            Method = method;
            _func = func;
        }
        
        public PuzzleCell[] ApplyFunc() => _func.Invoke();
        public bool Apply(PuzzleCell cell) => _predicate.Invoke(cell);
        public override string ToString() => $"{Name} ({Difficulty})";
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class NumberPlaceSolutionAttribute : Attribute
    {
        public PuzzleSolutionDifficulty Level { get; }
        public PuzzleSolutionType Type { get; } = PuzzleSolutionType.Predicate;

        public NumberPlaceSolutionAttribute(PuzzleSolutionDifficulty level)
        {
            this.Level = level;
        }
        
        public NumberPlaceSolutionAttribute(PuzzleSolutionDifficulty level, PuzzleSolutionType type)
        {
            this.Level = level;
            this.Type = type;
        }
    }

    public enum PuzzleSolutionType
    {
        Predicate,
        Func,
    }

    public enum PuzzleSolutionDifficulty
    {
        Easy,
        Normal,
        Hard,
    }
}