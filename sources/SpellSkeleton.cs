using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AmongUsNS
{

    internal class SpellSkeleton : Spell

    {
        
        

        public override SpellTargets Targets => new SpellTargets { ById= "corpse" };
      

        public override void InitSpellEffect(GameCard card)
        {
            CardData target = card.Parent.CardData;
            if (target.MyGameCard.CurrentStatusbar != null)
            {
                AbortSpell();
            }
            else
                base.InitSpellEffect(MyGameCard);
            
        }

        public override void SpellEffect()
        {
            GameCard target = MyGameCard.Parent;
            CardData result = WorldManager.instance.CreateCard(target.transform.position, "amongus_created_skeleton");
            
            target.DestroyCard(false, false);
            base.SpellEffect();
            
        }

    }
}
