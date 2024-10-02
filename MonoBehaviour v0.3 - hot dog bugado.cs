using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenLib.Utils;
using SmartAppliances;
using SmartAppliances.Customs.Appliances;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;




public class SmartMono : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity microwaveEntity;
    private Entity storedItemHolderComponent = Entity.Null;
    private bool shouldCheckEntities = false;
    private NativeArray<Entity> itemEntities;
    private static int microwaveStoredItemId = -1;

    // Step 1: Add a private field for ConveyItemsView
    private ConveyItemsView conveyItemsView;

    private void Start()
    {
        InitializeEntityManager();
        InitializeMicrowaveEntity();
        microwaveStoredItemId = -1;

    }

    private void Update()
    {
        if (!IsDayTime())
        {
            return;
        }

        shouldCheckEntities = true;
        ModMW.Logger.LogInfo($"shouldCheckEntities set to: {shouldCheckEntities}");

        if (shouldCheckEntities)
        {
            ProcessMicrowaveEntities();
        }
    }

    private void InitializeEntityManager()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void InitializeMicrowaveEntity()
    {
        microwaveEntity = entityManager.CreateEntity();

        var applianceComponent = new CAppliance { ID = SmartMicrowave.ID };
        entityManager.AddComponentData(microwaveEntity, applianceComponent);

        entityManager.AddComponent<CPosition>(microwaveEntity);
        entityManager.AddComponentData(microwaveEntity, new CItemHolder());
        entityManager.AddComponentData(microwaveEntity, new CRequiresActivation());
        entityManager.AddComponentData(microwaveEntity, new CItemTransferRestrictions
        {
            AllowWhenInactive = true
        });
        entityManager.AddComponentData(microwaveEntity, new CIsInactive());
        entityManager.AddComponentData(microwaveEntity, new CRestrictProgressVisibility
        {
            HideWhenInactive = true
        });
        entityManager.AddComponentData(microwaveEntity, new CTakesDuration
        {
            Total = 3.5f
        });
        entityManager.AddComponentData(microwaveEntity, new CApplyProcessAfterDuration());
        entityManager.AddComponentData(microwaveEntity, new CSetEnabledAfterDuration());
        entityManager.AddComponentData(microwaveEntity, new CBreakIfBadDuration
        {
            CatchFire = true,
            TriggeredByBadProcess = true,
            TriggeredByNoProcess = true
        });
        entityManager.AddComponentData(microwaveEntity, new CDeactivateAtNight());
        ModMW.Logger.LogInfo($"Created microwave entity with ID: {SmartMicrowave.ID}");
    }



    private NativeArray<Entity> GetMicrowaveEntities()
    {
        EntityQuery query = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<CAppliance>(),
            ComponentType.ReadOnly<CItemHolder>(),
            ComponentType.ReadOnly<CTakesDuration>()
        );
        return query.ToEntityArray(Allocator.TempJob);
    }

    private void ProcessMicrowaveEntities()
    {
        NativeArray<Entity> entities = GetMicrowaveEntities();
        foreach (var applianceEntity in entities)
        {
            var applianceComponent = entityManager.GetComponentData<CAppliance>(applianceEntity);
            var itemHolderComponent = entityManager.GetComponentData<CItemHolder>(applianceEntity).HeldItem;

            if (applianceComponent.ID != SmartMicrowave.ID || itemHolderComponent == Entity.Null) continue;
            ModMW.Logger.LogInfo($"[ProcessMicrowaveEntities] Stored item ID: {microwaveStoredItemId}");
            // Simplified check to remove CIsInactive component

            // Existing logic for checking microwave interactions
            CheckMicrowaveInteractions();
            if (microwaveStoredItemId == ProcessEntities() && entityManager.HasComponent<CIsInactive>(applianceEntity))
            {
                entityManager.RemoveComponent<CRequiresActivation>(applianceEntity);
                entityManager.RemoveComponent<CIsInactive>(applianceEntity);
                ModMW.Logger.LogInfo($"Removed CIsInactive and CRequiresActivation");

            }
            else if (microwaveStoredItemId != ProcessEntities() && !entityManager.HasComponent<CIsInactive>(applianceEntity))
            {
                entityManager.AddComponent<CIsInactive>(applianceEntity);
                entityManager.AddComponent<CRequiresActivation>(applianceEntity);
                ModMW.Logger.LogInfo($"Addeded CIsInactive and CRequiresActivation");

            }
            shouldCheckEntities = false;
        }
        entities.Dispose();
    }

    private int ProcessEntities()
    {
        ModMW.Logger.LogInfo("ProcessEntities method called.");
        int itemId = -1; // Default value if no item is found

        NativeArray<Entity> entities = GetMicrowaveEntities();

        foreach (var applianceEntity in entities)
        {
            try
            {
                var applianceComponent = entityManager.GetComponentData<CAppliance>(applianceEntity);
                var itemHolderComponent = entityManager.GetComponentData<CItemHolder>(applianceEntity).HeldItem;
                if (applianceComponent.ID != SmartMicrowave.ID || itemHolderComponent == Entity.Null) continue;     
                var cItem = entityManager.GetComponentData<CItem>(itemHolderComponent);
                itemId = cItem.ID; // Store the item ID
                ModMW.Logger.LogInfo($"Item ID is {cItem.ID}");
            }
            catch (Exception e)
            {
                // Handle exceptions. Do not code here
            }
        }

        // Dispose of the NativeArrays to avoid memory leaks
        entities.Dispose();

        return itemId;
    }

    private void CheckMicrowaveInteractions()
    {
        ModMW.Logger.LogInfo("CheckMicrowaveInteractions method called.");
        NativeArray<Entity> entities = GetMicrowaveEntities();

        // Create an EntityQuery to find all entities with the CAttemptingInteraction component
        EntityQuery interactionQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<CAttemptingInteraction>());
        NativeArray<CAttemptingInteraction> interactions = interactionQuery.ToComponentDataArray<CAttemptingInteraction>(Allocator.TempJob);

        foreach (var interaction in interactions)
        {
            foreach (var applianceEntity in entities)
            {
                var applianceComponent = entityManager.GetComponentData<CAppliance>(applianceEntity);
                if (applianceComponent.ID != SmartMicrowave.ID) continue;

                int storedItemId = ProcessEntities();
                ModMW.Logger.LogInfo($"[CheckMicrowaveInteractions] Stored item ID: {storedItemId}");

                foreach (var property in typeof(ItemReferences).GetProperties())
                {
                    if (property.PropertyType == typeof(int))
                    {
                        int referenceItemId = (int)property.GetValue(null);
                        if (storedItemId == referenceItemId)
                        {
                            var itemGameObject = (Item)GDOUtils.GetExistingGDO(referenceItemId);
                            if (itemGameObject == null) continue;

                            ModMW.Logger.LogInfo($"Item Game Object found: {itemGameObject}");

                            List<Item.ItemProcess> processes = itemGameObject.DerivedProcesses;
                            foreach (Item.ItemProcess process in processes)
                            {
                                ModMW.Logger.LogInfo($"IsBad: {process.IsBad}");
                                if (interaction.Type != InteractionType.Act || process.IsBad) continue;

                                microwaveStoredItemId = storedItemId;
                                ModMW.Logger.LogInfo($"Stored item ID: {microwaveStoredItemId}");
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Dispose of the NativeArrays to avoid memory leaks
        interactions.Dispose();
        entities.Dispose();
    }

    private bool IsDayTime()
    {
        // Retrieve the SIsDayTime component
        EntityQuery sIsDayQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Kitchen.SIsDayTime>());
        if (sIsDayQuery.IsEmpty)
        {
            return false;
        }

        var sIsDayEntity = sIsDayQuery.GetSingletonEntity();
        var sIsDay = entityManager.GetComponentData<Kitchen.SIsDayTime>(sIsDayEntity);

        // Check if it is daytime
        if (!entityManager.HasComponent<Kitchen.SIsDayTime>(sIsDayEntity))
        {
            return false;
        }

        return true;
    }
}

/* IMPLEMENT LATER
 
  if (cApplianceEntity != Entity.Null)
{
using UpdateView
View Data
ConveyItemsView
    }
}*/