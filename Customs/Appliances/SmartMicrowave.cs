using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace SmartAppliances.Customs.Appliances
{
    public class SmartMicrowave : CustomAppliance
    {
        public override string UniqueNameID => "SmartMicrowave";
        public override GameObject Prefab => ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Microwave)).Prefab;
        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            new Appliance.ApplianceProcesses
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                IsAutomatic = true,
                Speed = 0.01f,
                Validity = ProcessValidity.Generic
            }
        };

        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>
        {
            new CItemHolder(),
            new CRequiresActivation(),
            new CItemTransferRestrictions
            {
                AllowWhenInactive = true
            },
            new CIsInactive(),
            new CRestrictProgressVisibility
            {
                HideWhenInactive = true
            },
            new CTakesDuration
            {
                Total = 5
            },
            new CApplyProcessAfterDuration(),
            new CSetEnabledAfterDuration(),
            new CBreakIfBadDuration
            {
                CatchFire = true,
                TriggeredByBadProcess = true,
                TriggeredByNoProcess = true
            },
            new CDeactivateAtNight()
        };

        public override bool IsPurchasableAsUpgrade => true;
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override List<Process> RequiresProcessForShop => new List<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.RequireOven),
        };

        public override bool IsAnUpgrade => true;
        
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (
                Locale.English, new ApplianceInfo 
                {
                    Name = "Smart Microwave", 
                    Description = "When you're a pro chef you don't have to prove anything",
                    Sections = new List<Appliance.Section>
                    {
                        new Appliance.Section
                        {
                            Title = "High Power",
                            Description = "Activate to cook anything in a few seconds"
                        }
                    }
                }
            )
        };
    }
}