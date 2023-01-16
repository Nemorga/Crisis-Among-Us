using AmongUsNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static WorldManager;

namespace AmongUsNS
{
     class Pacthes
    {
        [HarmonyPatch(typeof(CreatePackLine), "Start")]
        [HarmonyPostfix]
        static void AddBooster(ref string[] ___BoostersToCreate)
        {
            if (!___BoostersToCreate.Contains("island2"))
                ___BoostersToCreate = ___BoostersToCreate.AddToArray("amongus_magicbooster");

        }
        [HarmonyPatch(typeof(WorldManager), "GetSaveRound")]
        [HarmonyPostfix]

        static void SaveAUVar(ref SaveRound __result)
        {
            AmongUs.L.LogInfo("saving AU variables");
            SerializedKeyValuePairHelper.SetOrAdd(__result.ExtraKeyValues, nameof(AmongUs.AU_ThingKilled), AmongUs.AU_ThingKilled.ToString());
            SerializedKeyValuePairHelper.SetOrAdd(__result.ExtraKeyValues, nameof(AmongUs.AU_FriendlyTraveler), AmongUs.AU_FriendlyTraveler.ToString());
            SerializedKeyValuePairHelper.SetOrAdd(__result.ExtraKeyValues, nameof(AmongUs.AU_OasisKilled), AmongUs.AU_OasisKilled.ToString());
            SerializedKeyValuePairHelper.SetOrAdd(__result.ExtraKeyValues, nameof(AmongUs.AU_First_Infect), AmongUs.AU_First_Infect.ToString());

        }
        [HarmonyPatch(typeof(WorldManager), "LoadSaveRound")]
        [HarmonyPostfix]
        static void LoadAUVar(SaveGame ___CurrentSaveGame)
        {

            if (___CurrentSaveGame.LastPlayedRound.ExtraKeyValues.Count() != 0)
            {

                List<SerializedKeyValuePair> SKVP = ___CurrentSaveGame.LastPlayedRound.ExtraKeyValues;
                AmongUs.AU_ThingKilled = SKVP.GetWithKey(nameof(AmongUs.AU_ThingKilled)) != null ? Convert.ToInt32(SKVP.GetWithKey(nameof(AmongUs.AU_ThingKilled)).Value) : 0;
                AmongUs.AU_FriendlyTraveler = SKVP.GetWithKey(nameof(AmongUs.AU_FriendlyTraveler)) != null ? Convert.ToInt32(SKVP.GetWithKey(nameof(AmongUs.AU_FriendlyTraveler)).Value) : 0;
                AmongUs.AU_OasisKilled = SKVP.GetWithKey(nameof(AmongUs.AU_OasisKilled)) != null ? SKVP.GetWithKey(nameof(AmongUs.AU_OasisKilled)).Value == bool.TrueString ? true : false : false;
                AmongUs.AU_OasisKilled = SKVP.GetWithKey(nameof(AmongUs.AU_First_Infect)) != null ? SKVP.GetWithKey(nameof(AmongUs.AU_First_Infect)).Value == bool.TrueString ? true : false : false;
            }
            AmongUs.L.LogInfo("Loaded AU variables: \nReady = " + AmongUs.AU_ThingKilled + "\nEnd= " + AmongUs.AU_OasisKilled.ToString());


        }
        [HarmonyPatch(typeof(WorldManager), "StartNewRound")]
        [HarmonyPostfix]

        static void ResetAUVar()
        {
            AmongUs.L.LogInfo("reseting AU variables");

            AmongUs.AU_ThingKilled = 0;
            AmongUs.AU_FriendlyTraveler = 0;
            AmongUs.AU_OasisKilled = false;
            AmongUs.AU_First_Infect = false;

        }
        [HarmonyPatch(typeof(CardData), "CanHaveCardOnTop")]
        [HarmonyPostfix]

        static void SpellinConflict(ref bool __result, CardData __instance, CardData otherCard)
        {
            if (__instance.MyGameCard.InConflict && otherCard is Spell)
            {
                Spell spell = otherCard as Spell;
                if (spell.GetValidTarget(__instance))
                    __result = true;

            }

        }
        [HarmonyPatch(typeof(CardData), "CanHaveCardsWhileHasStatus")]
        [HarmonyPostfix]

        static void SpellonStatus(ref bool __result, CardData __instance)
        {
            GameCard card = WorldManager.instance.DraggingCard;
            if (card != null && card.CardData is Spell spell && spell.Id == "amongus_spell_speed" && spell.GetValidTarget(__instance))
                __result = true;


        }

        [HarmonyPatch(typeof(GameCard), "Update")]
        [HarmonyPostfix]
        static void SpecialTextOverride(GameCard __instance)
        {
            if (__instance.CardData is Spell)
            {



                Spell spell = __instance.CardData as Spell;
                PerformanceHelper.SetActive(__instance.SpecialText.gameObject, true);
                __instance.SpecialText.text = spell.Charges + "|" + spell.ChargesMax;
                PerformanceHelper.SetActive(__instance.SpecialIcon.gameObject, true);
                __instance.SpecialIcon.sprite = __instance.CoinIcon.sprite;



            }
        }
        [HarmonyPatch(typeof(WorldManager), "EndOfMonth")]
        [HarmonyPostfix]

        static void ResetSkeleton(WorldManager __instance)
        {
            foreach (CardData skeleton in __instance.GetCards("amongus_created_skeleton"))
            {
                __instance.CreateCard(skeleton.transform.position, "corpse", faceUp: true, checkAddToStack: false);
                List<Equipable> allEquipables = skeleton.GetAllEquipables();
                foreach (Equipable item in allEquipables)
                {
                    __instance.CreateCard(skeleton.transform.position, item.Id, faceUp: true, checkAddToStack: false, playSound: false).MyGameCard.SendIt();
                }
                skeleton.MyGameCard.DestroyCard(spawnSmoke: true);
            }

        }
        [HarmonyPatch(typeof(Villager), "GetActionTimeModifier")]
        [HarmonyPostfix]

        static void SetSkeletonTimeModifier(ref float __result, string ___Id)
        {
            if (___Id == "amongus_created_skeleton")
            {
                __result *= 1.5f;
            }
            if (___Id == "amongus_the_thing")
            {
                __result *= 1.3f;
            }
        }
        [HarmonyPatch(typeof(Combatable), "RealBaseCombatStats", MethodType.Getter)]
        [HarmonyPostfix]

        static void AddSspecialHitForStatusEffect(ref CombatStats __result, Combatable __instance)
        {
            SpecialHit GiantHit = new SpecialHit() { Chance = 30f, HitType = SpecialHitType.Damage, Target = SpecialHitTarget.AllEnemy };
            if (__instance.HasStatusEffectOfType<StatusEffect_Giant>() && !__result.SpecialHits.Contains(GiantHit))
            {

                __result.SpecialHits.Add(GiantHit);
            }
        }

        [HarmonyPatch(typeof(WorldManager), "KillVillagerCoroutine")]
        [HarmonyPrefix]

        static void ModifyVillagerDeathForTheThing(ref IEnumerator __result, Combatable combatable, Action onComplete, ref bool __runOriginal)
        {

            if (combatable is TheThing thing)
            {
                __runOriginal = false;
                __result = thing.RevealCoroutine(thing, onComplete);
            }
        }
        [HarmonyPatch(typeof(WorldManager), "Update")]
        [HarmonyPostfix]

        static void RevealAllThingPatch(WorldManager __instance)
        {
            if (__instance.CurrentGameState == GameState.Playing && __instance.currentAnimationRoutine == null && __instance.starterPack == null && __instance.currentAnimation == null && !TransitionScreen.InTransition && __instance.CurrentBoard.Id != "forest")
            {
                int villagers = WorldManager.instance.GetCards<Villager>().Where(x => x is not TheThing).Count();
                List<TheThing> things = WorldManager.instance.GetCards<TheThing>().ToList();
                if (things.Count() >= villagers)
                {
                    __instance.QueueCutscene(TheThing.RevealAllCoroutine(things));
                }

            }
        }
        [HarmonyPatch(typeof(WorldManager), "Update")]
        [HarmonyPostfix]

        static void StrangeTraverlerPatch(WorldManager __instance)
        {
            if (__instance.CurrentGameState == GameState.Playing && __instance.currentAnimationRoutine == null && __instance.starterPack == null && __instance.currentAnimation == null && !TransitionScreen.InTransition && __instance.CurrentBoard.Id != "forest"  && __instance.CurrentMonth >= 60)
            {
                if (__instance.GetCard<StrangePortal>() == null && __instance.GetCard<PirateBoat>() == null && __instance.GetCard<TravellingCart>() == null && __instance.CurrentMonth % 5 == 0 && __instance.MonthTimer <= 1f && AmongUs.AU_TravelerCanAppear)
                {
                    AmongUs.AU_TravelerCanAppear = false;
                    __instance.QueueCutscene(Traveler.TravelerArrival());
                }


            }
            if (__instance.MonthTimer > 1f)
            {
                AmongUs.AU_TravelerCanAppear = true;
            }
        }
    }
}
