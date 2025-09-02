using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MantenseiLib;
using Random = UnityEngine.Random;

namespace NumberPlace.Standard
{
    /// <summary>
    /// ビット演算最適化版数独パズル生成機
    /// 候補数字管理にビット演算を使用し、高速化を実現
    /// </summary>
    public class RandomPuzzleGenerator
    {
        public bool LogEnabled = false;
        public int targetHints = 25;
        private int maxIterations = 10000;
        private bool allowMultipleSolutions = false;

        public int maxRemovalAttempts = 3;
        public bool useMultiplePassRemoval = true;

        private int[][] _solvedGrid;
        public int[][] SolvedGrid => _solvedGrid;
        private int[][] _puzzleGrid;
        public int[][] PuzzleGrid => _puzzleGrid;

        public int BlockSize { get; private set; }
        public int Size => (int)Mathf.Pow(BlockSize, 2);

        // ビット演算用の定数とマスク
        private int _fullMask; // 全ての数字が利用可能な状態のマスク
        private int[] _numberMasks; // 各数字に対応するビットマスク

        // 高速化用の制約テーブル
        private int[] _rowConstraints;
        private int[] _colConstraints;
        private int[] _blockConstraints;

        public bool IsRunning { get; private set; } = false;
        public event Action onRunning;

        public string ProgressStatus { get; private set; } = "";
        public float ProgressPercentage { get; private set; } = 0f;

        private int _operationsPerFrame = 150000; // ビット演算により高速化されるため増量

        public RandomPuzzleGenerator(int blockSize = 3, int targetHints = 25, bool allowMultipleSolutions = false)
        {
            this.BlockSize = blockSize;
            this.targetHints = targetHints;
            this.allowMultipleSolutions = allowMultipleSolutions;

            InitializeBitMasks();
        }

        private void InitializeBitMasks()
        {
            // 1からSizeまでの数字に対応するビットマスクを作成
            _numberMasks = new int[Size + 1];
            for (int i = 1; i <= Size; i++)
            {
                _numberMasks[i] = 1 << (i - 1);
            }

            // 全ての数字が使用可能な状態のマスク
            _fullMask = (1 << Size) - 1;

            // 制約テーブルの初期化
            _rowConstraints = new int[Size];
            _colConstraints = new int[Size];
            _blockConstraints = new int[Size];
        }

        public IEnumerator GenerateMinimalPuzzleCoroutine(Action onComplete = null)
        {
            // Debug.Log($"新しい数独パズルを生成します...（目標ヒント数：{targetHints}）");
            IsRunning = true;
            ProgressPercentage = 0f;

            // 1. 完全解生成
            ProgressStatus = "完全解を生成中...";
            yield return GenerateFullSolutionCoroutine();
            ProgressPercentage = 30f;

            // 2. パズル生成
            if (allowMultipleSolutions)
            {
                ProgressStatus = "パズルを生成中（複数解許可）...";
                yield return GenerateWithMultipleSolutionsCoroutine();
            }
            else
            {
                ProgressStatus = "パズルを生成中（一意解保証）...";
                yield return GenerateWithUniqueSolutionCoroutine();
            }

            ProgressStatus = "生成完了！";
            ProgressPercentage = 100f;
            IsRunning = false;
            onComplete?.Invoke();
        }


        // GenerateFullSolutionCoroutine()メソッドの最後に以下を追加
        private IEnumerator GenerateFullSolutionCoroutine()
        {
            ProgressStatus = "完全解のグリッドを初期化中...";
            _solvedGrid = new int[Size][];
            for (int i = 0; i < Size; i++)
            {
                _solvedGrid[i] = new int[Size];
            }

            ProgressStatus = "完全解を計算中...";
            var solver = new OptimizedStateMachineSolver(this);
            yield return solver.SolveCoroutine(_solvedGrid);

            ProgressStatus = "数字をシャッフル中...";
            ShuffleNumbers(_solvedGrid);

            ProgressStatus = "完全解の生成完了";
        }

        private IEnumerator GenerateWithUniqueSolutionCoroutine()
        {
            _puzzleGrid = CloneGrid(_solvedGrid);

            if (useMultiplePassRemoval)
            {
                yield return MultiplePassRemovalCoroutine();
            }
            else
            {
                yield return SinglePassRemovalCoroutine();
            }

            ProgressStatus = $"パズル生成完了 - 最終ヒント数: {CountHints(_puzzleGrid)}";
            // Debug.Log($"最終ヒント数: {CountHints(_puzzleGrid)}");
        }

        private IEnumerator MultiplePassRemovalCoroutine()
        {
            var uniquenessChecker = new OptimizedUniquenessChecker(this);
            int totalOperations = 0;
            int maxTotalOperations = Size * Size * maxRemovalAttempts;

            for (int attempt = 0; attempt < maxRemovalAttempts; attempt++)
            {
                ProgressStatus = $"削除パス {attempt + 1}/{maxRemovalAttempts}...";
                int removedInThisPass = 0;

                List<Vector2Int> cellPositions = new List<Vector2Int>();
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if (_puzzleGrid[i][j] != 0)
                        {
                            cellPositions.Add(new Vector2Int(i, j));
                        }
                    }
                }
                cellPositions = cellPositions.OrderBy(x => Random.value).ToList();

                foreach (var pos in cellPositions)
                {
                    int row = pos.x;
                    int col = pos.y;
                    totalOperations++;

                    if (_puzzleGrid[row][col] == 0) continue;

                    int temp = _puzzleGrid[row][col];
                    _puzzleGrid[row][col] = 0;

                    bool isUnique = false;
                    yield return uniquenessChecker.CheckUniquenesCoroutine(_puzzleGrid, (result) => isUnique = result);

                    if (isUnique)
                    {
                        removedInThisPass++;
                        // if (LogEnabled)
                        //     Debug.Log($"パス{attempt + 1}: セル ({row},{col}) 削除成功。現在ヒント数: {CountHints(_puzzleGrid)}");
                    }
                    else
                    {
                        _puzzleGrid[row][col] = temp;
                    }

                    float baseProgress = 30f;
                    float progressRange = 70f;
                    ProgressPercentage = baseProgress + (progressRange * totalOperations / maxTotalOperations);

                    int currentHints = CountHints(_puzzleGrid);
                    ProgressStatus = $"パス {attempt + 1}/{maxRemovalAttempts} - 現在ヒント数: {currentHints}, 目標: {targetHints}";

                    if (totalOperations % 15 == 0) // ビット演算高速化により頻度を上げる
                    {
                        onRunning?.Invoke();
                        yield return null;
                    }

                    if (currentHints <= targetHints)
                    {
                        ProgressStatus = $"目標達成！ヒント数: {currentHints}";
                        // Debug.Log($"目標達成: ヒント数 = {currentHints}, パス = {attempt + 1}");
                        break;
                    }
                }

                // Debug.Log($"パス {attempt + 1} 完了: {removedInThisPass} セル削除");

                if (removedInThisPass == 0)
                {
                    // Debug.Log($"パス {attempt + 1} で削除されたセルがないため、早期終了");
                    break;
                }
            }
        }

        private IEnumerator SinglePassRemovalCoroutine()
        {
            int totalCells = Size * Size;
            List<Vector2Int> cellPositions = new List<Vector2Int>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    cellPositions.Add(new Vector2Int(i, j));
                }
            }
            cellPositions = cellPositions.OrderBy(x => Random.value).ToList();

            ProgressStatus = $"セルを削除中... 目標ヒント数: {targetHints}";
            int processedCells = 0;
            int removedCells = 0;
            var uniquenessChecker = new OptimizedUniquenessChecker(this);

            foreach (var pos in cellPositions)
            {
                int row = pos.x;
                int col = pos.y;
                processedCells++;

                if (_puzzleGrid[row][col] == 0) continue;

                int temp = _puzzleGrid[row][col];
                _puzzleGrid[row][col] = 0;

                bool isUnique = false;
                yield return uniquenessChecker.CheckUniquenesCoroutine(_puzzleGrid, (result) => isUnique = result);

                if (isUnique)
                {
                    removedCells++;
                    // if (LogEnabled)
                    //     Debug.Log($"セル ({row},{col}) 削除成功。ヒント数: {CountHints(_puzzleGrid)}");
                }
                else
                {
                    _puzzleGrid[row][col] = temp;
                }

                float baseProgress = 30f;
                float progressRange = 70f;
                ProgressPercentage = baseProgress + (progressRange * processedCells / totalCells);
                int currentHints = CountHints(_puzzleGrid);
                ProgressStatus = $"セル削除中... {processedCells}/{totalCells} (削除済み: {removedCells}, 現在ヒント数: {currentHints})";

                if (processedCells % 15 == 0)
                {
                    onRunning?.Invoke();
                    yield return null;
                }

                if (currentHints <= targetHints)
                {
                    ProgressStatus = $"目標達成！ヒント数: {currentHints}";
                    // Debug.Log($"目標達成: ヒント数 = {currentHints}");
                    break;
                }
            }
        }

        private IEnumerator GenerateWithMultipleSolutionsCoroutine()
        {
            _puzzleGrid = CloneGrid(_solvedGrid);
            int totalCells = Size * Size;

            List<Vector2Int> cellPositions = new List<Vector2Int>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    cellPositions.Add(new Vector2Int(i, j));
                }
            }
            cellPositions = cellPositions.OrderBy(x => Random.value).ToList();

            ProgressStatus = $"セルを削除中... 目標ヒント数: {targetHints} (複数解許可)";
            int processedCells = 0;
            int removedCells = 0;
            var solver = new OptimizedStateMachineSolver(this);

            foreach (var pos in cellPositions)
            {
                int row = pos.x;
                int col = pos.y;
                processedCells++;

                if (_puzzleGrid[row][col] == 0) continue;

                int temp = _puzzleGrid[row][col];
                _puzzleGrid[row][col] = 0;

                int[][] testGrid = CloneGrid(_puzzleGrid);
                bool hasSolution = false;
                yield return solver.SolveCoroutine(testGrid, (result) => hasSolution = result);

                if (hasSolution)
                {
                    removedCells++;
                }
                else
                {
                    _puzzleGrid[row][col] = temp;
                }

                float baseProgress = 30f;
                float progressRange = 70f;
                ProgressPercentage = baseProgress + (progressRange * processedCells / totalCells);
                int currentHints = CountHints(_puzzleGrid);
                ProgressStatus = $"セル削除中... {processedCells}/{totalCells} (削除済み: {removedCells}, 現在ヒント数: {currentHints})";

                if (processedCells % 15 == 0)
                {
                    onRunning?.Invoke();
                    yield return null;
                }

                if (currentHints <= targetHints)
                {
                    ProgressStatus = $"目標達成！ヒント数: {currentHints}";
                    break;
                }
            }

            ProgressStatus = $"パズル生成完了 - 最終ヒント数: {CountHints(_puzzleGrid)}";
            // Debug.Log($"最終ヒント数: {CountHints(_puzzleGrid)}");
        }

        // 新しいメソッドを追加
        /// <summary>
        /// 完成した盤面の数字をランダムにシャッフル
        /// </summary>
        private void ShuffleNumbers(int[][] grid)
        {
            // 1からSizeまでの数字のマッピングテーブルを作成
            int[] numberMapping = new int[Size + 1]; // インデックス0は使用しない
            List<int> shuffledNumbers = new List<int>();

            // 1からSizeまでの数字をリストに追加
            for (int i = 1; i <= Size; i++)
            {
                shuffledNumbers.Add(i);
            }

            // リストをシャッフル
            for (int i = shuffledNumbers.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = shuffledNumbers[i];
                shuffledNumbers[i] = shuffledNumbers[randomIndex];
                shuffledNumbers[randomIndex] = temp;
            }

            // マッピングテーブルを作成
            for (int i = 0; i < Size; i++)
            {
                numberMapping[i + 1] = shuffledNumbers[i];
            }

            // グリッド全体の数字をマッピングに従って置き換え
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (grid[row][col] != 0)
                    {
                        grid[row][col] = numberMapping[grid[row][col]];
                    }
                }
            }

            // if (LogEnabled)
            //     Debug.Log($"数字シャッフル完了: {string.Join(",", shuffledNumbers)}");
        }

        #region ユーティリティ
        /// <summary>
        /// ビット演算を使った高速有効性チェック
        /// </summary>
        public bool IsValidFast(int[][] grid, int row, int col, int num)
        {
            int mask = _numberMasks[num];

            // 行チェック（ビット演算）
            int rowMask = 0;
            for (int i = 0; i < Size; i++)
            {
                if (grid[row][i] != 0)
                    rowMask |= _numberMasks[grid[row][i]];
            }
            if ((rowMask & mask) != 0) return false;

            // 列チェック（ビット演算）
            int colMask = 0;
            for (int i = 0; i < Size; i++)
            {
                if (grid[i][col] != 0)
                    colMask |= _numberMasks[grid[i][col]];
            }
            if ((colMask & mask) != 0) return false;

            // ブロックチェック（ビット演算）
            int blockMask = 0;
            int blockRow = (row / BlockSize) * BlockSize;
            int blockCol = (col / BlockSize) * BlockSize;

            for (int i = 0; i < BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    if (grid[blockRow + i][blockCol + j] != 0)
                        blockMask |= _numberMasks[grid[blockRow + i][blockCol + j]];
                }
            }
            if ((blockMask & mask) != 0) return false;

            return true;
        }

        /// <summary>
        /// セルの利用可能な数字をビットマスクで取得
        /// </summary>
        public int GetAvailableNumbersMask(int[][] grid, int row, int col)
        {
            if (grid[row][col] != 0) return 0; // 既に数字が入っている

            int usedMask = 0;

            // 行の使用済み数字
            for (int i = 0; i < Size; i++)
            {
                if (grid[row][i] != 0)
                    usedMask |= _numberMasks[grid[row][i]];
            }

            // 列の使用済み数字
            for (int i = 0; i < Size; i++)
            {
                if (grid[i][col] != 0)
                    usedMask |= _numberMasks[grid[i][col]];
            }

            // ブロックの使用済み数字
            int blockRow = (row / BlockSize) * BlockSize;
            int blockCol = (col / BlockSize) * BlockSize;
            for (int i = 0; i < BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    if (grid[blockRow + i][blockCol + j] != 0)
                        usedMask |= _numberMasks[grid[blockRow + i][blockCol + j]];
                }
            }

            return _fullMask & (~usedMask); // 利用可能な数字のマスク
        }

        /// <summary>
        /// ビットマスクから数字のリストを取得
        /// </summary>
        public List<int> GetNumbersFromMask(int mask)
        {
            List<int> numbers = new List<int>();
            for (int i = 1; i <= Size; i++)
            {
                if ((mask & _numberMasks[i]) != 0)
                {
                    numbers.Add(i);
                }
            }
            return numbers;
        }

        /// <summary>
        /// ビットカウント（利用可能な数字の個数）
        /// </summary>
        public int CountBits(int mask)
        {
            int count = 0;
            while (mask != 0)
            {
                count++;
                mask &= mask - 1; // 最下位ビットを削除
            }
            return count;
        }

        public bool FindNextEmptyCell(int[][] grid, ref int row, ref int col)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (grid[r][c] == 0)
                    {
                        row = r;
                        col = c;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 最も制約の厳しいセルを見つける（MRV: Most Restricting Variable）
        /// </summary>
        public bool FindMostConstrainedCell(int[][] grid, ref int row, ref int col)
        {
            int minChoices = Size + 1;
            bool found = false;

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (grid[r][c] == 0)
                    {
                        int availableMask = GetAvailableNumbersMask(grid, r, c);
                        int choices = CountBits(availableMask);

                        if (choices < minChoices)
                        {
                            minChoices = choices;
                            row = r;
                            col = c;
                            found = true;

                            if (choices == 1) break; // 最も制約の厳しいセルを発見
                        }
                    }
                }
                if (minChoices == 1) break;
            }

            return found;
        }

        private int[][] CloneGrid(int[][] grid)
        {
            int[][] clone = new int[grid.Length][];
            for (int i = 0; i < grid.Length; i++)
            {
                clone[i] = new int[grid[i].Length];
                Array.Copy(grid[i], clone[i], grid[i].Length);
            }
            return clone;
        }

        private int CountHints(int[][] grid)
        {
            int count = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (grid[i][j] != 0) count++;
                }
            }
            return count;
        }
        #endregion

        #region 最適化された状態機械実装
        /// <summary>
        /// ビット演算最適化版バックトラッキング状態機械
        /// </summary>
        private class OptimizedStateMachineSolver
        {
            private readonly RandomPuzzleGenerator _generator;
            private Stack<OptimizedSolverState> _stateStack;
            private int _operationCount;

            public OptimizedStateMachineSolver(RandomPuzzleGenerator generator)
            {
                _generator = generator;
            }

            public IEnumerator SolveCoroutine(int[][] grid, Action<bool> onComplete = null)
            {
                _stateStack = new Stack<OptimizedSolverState>();
                _operationCount = 0;

                int startRow = 0, startCol = 0;
                if (_generator.FindMostConstrainedCell(grid, ref startRow, ref startCol))
                {
                    int availableMask = _generator.GetAvailableNumbersMask(grid, startRow, startCol);
                    _stateStack.Push(new OptimizedSolverState(startRow, startCol, availableMask));
                }
                else
                {
                    onComplete?.Invoke(true);
                    yield break;
                }

                while (_stateStack.Count > 0)
                {
                    var currentState = _stateStack.Pop();
                    _operationCount++;

                    if (_operationCount % _generator._operationsPerFrame == 0)
                    {
                        yield return null;
                    }

                    if (currentState.AvailableMask != 0)
                    {
                        // 最初の利用可能な数字を取得
                        int num = GetFirstAvailableNumber(currentState.AvailableMask);

                        // その数字を試行済みにする
                        currentState.AvailableMask &= ~_generator._numberMasks[num];

                        if (_generator.IsValidFast(grid, currentState.Row, currentState.Col, num))
                        {
                            grid[currentState.Row][currentState.Col] = num;

                            int nextRow = currentState.Row, nextCol = currentState.Col;
                            if (_generator.FindMostConstrainedCell(grid, ref nextRow, ref nextCol))
                            {
                                // 現在の状態を戻す（他の数字も試すため）
                                _stateStack.Push(currentState);

                                // 次のセルの状態を追加
                                int nextAvailableMask = _generator.GetAvailableNumbersMask(grid, nextRow, nextCol);
                                _stateStack.Push(new OptimizedSolverState(nextRow, nextCol, nextAvailableMask));
                            }
                            else
                            {
                                // 解決完了
                                onComplete?.Invoke(true);
                                yield break;
                            }
                        }
                        else
                        {
                            // この数字は無効、次の数字を試す
                            _stateStack.Push(currentState);
                        }
                    }
                    else
                    {
                        // この位置では解決不可能、バックトラック
                        grid[currentState.Row][currentState.Col] = 0;
                    }
                }

                onComplete?.Invoke(false);
            }

            private int GetFirstAvailableNumber(int mask)
            {
                for (int i = 1; i <= _generator.Size; i++)
                {
                    if ((mask & _generator._numberMasks[i]) != 0)
                        return i;
                }
                return 0;
            }
        }

        private class OptimizedSolverState
        {
            public int Row;
            public int Col;
            public int AvailableMask;

            public OptimizedSolverState(int row, int col, int availableMask)
            {
                Row = row;
                Col = col;
                AvailableMask = availableMask;
            }
        }

        /// <summary>
        /// ビット演算最適化版一意解チェック
        /// </summary>
        private class OptimizedUniquenessChecker
        {
            private readonly RandomPuzzleGenerator _generator;

            public OptimizedUniquenessChecker(RandomPuzzleGenerator generator)
            {
                _generator = generator;
            }

            public IEnumerator CheckUniquenesCoroutine(int[][] grid, Action<bool> onComplete)
            {
                int solutionCount = 0;
                yield return CountSolutionsCoroutine(CloneGrid(grid), 2, (count) => solutionCount = count);
                onComplete?.Invoke(solutionCount == 1);
            }

            private IEnumerator CountSolutionsCoroutine(int[][] grid, int maxSolutions, Action<int> onComplete)
            {
                var stack = new Stack<OptimizedCounterState>();
                int solutionCount = 0;
                int operationCount = 0;

                int startRow = 0, startCol = 0;
                if (_generator.FindMostConstrainedCell(grid, ref startRow, ref startCol))
                {
                    int availableMask = _generator.GetAvailableNumbersMask(grid, startRow, startCol);
                    stack.Push(new OptimizedCounterState(startRow, startCol, availableMask));
                }
                else
                {
                    onComplete?.Invoke(1);
                    yield break;
                }

                while (stack.Count > 0 && solutionCount < maxSolutions)
                {
                    var currentState = stack.Pop();
                    operationCount++;

                    if (operationCount % _generator._operationsPerFrame == 0)
                    {
                        yield return null;
                    }

                    if (currentState.AvailableMask != 0)
                    {
                        int num = GetFirstAvailableNumber(currentState.AvailableMask);
                        currentState.AvailableMask &= ~_generator._numberMasks[num];

                        if (_generator.IsValidFast(grid, currentState.Row, currentState.Col, num))
                        {
                            grid[currentState.Row][currentState.Col] = num;

                            int nextRow = currentState.Row, nextCol = currentState.Col;
                            if (_generator.FindMostConstrainedCell(grid, ref nextRow, ref nextCol))
                            {
                                stack.Push(currentState);
                                int nextAvailableMask = _generator.GetAvailableNumbersMask(grid, nextRow, nextCol);
                                stack.Push(new OptimizedCounterState(nextRow, nextCol, nextAvailableMask));
                            }
                            else
                            {
                                solutionCount++;
                                grid[currentState.Row][currentState.Col] = 0;
                                stack.Push(currentState);
                            }
                        }
                        else
                        {
                            stack.Push(currentState);
                        }
                    }
                    else
                    {
                        grid[currentState.Row][currentState.Col] = 0;
                    }
                }

                onComplete?.Invoke(solutionCount);
            }

            private int GetFirstAvailableNumber(int mask)
            {
                for (int i = 1; i <= _generator.Size; i++)
                {
                    if ((mask & _generator._numberMasks[i]) != 0)
                        return i;
                }
                return 0;
            }

            private int[][] CloneGrid(int[][] grid)
            {
                int[][] clone = new int[grid.Length][];
                for (int i = 0; i < grid.Length; i++)
                {
                    clone[i] = new int[grid[i].Length];
                    Array.Copy(grid[i], clone[i], grid[i].Length);
                }
                return clone;
            }
        }

        private class OptimizedCounterState
        {
            public int Row;
            public int Col;
            public int AvailableMask;

            public OptimizedCounterState(int row, int col, int availableMask)
            {
                Row = row;
                Col = col;
                AvailableMask = availableMask;
            }
        }
        #endregion
    }
}