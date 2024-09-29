Here's what i think its going on:

This is supposed to be a Automated Microwave, you place the raw food inside, it detects it and cooks. I'm using an Idea similar to the TriggerActivation IsPossible>Perform where it removes the IsInactive component. 
But, when i check the log i get this error "TryGet Failed to find KitchenData.Appliance with ID 0"
which i supose is coming from the Cooking Proccess which then turns out to IsBad=True which triggers the Break If Bad Proccess part of the Microwave.

Sidenote, i'm using the lobster template as placeholders for the prefabs, since i'm going to do that after everything it working properly.
