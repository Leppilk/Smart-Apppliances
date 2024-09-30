using UnityEngine;
using Unity.Entities;
using SmartAppliances;
using SmartAppliances.Customs.Appliances;
using Kitchen;
using Unity.Collections;
using System;
using KitchenLib.References;
using static UnityEngine.EventSystems.EventTrigger;
using KitchenData;
using KitchenLib.Utils;
using System.Collections.Generic;
using System.Diagnostics;


public class SmartMono : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity microwaveEntity;
    private Entity storedItemHolderComponent = Entity.Null;
    private bool shouldCheckEntities = false;
    private NativeArray<Entity> itemEntities;

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
        entityManager.AddComponentData(microwaveEntity, new CAutomatedInteractor
        {
            Type = InteractionType.Act
        });
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

    private void Update()
    {
        // Retrieve the SIsDayTime component
        EntityQuery sIsDayQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Kitchen.SIsDayTime>());
        if (sIsDayQuery.IsEmpty)
        {
            return;
        }

        var sIsDayEntity = sIsDayQuery.GetSingletonEntity();
        var sIsDay = entityManager.GetComponentData<Kitchen.SIsDayTime>(sIsDayEntity);

        // Check if it is daytime
        if (!entityManager.HasComponent<Kitchen.SIsDayTime>(sIsDayEntity))
        {
            return;
        }

        // Set the flag to true if it is daytime
        shouldCheckEntities = true;
        ModMW.Logger.LogInfo($"shouldCheckEntities set to: {shouldCheckEntities}");

        if (shouldCheckEntities)
        {
            ModMW.Logger.LogInfo($"shouldCheckEntities reset to: {shouldCheckEntities}");

            // Create an EntityQuery to find all entities with both CAppliance and CItemHolder components
            EntityQuery query = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<CAppliance>(),
                ComponentType.ReadOnly<CItemHolder>()
            );
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);

            // Create an EntityQuery to find all entities with the CItem component
            EntityQuery itemQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<CItem>());
            NativeArray<Entity> itemEntities = itemQuery.ToEntityArray(Allocator.TempJob);

            Entity cApplianceEntity = Entity.Null;

            foreach (var entity in entities)
            {
                try
                {
                    var applianceComponent = entityManager.GetComponentData<CAppliance>(entity);
                    var itemHolderComponent = entityManager.GetComponentData<CItemHolder>(entity).HeldItem;

                    if (applianceComponent.ID != SmartMicrowave.ID || itemHolderComponent == Entity.Null) continue;

                    cApplianceEntity = entity;

                    var cItem = entityManager.GetComponentData<CItem>(itemHolderComponent);
                    ModMW.Logger.LogInfo($"Item ID is {cItem.ID}");

                    // Iterate through ItemReferences properties to find a matching ID
                    foreach (var property in typeof(ItemReferences).GetProperties())
                    {
                        if (property.PropertyType == typeof(int))
                        {
                            int itemId = (int)property.GetValue(null);

                            if (cItem.ID == itemId)
                            {
                                ModMW.Logger.LogInfo($"Item ID {cItem.ID} matches with ItemReferences.{property.Name}");
                                // Perform your logic here, e.g., check if the item is bad


                                var itemGameObject = (Item)GDOUtils.GetExistingGDO(itemId);
                                if (itemGameObject != null)
                                {
                                    // Correctly assign the DerivedProcesses list
                                    itemGameObject.DerivedProcesses = new List<Item.ItemProcess>();
                                    var derivedProcesses = itemGameObject.DerivedProcesses;
                                    ModMW.Logger.LogInfo($"derivedProcesses value: {derivedProcesses}");
 
                                    // Access the IsBad values from the DerivedProcesses list
                                    foreach (var process in itemGameObject.DerivedProcesses)
                                    {
                                        ModMW.Logger.LogInfo($"IsBad value: {process.IsBad}"); 
                                    }

                                }
                                else
                                {
                                    ModMW.Logger.LogInfo($"Item Game Object is null.");
                                }
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //catch any exceptions. do not code here
                }

                // Store the current value of itemHolderComponent
                ModMW.Logger.LogInfo($"Microwave with ItemHolder: {entityManager.GetComponentData<CItemHolder>(entity).HeldItem}");
            }

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
            }

            // Dispose of the NativeArrays to avoid memory leaks
            entities.Dispose();
            itemEntities.Dispose();
            shouldCheckEntities = false;
        }
    }
}

