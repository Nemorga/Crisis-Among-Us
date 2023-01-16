using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static StatusEffectElement;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsNS
{

    internal class SpellGiant : Spell

    {
        

        public override SpellTargets Targets =>  new SpellTargets { ByType = typeof(Villager) };
     

       
        public override void InitSpellEffect(GameCard card)
        {
           
                base.InitSpellEffect(MyGameCard);
        }

        public override void SpellEffect()
        {
            GameCard target = MyGameCard.Parent;
            
            target.CardData.AddStatusEffect(new StatusEffect_Giant());
            AudioManager.me.PlaySound2D(AudioManager.me.Buff, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);

            base.SpellEffect();
            
        }

    }
    public class StatusEffect_Giant : StatusEffect
    {
        [ExtraData("giant_timer")]
        public float GiantTimer=0f;
        public Vector3 ChangeRate = new Vector3(0.008f, 0.008f, 0);
        [ExtraData("giant_change")]
        public Vector3 SizeChange = Vector3.zeroVector;
        [ExtraData("giant_initial")]
        public Vector3 Initial_Size= Vector3.zeroVector;
        public bool Init;




        protected override string TermId => "amongus_SE_Giant";
        public override Color ColorA => ColorManager.instance.StatusEffect_Frenzy_A;
        public override Color ColorB => Color.black;

        public override Sprite Sprite => AmongUs.MySprites["giant"];

        
        public override void Update()
        {
            
            

            Vector3 strtscale = (Vector3)AccessTools.Field(base.ParentCard.MyGameCard.GetType(), "startScale").GetValue(base.ParentCard.MyGameCard);
            if (!Init)
            {
                
              
                Initial_Size = strtscale ;
                Init= true;
                AccessTools.Field(base.ParentCard.GetType(), "_combatableDescription").SetValue(base.ParentCard, null);
            }
            if (Initial_Size == strtscale && SizeChange != Vector3.zeroVector)
            {
                AccessTools.Field(base.ParentCard.MyGameCard.GetType(), "startScale").SetValue(base.ParentCard.MyGameCard, Initial_Size + SizeChange);
                
                
            }
            FillAmount = 1f - GiantTimer / 30f;
            GiantTimer += Time.deltaTime * WorldManager.instance.TimeScale;
            if (GiantTimer <= 5f && SizeChange.x < (Initial_Size.x/4f) )
            {
                SizeChange += ChangeRate;
                Vector3 change = Initial_Size + SizeChange;
                AccessTools.Field(base.ParentCard.MyGameCard.GetType(), "startScale").SetValue(base.ParentCard.MyGameCard, change);

            }
            if(GiantTimer>=25f && SizeChange.x >0)
            {
                SizeChange -= ChangeRate;
                Vector3 change = Initial_Size + SizeChange;
                AccessTools.Field(base.ParentCard.MyGameCard.GetType(), "startScale").SetValue(base.ParentCard.MyGameCard, change);
            }
            
            
           
            if (GiantTimer >= 30f)
            {
                
                if(strtscale != Initial_Size)
                   AccessTools.Field(base.ParentCard.MyGameCard.GetType(), "startScale").SetValue(base.ParentCard.MyGameCard, Initial_Size);
                AccessTools.Field(base.ParentCard.GetType(), "_combatableDescription").SetValue(base.ParentCard, null);
                GiantTimer = 0f;
                base.ParentCard.RemoveStatusEffect(this);
            }
            

            base.Update();
        }
        
       
    }
}
