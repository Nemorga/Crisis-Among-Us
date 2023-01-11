using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.TextCore.LowLevel;

namespace AmongUsNS
{
    
    internal class CreatedSkeleton : Villager
    {
        
       
      
        protected override void Awake()
        {
        
            base.Awake();
            
        }
        public override int GetRequiredFoodCount()
        {
            return 0;
        }
       
        public override void Clicked()
        {
            base.Clicked();
            
        }
        public override void UpdateCard()
        {
            if (HasStatusEffectOfType<StatusEffect_Drunk>())
                RemoveStatusEffect<StatusEffect_Drunk>();
            base.UpdateCard();
            string desc = Description.Replace("---MISSING---", "Un mort-vivant esclave créé par nécromancie");
            descriptionOverride = desc;

        }


    }
}
