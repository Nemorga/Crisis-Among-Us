using BepInEx;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using BepInEx.Logging;
using BerryLoaderNS;
using System.Reflection.Emit;
using System.Resources;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Drawing;
using System.Collections;
using UnityEngine.Networking;



namespace AmongUsNS
{

    [BepInPlugin("Crisis_Among_Us", "Crisis : Among Us", "1.0.0")]
    [BepInDependency("BerryLoader")]
    class AmongUs : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource L;
        public static Harmony harmonyinstance;
        public static Dictionary<string, AudioClip> MyAudioClips = new Dictionary<string, AudioClip>();
        public static string Mypath;
        public static Dictionary<string, Sprite> MySprites = new Dictionary<string, Sprite>();
        
        //AmongUs Run Variable
        public static int AU_ThingKilled;
        public static bool AU_TravelerCanAppear;
        public static bool AU_OasisKilled;
        public static int AU_FriendlyTraveler;
        public static bool AU_First_Infect;

        private void Awake()
        {
            Mypath = Directory.GetParent(this.Info.Location).ToString();
            L = Logger;
            L.LogInfo("They're among us");
            harmonyinstance = new Harmony("AmongUs");
            harmonyinstance.PatchAll(typeof(Pacthes));
            LocAPI.LoadTsvFromFile(Path.Combine(Mypath,"Loc", "AmongUs_Loc.tsv"));
            InteractionAPI.CreateCardTypeInteraction(CardType.Resources, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (instance is Spell)
                        result = true;
                    else if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Mobs, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Food, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Humans, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Equipable, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Mobs, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }

            });
            InteractionAPI.CreateCardTypeInteraction(CardType.Structures, (CardData instance, CardData otherCard, ref bool? result) =>
            {
                if (otherCard is Spell)
                {
                    Spell spell = otherCard as Spell;
                    result = false;
                    if (spell.GetValidTarget(instance) && !spell.MyGameCard.HasChild)
                        result = true;
                }
                if (instance.HasStatusEffectOfType<StatusEffect_Fire>() && otherCard is Villager)
                    result = true;

            });
            
            

            LoadAudio(Path.Combine(Mypath, "Sounds", "spell_base.mp3"), "spell_base");
            LoadAudio(Path.Combine(Mypath, "Sounds", "fire.mp3"), "SL_fire");
            LoadAudio(Path.Combine(Mypath, "Sounds", "fire_end.mp3"), "SL_fire_end");
            LoadAudio(Path.Combine(Mypath, "Sounds", "reveal.mp3"), "SL_reveal");
            LoadAudio(Path.Combine(Mypath, "Sounds", "reveal_all.mp3"), "SL_reveal_all");
            LoadAudio(Path.Combine(Mypath, "Sounds", "oasis_death.mp3"), "SL_oasis_death");
            

            

            Dictionary<string, string> field = AccessTools.Field(typeof(Subprint), "specialCardIds").GetValue(typeof(Subprint)) as Dictionary<string, string>;
            field.Add("amongus_dummy_magic_user", "wizard|mage");
            AccessTools.Field(typeof(Subprint), "specialCardIds").SetValue(typeof(Subprint), field);


        }
        private void Start()
        {

            StartCoroutine(GetStatusSprite("still"));
            StartCoroutine(GetStatusSprite("giant"));
            StartCoroutine(GetStatusSprite("fire"));
            StartCoroutine(GetStatusSprite("contaminate"));
            StartCoroutine(GetStatusSprite("phase1"));
            StartCoroutine(GetStatusSprite("phase2"));
            StartCoroutine(GetStatusSprite("phase3"));
            StartCoroutine(GetStatusSprite("phase4"));
            StartCoroutine(GetStatusSprite("phase5"));
            StartCoroutine(GetStatusSprite("revealed"));

        }
        void LoadAudio(string path, string key)
        {

            StartCoroutine(ResourceHelper.GetAudioClip(path, (AudioClip ac) => { MyAudioClips.Add(key, ac); }, delegate { L.LogError("Couldn't load sound"); }));

        }
        IEnumerator GetStatusSprite(string id)
        {
            L.LogInfo("Coroutine started for " + id);
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(Path.Combine(Mypath, "Images", id + ".png")))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    L.LogError(uwr.error);
                }
                else
                {

                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    Sprite MySprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 162, 1);
                    MySprites.Add(id, MySprite);

                }
            }

        }



    }

        
    
}

   