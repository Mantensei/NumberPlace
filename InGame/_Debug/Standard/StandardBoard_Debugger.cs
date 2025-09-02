#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumberPlace.Standard;
using System;
using UnityEngine.EventSystems;
using System.Linq;

namespace NumberPlace.Debugger
{
    public class StandardBoard_Debugger : MonoBehaviour
    {
        PuzzleGenerator Generator;

        void Init()
        {
            var cells = GetComponentsInChildren<Cell>();
            var size = 3;

            Generator = new PuzzleGenerator(cells, size);
        }

        //    void Update()
        //    {
        //        GameObject selectedButton = null;
        //        Cell selectedCell = null;

        //        if (Input.anyKeyDown)
        //        {
        //            var eventSystem = EventSystem.current;
        //            selectedButton = eventSystem.currentSelectedGameObject?.gameObject;
        //            selectedCell = selectedButton?.GetComponentInParent<Cell>();
        //        }
        //        //if (selectedButton == null) return;

        //        if (Input.GetKeyDown(KeyCode.Delete))
        //        {
        //            Init();


        //            if (Generator.TryEmptyCell(new PuzzleCell(selectedCell)))
        //            {
        //                selectedCell.ReceiveInfo
        //                (
        //                    new StandardRule_Instruction()
        //                    {
        //                        option = InstructionOption.Empty
        //                    }
        //                );
        //            }
        //        }

        //        if (Input.GetKeyDown(KeyCode.Return))
        //        {
        //            Init();
        //            Generator.DigHoles();

        //            foreach(var pc in Generator.Cells)
        //            {
        //                var cells = GetComponentsInChildren<Cell>();

        //                if (pc.State == PuzzleCellState.Empty)
        //                {
        //                    cells.FirstOrDefault(x => x.Row() == pc.Y && x.Column() == pc.X)
        //                    .ReceiveInfo
        //                    (
        //                        new StandardRule_Instruction()
        //                        {
        //                            option = InstructionOption.Empty
        //                        }
        //                    );
        //                }
        //            }
        //        }
        //    }
    }

}

#endif