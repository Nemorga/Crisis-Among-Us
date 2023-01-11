using BerryLoaderNS;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace AmongUsNS
{

    public class Spell : Resource
    {
        [ExtraData("charges")]
        public int Charges=1;
        public int ChargesMax =1;
        public float clicked = 0;

        
        public virtual SpellTargets Targets { get; set; }
        protected override void Awake()
        {
            if (Charges == -1)
                Charges = UnityEngine.Random.Range(1, ChargesMax);
            
            MyGameCard.SpecialIcon.sprite = SpriteManager.instance.FoodIcon;
            base.Awake();
            var mo = this.GetComponent<ModOverride>();
            mo.Color = ColorManager.instance.LocationCard2;
            mo.Color2 = ColorManager.instance.LocationCard; //LocationCard2;
            mo.IconColor = ColorManager.instance.GoldCardIcon;
            mo.Foil = true;
            
            SetFoil();
        }
        public override void UpdateCard()
        {

            if (clicked != 0)
                clicked += Time.deltaTime;
            if (clicked >= 0.5f)
                clicked = 0;
            
            string desc = $"\nA actuellement {Charges}/{ChargesMax} charges";
            var index = Description.IndexOf("\n");
            string desc2 = index>0? Description.Remove(index): Description;
            descriptionOverride = desc2+desc;
            
            base.UpdateCard();
            
        }   

        public override void Clicked()
        {
            
            clicked += Time.deltaTime;
            if (clicked >= 0.02 && clicked < 0.5f)
                DoubleClicked();
            
            base.Clicked();
            
        }
        public virtual void DoubleClicked()
        {

        }
        public override  void StoppedDragging()
        {
            if(MyGameCard.HasParent)
            {
                CardData card = MyGameCard.Parent.CardData;
                if(GetValidTarget(card))
                {
                    InitSpellEffect(MyGameCard);                   
                }
            }
            else
                base.StoppedDragging();
        }

        public virtual void SpellEffect()
        {
            Charges--;
            
            if (Charges <= 0) 
            {
                MyGameCard.DestroyCard(true, true);
            }
            else if(MyGameCard.HasParent)
            {
                MyGameCard.RemoveFromParent();
                MyGameCard.SendIt();
                AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
            }
           

        }
        public virtual void InitSpellEffect(GameCard card)
        {
            if (WorldManager.instance.RemovingCards)
                AbortSpell();
            WorldManager.instance.QueueCutscene(SpellAnim(card, card.HasParent? card.Parent.transform.position: card.transform.position));
        }
        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard is Spell)
                return true;
            return false;
        }
        public virtual void AbortSpell()
        {
            AudioManager.me.PlaySound2D(AudioManager.me.Block, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
            MyGameCard.RemoveFromParent();
            MyGameCard.SendIt();
            AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
        }
        public virtual bool GetValidTarget(CardData card)
        {
            
            if (Targets.ById != null && Targets.ById == card.Id)
                return true;
            if (Targets.ByIds != null && Targets.ByIds.Contains(card.Id))
                return true;
            if (Targets.ByCardType != null && (CardType)Enum.Parse(typeof(CardType), Targets.ByCardType, true) == card.MyCardType)
                return true;
            if (Targets.ByType != null && Targets.ByType.IsAssignableFrom(card.GetType()))
                return true;
            if (Targets.ForbidenIds != null && Targets.ForbidenIds.Contains(card.Id))
                return false;
            if (Targets.HasStatus && card.MyGameCard.GetRootCard().CurrentStatusbar != null)
                return true;
           
            return false;
        }
        public virtual IEnumerator SpellAnim(GameCard card, Vector3 position)
        {
            
            GameCamera.instance.TargetPositionOverride = card.transform.position;
            yield return new WaitForSeconds(0.35f);
            card.RotWobble(2f);
            Vector3 bounce = new Vector3 (0f, 8f, 0f);
            GameCard othercard = card.Parent?? null;
            if (othercard !=null)
            {
                card.RemoveFromStack();
            }
            card.Velocity = bounce;
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["spell_base"], UnityEngine.Random.Range(0.5f, 0.7f), 0.25f);
            int secure = 0;
            while (card.Velocity.HasValue) 
                {
                secure++;
                
                yield return new WaitForSeconds(0.1f);
                Vector2 posmod = UnityEngine.Random.insideUnitCircle.normalized * 0.3f;
                Vector3 pos = position;
                pos = new Vector3(pos.x + posmod.x, 0, pos.z + posmod.y);
                WorldManager.instance.CreateSmoke(pos);
                float range = secure / 10f;
                AudioManager.me.PlaySound2D(WorldManager.instance.GameDataLoader.GetCardFromId("gold").PickupSound, UnityEngine.Random.Range(1f-range, 1f+range), UnityEngine.Random.Range(0.3f,0.5f));
                
            }
            AudioManager.me.PlaySound2D(WorldManager.instance.GameDataLoader.GetCardFromId("key").PickupSound, 0.8f, 0.7f);
            yield return new WaitForSeconds(0.07f);
            AudioManager.me.PlaySound2D(WorldManager.instance.GameDataLoader.GetCardFromId("key").PickupSound, 0.35f, 0.7f);
            if(othercard != null)
                card.SetParent(othercard);
            yield return new WaitForSeconds(0.35f);
            SpellEffect();
            yield return new WaitForSeconds(0.15f);
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);
            
            
        }
        
    }

    public class SpellTargets
    {
       public string ById;
       public string[] ByIds;
       public string[] ForbidenIds;
       public bool HasStatus = false;
       public string ByCardType;
       public Type ByType;

    
    }
}
