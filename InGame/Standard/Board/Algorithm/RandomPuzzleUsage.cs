//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using MantenseiLib;
//using NumberPlace.Standard;
//using System.Linq;
//using System;
//using TMPro;

//public class RandomPuzzleUsage : MonoBehaviour
//{
//    [SerializeField] private RandomPuzzleGenerator puzzleGenerator;
//    [SerializeField] private int targetHints = 25;
//    [SerializeField] private int timeoutSeconds = 60; // タイムアウト（秒）
//    [SerializeField] private bool useMultiThreading = true; // マルチスレッドを使用するか

//    // UIコンポーネント（必要に応じて追加）
//    [SerializeField] private TextMeshProUGUI statusText;
//    [SerializeField] private TextMeshProUGUI hintCountText;
//    [SerializeField] private UnityEngine.UI.Button generateButton;

//    private PuzzleGenerator currentPuzzle;
//    private System.Threading.CancellationTokenSource cancellationToken;

//    private void Start()
//    {
//        // 必要に応じてRandomPuzzleGeneratorを作成
//        if (puzzleGenerator == null)
//        {
//            //puzzleGenerator = gameObject.AddComponent<RandomPuzzleGenerator>();
//        }

//        // ボタンのリスナーを設定（UIがある場合）
//        if (generateButton != null)
//        {
//            generateButton.onClick.AddListener(StartPuzzleGeneration);
//        }
//    }

//    public void StartPuzzleGeneration()
//    {
//        if (useMultiThreading)
//        {
//            StartCoroutine(GeneratePuzzleAsync());
//        }
//        else
//        {
//            GeneratePuzzle();
//        }
//    }

//    // 同期的にパズルを生成（UIがブロックされる可能性あり）
//    private void GeneratePuzzle()
//    {
//        UpdateStatus("パズル生成中...");
        
//        // 生成開始時間
//        DateTime startTime = DateTime.Now;
        
//        // パズル生成
//        try
//        {
//            int[][] puzzleGrid = puzzleGenerator.GenerateMinimalPuzzle();
//            currentPuzzle = puzzleGenerator.CreatePuzzleGenerator();

//            // 結果表示
//            int hintCount = CountHints(puzzleGrid);
//            UpdateStatus($"パズル生成完了! ヒント数: {hintCount}");
//            UpdateHintCount(hintCount);
            
//            // パズルをゲームに適用
//            ApplyPuzzleToGame(currentPuzzle);
//        }
//        catch (Exception e)
//        {
//            UpdateStatus($"エラー: {e.Message}");
//            Debug.LogError($"パズル生成エラー: {e}");
//        }
//    }

//    // 非同期でパズルを生成（UIブロックを防ぐ）
//    private IEnumerator GeneratePuzzleAsync()
//    {
//        UpdateStatus("パズル生成中...");
        
//        // キャンセレーショントークンを作成
//        if (cancellationToken != null)
//        {
//            cancellationToken.Cancel();
//        }
//        cancellationToken = new System.Threading.CancellationTokenSource();
        
//        // 生成開始時間
//        DateTime startTime = DateTime.Now;
        
//        // バックグラウンドスレッドでパズルを生成
//        int[][] puzzleGrid = null;
//        Exception error = null;
        
//        // スレッドプールで実行
//        bool isComplete = false;
//        System.Threading.ThreadPool.QueueUserWorkItem(_ => 
//        {
//            try
//            {
//                puzzleGrid = puzzleGenerator.GenerateMinimalPuzzle();
//                isComplete = true;
//            }
//            catch (Exception e)
//            {
//                error = e;
//                isComplete = true;
//            }
//        });
        
//        // 完了またはタイムアウトまで待機
//        while (!isComplete)
//        {
//            // タイムアウトチェック
//            if ((DateTime.Now - startTime).TotalSeconds > timeoutSeconds)
//            {
//                cancellationToken.Cancel();
//                UpdateStatus("タイムアウト: 生成に時間がかかりすぎています");
//                yield break;
//            }
            
//            // 進行状況の更新（例：経過時間）
//            float elapsedSeconds = (float)(DateTime.Now - startTime).TotalSeconds;
//            UpdateStatus($"パズル生成中... ({elapsedSeconds:F1}秒経過)");
            
//            yield return new WaitForSeconds(0.2f);
//        }
        
//        // エラーチェック
//        if (error != null)
//        {
//            UpdateStatus($"エラー: {error.Message}");
//            Debug.LogError($"パズル生成エラー: {error}");
//            yield break;
//        }
        
//        // 結果を処理
//        try
//        {
//            currentPuzzle = puzzleGenerator.CreatePuzzleGenerator();
            
//            int hintCount = CountHints(puzzleGrid);
//            UpdateStatus($"パズル生成完了! ヒント数: {hintCount}");
//            UpdateHintCount(hintCount);
            
//            // パズルをゲームに適用
//            ApplyPuzzleToGame(currentPuzzle);
//        }
//        catch (Exception e)
//        {
//            UpdateStatus($"エラー: {e.Message}");
//            Debug.LogError($"結果処理エラー: {e}");
//        }
//    }

//    // パズルをゲームに適用する（必要に応じて実装）
//    private void ApplyPuzzleToGame(PuzzleGenerator puzzle)
//    {
//        // ここに、生成されたパズルをゲームボードに適用するコードを記述
//        // 例：ゲームマネージャーにパズルを渡す、セルを更新する、など
//        Debug.Log("新しいパズルがゲームに適用されました");
        
//        // デモ：パズルをデバッグログに表示
//        puzzleGenerator.PrintGrid(GetPuzzleGridFromGenerator(puzzle));
//    }

//    // PuzzleGeneratorから二次元配列を取得
//    private int[][] GetPuzzleGridFromGenerator(PuzzleGenerator generator)
//    {
//        int size = generator.Size;
//        int[][] grid = new int[size][];
        
//        for (int i = 0; i < size; i++)
//        {
//            grid[i] = new int[size];
//        }
        
//        foreach (var cell in generator.Cells)
//        {
//            if (cell.IsVisible)
//            {
//                grid[cell.Y][cell.X] = cell.Num + 1; // 0-indexedを1-indexedに変換
//            }
//            else
//            {
//                grid[cell.Y][cell.X] = 0; // 空のセル
//            }
//        }
        
//        return grid;
//    }

//    // ヒント数を数える
//    private int CountHints(int[][] grid)
//    {
//        int count = 0;
//        for (int i = 0; i < grid.Length; i++)
//        {
//            for (int j = 0; j < grid[i].Length; j++)
//            {
//                if (grid[i][j] != 0)
//                {
//                    count++;
//                }
//            }
//        }
//        return count;
//    }

//    // ステータステキストを更新
//    private void UpdateStatus(string message)
//    {
//        if (statusText != null)
//        {
//            statusText.text = message;
//        }
//        Debug.Log(message);
//    }

//    // ヒント数テキストを更新
//    private void UpdateHintCount(int count)
//    {
//        if (hintCountText != null)
//        {
//            hintCountText.text = $"ヒント数: {count}";
//        }
//    }

//    private void OnDestroy()
//    {
//        // キャンセレーショントークンをクリーンアップ
//        if (cancellationToken != null)
//        {
//            cancellationToken.Cancel();
//            cancellationToken.Dispose();
//        }
//    }
//}