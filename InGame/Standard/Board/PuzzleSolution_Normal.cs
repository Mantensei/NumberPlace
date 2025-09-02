using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace NumberPlace.Standard
{
    public partial class PuzzleGenerator
    {
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Normal, PuzzleSolutionType.Func)]
        PuzzleCell[] NakedSubsets()
        {
            UpdateAllCandidates();

            bool SearchLine(PuzzleCell[][] cellLines)
            {
                bool update = false;

                foreach(var line in cellLines)
                {
                    if (CalcCommonDivisor(line, out var result))
                    {
                        foreach(var cell in line)
                        {
                            foreach(var hashSet in result)
                            {
                                if(cell.Candidates.Count > hashSet.Count)
                                {
                                    var hashCount = cell.Candidates.Count();
                                    cell.Candidates.ExceptWith(hashSet);

                                    if(hashCount != cell.Candidates.Count())
                                        update = true;
                                }
                            }

                        }
                    }
                }

                return update;
            }

            int safety = 0;
            while (true)
            {
                //何も更新されなかったら終了
                if (!SearchLine(GetRows()) && !SearchLine(GetColumns()) && !SearchLine(GetBlocks()))
                {
                    //Debug.Log("finish:" + safety);
                    break;
                }

                safety++;
                if(safety >= 20)
                {
                    //Debug.Log("safety");
                    break;
                }
            }

            return _cloneBoard.Where(x => x.Candidates.Count == 1).ToArray();
        }

        bool CalcCommonDivisor(PuzzleCell[] source, out HashSet<int>[] result)
        {
            var combinations = GetCombinations(source);
            var hashList = new List<HashSet<int>>();
            bool found = false;

            foreach (var combination in combinations)
            {
                if (FindCommonElements(combination, out var hashSet))
                {
                    hashList.Add(hashSet);
                    found = true;
                }
            }

            result = hashList.ToArray();
            return found;
        }

        bool FindCommonElements(HashSet<int>[] sets, out HashSet<int> commonElements)
        {
            if (sets.Length == 0)
            {
                commonElements = new HashSet<int>();
                return false;
            }

            // 最初のセットからスタート
            commonElements = new HashSet<int>(sets[0]);

            // 残りの各セットとの交差を取る
            for (int i = 1; i < sets.Length; i++)
            {
                commonElements.IntersectWith(sets[i]);
            }

            // 共通要素が存在するか
            return commonElements.Count > 0;
        }

        public List<HashSet<int>[]> GetCombinations(PuzzleCell[] source)
        {
            var result = new List<HashSet<int>[]>();
            var group = source.Select(x => x.Candidates).ToArray();

            //例）length=3で、(0,1,2,3)の要素...共通項の要素数がlengthを超えているのでスキップ
            //例）Size=9で9個の要素...何も解決しないのでスキップ
            //例）Size=9で8個の要素...この関数で解決できるなら、余る要素は１個のみ。であれば他の解法でも確定できているはずなのでスキップ
            var length = group.Length;
            for (int r = 1; r <= length - 2; r++)
            {
                foreach (var combination in GetCombinations(group, r))
                {
                    if (combination.Contains(x => x.Count > length))
                        continue;

                    result.Add(combination.ToArray());
                }
            }

            return result;
        }

        // 汎用組み合わせ列挙（r個取り出す）
        public IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> source, int r)
        {
            if (r == 0) yield return Enumerable.Empty<T>();
            else
            {
                int index = 0;
                foreach (var item in source)
                {
                    var remaining = source.Skip(index + 1);
                    foreach (var subcombo in GetCombinations(remaining, r - 1))
                    {
                        yield return new[] { item }.Concat(subcombo);
                    }
                    index++;
                }
            }
        }

        // ポインティングペア/トリプル - メインメソッド
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Normal, PuzzleSolutionType.Func)]
        PuzzleCell[] LockedCandidatePointing()
        {
            // 全てのセルの候補を更新
            UpdateAllCandidates();

            // 更新されたセルを格納するリスト
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // 全てのブロックに対して処理
            for (int blockIndex = 0; blockIndex < Size; blockIndex++)
            {
                // 各ブロックのポインティング候補を処理
                ProcessBlockPointing(blockIndex, updatedCells);
            }

            return updatedCells.ToArray();
        }

        // 各ブロックのポインティング処理
        private void ProcessBlockPointing(int blockIndex, List<PuzzleCell> updatedCells)
        {
            var blockCells = GetIndexOfGroup((int)IndexType.Block, blockIndex).Where(c => c.State == PuzzleCellState.Empty).ToArray();
            if (blockCells.Length == 0) return;

            // 各数字について処理
            for (int num = 0; num < Size; num++)
            {
                var cellsWithNum = blockCells.Where(c => c.Candidates.Contains(num)).ToArray();
                if (cellsWithNum.Length < 2 || cellsWithNum.Length >= Size) continue;

                // 行ポインティングと列ポインティングを確認
                for (int indexType = 0; indexType <= 1; indexType++)
                {
                    CheckPointing(blockIndex, num, cellsWithNum, (IndexType)indexType, updatedCells);
                }
            }
        }

        // ポインティングのチェック（行または列）
        private void CheckPointing(int blockIndex, int num, PuzzleCell[] cellsWithNum, IndexType indexType, List<PuzzleCell> updatedCells)
        {
            var groups = cellsWithNum.GroupBy(c => c.IndexOfAddress((int)indexType)).ToArray();
            foreach (var group in groups)
            {
                if (group.Count() == cellsWithNum.Length)
                {
                    // このブロックのこの数字の候補が全て同じ行または列にある
                    int address = group.Key;
                    RemoveCandidateFromLine(blockIndex, address, indexType, num, updatedCells);
                }
            }
        }

        // 同じ行または列の他のブロックから候補を削除
        private void RemoveCandidateFromLine(int blockIndex, int address, IndexType indexType, int num, List<PuzzleCell> updatedCells)
        {
            var sameLine = GetIndexOfGroup((int)indexType, address).Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.Block != blockIndex &&
                c.Candidates.Contains(num)).ToArray();

            RemoveCandidatesAndUpdateList(sameLine, num, updatedCells);
        }

        // クレーミングペア/トリプル - メインメソッド
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Normal, PuzzleSolutionType.Func)]
        PuzzleCell[] ClaimingPairs()
        {
            // 全てのセルの候補を更新
            UpdateAllCandidates();

            // 更新されたセルを格納するリスト
            List<PuzzleCell> updatedCells = new List<PuzzleCell>();

            // 行と列ごとにクレーミングをチェック
            for (int indexType = 0; indexType <= 1; indexType++)
            {
                for (int address = 0; address < Size; address++)
                {
                    ProcessLineClaiming(address, (IndexType)indexType, updatedCells);
                }
            }

            return updatedCells.ToArray();
        }

        // 行または列のクレーミング処理
        private void ProcessLineClaiming(int address, IndexType indexType, List<PuzzleCell> updatedCells)
        {
            var lineCells = GetIndexOfGroup((int)indexType, address).Where(c => c.State == PuzzleCellState.Empty).ToArray();
            if (lineCells.Length == 0) return;

            // 各数字について処理
            for (int num = 0; num < Size; num++)
            {
                var cellsWithNum = lineCells.Where(c => c.Candidates.Contains(num)).ToArray();
                if (cellsWithNum.Length < 2) continue;

                CheckLineBlockClaiming(address, indexType, num, cellsWithNum, updatedCells);
            }
        }

        // 行または列のブロッククレーミングチェック
        private void CheckLineBlockClaiming(int address, IndexType indexType, int num, PuzzleCell[] cellsWithNum, List<PuzzleCell> updatedCells)
        {
            var blockGroups = cellsWithNum.GroupBy(c => c.Block).ToArray();
            foreach (var group in blockGroups)
            {
                if (group.Count() == cellsWithNum.Length)
                {
                    // この行または列のこの数字の候補が全て同じブロックにある
                    int block = group.Key;
                    RemoveCandidateFromSameBlock(block, address, indexType, num, updatedCells);
                }
            }
        }

        // 同じブロックの他のセルから候補を削除
        private void RemoveCandidateFromSameBlock(int block, int address, IndexType indexType, int num, List<PuzzleCell> updatedCells)
        {
            var sameBlock = GetIndexOfGroup((int)IndexType.Block, block).Where(c =>
                c.State == PuzzleCellState.Empty &&
                c.IndexOfAddress((int)indexType) != address &&
                c.Candidates.Contains(num)).ToArray();

            RemoveCandidatesAndUpdateList(sameBlock, num, updatedCells);
        }

        // セルのリストから候補を削除し、更新リストに追加する共通処理
        private void RemoveCandidatesAndUpdateList(PuzzleCell[] cells, int num, List<PuzzleCell> updatedCells)
        {
            foreach (var cell in cells)
            {
                cell.Candidates.Remove(num);
                if (!updatedCells.Contains(cell))
                    updatedCells.Add(cell);
            }
        }
    }
}