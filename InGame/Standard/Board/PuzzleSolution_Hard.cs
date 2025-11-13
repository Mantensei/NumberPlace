using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NumberPlace.Standard
{
    public partial class PuzzleGenerator
    {
        // X-Wing
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Hard, PuzzleSolutionType.Func)]
        PuzzleCell[] XWing()
        {
            UpdateAllCandidates();
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // 行と列の両方でX-Wingをチェック
            for (int indexType = 0; indexType <= 1; indexType++)
            {
                IndexType primary = (IndexType)indexType;
                IndexType secondary = indexType == 0 ? IndexType.Column : IndexType.Row;

                // 各数字について処理
                for (int num = 0; num < Size; num++)
                {
                    FindXWing(num, primary, secondary, updatedCells);
                }
            }

            return updatedCells.ToArray();
        }

        private void FindXWing(int num, IndexType primary, IndexType secondary, List<PuzzleCell> updatedCells)
        {
            // 各行/列を確認し、特定の数字の候補を持つセルの位置を記録
            var positions = new Dictionary<int, List<int>>();

            for (int i = 0; i < Size; i++)
            {
                var cells = GetIndexOfGroup((int)primary, i)
                    .Where(c => c.State == PuzzleCellState.Empty && c.Candidates.Contains(num));

                var secondaryPositions = cells.Select(c => c.IndexOfAddress((int)secondary)).ToList();

                // この数字の候補が2つだけある行/列だけを記録
                if (secondaryPositions.Count == 2)
                {
                    positions[i] = secondaryPositions;
                }
            }

            // X-Wing パターンを探す
            foreach (var entry1 in positions)
            {
                foreach (var entry2 in positions.Where(e => e.Key > entry1.Key))
                {
                    // 2つの行/列で同じ列/行に候補がある場合
                    if (entry1.Value.SequenceEqual(entry2.Value))
                    {
                        // X-Wing パターンが見つかった
                        int primary1 = entry1.Key;
                        int primary2 = entry2.Key;
                        int secondary1 = entry1.Value[0];
                        int secondary2 = entry1.Value[1];

                        // 同じ列/行の他のセルから候補を除去
                        RemoveCandidateFromXWing(num, primary, secondary, primary1, primary2, secondary1, secondary2, updatedCells);
                    }
                }
            }
        }

        private void RemoveCandidateFromXWing(int num, IndexType primary, IndexType secondary,
            int primary1, int primary2, int secondary1, int secondary2, List<PuzzleCell> updatedCells)
        {
            // 2つの列/行から候補を除去
            for (int i = 0; i < Size; i++)
            {
                if (i != primary1 && i != primary2)  // X-Wingの4隅以外
                {
                    // secondary1の列/行から除去
                    var cell1 = _cloneBoard.FirstOrDefault(c =>
                        c.IndexOfAddress((int)primary) == i &&
                        c.IndexOfAddress((int)secondary) == secondary1 &&
                        c.State == PuzzleCellState.Empty &&
                        c.Candidates.Contains(num));

                    if (cell1 != null)
                    {
                        cell1.Candidates.Remove(num);
                        if (!updatedCells.Contains(cell1))
                            updatedCells.Add(cell1);
                    }

                    // secondary2の列/行から除去
                    var cell2 = _cloneBoard.FirstOrDefault(c =>
                        c.IndexOfAddress((int)primary) == i &&
                        c.IndexOfAddress((int)secondary) == secondary2 &&
                        c.State == PuzzleCellState.Empty &&
                        c.Candidates.Contains(num));

                    if (cell2 != null)
                    {
                        cell2.Candidates.Remove(num);
                        if (!updatedCells.Contains(cell2))
                            updatedCells.Add(cell2);
                    }
                }
            }
        }

        // Y-Wing (XYZ-Wing特殊ケース)
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Hard, PuzzleSolutionType.Func)]
        PuzzleCell[] YWing()
        {
            UpdateAllCandidates();
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // ピボットとなる可能性のあるセルを検索 (候補が2つのセル)
            var pivotCells = _cloneBoard.Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.Candidates.Count == 2).ToArray();

            foreach (var pivot in pivotCells)
            {
                // ピボットの2つの候補を取得
                var pivotCandidates = pivot.Candidates.ToArray();
                if (pivotCandidates.Length != 2) continue;

                int x = pivotCandidates[0];
                int y = pivotCandidates[1];

                // ピンセルを検索 (xy, xz の候補を持つセル)
                FindYWingPincers(pivot, x, y, updatedCells);
            }

            return updatedCells.ToArray();
        }

        private void FindYWingPincers(PuzzleCell pivot, int x, int y, List<PuzzleCell> updatedCells)
        {
            // ピボットと共有するセル (行、列、またはブロック)
            var connectedCells = GetAroundCells(pivot).Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.Candidates.Count == 2 &&
                !c.Address.Equals(pivot.Address)).ToArray();

            // x を含むピンセル
            var xPincers = connectedCells.Where(c =>
                c.Candidates.Contains(x) &&
                !c.Candidates.Contains(y)).ToArray();

            // y を含むピンセル
            var yPincers = connectedCells.Where(c =>
                c.Candidates.Contains(y) &&
                !c.Candidates.Contains(x)).ToArray();

            // 全てのピンセルの組み合わせをチェック
            foreach (var xPincer in xPincers)
            {
                int z = xPincer.Candidates.First(n => n != x);  // XZ ピンセルのZ値

                foreach (var yPincer in yPincers)
                {
                    if (yPincer.Candidates.Contains(z))  // YZ ピンセル
                    {
                        // Y-Wing が見つかった
                        // xPincerとyPincerの両方から見えるセルからzを除去
                        var xCells = GetAroundCells(xPincer);
                        var yCells = GetAroundCells(yPincer);

                        // 両方から見えるセルを検索
                        var commonCells = xCells.Intersect(yCells, new AddressEqualityComparer())
                            .Where(c =>
                                c.State == PuzzleCellState.Empty &&
                                c.Candidates.Contains(z) &&
                                !c.Address.Equals(pivot.Address) &&
                                !c.Address.Equals(xPincer.Address) &&
                                !c.Address.Equals(yPincer.Address)).ToArray();

                        // z を除去
                        foreach (var cell in commonCells)
                        {
                            cell.Candidates.Remove(z);
                            if (!updatedCells.Contains(cell))
                                updatedCells.Add(cell);
                        }
                    }
                }
            }
        }

        // Swordfish
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Hard, PuzzleSolutionType.Func)]
        PuzzleCell[] Swordfish()
        {
            UpdateAllCandidates();
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // 行と列の両方でSwordfishをチェック
            for (int indexType = 0; indexType <= 1; indexType++)
            {
                IndexType primary = (IndexType)indexType;
                IndexType secondary = indexType == 0 ? IndexType.Column : IndexType.Row;

                // 各数字について処理
                for (int num = 0; num < Size; num++)
                {
                    FindSwordfish(num, primary, secondary, updatedCells);
                }
            }

            return updatedCells.ToArray();
        }

        private void FindSwordfish(int num, IndexType primary, IndexType secondary, List<PuzzleCell> updatedCells)
        {
            // 各行/列を確認し、特定の数字の候補を持つセルの位置を記録
            var positions = new Dictionary<int, List<int>>();

            for (int i = 0; i < Size; i++)
            {
                var cells = GetIndexOfGroup((int)primary, i)
                    .Where(c => c.State == PuzzleCellState.Empty && c.Candidates.Contains(num));

                var secondaryPositions = cells.Select(c => c.IndexOfAddress((int)secondary)).ToList();

                // この数字の候補が2つか3つある行/列だけを記録
                if (secondaryPositions.Count >= 2 && secondaryPositions.Count <= 3)
                {
                    positions[i] = secondaryPositions;
                }
            }

            // 3つの行/列を選ぶ全ての組み合わせを試す
            var keys = positions.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                for (int j = i + 1; j < keys.Length; j++)
                {
                    for (int k = j + 1; k < keys.Length; k++)
                    {
                        int key1 = keys[i];
                        int key2 = keys[j];
                        int key3 = keys[k];

                        // 3つの行/列のセルの位置をマージ
                        var allPositions = new HashSet<int>();
                        allPositions.UnionWith(positions[key1]);
                        allPositions.UnionWith(positions[key2]);
                        allPositions.UnionWith(positions[key3]);

                        // Swordfishパターン：3つの行/列で合計3つの列/行に候補がある
                        if (allPositions.Count == 3)
                        {
                            var secondaryPositions = allPositions.ToArray();

                            // 同じ列/行の他のセルから候補を除去
                            RemoveCandidateFromSwordfish(num, primary, secondary,
                                new[] { key1, key2, key3 }, secondaryPositions, updatedCells);
                        }
                    }
                }
            }
        }

        private void RemoveCandidateFromSwordfish(int num, IndexType primary, IndexType secondary,
            int[] primaryKeys, int[] secondaryPositions, List<PuzzleCell> updatedCells)
        {
            // 3つの列/行の3つの候補から候補を除去
            foreach (int secondaryPos in secondaryPositions)
            {
                for (int i = 0; i < Size; i++)
                {
                    // Swordfishの9隅以外のセルから候補を除去
                    if (!primaryKeys.Contains(i))
                    {
                        var cell = _cloneBoard.FirstOrDefault(c =>
                            c.IndexOfAddress((int)primary) == i &&
                            c.IndexOfAddress((int)secondary) == secondaryPos &&
                            c.State == PuzzleCellState.Empty &&
                            c.Candidates.Contains(num));

                        if (cell != null)
                        {
                            cell.Candidates.Remove(num);
                            if (!updatedCells.Contains(cell))
                                updatedCells.Add(cell);
                        }
                    }
                }
            }
        }

        // XYZ-Wing
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Hard, PuzzleSolutionType.Func)]
        PuzzleCell[] XYZWing()
        {
            UpdateAllCandidates();
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // ピボットとなる可能性のあるセルを検索 (候補が3つのセル)
            var pivotCells = _cloneBoard.Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.Candidates.Count == 3).ToArray();

            foreach (var pivot in pivotCells)
            {
                // ピボットの3つの候補を取得
                var xyz = pivot.Candidates.ToArray();
                if (xyz.Length != 3) continue;

                for (int i = 0; i < 3; i++)
                {
                    int z = xyz[i];  // 共通の候補
                    int x = xyz[(i + 1) % 3];
                    int y = xyz[(i + 2) % 3];

                    FindXYZWingPincers(pivot, x, y, z, updatedCells);
                }
            }

            return updatedCells.ToArray();
        }

        private void FindXYZWingPincers(PuzzleCell pivot, int x, int y, int z, List<PuzzleCell> updatedCells)
        {
            // ピボットと共有するセル (行、列、またはブロック)
            var connectedCells = GetAroundCells(pivot).Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.Candidates.Count == 2 &&
                !c.Address.Equals(pivot.Address)).ToArray();

            // x, z を含むピンセル
            var xzPincers = connectedCells.Where(c =>
                c.Candidates.Contains(x) &&
                c.Candidates.Contains(z) &&
                !c.Candidates.Contains(y)).ToArray();

            // y, z を含むピンセル
            var yzPincers = connectedCells.Where(c =>
                c.Candidates.Contains(y) &&
                c.Candidates.Contains(z) &&
                !c.Candidates.Contains(x)).ToArray();

            // 全てのピンセルの組み合わせをチェック
            foreach (var xzPincer in xzPincers)
            {
                foreach (var yzPincer in yzPincers)
                {
                    // XYZ-Wing が見つかった
                    // 3つのセル (pivot, xzPincer, yzPincer) 全てから見えるセルからzを除去
                    var pivotCells = GetAroundCells(pivot);
                    var xzCells = GetAroundCells(xzPincer);
                    var yzCells = GetAroundCells(yzPincer);

                    // 3つのセル全てから見えるセルを検索
                    var commonCells = pivotCells
                        .Intersect(xzCells, new AddressEqualityComparer())
                        .Intersect(yzCells, new AddressEqualityComparer())
                        .Where(c =>
                            c.State == PuzzleCellState.Empty &&
                            c.Candidates.Contains(z) &&
                            !c.Address.Equals(pivot.Address) &&
                            !c.Address.Equals(xzPincer.Address) &&
                            !c.Address.Equals(yzPincer.Address)).ToArray();

                    // z を除去
                    foreach (var cell in commonCells)
                    {
                        cell.Candidates.Remove(z);
                        if (!updatedCells.Contains(cell))
                            updatedCells.Add(cell);
                    }
                }
            }
        }
    }

    // アドレスで比較するためのコンパレータークラス
    public class AddressEqualityComparer : IEqualityComparer<PuzzleCell>
    {
        public bool Equals(PuzzleCell x, PuzzleCell y)
        {
            return x.Address == y.Address;
        }

        public int GetHashCode(PuzzleCell obj)
        {
            return obj.Address.GetHashCode();
        }
    }
}