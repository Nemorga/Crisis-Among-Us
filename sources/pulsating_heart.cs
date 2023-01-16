using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AmongUsNS
{

    internal class PulsatingHeart : Resource
    { 
        protected override void Awake()
        {
            base.Awake();
            var mo = this.GetComponent<ModOverride>();
            mo.Color = ColorManager.instance.AggresiveMobCard;
            mo.Color2 = ColorManager.instance.AggresiveMobCard2; 
            mo.IconColor = ColorManager.instance.AggresiveMobIcon;
        }
        public override void UpdateCard()
        {
            if (WorldManager.instance.IsPlaying && !WorldManager.instance.InAnimation && MyGameCard.HasChild && (MyGameCard.Child.CardData.Id == "mage" || MyGameCard.Child.CardData.Id == "wizard") &&!AmongUs.AU_OasisKilled)
                WorldManager.instance.QueueCutscene(Oasis_summon(MyGameCard,MyGameCard.Child));
                base.UpdateCard();
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "mage" || otherCard.Id == "wizard")
                return true;
            else
                return false;
        }
        public static IEnumerator Oasis_summon(GameCard heart,GameCard mage)
        {
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            GameCamera.instance.TargetPositionOverride = mage.transform.position;
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_ritual");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_ritual_text");
            yield return Cutscenes.WaitForAnswer(SokLoc.Translate("label_amongus_cs_ritual_yes"),SokLoc.Translate("label_amongus_cs_ritual_no"));
            Cutscenes.Text = "";
            
            yield return new WaitForSeconds(0.5f);
            if (WorldManager.instance.ContinueButtonIndex == 0)
            {
                
                Vector3 pos = heart.transform.position;
                heart.DestroyCard(spawnSmoke: true);
                mage.SendIt();
                yield return new WaitForSeconds(0.25f);
                CardData card = WorldManager.instance.CreateCard(pos, "amongus_oasis", faceUp: true, checkAddToStack: false);
                GameCamera.instance.TargetPositionOverride = card.MyGameCard.transform.position;
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_1");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_1");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_1"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_2");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_2");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_2"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_3");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_3");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_3"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_4");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_4");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_4"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_5");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_5");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_5"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_6");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_6");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_6"));
                Cutscenes.Title = SokLoc.Translate("label_amongus_cs_other_speak_title_7");
                Cutscenes.Text = SokLoc.Translate("label_amongus_cs_other_speak_text_7");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_amongus_cs_other_speak_answer_7"));
            }
            else
            {
                mage.RemoveFromStack();
            }
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);
        }
    }
}
