using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AmongUsNS
{
    
    internal class ThingCreature : Enemy
    {
        
       
     
      
        public override void Die()
        {
            List<Equipable> equipables = GetAllEquipables();
            if (equipables.Count != 0)
            {
                foreach(Equipable equipable in equipables) 
                {
                    CardData card = WorldManager.instance.CreateCard(MyGameCard.transform.position, equipable, true, false, true, false);
                    card.MyGameCard.SendIt();
                    var removed = MyGameCard.EquipmentChildren.Remove(equipable.MyGameCard);
                    
                    equipable.MyGameCard.DestroyCard(false, false);
                }
            }
            AmongUs.AU_ThingKilled++;
            if(AmongUs.AU_ThingKilled >= (7+(WorldManager.instance.CurrentMonth/20)))
            {
                CardData card = WorldManager.instance.CreateCard(MyGameCard.transform.position, "amongus_pulsating_heart", true, false, true);
                card.MyGameCard.SendIt();
                AmongUs.AU_ThingKilled =0;
            }
            base.Die();
        }
       
        
     
        


    }
  
}
