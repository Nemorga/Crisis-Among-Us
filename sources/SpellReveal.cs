using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static StatusEffectElement;

namespace AmongUsNS
{

    internal class SpellReveal : Spell

    {
        

        public override SpellTargets Targets =>  all ? new SpellTargets() : new SpellTargets { ByType = typeof(Villager) };
        public bool all= false;
      
        public override void DoubleClicked()
        {
            if (all) 
            {
                InitSpellEffect(MyGameCard);
            }
            base.DoubleClicked();
        }

        public override void InitSpellEffect(GameCard card)
        {
           
                base.InitSpellEffect(MyGameCard);
        }

        public override void SpellEffect()
        {
            if(!all)
            { 
            GameCard target = MyGameCard.Parent;
            target.CardData.AddStatusEffect(new StatusEffect_Revealed());
            AudioManager.me.PlaySound2D(AudioManager.me.Buff, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);

            base.SpellEffect();
            
            }
            else 
            {
                List<string> forbiden = new List<string> { "dog", "amongus_created_skeleton" };
                List<Villager> villagers = WorldManager.instance.GetCards<Villager>().Where(x => !forbiden.Contains(x.Id)).ToList();
                foreach(Villager villager in villagers) 
                {
                    villager.AddStatusEffect(new StatusEffect_Revealed());
                
                }


            }
            base.SpellEffect();
        }

    }
    public class StatusEffect_Revealed : StatusEffect
    {
        [ExtraData("still_revealed")]
        public float RevealTimer;

        protected override string TermId => base.ParentCard is TheThing ? "amongus_SE_Revealed" : "amongus_SE_Normal";
        public override Color ColorA => base.ParentCard is TheThing ? ColorManager.instance.StatusEffect_Poison_A : ColorManager.instance.StatusEffect_WellFed_A;
        public override Color ColorB => base.ParentCard is TheThing ? ColorManager.instance.StatusEffect_Poison_B : ColorManager.instance.StatusEffect_WellFed_B;

        public override Sprite Sprite => AmongUs.MySprites["revealed"];

        public override void Update()
        {
            
            
            
            
            FillAmount = 1f - RevealTimer/ WorldManager.instance.MonthTime;
            RevealTimer += Time.deltaTime * WorldManager.instance.TimeScale;
            if (RevealTimer >= WorldManager.instance.MonthTime)
            {

                RevealTimer = 0f;
                base.ParentCard.RemoveStatusEffect(this);
            }

            base.Update();
        }
       
    }
}
