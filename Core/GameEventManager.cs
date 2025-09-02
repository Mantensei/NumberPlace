using MantenseiLib;
using NumberPlace.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NumberPlace
{
    public class GameEventManager : SingletonMonoBehaviour<GameEventManager>
    {
        public static void ForeachSafetyFindObjects<T>(Action<T> action)
        {
            foreach (var listener in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<T>())
            {
                if(listener?.IsSafe() == true)
                    action.Invoke(listener);
            }
        }

        public static void NotifyEvent(GameEvent timing, object content = null)
        {
            ForeachSafetyFindObjects<ICallByGameEvent>(listener => listener.CallByGameEvent(new GameEventInfo(timing, content)));
        }

        public static void OnGameStart(Difficulty difficulty)
        {
            NotifyEvent(GameEvent.GameStart, difficulty);
        }
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
    }

    public class GameEventInfo
    {
        public GameEventInfo(GameEvent gameEvent, object content = null)
        {
            EventTiming = gameEvent;
            Content = content;
        }

        public GameEvent EventTiming { get; }
        public object Content { get; }
    }

    public interface ICallByGameEvent
    {
        void CallByGameEvent(GameEventInfo eventInfo);
    }

    [Flags]
    public enum GameEventFlag
    {
        None = 0b_0000,
        Start = 0b_0001,
        Clear = 0b_0010,
        Miss = 0b_0100,
        Exit = 0b_1000,

        Game = 0b_0001_0000,
        Stage = 0b_0010_0000,
        PlayMode = 0b_0100_0000,
    }

    public enum GameEvent
    {
        None = GameEventFlag.None,

        GameStart = GameEventFlag.Game | GameEventFlag.Start,
        GameClear = GameEventFlag.Game | GameEventFlag.Clear,
        GameOver = GameEventFlag.Game | GameEventFlag.Miss,
        GameExit = GameEventFlag.Game | GameEventFlag.Exit,

        StageStart = GameEventFlag.Stage | GameEventFlag.Start,
        StageClear = GameEventFlag.Stage | GameEventFlag.Clear,
        StageMiss = GameEventFlag.Stage | GameEventFlag.Miss,
        StageExit = GameEventFlag.Stage | GameEventFlag.Exit,

        Pause = GameEventFlag.PlayMode | 0b1000,
        Play = GameEventFlag.PlayMode | 0b0001,
        Fast = GameEventFlag.PlayMode | 0b0010,
    }

    public static class GameEventExtensions
    {
        public static bool HasFlag(this GameEvent gameEvent, GameEventFlag flag)
        {
            return ((GameEventFlag)gameEvent & flag) != 0;
        }
    }
}

