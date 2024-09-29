using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenSmartAppliances2.Customs.Appliances;
using UnityEngine;

namespace KitchenSmartAppliances2.Customs.Items
{
    public class RawLobster : CustomItem
    {
        // UniqueNameID - This is used internally to generate the ID of this GDO. Once you've set it, don't change it.
        public override string UniqueNameID => "RawLobster";

        // Prefab - This is the GameObject used for this Item's visual. AssignMaterialsByNames() is a helper method that assigns materials to the GameObject based on the names of the materials.
        public override GameObject Prefab => ModMW.Bundle.LoadAsset<GameObject>("Raw Lobster").AssignMaterialsByNames();

        // DedicatedProvider - The Appliance used for this Item's provider.
        //public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<SmartMicrowave>().GameDataObject;
    }
}