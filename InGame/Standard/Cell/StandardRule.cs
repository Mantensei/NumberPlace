using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumberPlace;
using Unity.VisualScripting;
using MantenseiLib;
using System.Net;

namespace NumberPlace.Standard
{
    public class StandardRule : HubChild<Cell>, ICellInfoReceiver
    {
        public void ReceiveInfo(object value)
        {
            if (value is StandardRule_Instruction instruction)
            {
                if (instruction.IsInit(out var initializer))
                {
                    HUB.Meta.Value = instruction.num;
                    Initialize(initializer);
                    instruction.type = InstructionType.SetNum;
                }

                this.PassCellInfo2Children(value);
            }
            else
            {
                return;
            }
        }

        public void Initialize(IInitializableInstruction instruction)
        {
            var meta = HUB.Meta;

            var address = instruction.id;
            var gridSize = instruction.gridsSize;
            var blockSize = instruction.blockSize;

            meta.ID = address;
            meta.Row = address / gridSize;
            meta.Column = address % gridSize;
            meta.Group = (meta.Row / blockSize) * blockSize + (meta.Column / blockSize);

            HUB.transform.position = new Vector3(meta.Column, meta.Row);
        }
    }

    public class StandardRule_Instruction : Instruction, IIntValue
    {
        public int num;
        public InstructionType type;
        public InstructionOption option;
        public object extraValue;

        public int ToInt() => num;
    }

    public class StandardRule_Initialize_Instruction : StandardRule_Instruction, IInitializableInstruction
    {
        public int id { get; set; }
        public int blockSize { get; set; }
    }

    public enum InstructionType
    {
        None,
        SetNum,
        Memo,
        HighLight,
    }

    public enum InstructionOption 
    {
        None,
        Clear,
        Empty,
        Open,
    }
}