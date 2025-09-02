using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace.UI
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [GetComponent(HierarchyRelation.Children)]
        public HP_UI HP{ get; private set; }

        [GetComponent(HierarchyRelation.Children)]
        public Timer_UI Timer { get; private set; }
    }

}