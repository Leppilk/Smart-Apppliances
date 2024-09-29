using Kitchen;
using KitchenMods;
using KitchenSmartAppliances2;
using KitchenSmartAppliances2.Customs.Appliances;
using System;
using Unity.Collections;
using Unity.Entities;



public class SmartMicrowaveSystem : SystemBase, IModSystem
{
    ModMW mod = new ModMW();
    private EntityQuery Appliances;

    protected override void OnCreate()
    {
        Appliances = GetEntityQuery(new QueryHelper().All(typeof(CAppliance)));
    }


    protected override void OnUpdate()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create the Smart Microwave entity if it doesn't exist
        if (SmartMicrowave.SmartMicrowaveEntity.Equals(Entity.Null) || !entityManager.Exists(SmartMicrowave.SmartMicrowaveEntity))
        {
            SmartMicrowave.SmartMicrowaveEntity = entityManager.CreateEntity(
                //typeof(Appliance.ApplianceProcesses),
                typeof(CIsInactive),
                typeof(CRequiresActivation),
                typeof(CItemHolder),
                typeof(CDeactivateAtNight),
                typeof(CItemTransferRestrictions),
                typeof(CLockedWhileDuration),
                typeof(CAppliance),
                typeof(CRestrictProgressVisibility),
                typeof(CTakesDuration),
                typeof(CApplyProcessAfterDuration),
                typeof(CSetEnabledAfterDuration),
                typeof(CBreakIfBadDuration)

            );

            // Log for debugging
            ModMW.LogInfo($"Smart Microwave Entity created: {SmartMicrowave.SmartMicrowaveEntity}");
        }

        var smartMicrowaveEntity = SmartMicrowave.SmartMicrowaveEntity;


        var appliances = Appliances.ToEntityArray(Allocator.TempJob);

        // Create a NativeArray to hold only the Smart Microwave
        var smartMicrowaveArray = new NativeArray<Entity>(1, Allocator.TempJob);
        smartMicrowaveArray[0] = smartMicrowaveEntity;

        // Create the combined array
        var combinedAppliances = new NativeArray<Entity>(appliances.Length + smartMicrowaveArray.Length, Allocator.TempJob);

        // Copy existing appliances to the combined array
        for (int i = 0; i < appliances.Length; i++)
        {
            combinedAppliances[i] = appliances[i];
        }

        // Copy the Smart Microwave entity to the combined array
        combinedAppliances[appliances.Length] = smartMicrowaveEntity;

        // Process combined appliances
        foreach (var appliance in combinedAppliances)
        {
            try
            {
                var appl = EntityManager.GetComponentData<CAppliance>(appliance);
                var heldItem = EntityManager.GetComponentData<CItemHolder>(appliance).HeldItem;
                CTakesDuration duration = EntityManager.GetComponentData<CTakesDuration>(appliance);

                // Check if the appliance is the Smart Microwave or if the held item is null
                if (appliance.Equals(smartMicrowaveEntity) || heldItem == Entity.Null) continue;

                var Item = EntityManager.GetComponentData<CItem>(heldItem);

                ModMW.LogInfo($"{heldItem} dude0");
                ModMW.LogInfo($"{duration.Remaining} dude1");
                if (duration.Remaining <= 0f || heldItem != Entity.Null) //Item.IsPartial == false && Item.IsTransient && Item.IsGroup && Item.Category == ItemCategory.Generic && Item.Items.Count > 1)
                {
                    ModMW.LogInfo($"dude2");
                    if (EntityManager.HasComponent<CRequiresActivation>(smartMicrowaveEntity)
                        && EntityManager.HasComponent<CLockedWhileDuration>(smartMicrowaveEntity) &&
                            !duration.Active)
                    {
                        // Toggle the CIsInactive component
                        ModMW.LogInfo($"dude3");
                        if (EntityManager.HasComponent<CIsInactive>(appliance))
                        {
                            ModMW.LogInfo($"dude4");
                            EntityManager.RemoveComponent<CRequiresActivation>(appliance);
                            EntityManager.RemoveComponent<CIsInactive>(appliance);
                            ModMW.LogInfo($"appliancedude: {appliance}");
                        }
                    }
                    else
                    {
                        ModMW.LogInfo($"dude5");
                        EntityManager.AddComponent<CRequiresActivation>(appliance);
                        EntityManager.AddComponent<CIsInactive>(appliance);
                        ModMW.LogInfo($"Smart Microwave Entity: {smartMicrowaveEntity}");
                    }
                    ModMW.LogInfo($"dude6");
                }
            }
            catch (Exception e)
            {
                //ModMW.LogInfo($"It fails the TryGet method when removing CIsInactive and CRequiresActivation and the Appliance Break on Failure");
            }
        }
        ModMW.LogInfo($"dude7");
        // Dispose of the appliances array
        appliances.Dispose();
        smartMicrowaveArray.Dispose();
        combinedAppliances.Dispose();
    }
}


