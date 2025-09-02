using MantenseiLib;
using System;
using UnityEngine;

namespace NumberPlace
{
    [InitializableClass]
    public partial class StatusManager : Singleton<StatusManager>
    {

    }

    public partial class StatusManager
    {
        [InitializeStatic(-1)]
        public static float _time;
        public static float Time
        {
            get => _time;
            private set => _time = value;
        }

        public void AddTime(float deltaTime)
        {
            if (Time < 0f) Time = 0f; // ‰Šú‰»
            Time += deltaTime;
        }
    }

    public partial class StatusManager
    {
        [InitializeStatic(3)]
        static int _hp;

        public static int HP
        {
            get => _hp;
            private set
            {
                _hp = value;
                _hp = Mathf.Max(0, _hp);
            }
        }

        [InitializeStatic]
        public static Action<int> OnHPChanged;

        public static void RemoveHP()
        {
            HP --;
            OnHPChanged?.Invoke(HP);

            GameEventManager.NotifyEvent(GameEvent.StageMiss);

            if (HP <= 0)
            {
                GameEventManager.NotifyEvent(GameEvent.GameOver);
            }
        }
    }
}


