using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using NumberPlace.Standard;
using NumberPlace.UI;
using MantenseiLib.UI;

namespace NumberPlace
{
    public class MemoView : HubChild<Cell>, ICellInfoReceiver
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        TextMeshProUGUI _text;

        bool existsMemo => _memo?.Contains(true) == true;

        bool[] _memo;
        public CellState State => HUB.State();

        public bool[] MemoState
        {
            get => (bool[])_memo.Clone();
            set => _memo = (bool[])value.Clone();
        }

        public void SetMemoState(bool[] state)
        {
            _memo = (bool[])state.Clone();
            UpdateText();
        }

        public void ReceiveInfo(object value)
        {
            if (value is StandardRule_Instruction instruction)
            {
                if(instruction.IsInit(out var initializer))
                {
                    InitializeMemo(initializer);
                }

                if(instruction.type == InstructionType.Memo)
                {
                    SetNum(instruction.num);
                }

                if (instruction.option == InstructionOption.Empty)
                {
                    SetNum(-1);
                }

                UpdateText();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (HUB.IsCurrent())
                {
                    AutoWrite();
                }
            }
        }

        public bool AutoWrite()
        {
            if(existsMemo || HUB.State() != CellState.Empty)
                return false;

            var aroundCells =
                        HUB.GetAroundCells()
                        .Where(x => x.IsCorrect())
                        .Select(x => x.GetValue())
                        .Distinct()
                        ;

            var candidates =
                Enumerable.Range(0, HUB.Board.Size)
                .Except(aroundCells)
                .ToArray()
                ;

            foreach (var candidate in candidates)
            {
                SetNum(candidate);
            }

            return true;
        }

        void InitializeMemo(IInitializableInstruction initializer)
        {
            _memo = Enumerable.Repeat(false, initializer.gridsSize).ToArray();
        }

        public void RemoveNum(int num)
        {
            if (num >= 0 && num < _memo.Length)
            {
                _memo[num] = false;
                UpdateText();
            }
        }

        public void SetNum(int num)
        {
            if (num < 0)
            {
                Clear();
            }
            else
            {
                if (num < _memo.Length) 
                    _memo[num] = !_memo[num];
            }

            UpdateText();
        }

        void Clear()
        {
            for (int i = 0; i < _memo.Length; i++)
            {
                _memo[i] = false;
            }
        }

        string UpdateText(int highLightNum = -1)
        {
            if (State == CellState.Empty)
                _text.gameObject.SetActive(true);
            else
                _text.gameObject.SetActive(false);

            string HighLight(int n) => $"<color={ColorManager.Instance.MemoHighLightColorCode}><b>{n}</b></color>";
            string Invisible(int n) => $"<color=#0000>{n}</color>";

            int sqrt = (int)Mathf.Sqrt(_memo.Length);
            var memoStr =
                _memo.Select((x, i) =>
                {
                    var s = string.Empty;
                    var displayNum = i + 1;
                    s = x ? highLightNum == i ? HighLight(displayNum) : displayNum.ToString() : Invisible(displayNum);
                    if (displayNum % sqrt == 0)
                        s += "\n";

                    return s;
                });

            var result = string.Join("", memoStr);
            _text.text = result;

            return result;
        }

        public void HighLight(int num)
        {
            UpdateText(num);
        }

        public void ResetHighLight()
        {
            UpdateText();
        }
    }
}