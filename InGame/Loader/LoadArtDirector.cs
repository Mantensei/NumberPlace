using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NumberPlace.Load
{
    public class LoadArtDirector : BaseMonoBehaviour
    {
        [GetComponent]
        TextMeshProUGUI text;

        protected override void Update()
        {
            base.Update();

            if (CellManager.Instance.Generating)
            {
                text.text = CellManager.Instance.GeneratingStatus;
            }
        }
    } 
}
