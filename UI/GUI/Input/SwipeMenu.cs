using MantenseiLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NumberPlace.UI
{
    public class SwipeMenu : BaseMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private bool _isSwiping = false;

        public static bool allowSwipeMode = true;
        public static Action OnSwipeAction;

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        Button _source;

        List<ISwipeMenuListener> ChildElements = new();


        protected override void Start()
        {
            base.Start();
            foreach (Transform element in transform)
            {
                var listener = element.GetComponent<ISwipeMenuListener>();
                if (listener == null) continue;
                ChildElements.Add(listener);
                element.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnSwipeAction?.Invoke();

            if (allowSwipeMode)
            {
                _isSwiping = true;
                ChildElements.ForEach(e => e.gameObject.SetActive(true));
            }
            else
            {
                _isSwiping = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(!_isSwiping) return;

            var menuItem = eventData.pointerCurrentRaycast.gameObject.GetComponent<ISwipeMenuListener>();

            Color pressedColor = _source.colors.pressedColor;
            Color normalColor = _source.colors.normalColor;

            // 子要素にホバーしていない場合は親をハイライト
            _source.image.CrossFadeColor(pressedColor, 0f, true, true);

            // 全ての子要素を通常色に戻す
            foreach (Transform child in transform)
            {
                var childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.CrossFadeColor(normalColor, 0f, true, true);
                }
            }

            HighLight(eventData);
        }

        void HighLight(PointerEventData eventData)
        {
            var childImage = eventData.pointerCurrentRaycast.gameObject.GetComponent<ISwipeMenuListener>()
                ?.gameObject?.GetComponent<Image>();

            Color pressedColor = _source.colors.pressedColor;
            Color normalColor = _source.colors.normalColor;

            if (childImage != null)
            {
                // 子要素をハイライト色に（シェーダーレベルでの色合成）
                childImage.CrossFadeColor(pressedColor, 0f, true, true);

                // 親ボタンを通常色に
                _source.image.CrossFadeColor(normalColor, 0f, true, true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClick(eventData);
        }

        void OnClick(PointerEventData eventData)
        {
            if(!_isSwiping) return;

            _isSwiping = false;
            eventData.pointerCurrentRaycast.gameObject.GetComponent<ISwipeMenuListener>()?.OnPointerUp();

            ChildElements.ForEach(e => e.gameObject.SetActive(false));
        }
    }
    public interface ISwipeMenuListener : IMonoBehaviour
    {
        void OnPointerUp();
    }
}