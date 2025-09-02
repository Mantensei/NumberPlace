using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NumberPlace.UI
{
    public class Erase_Button : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            StandardInputManager.Instance.InjectNumber(0);
        }
    }

}