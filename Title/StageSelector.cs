using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;
using System.Linq;

namespace NumberPlace.Title
{
    public class StageSelector : BaseMonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        Button Button;

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        TextMeshProUGUI Text;

        protected override void Start()
        {
            base.Start();
            Button.onClick.AddListener(SelectStage);
        }

        void SelectStage()
        {
            FindObjectsByType<StageSelector>(FindObjectsSortMode.None)
                .ToList()
                .ForEach(listener => listener.Button.interactable = false);

            var loadScene = SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Single);
            loadScene.completed += (op) =>
            {
                Enum.TryParse<Difficulty>(Text.text, out var difficulty);
                GameEventManager.OnGameStart(difficulty);
            };
        }
    }

}