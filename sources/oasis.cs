using BerryLoaderNS;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UIElements;


namespace AmongUsNS
{

    public class Oasis : SLBoss
    {




        protected override void Awake()
        {

            base.Awake();

                


        }

        public override void Clicked()
        {

            base.Clicked();
        }
        public override void Die()
        {

            WorldManager.instance.QueueCutscene(OnDeathCutScene());
            AmongUs.AU_OasisKilled = true;
            
        }
        public IEnumerator OnDeathCutScene()
        {
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal_all"], 0.7f, 0.2f);
            GameCamera.instance.TargetPositionOverride = MyGameCard.transform.position;
            yield return new WaitForSeconds(1f);
            if(InConflict) 
            { 
                Cutscenes.Title = "Victoire?";
                Cutscenes.Text = "Oasis semble défaite mais, dans un râle d'agonie, des chancres charnus purulant explosent et aspergent vos villageois d'une substance délétère";
                yield return Cutscenes.WaitForContinueClicked("oh non!");
                DeathThroes(Math.Max(MyConflict.GetTeamSize(Team.Player) / 2,1), 10, true, new string[]{"StatusEffect_Bleeding"}, null);
                yield return new WaitForSeconds(1f);
            }
            Cutscenes.Title = "Victoire!";
            Cutscenes.Text = "Puis tout son corps se met à trembler et à s'agiter de remous...";
            yield return Cutscenes.WaitForContinueClicked("hum?");
            GameCamera.instance.TargetPositionOverride = MyGameCard.transform.position;
            MyGameCard.RotWobble(2f);
            for (int count = 0; count <=100;count++)
            {
                yield return new WaitForSeconds(0.025f);
                Vector3  posmod = UnityEngine.Random.insideUnitCircle.normalized * 0.05f;
                posmod.y = 0f;
                MyGameCard.transform.position += posmod;
                if (count >= 60 && count % 5 == 0)
                {
                    WorldManager.instance.CreateSmoke(MyGameCard.transform.position);
                    AudioManager.me.PlaySound2D(AudioManager.me.HitMelee, UnityEngine.Random.Range(0.5f, 2f), UnityEngine.Random.Range(0.1f, 0.2f));
                    yield return new WaitForSeconds(0.05f);
                }
                if(count==90)
                    AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_oasis_death"], 0.7f, 0.2f);
            }
            TryDropItems();
            MyGameCard.DestroyCard();
            Cutscenes.Text = "Et dans un élan finale, l'horreure Oasis explose.";
            yield return Cutscenes.WaitForContinueClicked("Bon débaras!");
            Cutscenes.Title = "";
            Cutscenes.Text = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            yield return new WaitForSeconds(1f);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);

        }
        [PhaseActions(0)]
        public void RevealAlly()
        {
            if (MyConflict.Participants.Where(x => x is TheThing).Count() > 0)
                WorldManager.instance.StartCoroutine(RevealAllinConflictCoroutine());
            else
                Summon("amongus_the_thing1", 4);
             
        }
        public  IEnumerator RevealAllinConflictCoroutine()
        {


            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            WorldManager.instance.CurrentGameState = WorldManager.GameState.Paused;
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal_all"], 0.7f, 0.2f);
            Cutscenes.Title = "Perte d'alliés";
            Cutscenes.Text = "Oasis fait un seul geste et, parmi les combattant,tous les villageois remplacés par des monstres se transforment.";
            yield return Cutscenes.WaitForContinueClicked("Oh non!");
            Cutscenes.Text = "Les villageois se transforment...";
            List<Combatable> things = MyConflict.Participants.Where(x=> x is TheThing).ToList();   
            foreach (TheThing thing in things)
            {
                GameCamera.instance.TargetPositionOverride = thing.MyGameCard.transform.position;
                yield return new WaitForSeconds(1f);
                AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal"], UnityEngine.Random.Range(0.6f, 1.5f), UnityEngine.Random.Range(0.15f, 0.3f));
                List<Equipable> allEquipables = thing.GetAllEquipables();
                thing.MyGameCard.DestroyCard(spawnSmoke: true, false);
                Enemy monster = (Enemy)WorldManager.instance.CreateCard(thing.MyGameCard.transform.position, "amongus_the_thing" + thing.Stage, faceUp: true, checkAddToStack: false, false);
                foreach (Equipable item in allEquipables)
                {
                    monster.CreateAndEquipCard(item.Id, false);
                }
                monster.HealthPoints = monster.ProcessedCombatStats.MaxHealth;
                MyConflict.JoinConflict(monster);
                yield return new WaitForSeconds(1f);
                
            }
            Cutscenes.Title = "";
            Cutscenes.Text = "";
            WorldManager.instance.CurrentGameState = WorldManager.GameState.Playing;
            GameCamera.instance.TargetPositionOverride = null;
            yield return new WaitForSeconds(1f);
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);




        }
        [PhaseActions(1)]
        public void SummonThings()
        {
            int amount = Math.Max(MyConflict?.GetTeamSize(Team.Player)??0 / 2, 1);
            Summon("amongus_the_thing2", amount);
        }
        [PhaseActions(2, Loop:true)]
        public void RemoveStun()
        {
            if (this.HasStatusEffectOfType<StatusEffect_Stunned>())
                this.RemoveStatusEffect<StatusEffect_Stunned>();
        }
        [PhaseActions(2)]
        public void KillVillagerAndBuff()
        {
            Villager target = MyConflict?.Participants?.Where(x => x is Villager && x is not TheThing).First() as Villager;
            if (target != null)
            {
                CastSpell("amongus_spell_kill", target);

            }
            AddStatusEffect(new StatusEffect_Frenzy());
            AddStatusEffect(new StatusEffect_Invulnerable());
        }
        [PhaseActions(3)]
        public void BecomGiant()
        {
            CastSpell("amongus_spell_Giant", this);
            AddStatusEffect(new StatusEffect_Invulnerable());
        }
        [PhaseActions(4)]
        public void SummonThings2()
        {
            int power = Math.Min(Math.Max(MyConflict?.GetTeamSize(Team.Player)??0 / 2, 1),4);
            Summon("amongus_the_thing"+power.ToString(), 5);
        }
        
        [PhaseActions(4, Loop: true)]
        public void RemoveStunAndBuff()
        {
            if (this.HasStatusEffectOfType<StatusEffect_Stunned>())
                this.RemoveStatusEffect<StatusEffect_Stunned>();
            if (!HasStatusEffectOfType<StatusEffect_Frenzy>())
                AddStatusEffect(new StatusEffect_Frenzy());

        }


        public override void UpdateCard()
        {
            
            base.UpdateCard();
            string desc = Description.Replace("---MISSING---", "L'ignominie, la créature responsable des choses qui remplacent les villageois");
            descriptionOverride = desc;
            
            
                   
        } 
        


    }
    
}
