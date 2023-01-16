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

    internal class SpellStill : Spell

    {
        

        public override SpellTargets Targets => new SpellTargets { ByType = typeof(Mob) };
      
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "iron_bar")
                return true;
            return base.CanHaveCard(otherCard);
        }

        public override void InitSpellEffect(GameCard card)
        {
           
                base.InitSpellEffect(MyGameCard);
        }

        public override void SpellEffect()
        {
            GameCard target = MyGameCard.Parent;
            target.CardData.AddStatusEffect(new StatusEffect_Still());
            AudioManager.me.PlaySound2D(AudioManager.me.Buff, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);

            base.SpellEffect();
            
        }

    }
    public class StatusEffect_Still : StatusEffect
    {
        [ExtraData("still_timer")]
        public float StillTimer;

        protected override string TermId => "amongus_SE_Still";
        public override Color ColorA => ColorManager.instance.StatusEffect_Drunk_A;
        public override Color ColorB => ColorManager.instance.StatusEffect_Drunk_B;

        public override Sprite Sprite => AmongUs.MySprites["still"];

        public override void Update()
        {
            
            
            Mob mob = base.ParentCard as Mob;
            mob.MoveTimer = 0;
            float StillTargetTimer = base.ParentCard is Enemy ? 30f : 90f;
            FillAmount = 1f - StillTimer / StillTargetTimer;
            StillTimer += Time.deltaTime * WorldManager.instance.TimeScale;
            if (StillTimer >= StillTargetTimer)
            {
                
                StillTimer = 0f;
                base.ParentCard.RemoveStatusEffect(this);
            }

            base.Update();
        }
       
    }
}
