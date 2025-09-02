using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace NumberPlace.UI
{
    public class GameOverCatcher : MonoBehaviour, ICallByGameEvent
    {
        [SerializeField]
        GameObject GameOverPanel;

        [SerializeField]
        TextMeshProUGUI Title;

        [SerializeField]
        Button Button;

        void Start()
        {
            Button.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Title");
            });
        }

        public void CallByGameEvent(GameEventInfo eventInfo)
        {
            var eventTiming = eventInfo.EventTiming;

            if (eventTiming == GameEvent.GameOver)
            {
                GameOverPanel.SetActive(true);

                Title.text = "Game Over";
                //EventSystem.current.SetSelectedGameObject(Button.gameObject);
            }
            else if (eventTiming == GameEvent.StageClear
                || eventTiming == GameEvent.GameClear)
            {
                GameOverPanel.SetActive(true);

                Title.text = "Clear!";
                //EventSystem.current.SetSelectedGameObject(Button.gameObject);
            }
            else if (eventTiming == GameEvent.GameStart)
            {
                GameOverPanel.SetActive(false);
            }
        }
    }
}