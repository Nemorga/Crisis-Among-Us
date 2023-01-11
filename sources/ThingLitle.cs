using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.TextCore.LowLevel;

namespace AmongUsNS
{
    
    internal class ThingLitle : Enemy
    {
        
       
  
        protected override void Awake()
        {
            base.Awake();
            
            
           
            
        }

        public override void Clicked()
        {
            base.Clicked();
            
        }
        public override void UpdateCard()
        {
            base.UpdateCard();
            string desc = Description.Replace("---MISSING---", "Un petit amalgame de chaires et de morceaux d'animaux assemblé lamentablement.");
            descriptionOverride = desc;

        }


    }
}
