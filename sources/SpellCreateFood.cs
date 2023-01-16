using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AmongUsNS
{

    internal class CreateFood : Spell

    {
        public CardBag FoodToCreate;

        public override SpellTargets Targets => new SpellTargets();
     
        public override void DoubleClicked()
        {
            InitSpellEffect(MyGameCard);
            base.DoubleClicked();
        }
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "magic_dust")
                return true;
            return base.CanHaveCard(otherCard);
        }


        public override void SpellEffect()
        {
            List<string> GivenCard= new List<string>();
            for (int i = 0; i < FoodToCreate.CardsInPack; i++)
            {
               ICardId card= WorldManager.instance.GetRandomCard(FoodToCreate.Chances.Where((CardChance x) => ((WorldManager.instance.HasFoundCard(x.PrerequisiteCardId) || x.PrerequisiteCardId=="")&& !GivenCard.Contains(x.Id))).ToList(), false);
                
                CardData cardData = WorldManager.instance.CreateCard(transform.position, card.Id, faceUp: true, checkAddToStack: false);
                AudioManager.me.PlaySound2D(AudioManager.me.Eat, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
                cardData.MyGameCard.SendIt();
                GivenCard.Add(card.Id);

            }
            MyGameCard.SendIt();
            AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);

            base.SpellEffect();
            
        }

    }
}
