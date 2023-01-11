using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static StatusEffectElement;

namespace AmongUsNS
{

    internal class SpellFeast : Spell

    {
        

        public override SpellTargets Targets => base.Targets = new SpellTargets();
        protected override void Awake()
        {
            base.Awake();
          
        }


        public override void Clicked()
        {
            base.Clicked();
          
        }
        public override void DoubleClicked()
        {
            InitSpellEffect(MyGameCard);
            base.DoubleClicked();
        }

       

        public override void SpellEffect()
        {
            foreach(GameCard card in WorldManager.instance.AllCards)
            {
                if(card.MyBoard == WorldManager.instance.CurrentBoard && card.CardData is Villager villager &&  villager.Id != "amongus_created_skeleton") 
                {
                    villager.AddStatusEffect(new StatusEffect_WellFed());
                    AudioManager.me.PlaySound2D(AudioManager.me.Buff, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
                }

            }
            
            

            base.SpellEffect();
            
        }

    }
   
}
