using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AmongUsNS
{

    internal class SpellHeal : Spell

    {
        

        public override SpellTargets Targets => base.Targets = new SpellTargets { ByType = typeof(Villager) };
        protected override void Awake()
        {
            
            
            base.Awake();
          
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "bone")
                return true;
            return base.CanHaveCard(otherCard);
        }
        public override void Clicked()
        {
            base.Clicked();
          
        }
 
       
        public override void InitSpellEffect(GameCard card)
        {
            Villager target = card.Parent.CardData as Villager;
            if (target.HealthPoints >= target.ProcessedCombatStats.MaxHealth)
            {

                AbortSpell();
                
            }
            else
                base.InitSpellEffect(MyGameCard);
        }

        public override void SpellEffect()
        {
            GameCard target = MyGameCard.Parent;
            Villager targetdata = target.CardData as Villager;
            int amount = targetdata.ProcessedCombatStats.MaxHealth - targetdata.HealthPoints;
            AudioManager.me.PlaySound2D(AudioManager.me.Buff, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
            targetdata.CreateHitText(amount.ToString(), PrefabManager.instance.HealHitText);
            targetdata.HealthPoints = targetdata.ProcessedCombatStats.MaxHealth;
            base.SpellEffect();
            
        }

    }
}
