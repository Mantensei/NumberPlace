using MantenseiLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace NumberPlace.Standard
{
    public class NumberTable
    {     
        public int[][] GenerateTable(int blockSize)
        {
            int size = blockSize * blockSize;
            var table = new int[size][];
            for (int i = 0; i < size; i++)
                table[i] = new int[size];

            FillBoard(table, blockSize);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var num = table[i][j];
                    table[i][j] = num - 1;
                }
            }

            //呼ばなくてもランダム性は高いのでパフォーマンスと相談しながら
            ShuffleTable(ref table, blockSize);

            return table;
        }

        void ShuffleTable<T>(ref T[][] source, int blockSize)
        {
            int size = (int)Mathf.Pow(blockSize, 2);

            void Transpose(ref T[][] matrix)
            {
                int n = matrix.Length;
                int m = matrix[0].Length;
                var result = new T[m][];
                for (int i = 0; i < m; i++)
                    result[i] = new T[n];

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                        result[j][i] = matrix[i][j];

                matrix = result;
            }

            void ShuffleRows<_T>(ref _T[] table)
            {
                var result = new _T[blockSize][];

                //ブロック内でのシャッフル
                for (int i = 0; i < blockSize; i++)
                {
                    int start = i * blockSize;
                    var group = table.Skip(start).Take(blockSize).Shuffle().ToArray();
                    result[i] = group;
                }

                //ブロック間でのシャッフル
                result = result.Shuffle().ToArray();

                table = result.SelectMany(x => x).ToArray();
            }


            void ShuffleColumns(ref T[][] table)
            {
                Transpose(ref table);
                ShuffleRows(ref table);
            }

            for (int i = 0; i < Mathf.Pow(size, 1); i++)
            {
                try
                {
                    ShuffleRows(ref source);
                    ShuffleColumns(ref source);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Shuffle failed: {e.Message}");
                    continue;
                }
            }
        }

        bool FillBoard(int[][] table, int blockSize, int row = 0, int col = 0)
        {
            int size = blockSize * blockSize;

            if (row == size) return true;

            int nextRow = col == size - 1 ? row + 1 : row;
            int nextCol = (col + 1) % size;

            var numbers = Enumerable.Range(1, size).Shuffle().ToArray();

            foreach (var num in numbers)
            {
                if (IsValid(table, blockSize, row, col, num))
                {
                    table[row][col] = num;

                    if (FillBoard(table, blockSize, nextRow, nextCol))
                        return true;

                    table[row][col] = -1; // バックトラック
                }
            }

            return false;
        }

        bool IsValid(int[][] table, int blockSize, int row, int col, int num)
        {
            int size = blockSize * blockSize;

            // 行・列チェック
            for (int i = 0; i < size; i++)
            {
                if (table[row][i] == num || table[i][col] == num)
                    return false;
            }

            // ブロックチェック
            int blockRow = (row / blockSize) * blockSize;
            int blockCol = (col / blockSize) * blockSize;

            for (int i = 0; i < blockSize; i++)
            {
                for (int j = 0; j < blockSize; j++)
                {
                    if (table[blockRow + i][blockCol + j] == num)
                        return false;
                }
            }

            return true;
        }
    } 
}
