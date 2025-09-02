using MantenseiLib;
using NumberPlace.Events;
using NumberPlace.Standard;
using NumberPlace.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using MantenseiLib.UI;

namespace NumberPlace
{
    public class NumberView : HubChild<Cell>, ICellInfoReceiver, INumberUsedUpListener
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        TextMeshProUGUI _text;

        CellState State
        {
            get => HUB.Meta.State;
            set => HUB.Meta.State = value;
        }

        public int Value { get; private set; } = -1;

        public void ReceiveInfo(object value)
        {
            if (value is StandardRule_Instruction instruction)
            {
                if (instruction.IsInit())
                {
                    if (instruction.option == InstructionOption.Open)
                    {
                        State = CellState.Opened;
                        SetNum(instruction.num);
                        return;
                    }
                    else if (instruction.option == InstructionOption.Clear)
                    {
                        instruction.num = -1;
                    }
                }

                if (instruction.type == InstructionType.SetNum)
                {
                    if (!HUB.IsCorrect())
                    {
                        //Value = instruction.num;
                        //UpdateColor();
                        SetNum(instruction.num);
                    }
                }

                if (instruction.option == InstructionOption.Empty)
                {
                    if (State != CellState.Opened)
                    {
                        State = CellState.Empty;
                        SetNum(-1);
                    }
                }
            }
        }

        public bool Open()
        {
            if (HUB.IsCorrect()) return false;

            State = CellState.Opened;
            SetNum(HUB.GetValue());

            return true;
        }

        public void SetNum(int num)
        {
            if(Value == num) return; //連打での連続ミス防止

            Value = num;
            _text.text = (num + 1).ToString();

            UpdateState();
        }

        void UpdateState()
        {
            if (State == CellState.Opened)
            {
                //UpdateColor();
            }
            else
            {
                if (Value < 0)
                {
                    State = CellState.Empty;
                }
                else if (Value.Equals(HUB.Meta.Value))
                {
                    State = CellState.Correct;
                    foreach (var cell in HUB.GetAroundCells())
                    {
                        cell.GetComponentInChildren<MemoView>()?.RemoveNum(Value);
                    }
                }
                else
                {
                    State = CellState.Incorrect;
                }
            }

            CellStateChangeData data = new CellStateChangeData(HUB, State, Value);
            GameEventManager.ForeachSafetyFindObjects<ICellStateChangeListener>(listener => listener.OnCellStateChanged(data));

            UpdateColor();
        }

        void SetColor(Color color)
        {
            _text.color = color;
        }

        void UpdateColor()
        {
            switch (State)
            {
                case CellState.Empty:
                    SetColor(Color.clear);
                    break;
                case CellState.Opened:
                    SetColor(ColorManager.Dark);
                    break;
                case CellState.Correct:
                    SetColor(ColorManager.Instance.CorrectNumberColor);
                    break;
                case CellState.Incorrect:
                    SetColor(ColorManager.Instance.InorrectNumberColor);
                    break;
                default:
                    break;
            }
        }

        public void OnNumberUsedUp(int internalNum)
        {
            if (internalNum == Value)
            {
                if (HUB.IsCorrect())
                    _text.color = ColorManager.Instance.DisabledColor;
            }
        }
    }
}
