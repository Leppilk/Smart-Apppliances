using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenLib.Utils;
using SmartAppliances;
using SmartAppliances.Customs.Appliances;
using System;
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
    private int microwaveStoredItemId = -1;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        microwaveEntity = entityManager.CreateEntity();

        // Create and set the CAppliance component with the correct ID
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

    private void Update()
    {
        if (!IsDayTime())
        {
            return;
        }

        // Set the flag to true if it is daytime
        shouldCheckEntities = true;
        ModMW.Logger.LogInfo($"shouldCheckEntities set to: {shouldCheckEntities}");

        if (shouldCheckEntities)
        {
            NativeArray<Entity> entities = GetMicrowaveEntities();
            foreach (var applianceEntity in entities)
            {
                var applianceComponent = entityManager.GetComponentData<CAppliance>(applianceEntity);
                var itemHolderComponent = entityManager.GetComponentData<CItemHolder>(applianceEntity).HeldItem;

                if (applianceComponent.ID != SmartMicrowave.ID || itemHolderComponent == Entity.Null) continue;

                CheckMicrowaveInteractions();
                if (microwaveStoredItemId == ProcessEntities() && entityManager.HasComponent<CIsInactive>(applianceEntity))
                {
                    entityManager.RemoveComponent<CRequiresActivation>(applianceEntity);
                    entityManager.RemoveComponent<CIsInactive>(applianceEntity);
                    ModMW.Logger.LogInfo($"Microwave Stored item ID is {microwaveStoredItemId}");
                }
                else if (microwaveStoredItemId != ProcessEntities() && !entityManager.HasComponent<CIsInactive>(applianceEntity))
                {
                    entityManager.AddComponent<CIsInactive>(applianceEntity);
                    entityManager.AddComponent<CRequiresActivation>(applianceEntity);
                    ModMW.Logger.LogInfo($"Microwave Stored item ID is not {microwaveStoredItemId}");
                }
                shouldCheckEntities = false;
                ModMW.Logger.LogInfo($"shouldCheckEntities reset to: {shouldCheckEntities}");
            }
        }
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

                ModMW.Logger.LogInfo($"Found applianceEntity: {applianceEntity} and Item {itemHolderComponent}");
                var cItem = entityManager.GetComponentData<CItem>(itemHolderComponent);
                itemId = cItem.ID; // Store the item ID
                ModMW.Logger.LogInfo($"Item ID is {cItem.ID}");

                foreach (var property in typeof(ItemReferences).GetProperties())
                {
                    if (property.PropertyType == typeof(int))
                    {
                        int referenceItemId = (int)property.GetValue(null);
                        if (cItem.ID == referenceItemId)
                        {
                            ModMW.Logger.LogInfo($"Item ID {cItem.ID} matches with ItemReferences.{property.Name}");

                            var itemGameObject = (Item)GDOUtils.GetExistingGDO(referenceItemId);
                            if (itemGameObject != null)
                            {
                                ModMW.Logger.LogInfo($"Item Game Object found: {itemGameObject}");
                            }
                            else
                            {
                                ModMW.Logger.LogInfo("Item Game Object is null.");
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Catch any exceptions. Do not code here
            }
        }

        // Dispose of the NativeArrays to avoid memory leaks
        entities.Dispose();

        return itemId;
    }

    // Method to check if the microwave is the target of any interactions
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
                if (applianceComponent.ID != SmartMicrowave.ID || interaction.Type != InteractionType.Act) continue;

                ModMW.Logger.LogInfo($"Microwave Entity: {applianceEntity}");
                ModMW.Logger.LogInfo($"Microwave is the target of interaction type: {interaction.Type}");

                // Call ProcessEntities to check what was inside the microwave when it was acted upon
                microwaveStoredItemId = ProcessEntities();
                ModMW.Logger.LogInfo($"Stored item ID: {microwaveStoredItemId}");
            }
        }

        // Dispose of the NativeArray to avoid memory leaks
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
    var currentHeldItem = entityManager.GetComponentData<CItemHolder>(cApplianceEntity).HeldItem;
    ModMW.Logger.LogInfo($"Current HeldItem of cApplianceEntity: {currentHeldItem}");

    if (storedItemHolderComponent == currentHeldItem)
    {
        ModMW.Logger.LogInfo($"StoredItemHolderComponent matches current HeldItem");

        if (entityManager.HasComponent<CRequiresActivation>(cApplianceEntity))
        {
            entityManager.RemoveComponent<CRequiresActivation>(cApplianceEntity);
            ModMW.Logger.LogInfo($"Removed CRequiresActivation from entity: {cApplianceEntity}");
        }
        if (entityManager.HasComponent<CIsInactive>(cApplianceEntity))
        {
            entityManager.RemoveComponent<CIsInactive>(cApplianceEntity);
            ModMW.Logger.LogInfo($"Removed CIsInactive from entity: {cApplianceEntity}");
        }
    }
}*/