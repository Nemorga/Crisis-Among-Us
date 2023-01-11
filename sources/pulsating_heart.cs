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

        public override void Clicked()
        {
            
            
            base.Clicked();
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
            Cutscenes.Title = "Un rituel?";
            Cutscenes.Text = "Le villageois pense pouvoir faire un rituel qui appellera l'entité responsable de la prolifération de la chose. Doit-il le mener à bien?";
            yield return Cutscenes.WaitForAnswer("Oui, qu'on en finisse une bonne fois pour toute!","Euh...peut-être pas...");
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            yield return new WaitForSeconds(0.5f);
            if (WorldManager.instance.ContinueButtonIndex == 0)
            {
                Cutscenes.Title = "Le Rituel";
                Vector3 pos = heart.transform.position;
                heart.DestroyCard(spawnSmoke: true);
                mage.SendIt();
                yield return new WaitForSeconds(0.25f);
                CardData card = WorldManager.instance.CreateCard(pos, "amongus_oasis", faceUp: true, checkAddToStack: false);
                GameCamera.instance.TargetPositionOverride = card.MyGameCard.transform.position;
                Cutscenes.Title = "La Chose apparait";
                Cutscenes.Text = "\"<i>Mais? Quoi? Comment? Vous osez m'invoquer ici? Pensez-vous que votre petite communauté est plus importante qu'une autre?</i>\"";
                yield return Cutscenes.WaitForContinueClicked("heuu?");
                Cutscenes.Title = "La Chose parle:";
                Cutscenes.Text = "\"<i>Vous n'êtes rien, un grain de sable perdu dans une dune d'un désert infini. Coincé éternellement dans état liminaire avant la vrai réalisation et l'apothéose de l'existence.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("ah?");
                Cutscenes.Title = "La Chose soliloque:";
                Cutscenes.Text = "\"<i>Vous pensez surement que suis responsable de l'infestation qui vous accable. Il n'en est rien. Je suis une, je suis l'ensemble, je suis l'Autre, je suis la Chose, je suis Oasis. Je SUIS l'infestation.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("Mais...");
                Cutscenes.Title = "Oasis monologue:";
                Cutscenes.Text = "\"<i>Mais je ne suis pas une ennemie. Je ne suis pas là pour vous détruire. Je suis là pour vous sauvegarder, à jamais, à travers moi, en moi, comme mes pareils, mes semblables.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("Ah? heuu...");
                Cutscenes.Title = "Oasis continue, encore:";
                Cutscenes.Text = "\"<i>Vous vous pensez peut-être remplacés par des monstres, non : vous rejoignez un tout plus grand que la sommes de ses parties. C'est vous, faibles et isolé, qui êtes des monstres.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("C'est...");
                Cutscenes.Title = "Oasis est inarétable:";
                Cutscenes.Text = "\"<i>Et quand l'un de vous se joint à moi, il devient autre et beaucoup plus qu'il ne l'était à l'origine. Il devient important. Plus qu'un grain de sable, plus qu'une dune, il devient le désert.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("Peut-être, m-");
                Cutscenes.Title = "Oasis finit, enfin:";
                Cutscenes.Text = "\"<i>Alors, je vais vous faire une faveure. Et nous allons expédier les choses, vous cesserez votre existence vaine, et rejoindrez l'Autre, et serez enfin aussi important que le tout auquel vous appartenez.</i>\"";
                yield return Cutscenes.WaitForContinueClicked("Bon!");
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
