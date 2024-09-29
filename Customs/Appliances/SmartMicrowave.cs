using Kitchen;
using Kitchen.Components;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace KitchenSmartAppliances2.Customs.Appliances
{
    
    public partial class SmartMicrowave : CustomAppliance
    {
        
        // UniqueNameID - This is used internally to generate the ID of this GDO. Once you've set it, don't change it.
        public override string UniqueNameID => "Smart Microwave";

        // Prefab to display in-game
        public override GameObject Prefab => ModMW.Bundle.LoadAsset<GameObject>("Lobster Provider").AssignMaterialsByNames();

        // Appliance processes
        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            new Appliance.ApplianceProcesses
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                IsAutomatic = false,
                Validity = ProcessValidity.Generic,
                Speed = 0.01f,
            }
        };
        // Appliance properties
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>
        {
            new CItemHolder
            {
                HeldItem = new Entity()
            },
            new CRequiresActivation(),
            new CItemTransferRestrictions
            {
                AllowWhenActive = false,
                AllowWhenInactive = true
            },
            new CIsInactive(),
            new CRestrictProgressVisibility
            {
                HideWhenActive = false,
                HideWhenInactive = true,
                ObfuscateWhenActive = false,
                ObfuscateWhenInactive = false
            },
            new CTakesDuration
            {
                Total = 4.0f, // Duration of 4 seconds
                Remaining = 0.0f,
                Active = false,
                Manual = false,
                ManualNeedsEmptyHands = false,
                RelevantTool = DurationToolType.None,
                Mode = InteractionMode.Items,
                RequiresRelease = false,
                PreserveProgress = false,
                IsInverse = false,
                IsLocked = false,
                CurrentChange = 0.0f,
            },
            new CApplyProcessAfterDuration
            {BreakOnFailure = false},
            new CSetEnabledAfterDuration
            {Activate =  false},
            new CBreakIfBadDuration
            {
                CatchFire = true,
                TriggeredByBadProcess = true,
                TriggeredByNoProcess = true,
            },
            new CDeactivateAtNight(),
            
            /*/new CConveyPushItems
            {
                Delay = 0,
                Push = false,
                Grab = false,
                Reversed = false,
                GrabSpecificType = true,
                SpecificType = 0,
                SpecificComponents = new KitchenData.ItemList(),
                IgnoreProcessingItems = false,
                Progress = 0,
                State = CConveyPushItems.ConveyState.None,
            }/*/
        };

        // Other properties
        public override bool IsNonInteractive => false;
        public override OccupancyLayer Layer => OccupancyLayer.Default;
        public override bool ForceHighInteractionPriority => false;
        public override int PurchaseCostOverride => 250;
        public override EntryAnimation EntryAnimation => EntryAnimation.Placement;
        public override ExitAnimation ExitAnimation => ExitAnimation.Destroy;
        public override bool SkipRotationAnimation => false;
        public override bool IsPurchasable => false;
        public override bool IsPurchasableAsUpgrade => true;
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking | ShoppingTags.Technology;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override bool StapleWhenMissing => false;
        public override bool SellOnlyAsDuplicate => false;
        public override bool SellOnlyAsUnique => false;
        public override bool PreventSale => false;
        public override bool IsNonCrated => false;

        // Localized info
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, new ApplianceInfo
            {
            Name = "Smart Microwave",
            Description = "Just Watch.",
            })
        };

        public static Entity SmartMicrowaveEntity;
        public EntityManager entityManager;
        public override void OnRegister(Appliance gameDataObject)
        {
            base.OnRegister(gameDataObject);

            // Access the EntityManager from the World
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            SmartMicrowaveEntity = entityManager.CreateEntity(typeof(CIsInactive), typeof(CRequiresActivation), typeof(CItemHolder), typeof(CTakesDuration), typeof(CDeactivateAtNight), typeof(CLockedWhileDuration));

            // AnimationSoundSource - This is used to play a sound when the Appliance is interacted with.
            AnimationSoundSource soundSource = gameDataObject.Prefab.GetChild("Locker/Locker").TryAddComponent<AnimationSoundSource>();
            soundSource.SoundList = new List<AudioClip>() { ModMW.Bundle.LoadAsset<AudioClip>("Fridge_mixdown") };
            soundSource.Category = SoundCategory.Effects;
            soundSource.ShouldLoop = false;

            // ItemSourceView - This is used to display the Item provided by the Appliance, and trigger the Animation on interaction.
            ItemSourceView sourceView = gameDataObject.Prefab.TryAddComponent<ItemSourceView>();
            var quad = gameDataObject.Prefab.GetChild("Locker/Quad").GetComponent<MeshRenderer>();
            quad.materials = MaterialUtils.GetMaterialArray("Flat Image");
            ReflectionUtils.GetField<ItemSourceView>("Renderer").SetValue(sourceView, quad);
            ReflectionUtils.GetField<ItemSourceView>("Animator").SetValue(sourceView, gameDataObject.Prefab.GetChild("Locker/Locker").GetComponent<Animator>());
        }

    }
}


