using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.TextCore.LowLevel;

namespace AmongUsNS
{
    
    internal class CreatedSkeleton : Villager
    {
        
        public override int GetRequiredFoodCount()
        {
            return 0;
        }
       
        public override void UpdateCard()
        {
            if (HasStatusEffectOfType<StatusEffect_Drunk>())
                RemoveStatusEffect<StatusEffect_Drunk>();
            if (HasStatusEffectOfType<StatusEffect_Bleeding>())
                RemoveStatusEffect<StatusEffect_Bleeding>();
            if (HasStatusEffectOfType<StatusEffect_Poison>())
                RemoveStatusEffect<StatusEffect_Poison>();
            if (HasStatusEffectOfType<StatusEffect_WellFed>())
                RemoveStatusEffect<StatusEffect_WellFed>();
            base.UpdateCard();
           

        }


    }
}
