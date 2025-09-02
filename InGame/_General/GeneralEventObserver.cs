using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;

namespace NumberPlace
{
    public class GeneralEventObserver : MonoBehaviour, IBoardGenerateHandler
    {
        [SerializeField]
        GameEvent GameEvent;

        [SerializeField]
        NumberPlaceEvent NumberPlaceEvent;

        [SerializeField]
        ActionType ActionType;

        bool nothing => GameEvent == GameEvent.None;

        public void HandleBoardInfo(BoardData data)
        {
            if (NumberPlaceEvent == NumberPlaceEvent.BoardGenerated)
                OnSomeEvent();
        }

        void OnSomeEvent()
        {
            switch (ActionType)
            {
                case ActionType.Deactivate:
                    gameObject.SetActive(false); break;
                default:
                    break;
            }

        }
    }

    public enum NumberPlaceEvent
    {
        None,
        BoardGenerated,
    }

    public enum ActionType
    {
        None,
        Deactivate,
    }
}