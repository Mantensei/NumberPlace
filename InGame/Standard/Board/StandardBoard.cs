using MantenseiLib;
using NumberPlace;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NumberPlace.Standard
{
    public class StandardBoard : BaseMonoBehaviour, IBoard, IBoardGenerateHandler, ICallByGameEvent
    {
        [SerializeField] Sprite _blockGrid_img;
        [GetComponent] RectTransform rectTransform;

        public int BlockSize => CellManager.Instance.Blocksize;
        public int Size => CellManager.Instance.Size;

        public Cell[] Cells => CellManager.Instance.Cells;

        protected override void Update()
         {
            base.Update();

#if DEBUG
            if(!CellManager.Instance.Generating && CellManager.Instance.BoardData == null)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    GenerateBoard(Difficulty.Easy);
                }
            }
#endif
        }

        public void GenerateBoard(Difficulty difficulty, int blockSize = 3)
        {
            if (CellManager.Instance.BoardData != null)
                return;

            var targetHints = difficulty.ToNumCount(blockSize);
            CellManager.Instance.GenerateBoard(blockSize, targetHints, transform);
        }      


        void GenerateBlockGrids(int blockSize)
        {
            if (_blockGrid_img == null) return;

            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            float blockWidth = width / blockSize;
            float blockHeight = height / blockSize;

            for (int row = 0; row < blockSize; row++)
            {
                for (int col = 0; col < blockSize; col++)
                {
                    var go = new GameObject($"BlockGrid_{row}_{col}");
                    go.transform.SetParent(transform, false);

                    var image = go.AddComponent<UnityEngine.UI.Image>();
                    image.sprite = _blockGrid_img;
                    image.preserveAspect = true;
                    image.raycastTarget = false;

                    var rt = image.rectTransform;
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(0, 0);
                    rt.pivot = new Vector2(0.5f, 0.5f);

                    float x = (col + 0.5f) * blockWidth - width / 2f;
                    float y = height / 2f - (row + 0.5f) * blockHeight;

                    rt.localPosition = new Vector3(x, y, 0f);
                    rt.sizeDelta = new Vector2(blockWidth, blockHeight);
                }
            }
        }

        void FitToCanvas()
        {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            float cellSizeX = width / Size;
            float cellSizeY = height / Size;

            for (int i = 0; i < Cells.Length; i++)
            {
                int row = i / Size;
                int col = i % Size;

                var cellTransform = Cells[i].transform;

                // 配置（左上が原点になるように調整）
                float x = (col + 0.5f) * cellSizeX - width / 2f;
                float y = height / 2f - (row + 0.5f) * cellSizeY;

                cellTransform.localPosition = new Vector3(x, y, 0f);

                // スケール調整（見た目のサイズ変更）
                cellTransform.localScale = new Vector3(cellSizeX, cellSizeY, 1f);
            }
        }

        public void HandleBoardInfo(BoardData data)
        {
            FitToCanvas();
            GenerateBlockGrids(data.BlockSize);
        }

        public void CallByGameEvent(GameEventInfo eventInfo)
        {
            if(eventInfo.EventTiming == GameEvent.GameStart)
            {
                if(eventInfo.Content is Difficulty difficulty)
                    GenerateBoard(difficulty);
            }
        }
    }
}

public static class DifficultyExtension
{
    public static int ToNumCount(this Difficulty difficulty, int blockSize)
    {
        // 難易度ごとの「81マスにおけるヒント数」
        int baseCount = difficulty switch
        {
            Difficulty.Easy => 40,
            Difficulty.Normal => 35,
            Difficulty.Hard => 30,
            Difficulty.Expert => 20,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };

        // スケーリングして、近似値にする（切り捨て or 四捨五入はお好みで）
        //return (int)Math.Round(baseCount * (blockSize / 81.0));
        return baseCount;
    }
}

namespace NumberPlace
{
    public interface IBoard : IMonoBehaviour
    {
        Cell[] Cells { get; }
        int Size { get; }
        int BlockSize { get; }
    }
}