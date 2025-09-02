using MantenseiLib;
using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace.UI
{
    public class MemoInjector : HubChild<NumInputButton>, ISwipeMenuListener
    {
        public void OnPointerUp()
        {
            StandardInputManager.Instance.InjectNumber(HUB.DisplayNum, true);
        }
    }
}