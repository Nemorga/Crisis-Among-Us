using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AmongUsNS
{

    internal class Traveler : Resource
    {
        [ExtraData("traveler_type")]
        public string type ="none";
        public float movetimer;
        [ExtraData("traveler_timer")]
        public float actiontimer;
        public CardBag spells;
        public CardBag resources;
        public override bool CanBeDragged => false;
        protected override void Awake()
        {
            
            base.Awake();
          
        }
        public void Move()
        {
            AudioManager.me.PlaySound2D(AudioManager.me.AnimalMove, 1f, 1.5f);
            MyGameCard.SendIt();
        }
        public override void UpdateCard()
        {
            movetimer += Time.deltaTime * WorldManager.instance.TimeScale;
            actiontimer += Time.deltaTime * WorldManager.instance.TimeScale;
            if (movetimer >= 10f)
            {
                Move();
                movetimer= 0f;
            }
            if (actiontimer >= WorldManager.instance.MonthTime / 2)
            {
                DoStuff();
                actiontimer= 0f;
            }


            base.UpdateCard();
        }
        public void DoStuff()
        {
            if (type == "none")
                WorldManager.instance.QueueCutscene(Leave());
            if (type == "bad")
                WorldManager.instance.QueueCutscene(Infect());
            if (type == "resource" || type=="magic")
                WorldManager.instance.QueueCutscene(Reward());
        }

        public override void Clicked()
        {
            base.Clicked();
         
        }
        public IEnumerator Reward()
        {
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            GameCamera.instance.TargetPositionOverride = MyGameCard.transform.position;
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_traveler");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_traveler_reward");
            yield return Cutscenes.WaitForContinueClicked("Ah!");
            CardBag reward = new CardBag();
            if (type == "magic")
                reward = spells;
            if (type == "resource")
                reward = resources;
            for (int i =0; i< reward.CardsInPack; i++)
            {
                ICardId cardid = reward.GetCard(false);
                CardData card = WorldManager.instance.CreateCard(MyGameCard.transform.position, cardid, true, false, true);
                card.MyGameCard.SendIt();
                yield return new WaitForSeconds(0.5f);

            }
            yield return Leave();
        }
        public IEnumerator Infect()
        {
            Villager victim = WorldManager.instance.GetCards<Villager>().First();
            TheThing.Assimilate(victim);
            yield return Leave();
        }
        public IEnumerator Leave()
        {
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            GameCamera.instance.TargetPositionOverride = MyGameCard.transform.position;
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_traveler");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_traveler_leave");
            yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_farwell"));
            yield return new WaitForSeconds(0.25f);
            MyGameCard.DestroyCard(true, true);
            yield return new WaitForSeconds(0.25f);
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);
        }
        public static IEnumerator TravelerArrival()
        {
            
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            GameCamera.instance.TargetPositionOverride = WorldManager.instance.MiddleOfBoard();
            yield return new WaitForSeconds(0.5f);
            WorldManager.instance.CreateSmoke(WorldManager.instance.MiddleOfBoard());
            yield return new WaitForSeconds(0.25f);
            Traveler traveler = (Traveler)WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "amongus_strange_traveler", true, false, true);
            float num = UnityEngine.Random.value;
            float timemod = Mathf.Clamp(80f / (float)WorldManager.instance.CurrentMonth, 0.75f,1.25f);
            int villagers = WorldManager.instance.GetCards<Villager>().Count();
            float villagermod = Mathf.Clamp(20f / (float)villagers,0.75f, 5f);
            if (WorldManager.instance.CurrentMonth >= 70 && !AmongUs.AU_First_Infect)
                num = 0f;
            if (WorldManager.instance.GetCards<TheThing>().Count == 0 && !AmongUs.AU_OasisKilled && WorldManager.instance.GetCards<Villager>().Count > 3 && num * timemod * villagermod < 0.5f)
            {
                traveler.type = "bad";
                AmongUs.AU_First_Infect = true;
            }
            else
            {
                float friendmod = 1f + ((float)AmongUs.AU_FriendlyTraveler / 10f);
                num = num * Mathf.Clamp(friendmod, 1f, 3f);
                if (num >= 0.6f)
                    traveler.type = "resource";
                if (num >= 0.85f)
                    traveler.type = "magic";

            }
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_traveler");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_traveler_arrival");
            yield return Cutscenes.WaitForAnswer(SokLoc.Translate("label_amongus_cs_traveler_answer_yes"), SokLoc.Translate("label_amongus_cs_traveler_answer_no"));
            Cutscenes.Text = "";
            if (WorldManager.instance.ContinueButtonIndex == 1)
            {
                if (traveler.type == "none" || num > 1f)
                {
                    yield return traveler.Leave();
                    yield break;
                }
                else
                {
                    traveler.MyGameCard.RotWobble(2f);
                    yield return new WaitForSeconds(1f);
                    Cutscenes.Text = SokLoc.Translate("label_amongus_cs_traveler_refuse");
                    if (traveler.type != "bad")
                        traveler.type = "none";
                    yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_what"));
                    traveler.MyGameCard.SendIt();

                }
                

            }
            else 
            {
                traveler.MyGameCard.RotWobble(2f);
                yield return new WaitForSeconds(1f);
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_traveler_grateful");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_allright"));
                traveler.MyGameCard.SendIt();
                AmongUs.AU_FriendlyTraveler++;

            }
            
            yield return new WaitForSeconds(1f);
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            return false;
        }
    }
}
