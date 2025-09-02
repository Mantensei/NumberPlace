using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NumberPlace.Standard
{
    public partial class PuzzleGenerator
    {
        [NumberPlaceSolution(PuzzleSolutionDifficulty.Easy)]
        bool ApplyNakedSingle(PuzzleCell cell)
        {
            var around = GetAroundNums(cell);
            var candidates = NumRange.Where(n => !around.Contains(n)).ToArray();

            if (candidates.Length == 1)
            {
                return true;
            }

            return false;
        }

        [NumberPlaceSolution(PuzzleSolutionDifficulty.Easy)]
        bool ApplyHiddenSingle(PuzzleCell cell)
        {
            return
                IsUniqueCandidateInGroup(cell, GetRow(cell))
                || IsUniqueCandidateInGroup(cell, GetColumn(cell))
                || IsUniqueCandidateInGroup(cell, GetBlock(cell))
                ;
        }
        bool IsUniqueCandidateInGroup(PuzzleCell removeCell, PuzzleCell[] aroundGroup)
        {
            int num = removeCell.Num;

            foreach (var c in aroundGroup.Empty().Remove(removeCell))
            {
                var around =
                    GetAroundCells(c)
                    .Visible()
                    .Remove(removeCell);

                if (!around.Select(x => x.Num).Contains(num))
                    return false;
            }

            return true;
        }
    }
}