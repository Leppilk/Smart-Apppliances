using UnityEngine;
using Unity.Entities;
using SmartAppliances;
using SmartAppliances.Customs.Appliances;
using Kitchen;
using Unity.Collections;
using System;

public class SmartMono : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity microwaveEntity;
    private CItemHolder itemHolder = new CItemHolder();

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

    // Existing code...

    private void Update()
    {
        // Create an EntityQuery to find all entities with both CAppliance and CItemHolder components
        EntityQuery query = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<CAppliance>(),
            ComponentType.ReadOnly<CItemHolder>()
        );
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);

        foreach (var entity in entities)
        {
            try
            {
                var applianceComponent = entityManager.GetComponentData<CAppliance>(entity);
                var itemHolderComponent = entityManager.GetComponentData<CItemHolder>(entity).HeldItem;
                

                if (applianceComponent.ID != SmartMicrowave.ID || itemHolderComponent == Entity.Null) continue;

  
                entityManager.RemoveComponent<CRequiresActivation>(entity);
                entityManager.RemoveComponent<CIsInactive>(entity);
                ModMW.Logger.LogInfo($"Found appliance entity with ID: {applianceComponent.ID}");
                ModMW.Logger.LogInfo($"Microwave with ItemHolder: {itemHolderComponent}");
            }
            catch (Exception e)
            {
              
            }
        }

        // Dispose of the NativeArray to avoid memory leaks
        entities.Dispose();

        ModMW.Logger.LogInfo($"Microwave with Entity: {microwaveEntity}");
        ModMW.Logger.LogInfo($"Found microwave entity with ID: {SmartMicrowave.ID}");
    }
}

