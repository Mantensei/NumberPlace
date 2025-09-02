using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;
using MantenseiLib;

namespace NumberPlace.UI
{
    public class Timer_UI : MonoBehaviour, IBoardGenerateHandler, ICallByGameEvent
    {
        [field:SerializeField]
        public TextMeshProUGUI Title_Txt { get; private set; }

        [field: SerializeField]
        public TextMeshProUGUI Content_Txt { get; private set; }

        bool pause = true;
        float pastTime => StatusManager.Time;

        private void Update()
        {
            if (pastTime < 0f)
            {
                var periodCount = (int)Time.time % 3 + 1;
                Content_Txt.text = $"<size=16>generating{Enumerable.Repeat(".", periodCount).JoinToString(JoinFormat.None)}</size>";
                //return;
            }

            if (!pause)
            {
                StatusManager.Instance.AddTime(Time.deltaTime);
                Content_Txt.text = $"{(int)(pastTime / 60):00}:{(int)(pastTime % 60):00}"; // MM:SS
            }
        }

        public void HandleBoardInfo(BoardData data)
        {
            pause = false;
        }

        public void CallByGameEvent(GameEventInfo eventInfo)
        {
            var state = eventInfo.EventTiming;
            if (state == GameEvent.StageClear || state == GameEvent.GameOver)
                pause = true;
        }
    }
}
