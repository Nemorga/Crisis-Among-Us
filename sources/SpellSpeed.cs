using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AmongUsNS
{

    internal class SpellSpeed : Spell

    {
        TimerAction SaveAction;
        Statusbar SaveBar;
        string SaveActionID;
        string SaveBpId;

        public override SpellTargets Targets => new SpellTargets { HasStatus = true, ForbidenIds = new string[]{ "kid","strange_portal", "chicken","egg"}  };
    
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "iron_bar")
                return true;
            return base.CanHaveCard(otherCard);
        }

        public override void InitSpellEffect(GameCard card)
        {
            GameCard target = card.Parent;


           
            if (target.GetRootCard().TimerActionId != "finish_blueprint" && target.GetRootCard().TimerActionId != "complete_harvest")
            {

                AbortSpell();

            }
            else if (target.CardData is Villager && target.GetRootCard().CardData.Id == "house")
            {
                AbortSpell();
            }
            else
            {
                SaveAction= target.GetRootCard().TimerAction;
                SaveBar = target.GetRootCard().CurrentStatusbar;
                SaveActionID = target.GetRootCard().TimerActionId;
                SaveBpId = target.GetRootCard().TimerBlueprintId;
                base.InitSpellEffect(MyGameCard);
            }
        }

        public override void SpellEffect()
        {
            
            GameCard target = MyGameCard.Parent.GetRootCard();
            target.TimerAction = SaveAction;
            target.TimerRunning= true;
            target.CurrentStatusbar = SaveBar;
            target.TimerActionId= SaveActionID;
            target.TimerBlueprintId= SaveBpId;
            target.CurrentTimerTime = target.TargetTimerTime;
            target.UpdateTimer();
           

            
            
            
            base.SpellEffect();
            
        }

    }
}
