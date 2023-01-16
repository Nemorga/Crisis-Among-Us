using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AmongUsNS
{

    internal class SpellKill : Spell

    {
        public bool DoKill = false;
        public int DoDamage = 0;
        

        public override SpellTargets Targets =>  new SpellTargets { ByType = typeof(Combatable) };
    
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "bone")
                return true;
            return base.CanHaveCard(otherCard);
        }

        public override void InitSpellEffect(GameCard card)
        {
            Combatable target = card.Parent.CardData as Combatable;
            if (target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
            {
                AbortSpell();
            }
            else
            {
                switch (target.ProcessedCombatStats.MaxHealth)
                {
                    case >= 200:
                        DoDamage = 40;
                        break;
                    case >= 100:
                        DoDamage = 30;
                        break;
                    case >= 50:
                        DoDamage = 20;
                        break;
                    case >= 25:
                        DoDamage = 15;
                        break;
                    case > 15:
                        DoDamage = 10;
                        break;

                    case <= 15:
                        DoKill = true;
                        break;

                }

                base.InitSpellEffect(MyGameCard);
            }
        }

        public override void SpellEffect()
        {
            GameCard target = MyGameCard.Parent;
            Combatable targetdata = target.CardData as Combatable;
            if (DoKill)
            {
                targetdata.CreateHitText(targetdata.HealthPoints.ToString(), PrefabManager.instance.CritHitText);
                targetdata.Damage(targetdata.HealthPoints);
                

            }
            else if (DoDamage > 0)
            {
                targetdata.Damage(DoDamage);
                targetdata.CreateHitText(DoDamage.ToString(), PrefabManager.instance.CritHitText);
            }
            AudioManager.me.PlaySound2D(AudioManager.me.HitMagic, UnityEngine.Random.Range(0.2f, 0.8f), 0.5f);
            DoDamage= 0;
            DoKill= false;
            base.SpellEffect();
            
        }

    }
}
