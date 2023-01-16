using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmongUsNS
{
    
    internal class SpellCleanse : Spell

    {
        public bool better = false;

        public override SpellTargets Targets =>  new SpellTargets();
      
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "magic_dust")
                return true;
            return base.CanHaveCard(otherCard);
        }

        public override void InitSpellEffect(GameCard card)
        {

            base.InitSpellEffect(MyGameCard);

        }

        public override void SpellEffect()
        {
            CardData card = MyGameCard.Parent.CardData;
            if (!better)
            {
                
                card.StatusEffects.RemoveAll(x => x is not StatusEffect_Giant && x is not BossFight);
                if (card.HasStatusEffectOfType<StatusEffect_Giant>())
                {
                    StatusEffect_Giant giant = (StatusEffect_Giant)card.StatusEffects.Where(x => x is StatusEffect_Giant).FirstOrDefault();
                    giant.GiantTimer = 25f;
                }
            }
            else 
            {

                card.StatusEffects.RemoveAll(x => x is not StatusEffect_Giant && x is not StatusEffect_Invulnerable && x is not StatusEffect_Revealed && x is not StatusEffect_WellFed && x is not StatusEffect_Frenzy && x is not BossFight);

            }



            base.SpellEffect();
            
        }
        public override bool GetValidTarget(CardData card)
        {
            if(card.StatusEffects.Count !=0)
                return true;


            return false;


        }
    }
}
