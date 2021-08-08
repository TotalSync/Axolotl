# Dev Notes & Code Explination

## **Table of Contents:**

1. [Introduction](#Introduction)
2. [Items](#Items)
3. [Interactables](#Interactables)

## Introduction

Hello, this document is aimed at listing out all of the code I wrote to help newcomers understand what they are looking at. First and foremost, I feel it is important to note that I am an Electrical Engineer by trade. I have no official learning in programming. If I am doing something that goes against convention, I am sorry, I do not know better. Do not take my advice as law, but more in the vein of suggestion.

I am using thunderkit to help develop this mod. If you do not know what thunderkit is, I highly suggest using it, as Twinner has put a massive amount of work into this tool, and arguably more helping people understand it. Thunderkit can be found [here](https://github.com/PassivePicasso/ThunderKit). Additionally the RoR2 modding community wiki is a fantastic, (if not scattered), resource for many things modding ([link](https://github.com/risk-of-thunder/R2Wiki/wiki)).

For overall tools, I highly suggest [downloading dnSpy](https://github.com/risk-of-thunder/R2Wiki/wiki/Code-Analysis-with-dnSpy), or an equivalent. This is an extremely powerful tool that lets you unpack and view the code of a dll. You can even use this on your own mod once it is compiled. This can be helpful to view the code you wrote in IL which is what your error message stack traces will be returned in.

(include error message image)

Now the last tool I suggest downloading is a Unity Runtime Viewer. I have packed the zip into this GitHub for later use for myself, but also for others :). It is the zipped file in the lib folder `Assets\_Axolotl\libs\multimod.zip`. When you open it, it should be quite self-explanatory. You would install this mod like a normal RoR2 mod. (Taking the entire multimod folder and placing it in your BepInEx plugins folder).

Enough with the initial BS of personal recommendations. Lets get started with the explanations. Last thing, this explanation expects you to have a basic understanding of how to use TK, although with enough tinkering you can bypass this statement.

## Items

Items are an one of the basic starting points in RoR2. Instead of opting for using a library to do most of the heavy lifting for me, I decided it would be a good idea to write a wrapper for myself. Perhaps this is my Engineer showing. Please note that Items and Equipment are very different things.

Items are composed of a number of important component:

1. Language Tokens
2. Item Display Rules (IDRs)
3. Hooks
4. Functionality
5. Model
6. Image
7. Item Tier
8. Unlockable Def

![An image of a new ItemDef](/src/img/itemdef.png)

Creating a new ItemDef from thunderkit will yield the shown data above. The first thing you notice is the Tier, and tokens. Most of these spaces are self-explanatory. Do note, that for any Language Token, the convention is to follow this format: `[MODPREFIX]_[ITEMFUNCTION]_[TYPETAG]` as an example, for the Item, Greedy Milk, its pickup token looks like: `AXOLOT_HEALTH_PER_LEVEL_PICKUP`. These tokens are used with R2API to replace the token name itself, with the appropriate text needed. I am currently transitioning to a new way of managing Language Tokens, so I will not include much here. Also do note, that when creating the language file strings, you can use RoR2's [styling](https://github.com/risk-of-thunder/R2Wiki/wiki/Style-Reference-Sheet) to help bring your items alive.

Another powerful tool in this is the bool `hidden`. This prevents the item from showing up on the players item bar at the top. I will expound upon this in the Starglass section.

### IDRs

IDRs are the next big topic. IDRs determine how items are shown on the player. These can be a bitch to do by hand. Thankfully KingEnderBrine made a lovely [helper mod](https://thunderstore.io/package/KingEnderBrine/ItemDisplayPlacementHelper/) for this.

Without getting into the full scope of IDRs, I will include a snippet of a single rule to give you a general idea of how they work.

```cs
idr.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
{
    new ItemDisplayRule
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        followerPrefab = ItemBodyModelPrefab,
        childName = "HandR",
        localPos = new Vector3(-0.19528F, 0.07389F, -0.17651F),
        localAngles = new Vector3(58.02077F, 167.347F, 93.26147F),
        localScale = new Vector3(0.05F, 0.05F, 0.05F)
    }
});
```

IDRs suck, but they add a lot to the game. I would suggest taking them seriously, and setting aside time to do them.

I personally had issues loading IDRs with my items. So instead of using the entire method ThunderKit suggest, I instead modified their code and ripped most of the data from it. Once I achieved that, I was able to use ItemAPI to instance all of my items, with the appropriate IDRs attached.

```cs
foreach (var item in ContentPackProvider.serializedContentPack.itemDefs)
{
   Log.LogInfo("Finding: " + item.nameToken);
   var ind = item_list.FindIndex(x => x.id == item.nameToken);
   var ban = item_ban_list.FindIndex(x => x == item.nameToken);
   if (ban == -1)
   {
       if (ind == -1 || item_list[ind].id != item.nameToken)
       {
           Log.LogWarning(nameof(Awake) + " Item Add Loop " + ": " + item.nameToken + " No IDR defined, or item is not in list.");
           Log.LogWarning("Index: " + ind + ". If this number is negative, then all is working normal.");
           ItemAPI.Add(new CustomItem(item, (ItemDisplayRuleDict)null));
       }
       else
       {
           ItemAPI.Add(new CustomItem(item, item_list[ind].idr));
       }
   }
   else
   {
       Log.LogMessage("Skipping " + item.nameToken + " because it was banned in the config.");
   }
}
```

The essence of what I am doing here, is I am meshing with my config system, and determining which items are outright disabled by them. If they are disabled, I skip them. If not, and I do not find an IDR in my own instance of an item, then I initialize the item with a `null` IDR. If I find an IDR, I load the item with my own IDR. I did this because I could not find a way to edit IDRs before after initialization.

### Hooks

Hooks are the next important topic. A hook is a part in code which allows multiple functions to be called from one event firing. When the event is triggered, all of the hooked functions fire. This is used to tie in the functionality of our code. How would we run our code otherwise? We can't just `while(true)` and move on with our lives. Most items will hook on the `RoR2.CharacterBody.RecaculateStats` function if the item is a base stat boost like Soldiers Syringe. Even with a bit of extra functionality, if you need to modify stats, you most likely will need a hook here. A part of making an item is determining what hooks you will need to use in order to achieve your goals.

Hooks can be written a few different ways. I use a number of different types in my code. The first way is by using a lambda (anonymous) function. Additionally in this example, I am using the On.[] Hook method. This is provided by MHOOK, and it can be used to prevent calling of the original function.

```cs
On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
   {
      //Functions to be called.
      orig(self);
   };
```

This method is useful if you have a few lines of code you want to add to a function. Do note, that you must include the appropriate arguments for the function to fire properly, as some events will send data with them when they trigger causing issues if the data that is sent is static. This issue in particular broke a good bit of my code. Here is a reference to the original function that was going to be called, and self is the object, in this instance CharacterBody, which is calling this function.

The second method is to just hook a whole pre-built function onto the even:

```cs
RoR2.CharacterBody.RecalculateStats += SomeFunction;
```

This means the function now technically acts as a delegate. I don't fully understand the differences yet, but I know there are some important differences. This delegate must have the same arguments as the data that is being sent with the function. Additionally you can unhook a function from an event by using the `-=` instead of the `+=`.

### Functionality

This part is where most of the work will be left up to the programmer. This is the part where you made the item do the do. You _make_ the item work. I can't be of much help on this front, and I would even go so far as to say, I shouldn't help too much here so you can become your own mod dev. A bit of tough love, but this is what it is all about. Making the mod do what you want. Just don't forget to set up the appropriate hooks to call the functions that you make for your items.

### Model, Image, & ItemTier

These are pretty self explanatory. Make a model, import it to Unity. Take the model, and make it into a prefab. Then assign the prefab into the itemDef. The same goes with the image. The only difference here is that you need to select the imported image and convert it into a sprite.

### Unlockable Defs

These are something that I have not dealt with yet. I will probably cover these more in the Survivor section when I get to that point.

### Greedy Milk

This item is relatively simple. It increases the amount of health you gain per level. There is nothing else special about this item. No weird work-around. Just, Item.

### Starglass

This item, like I mentioned earlier, actually uses two hidden helper items which count kills and also act as tallies for how many damage stacks a character has. This convention is actually quite similar to how infusion works.

### Teleporter Battery

This item works a little different than the other two as it doesn't have an IDR. In fact, I spawn a custom object in world on the Teleporter similar to how Lepton Daisy does. This is done in my `spawnItemDisplay()` function in its file.

## Interactables

Interactables were quite the jump from just working with items. I think it was a good idea, but I had to look into a lot of code on the way the game itself handles certain things. These are completely different from the items in that, they rely on GameObject instances existing and running themselves to a certain extent. These GameObjects, more often than not, have a specific Parent-Child Structure, and certain scripts that need to be placed on certain children to work properly. This is most notable with the Holograms for the price displays. These also need to be animated, so that brings up the complexity of Animators and Animations.

Much like Items, there are a number of important topic, then some additional topics for flair.

**Important:**

- Director integration
- Controller and Behaviour Scripts
- PurchaseInteractions
- Prefab Structure
- Networking
- Parenting

**Flair:**

- Animations
- Holograms

### Director Integration

Kicking off the important section, it would be a good idea to start with understanding how RoR2 handles spawning interactables. The director purchases interactables and monsters from a DirectorCardCategorySelection. These tend to be prefixed with dccs. These are essentially a 2d array containing sorted spawn cards.

![An Image of a dccs](/src/img/dccs.png)

This allows the Director to select an appropriate interactable to spawn. If you wanted to integrate your interactables into the spawn table, looking into the InteractablesAPI from R2API might be a good starting point. As it is right now, I do not have my interactables integrated into the spawn pool, so I do not know for a 100% fact how to do it.
Interactable spawn cards (isc) are of a similar vein. They supply the director with all of the information necessary to spawn the interactable in the world, and conditions for which it is to spawn or not to spawn.

![An Image of an Interactable Spawn Card](/src/img/isc.png)

### Controller and Behaviour Scripts

This is a difficult topic to describe. The easiest way to understand the relation between these two types of scripts is to consider the multishops in game. There are three terminals radially around a shaft in the middle. The controller is the shaft in the middle of the three terminals. The behaviours are each of the individual child terminals. The controller controls all of the terminals around them. When one shop is purchased, it reports to the controller that it purchased, and the controller closes the other shops.

![An Image of the Controller Behaviour Structure](/src/img/controller_behavior.png)

In this image, the Holder has the Controller script on it, and the Shops are the Children which have the Behavior scripts on them. I actually spawn each of the Shops using the Controller script. I do this because if the Director was spawning this, it would not care what spawnned in the shop. Its only concern is that the proper interactable was spawned. RoR2 does a similar thing in their spawning of the multishops.
I was unable to modify the original multishops to fit my need because the original shops pulled from a hard loot table that I could not configure. Creating them from scratch worked out better in the long run, but it was definitely more work.
To sum this section up, The Controller controls the children Behaviours. The Behaviours actually control the basic behaviors of the shop, but the controller can modify them if a certain condition is met. (i.e. another terminal was purchased and this terminal needs to close now.)

### PurchaseInteractions

PurchaseInteraction is a script that goes on every single interactable in game. This script controls deduction of money from the player among other things. It also can be altered to remove items from the player as well. The PurchaseInteraction goes on the object that has the Behaviour script, as the PurchaseInteraction is the script that allows for an interaction.

![An Imgae of the PurchaseInteraction](/src/img/purchase.png)
![An Image depicting the different types of costs PruchaseInteraction Supports](/src/img/purchase_cost.png)

In the first image, take note that the check box "Ignore Sphere-Cast for Interactability" is marked. This caused a number of small issues in my scripts when I was debugging. I am still not sure as to the full scope of this setting, but it messed with a good bit. The next thing to notice is the section Named "On Purchase (Interactor)". This is an explicit way of setting hooks. Do note that there is a way to dynamically set hooks.

```cs
gameObject.GetComponent<PurchaseInteraction>().onPurchase.AddListener(new UnityAction<Interactor>(Foo));
```

If you do set this hook dynamically, note that if you are calling a function from a different object, or a script not on the object with the PurchaseInteraction, you will have to navigate to it from the perspective of the PurchaseInteraction. For example, below, I modified my code to be a bit more explicit to show this navigation.

```cs
gameObject.GetComponent<PurchaseInteraction>().onPurchase.AddListener(new UnityAction<Interactor>(transform.parent.GetComponent<TargetMultiShopBehaviour>().purchaseCorrection));
```

If you want the terminal to be greyed out and unavailable to purchase, all you must do is `PurchaseInteraction.SetAvailable = false;`. The availability in networked, so you will have to do it this way instead of just directly setting `PurchaseInteraction.available = false;`.
If you are having issues with being able to purchase multiple items, logically this would be the component to check first. If you are creating an interactable that can be purchased multiple times (think chance shrines), there is a function to temporarily disable the PurchaseInteraction. I use this in my [Gatcha Chest](). A rather annoying bug I encountered with this though, was that I was unable to `PurchaseInteraction.SetAvailable = false;` because the code operated after the timer had started. The obvious solution was to use an if statement to determine if the interactable should be disabled permanently.

### Prefab Structure

Now onto the big topic. This topic alone took me a good while to piece together. All of the prefabs in RoR2 have a very particular structure. If you want to make an interactable that takes advantage of a lot of the pre-existing structures in place, then you will need to make sure you follow the appropriate structure. For example, assuming the correct structure, we can take advantage of the fact that the price above most interactables is already a script easily.

![An image of the cost hologram](/src/img/cost_hologram.png)

Lets start with the simpler piece, then integrate it into the full system. The gameobject with the behaviour script on it will have its own structure, but ideally it will be parented to the controller at some point.

![An image of a typical interactable layout](/src/img/interactable_layout.png)

To break down this structure show above, I will discuss each item and what it does.

---

#### **Behaviour Object**

##### **Interactable**

This is the parent object, and is the prefab itself. This object will contain:

- Behaviour Script
- PurchaseInteraction Script
- Highlight Script
- Ak Game Obj Script
- Dither Model Script\*

(\* = optional to my knowledge)

I have discussed the first two scripts enough for now, so I will focus on the last 3. The Highlight script, as its name implies, controls what GameObject is used for the base of the object Highlight. The contained GameObject is typically the main Mesh. (In our example Interactable above, it would be the InteractableMesh GameObject). The Ak Game Obj script is going to throw lots of errors and complain a ton if you place it into a scene. Feel free to ignore them. The Dither model does almost the exact same thing as the Highlight, but it dithers the texture for the RoR2 lighting feel (I think?).

##### **Display**

- (Box) Collider
- Entity Locator

Display contains the main collider for the Interactable. It doesn't have to be a box collider, I just chose that for my objects due to it fitting my Interactables better. The Entity Locator Points to the Parent Object. Again referencing our example structure, it would be pointing to `Interactable`.

##### **mdlInteractable**

- Animator
- Entity Locator
- Animation Events
- Child Locator
- Random Splat Bias

This is the main part of the GameObject now where we move away from actual functionality and more towards looks and feels. The animator is going to the main reason you will interact with this GameObject the most. You will need to make an [AnimationController]() object for this. This alone deserves its own section, and I will attempt address it later. The Entity Locator once again points to the main parent. I am not sure entirely what the Animation Events script does. I personally have just left it blank, but I know it deals with sound. I left the script that in case it not existing messes with some things. The Child Locator contains a reference to the DropCenter Transform.

![An image of the Child Locator](/src/img/dropcenter_loc.png)

The Random Splat Bias has something to do with drop tables. Once again I left it as I saw it on some vanilla items. My scripts pull from a custom drop table, so I do no think that this affects my Interactables in any way, but they may affect your interactables.

##### **(NOT SHOWN) Pickup Display**

Later I realized I did not include this GameObject in my example structure. This component is explicitly used if you are showing a pickup in the shop at all. This GameObject would be a child of the mdl GameObject. Below I have attached an image of my TargetMultiShop's PickupDisplay Components.

![An image of the PickupDisplay Components](/src/img/pickupdisplay_explicit.png)

For the most part all of the values shown here are generated default when you add the scripts to the object.

##### **ROOT**

This is just an Object which contains all of the sub-meshes that are a part of the object, but are not the main object itself. To my knowledge and through my experiences, I have not seen much functionality take place here.

##### **InteractableMesh**

This is the main mesh of the interactable. Not much to say here.

##### **DropCenter**

This is where the drop will initially be dropped from. I did not change the name, as again, I did not want to mess with possible errors in the code just because I changed the name of an object. I would highly suggest positioning this empty object in a place where the pickup orb can't collide with anything.

![DropCenter](/src/img/dropcenter.png)

---

#### **Controller Object**

Now that we have covered the structure of the Behaviour object, we can move to the structure of the Controller object. This object can tie more into the structure of the Behaviour object

The Controller Object structure is much more simple than the Behaviour. That being said, it has a few more nuances than the Behaviour. The Behaviour will be parented to the Controller Object once it is created, typically through `GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, parent.transform);`. This forces the parent to accept the new child, whereas, if you use `...Instantiate<GameObject>(prefab, parent.transform.position, parent.transform.rotation);` sometimes the child can be left as an orphan.

As another aside: make sure that your assets are imported through an assetbundle. TK offers an easy way through the use of their manifest.

![An image of FuckOffErrors](/src/img/manifest_bundle.png)

The Controller Scripts also contains a very important and useful line of code.

```cs
public class TargetMultiShopController : NetworkBehaviour, IHologramContentProvider
{
...
}
```

This interface `IHologramContentProvider` allows for very easy integration of the price holograms that appear above the shops. Be aware though that these will probably look for a vanilla PurchaseInteraction an may cause issues when loading a modded "price". Additionally by adding this interace VS should now be complaining about a number of functions not added to the script. These functions are all rather easy to add and expose almost no code.

```cs
// Used to check if cost hologram should be displayed
public bool ShouldDisplayHologram(GameObject viewer)
{
	return this.available;
}

//Copypasta for this one.
public GameObject GetHologramContentPrefab()
{
	return Resources.Load<GameObject>("Prefabs/CostHologramContent");
}

//Returns the cost type and the cost amount
public void UpdateHologramContent(GameObject hologramContentObject)
{
	CostHologramContent component = hologramContentObject.GetComponent<CostHologramContent>();
	if (component)
	{
		component.displayValue = this.cost;
		component.costType = this.costType;
	}
}
```

On the side of the actual structure for the Controller Object, it will only have one, maybe two children. The first is what you use as a location to spawn your Behaviour Object once your controller has finished setting up. The second is used by the `IHologramContentProvider`. In my attached images below, I show a multishop, so it has four children. I do this because I spawn three "different" shops and close all of them with the controller when any one of them is bought.

![](/src/img/targetholder.png)
![](/src/img/targetholder_comp.png)

As you can see in the second image, adding the Hologram Projector Script and attaching the transform of the HologramPivot allows everything to work smoothly.
