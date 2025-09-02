using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberPlace
{
    public class Instruction
    {

    }

    public interface IInitializableInstruction 
    {
        int id { get; }        
        int blockSize { get; }
        int gridsSize => blockSize * blockSize;
    }

    public static class Rule_Extension
    {
        public static IInitializableInstruction ToInit(this Instruction rule) => rule as IInitializableInstruction;
        public static bool IsInit(this Instruction rule) => rule is IInitializableInstruction;

        public static bool IsInit(this Instruction rule, out IInitializableInstruction initializer)
        {
            if (rule.IsInit())
            {
                initializer = rule as IInitializableInstruction;
                return true;
            }
            else
            {
                initializer = null;
                return false;
            }
        }
    }
}