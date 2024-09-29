Here's what i think its going on:

This is supposed to be a Automated Microwave, you place the raw food inside, it detects it and cooks. I'm using an Idea similar to the TriggerActivation IsPossible>Perform where it removes the IsInactive component. 
But, when i check the log i get this error "TryGet Failed to find KitchenData.Appliance with ID 0"
which i supose is coming from the Cooking Proccess which then turns out to IsBad=True which triggers the Break If Bad Proccess part of the Microwave.
Also for now i'm not checking if the food inside is already cooked.

whatever it is its calling this method

    public bool TryGet<T>(int id, out T output, bool warn_if_fail = false)
    {
        if (Objects.TryGetValue(id, out var value) && value is T)
        {
            T val = (T)(object)((value is T) ? value : null);
            output = val;
            return true;
        }

        if (warn_if_fail)
        {
            Debug.LogError($"Failed to find {typeof(T)} with ID {id}");
        }

        output = default(T);
        return false;
    }


Sidenote, i'm using the lobster template as placeholders for the prefabs, since i'm going to do that after everything it working properly.
