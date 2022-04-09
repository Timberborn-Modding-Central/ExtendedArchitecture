using System;
using System.Collections;
using System.Collections.Generic;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using UnityEngine;

namespace Elec.ExtendedArchitecture
{
    public class Localization : LabeledPrefab
    {
        private string GetPrefabName()
        {
            return GetComponent<Prefab>().PrefabName;
        }

        public string displayNameText;
        public string descriptionText;
        public string flavorText;
        
        void OnValidate()
        {
            DisplayNameLocKey = $"{GetPrefabName()}.DisplayName";
            DescriptionLocKey = $"{GetPrefabName()}.Description";
            FlavorDescriptionLocKey = $"{GetPrefabName()}.Flavor";
        }
    }
}
