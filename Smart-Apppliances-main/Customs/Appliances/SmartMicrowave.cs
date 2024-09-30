using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;
using SmartAppliances;

namespace SmartAppliances.Customs.Appliances
{
    public class SmartMicrowave : CustomAppliance
    {
        public static int ID => 939049684;
        public override string UniqueNameID => "SmartMicrowave";

        public override GameObject Prefab
        {
            get
            {
                GameObject microwavePrefab = ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Microwave)).Prefab;

                // Add SmartMono
                if (microwavePrefab.GetComponent<SmartMono>() == null)
                {
                    microwavePrefab.AddComponent<SmartMono>();
                }
                return microwavePrefab;
            }
        }
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
            new CAutomatedInteractor()
            {
                Type = InteractionType.Act 
            },
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
                Total = 3.5f
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
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 250;
        public override List<Process> RequiresProcessForShop => new List<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.RequireOven),
        };


        //public override bool IsAnUpgrade => true;
       
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
                            Description = "Now you don't even need to watch",
                            
                        }
                    }
                }
            ),
        };
        
    }
}