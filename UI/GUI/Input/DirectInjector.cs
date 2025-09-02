using MantenseiLib;
using NumberPlace.Standard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MantenseiLib.UI;

namespace NumberPlace.UI
{
    public class DirectInjector : HubChild<NumInputButton>, ISwipeMenuListener
    {
        static DirectInjector _currentActiveInjector;
        public bool IsDirectMode => _currentActiveInjector == this;
        int displayNum => HUB.DisplayNum;

        protected override void Start()
        {
            base.Start();

            CellManager.OnCurrentCellChanged += (c) =>
            {
                if (IsDirectMode && !StandardInputManager.Instance.IsUsedUp(displayNum - 1))
                {
                    StandardInputManager.Instance.InjectNumber(displayNum);
                    ButtonView.HighLight(displayNum - 1);
                }
            };

            //キー入力移動時に発動したら大惨事なのでキャンセル
            StandardInputManager.OnKeyMoved += () => SetToCurrent(null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SetToCurrent(null);
        }

        public void OnPointerUp()
        {
            SetToCurrent(this);
        }

        void SetToCurrent(DirectInjector injector)
        {
            var tmp = _currentActiveInjector;

            if (_currentActiveInjector == injector)
            {
                _currentActiveInjector = null;
            }
            else
            {
                _currentActiveInjector = injector;
                //Cellを押している状態でActivate後に同じセルを押しても
                //CurrentCellが同一セル判定でInvokeできないのでリセットする
                CellManager.Instance.ResetCurrentCell();
            }

            tmp?.UpdateVisual();
            _currentActiveInjector?.UpdateVisual();

            NumInputButton.OccupieObject = _currentActiveInjector?.gameObject;
            if (injector == null)
            {
                tmp?.Safe()?.gameObject?.SetActive(false);
                _currentActiveInjector?.Safe()?.gameObject?.SetActive(false);
            }
        }

        void UpdateVisual()
        {
            if (IsDirectMode)
                HUB.Text.color = ColorManager.White;
            else
                HUB.Text.color = ColorManager.Dark;

            ButtonView.HighLight(displayNum - 1);
        }
    }
}