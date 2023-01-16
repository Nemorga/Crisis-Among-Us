using BerryLoaderNS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AmongUsNS
{
    
    public class SLBoss : Enemy
    {

        public int[] bossphasehp;
        public string[] immunities;
     
       
      
        public virtual void DeathThroes(int Range, int Power, bool DoDamage = true, string[] status_effects = null, Action otherstuff = null)
        {
            if (!InConflict)
                return;
            List<AudioClip> sound = null;
            switch (ProcessedAttackType)
            {
                case AttackType.None:
                case AttackType.Melee:
                    sound = AudioManager.me.HitMelee;
                    break;
                case AttackType.Ranged:
                    sound = AudioManager.me.HitRanged;
                    break;
                case AttackType.Magic:
                    sound = AudioManager.me.HitMagic;
                    break;
                default:
                    break;
            }
            List<Combatable> villagers = MyConflict.Participants.Where(x => x.Team == Team.Player).ToList();
            int validtargets = villagers.Count;
            for (int i = 0; i < Math.Min(Range, validtargets); i++)
            {
                if (!DoDamage && status_effects == null)
                    break;
                if (DoDamage)
                {
                    villagers[i].Damage(Power);
                    villagers[i].CreateHitText(Power.ToString(), PrefabManager.instance.CritHitText);
                    AudioManager.me.PlaySound2D(sound, UnityEngine.Random.Range(0.5f, 0.7f), 0.25f);
                }
                if (status_effects != null)
                {
                    foreach (string effect in status_effects)
                    {
                        villagers[i].AddStatusEffect(effect);
                    }
                }

            }
            otherstuff?.Invoke();

        }
        public void CastSpell(string spellid, Combatable target = null)
        {
            Spell spell = (Spell)WorldManager.instance.CreateCard(target == null ? MyGameCard.transform.position : target.transform.position, spellid);
            spell.Charges = 0;
            if (target != null)
                spell.MyGameCard.SetParent(target.MyGameCard);
            spell.InitSpellEffect(spell.MyGameCard);
        }
        public void Summon(string id, int amount = 0)
        {
            int villagers = MyConflict.GetTeamSize(Team.Player);
            for (int i = 0; i < Math.Max(amount, villagers); i++)
            {
                if (amount == 0 && i % 3 == 0 || i < amount)
                {
                    AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal"], UnityEngine.Random.Range(0.6f, 1.5f), UnityEngine.Random.Range(0.15f, 0.3f));
                    Enemy monster = (Enemy)WorldManager.instance.CreateCard(MyGameCard.transform.position, id, playSound: false);
                    WorldManager.instance.CreateSmoke(monster.MyGameCard.transform.position);
                    MyConflict.JoinConflict(monster);

                }
            }
        }

        public override void UpdateCard()
        {
            if (immunities != null)
            {
                foreach (string effect in immunities)
                {
                    if (effect != null)
                    {
                        RemoveStatusEffect(effect);
                        
                    }

                }
            }
            if (InConflict && !HasStatusEffectOfType<BossFight>())
                AddStatusEffect(new BossFight());
            base.UpdateCard();
            


            
        } 
        


    }
    public class BossFight : StatusEffect
    {
        

        protected override string TermId => "amongus_SE_Boss";
        public override Color ColorA => Color.red;
        public override Color ColorB => Color.black;

        public override Sprite Sprite => phasesprite;
        public Sprite phasesprite = AmongUs.MySprites["phase1"];
        
        public int[] phasesHp=> Boss.bossphasehp;
        public bool Inited;
        [ExtraData("bf_phase")]
        public int CurrentPhase = 0;
        [ExtraData("bf_stop")]
        public bool stop=false;
        public Dictionary<int, MethodInfo> phases = new Dictionary<int, MethodInfo>();
        public Dictionary<int, MethodInfo> cleanups = new Dictionary<int, MethodInfo>();
        public Dictionary<int, MethodInfo> loops = new Dictionary<int, MethodInfo>();
        public SLBoss Boss => base.ParentCard as SLBoss;



        
        public void Start()
        {
            MethodInfo[] methods = Boss.GetType().GetMethods();
            
            foreach(MethodInfo method in methods)
            {
                PhaseActionsAttribute attribute = method.GetCustomAttribute<PhaseActionsAttribute>();
                if (attribute == null)
                    continue;
                if (attribute.Loop)
                    loops.Add(attribute.Phase, method);
                else if (attribute.cleanup)
                    cleanups.Add(attribute.Phase, method);
                else
                    phases.Add(attribute.Phase, method);

            }
            Inited = true;



        }
        public float GetPhaseHP(int phase)
        {
            if (phase < 0)
                return (float)Boss.ProcessedCombatStats.MaxHealth;
            if (phase >= phasesHp.Length)
                return 0;
            return (float)phasesHp[phase];
        }
        public override void Update()
        {
            if (!Inited)
                Start();
            
            FillAmount = 1f - ((GetPhaseHP(CurrentPhase)- (float)Boss.HealthPoints) / (GetPhaseHP(CurrentPhase) - GetPhaseHP(CurrentPhase+1)));
            bool flag = CurrentPhase == 0 ? WorldManager.instance.IsPlaying : true;
            if (Boss.InConflict && flag)
            {
                if (loops.ContainsKey(CurrentPhase))
                    loops[CurrentPhase].Invoke(Boss, null);
                if (!stop)
                {
                    
                    stop = true;

                    phasesprite = AmongUs.MySprites["phase"+(CurrentPhase+1).ToString()];
                    if (cleanups.ContainsKey(CurrentPhase))
                        cleanups[CurrentPhase].Invoke(Boss, null);
                    if (phases.ContainsKey(CurrentPhase))
                        phases[CurrentPhase].Invoke(Boss, null);
                    
                    
                }
                if(CurrentPhase+1 < phasesHp.Length && Boss.HealthPoints <= phasesHp[CurrentPhase+1])
                {
                    CurrentPhase++;
                    stop= false;
                }
           
            }
            base.Update();
            
        }
      
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class PhaseActionsAttribute : Attribute
    {
        public int Phase;
        public bool cleanup;
        public bool Loop;


        public PhaseActionsAttribute(int Phase,  bool Loop = false, bool cleanup = false)
        {
            this.Phase = Phase;
            this.Loop = Loop;
            this.cleanup = cleanup;
        }
    }
}
