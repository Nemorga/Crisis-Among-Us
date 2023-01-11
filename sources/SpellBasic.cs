using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AmongUsNS
{

    internal class SpellBasic : Spell
    {
        public bool Funny= true;
        public float bounce=0f;
        public bool DoWobble = false;
        public CardBag SpellToCreate;
        public override SpellTargets Targets => base.Targets = new SpellTargets();

        protected override bool CanHaveCard(CardData otherCard)
        {
            if(otherCard.Id == "stick" || otherCard.Id == "poop" || otherCard.Id == "brick" || otherCard.Id == "empty_bottle" || otherCard.Id == "plank" 
                || otherCard.Id == "magic_dust" || otherCard.Id == "charcoal" || otherCard.Id == "key") 
                return true;
            return base.CanHaveCard(otherCard);
        }
        protected override void Awake()
        {
            base.Awake();

        }
        public override void StoppedDragging()
        {
            
            base.StoppedDragging();
            

            
            
            List<GameCard> Cards = new List<GameCard>();
            GameCard card = MyGameCard;
            while (card.HasParent && card.Parent.CardData.Id == Id)
            {
                card = card.Parent;
                Cards.Add(card);
            }
            card = MyGameCard;
            Cards.Add(MyGameCard);
            while (card.HasChild && card.Child.CardData.Id == Id)
            {
                card = card.Child;
                Cards.Add(card);
            }


            bool destroyed = false;
            for (var i = 0; i <= Cards.Count - 1; i++)
            {
                if (Cards.Count < 2)
                    break;
                SpellBasic spell = Cards[i].CardData as SpellBasic;
                spell.DoWobble = true;
                spell.bounce = this.bounce;
                Cards[i].RotWobble(UnityEngine.Random.Range(1.5f, 2.5f));
                if (i <= 3 && Cards.Count >= 4)
                {
                    if (!destroyed)
                    {
                        CardData newspell =  WorldManager.instance.CreateCard(Cards[i].transform.position, SpellToCreate.GetCard());
                        newspell.MyGameCard.RotWobble(3f);
                        newspell.MyGameCard.SendIt();
                    }
                    Cards[i].DestroyCard(true, true);
                    destroyed = true;
                }
            }
            
            
        }

       
        public override void UpdateCard()
        {

            
            GameCard card = MyGameCard;
            
            bool flag = false;
            bool flag2 = false;
            var rotvelo = (float)AccessTools.Field(card.GetType(), "wobbleRotVelo").GetValue(card);
            if (card.HasParent && card.Parent.CardData.Id == Id)
                flag = true;
            if (card.HasChild && card.Child.CardData.Id == Id)
                flag2= true;
            if ((!flag && !flag2) || !Funny)
            {
                DoWobble = false;
                bounce = 0;
            }
            if ((flag || flag2) && Funny)
            {
                DoWobble = true;
                
            }
            if (rotvelo == 0 && DoWobble)
            {

                bounce++;
                bounce = ((float)Math.Sqrt(bounce * bounce + bounce));
                card.RotWobble(UnityEngine.Random.Range(1.5f+(bounce/4f),2.5f+(bounce / 2f)));
                

            }
            if (bounce>=8)
            {
                if (flag)
                    CardExplose(card.Parent);
                if (flag2)
                    CardExplose(card.Child);
                CardExplose(card);
                
                
            }
            

            base.UpdateCard();
        }
        public void CardExplose(GameCard card)
        {
            card.RemoveFromStack();
            card.SendIt();
            AudioManager.me.PlaySound2D(AudioManager.me.Block, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
           ((SpellBasic)card.CardData).bounce = 0f;
        }
        public override void SpellEffect()
        {

            if (MyGameCard.Parent.CardData is Spell) 
            {
                Spell spell = MyGameCard.Parent.CardData as Spell;
                int boost = Math.Min(spell.Charges + Charges, spell.ChargesMax);
                spell.Charges = boost;
                spell.MyGameCard.RotWobble(2f);
                Charges = 0;
                
            }
            else 
            {
                List<GameCard> cards = MyGameCard.GetAllCardsInStack();
                foreach (GameCard card in cards)
                {

                    card.RemoveFromStack();
                    card.SendIt();
                    AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);


                }


            }
            base.SpellEffect();
            Funny = true;

        }
        public override void InitSpellEffect(GameCard card)
        {
            Funny =false;
            if(!card.HasParent)
            {
                AbortSpell();
                Funny = true;
                return;
            
            }
            CardData othercardData = card.Parent.CardData;
            
            if(othercardData is Spell)
            {
                Spell spell = othercardData as Spell;
                if (spell.Charges >= spell.ChargesMax)
                {
                 
                    Funny = true;
                    return;
                }    
                
                    
                
            }

            base.InitSpellEffect(card);
            

        }
        public override bool GetValidTarget(CardData card)
        {
            bool flag = (card is Food || card is Resource || card is Equipable || card is Gold) && card.MyGameCard.HasParent && card is not Spell;
            bool flag2 = card is Spell && card.Id != Id;
            if (flag || flag2 && !MyGameCard.HasChild)
                return true;
            return false;
        }


    }
}
