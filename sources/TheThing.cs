using BerryLoaderNS;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace AmongUsNS
{
    
    internal class TheThing : Villager
    {
       
        public ThingType thingType = ThingType.None;
        [ExtraData("thing_type")]
        public string ToSaveType="";
        [ExtraData("thing_stage")]
        public int Stage = 1;
        [ExtraData("thing_action_time")]
        public float ActionTime = 120f;
        [ExtraData("thing_timer")]
        public float TimerToAction;
        [ExtraData("thing_falseid")]
        public string FalseId = "villager";
        [ExtraData("thing_ismover")]
        public bool IsMover = false;
        public float MoveTime;
        public float MoveTimer = 0;
        public float clicked = 0;
        public bool Init;
        

        

       
      
        protected override void Awake()
        {

            MoveTime = UnityEngine.Random.Range(45f, 90f);
            base.Awake();
            
        }
        
        public override int GetRequiredFoodCount()
        {
            return WorldManager.instance.InEatingAnimation?1:2;
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
            if(HasStatusEffectOfType<StatusEffect_Revealed>())
                WorldManager.instance.QueueCutscene(RevealCoroutine(this, delegate { AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null); }, false));

        }
        public override void OnEquipItem(Equipable equipable)
        {
            if (!string.IsNullOrEmpty(equipable.VillagerTypeOverride) && equipable.VillagerTypeOverride != FalseId && FalseId != "friendly_pirate" && FalseId != "dog")
            {
                ChangeForm(equipable.VillagerTypeOverride);
                
            }
            base.OnEquipItem(equipable);
        }
        public override void OnUnequipItem(Equipable equipable)
        {
            
            Equipable GetOverrideEquipable = GetAllEquipables().FirstOrDefault((Equipable x) => !string.IsNullOrEmpty(x.VillagerTypeOverride));
            if ( GetOverrideEquipable == null && FalseId != "villager" && FalseId != "friendly_pirate" && FalseId != "dog")
            {
                ChangeForm("villager");
            }
            base.OnUnequipItem(equipable);
        }
        public void SetTargetTime()
        {
            float basetime = WorldManager.instance.MonthTime;
            float timemod = UnityEngine.Random.Range(0.95f, 1.45f)-((float)Stage / 20f);
            switch (thingType)
            {
                case ThingType.Disruptor:
                    ActionTime = (basetime / 2f) * timemod;
                    break;
                case ThingType.Assimilator:
                    ActionTime = (basetime+(basetime/2)) * timemod;
                    break;
                case ThingType.Saboteur:
                    ActionTime = (basetime-(basetime/6)) * timemod;
                    break;
                case ThingType.Contaminator:
                    ActionTime = (basetime+ (basetime / 2)) * timemod;
                    break;
                case ThingType.Aggressor:
                    ActionTime = basetime * timemod;
                    break;

            }
            //Sauna.L.LogWarning(thingType.ToString()+" Set Targettime to: "+ActionTime.ToString());
        }
        public override void UpdateCard()
        {
            if (clicked != 0)
                clicked += Time.deltaTime;
            if (clicked >= 0.5f)
                clicked = 0;

            if (!Init)
            {
                if (thingType == ThingType.None)
                {
                    if (ToSaveType != "")
                        thingType = (ThingType)Enum.Parse(typeof(ThingType), ToSaveType, true);
                    else
                    {
                        ChooseType();
                        SetTargetTime();
                    }
                }
                CardData copy = WorldManager.instance.GameDataLoader.GetCardFromId(FalseId);
                NameTerm = copy.NameTerm;
                DescriptionTerm = copy.DescriptionTerm;
                Icon = copy.Icon;
                MyGameCard.UpdateIcon();
                Init= true;
                

            }
            
            if(IsMover)
                MoveTimer += Time.deltaTime * WorldManager.instance.TimeScale;
            if(MoveTimer >= MoveTime)
            {
                AudioManager.me.PlaySound2D(AudioManager.me.AnimalMove, UnityEngine.Random.Range(0.4f, 0.8f), 0.2f);
                MyGameCard.RemoveFromStack();
                MyGameCard.SendIt();
                MoveTimer= 0;
                    
            }
            TimerToAction += Time.deltaTime * WorldManager.instance.TimeScale;
            if (TimerToAction> ActionTime)
            {
                
                if (!MyGameCard.BeingDragged && !base.InConflict && WorldManager.instance.TimeScale > 0f)
                {
                    GameCard target = null;
                    target = Getarget();
                    if (target != null)
                    {
                        switch (thingType)
                        {
                            case ThingType.Disruptor:
                                AudioManager.me.PlaySound2D(AudioManager.me.AnimalMove, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
                                target.RemoveFromStack();
                                target.SendIt();
                                break;
                            case ThingType.Assimilator:
                                Assimilate((Villager)target.CardData);
                                break;
                            case ThingType.Saboteur:
                                WorldManager.instance.QueueCutscene(SetFire(target));
                                break;
                            case ThingType.Contaminator:
                                target.CardData.AddStatusEffect(new StatusEffect_contaminate() { ContaminateStrength = Stage });
                                break;
                            case ThingType.Aggressor:
                                WorldManager.instance.QueueCutscene(RevealCoroutine(this, delegate { AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null); },false));
                                break;
                                
                        }
                        
                        TimerToAction = 0;
                        if (Stage < 4)
                            Stage++;
                        //Sauna.L.LogWarning(thingType.ToString() + " ACTED");
                        SetTargetTime();

                    }
                }
               
                

            }
            if(TimerToAction > ActionTime + 30f)
            {
                float num = UnityEngine.Random.value;
                if (num < 0.3f && thingType!= ThingType.Assimilator)
                { 
                    ChooseType();
                    //Sauna.L.LogWarning("Changing type");
                }
                else if (Stage < 4 && num<0.6f)
                    Stage++;
                TimerToAction = 0;
                SetTargetTime();

            }
            base.UpdateCard();

        }
        public GameCard Getarget()
        {
            GameCard target = null;
            switch (thingType)
            {
                case ThingType.Disruptor:
                case ThingType.Assimilator:
                    {
                        List<string> forbiden= new List<string> {"dog", "amongus_created_skeleton", "trained_monkey" };
                        List<Villager> villagers = WorldManager.instance.GetCards<Villager>().Where(x => x is not TheThing && !forbiden.Contains(x.Id)).ToList();
                        
                        float num = 10f;
                        foreach (Villager villager in villagers)
                        {
                            if (villager.MyGameCard.BeingDragged || villager.InConflict)
                                continue;
                            Vector3 vec = MyGameCard.transform.position - villager.MyGameCard.transform.position;
                            vec.y = 0;
                            float dist = Vector3.Magnitude(vec);
                            float num2 = thingType == ThingType.Disruptor ? 1.5f : 1.2f;
                            if (dist < num2+((float)Stage/5) && dist < num)
                            {
                                target = villager.MyGameCard;
                                num = dist;
                            }

                        }
                        return target;
                    }
                case ThingType.Aggressor:
                    {

                        Villager villager = WorldManager.instance.GetCards<Villager>().Where(x => x is not TheThing).OrderBy(x => x.ProcessedCombatStats.CombatLevel).Last();
                        if(villager.ProcessedCombatStats.CombatLevel*1.5f < ProcessedCombatStats.CombatLevel * Stage)
                            target= villager.MyGameCard;    
                      
                        return target;
                    }
                case ThingType.Contaminator:
                    {
                        
                        List<Animal> animals = WorldManager.instance.GetCards<Animal>();

                        float num = 0f;
                        foreach (Animal animal in animals)
                        {
                            if (animal.MyGameCard.BeingDragged || animal.InConflict)
                                continue;
                            if(animal.HasStatusEffectOfType<StatusEffect_contaminate>())
                            {
                                target = null;
                                break;
                            }
                            Vector3 vec = MyGameCard.transform.position - animal.MyGameCard.transform.position;
                            vec.y = 0;
                            float dist = Vector3.Magnitude(vec);
                            if (dist < num || num ==0)
                            {
                                target = animal.MyGameCard;
                                num = dist;
                            }

                        }
                        return target;
                    }

                case ThingType.Saboteur:
                    {
                        
                        
                        List<GameCard> buildings = WorldManager.instance.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id).Where(x => x.CardData.MyCardType == CardType.Structures && x.CardData.IsBuilding &&x.CardData.Id != "campfire").ToList();

                        float num = 10f;
                        foreach (GameCard building in buildings)
                        {
                            if (building.BeingDragged || building.CardData.HasStatusEffectOfType<StatusEffect_Fire>())
                                continue;
                            Vector3 vec = MyGameCard.transform.position - building.transform.position;
                            vec.y = 0;
                            float dist = Vector3.Magnitude(vec);
                            if (dist < 2f + ((float)Stage / 5) && dist < num)
                            {
                                target = building;
                                num = dist;
                            }

                        }
                        return target;
                    }

                default: return target;

            }

            
        }
        public void SetMoverStatus()
        { 
            List<GameCard> villagers = WorldManager.instance.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id).Where(x => x.CardData is Villager).ToList() ;
            List<GameCard> allthings = villagers.Where(x => x.CardData is TheThing).ToList();
            int num = UnityEngine.Random.Range(2, villagers.Count());
            if (num < allthings.Count() && thingType != ThingType.Disruptor) 
                IsMover= true;
            else 
                IsMover= false;
        }
        public void ChooseType()
        {
            List<TheThing> allthing = WorldManager.instance.GetCards<TheThing>();
            int things = allthing.Count();
            int assimilator = allthing.Where(x => x.thingType == ThingType.Assimilator).Count();
            int saboteurs = allthing.Where(x => x.thingType == ThingType.Saboteur).Count();
            int agressors = allthing.Where(x => x.thingType == ThingType.Aggressor).Count();
            int contaminators = allthing.Where(x => x.thingType == ThingType.Contaminator).Count();
            int disruptors = allthing.Where(x => x.thingType == ThingType.Disruptor).Count();
            if (assimilator <=things/4f)
            { 
                thingType= ThingType.Assimilator;
                
            }
            else
            { 
                //Yeah those are stupide formulas but they're the only ones I was kinda happy with :<
                WeightedRandomBag<ThingType> bag = new WeightedRandomBag<ThingType>();
                bag.AddEntry(ThingType.Saboteur, (float)((contaminators + agressors+ disruptors) * (contaminators + agressors+disruptors) + 1) / ((saboteurs * saboteurs) + 2));
                bag.AddEntry(ThingType.Contaminator, (float)((saboteurs + agressors + disruptors) * (saboteurs + agressors + disruptors) + 1) / ((contaminators * contaminators) + 4));
                bag.AddEntry(ThingType.Aggressor, (float)((contaminators + saboteurs + disruptors) * (contaminators + saboteurs + disruptors) + 1) / ((agressors * agressors) + 2));
                bag.AddEntry(ThingType.Disruptor, (float)((contaminators + saboteurs + agressors) * (contaminators + saboteurs + agressors) + 1) / ((disruptors * disruptors) + 3));
                thingType= bag.Choose();
            }
            ToSaveType = thingType.ToString();
            //Sauna.L.LogWarning("CHOSEN: " + thingType.ToString()+" at Moon: "+WorldManager.instance.CurrentMonth.ToString());
        }
        public IEnumerator SetFire(GameCard building)
        {
            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_fire"], UnityEngine.Random.Range(0.7f, 1f), 0.4f);
            GameCamera.instance.TargetPositionOverride = building.transform.position;
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_thing_setfire_title");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_thing_setfire_text");
            yield return new WaitForSeconds(0.2f);
            building.CardData.AddStatusEffect(new StatusEffect_Fire() {FireTimer= Stage*5});
            yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);
        }
        public void ChangeForm(string id)
        {
            CardData typeoverride = WorldManager.instance.GameDataLoader.GetCardFromId(id);
            Icon = typeoverride.Icon;
            NameTerm = typeoverride.NameTerm;
            DescriptionTerm = typeoverride.DescriptionTerm;
            FalseId = typeoverride.Id;
            MyGameCard.UpdateIcon();
            WorldManager.instance.CreateSmoke(MyGameCard.transform.position + Vector3.up * 0.05f);

        }
        public static void Assimilate(Villager target)
        {
            TheThing newthing = (TheThing)WorldManager.instance.GameDataLoader.GetCardFromId("amongus_the_thing");
            newthing.FalseId = target.Id;
            newthing.InheritCombatStatsFrom = target.Id == "friendly_pirate" ? "pirate" : "villager";
            
            newthing.HealthPoints = target.HealthPoints;
            TheThing created = (TheThing)WorldManager.instance.CreateCard(target.MyGameCard.transform.position, newthing, true, false, false, false);
            string nameoverride = (string)AccessTools.Field(typeof(CardData), "nameOverride").GetValue(target);
            if (!string.IsNullOrEmpty(nameoverride))
                AccessTools.Field(typeof(CardData), "nameOverride").SetValue(created, nameoverride);
            created.MyGameCard.UpdateIcon();
            created.StatusEffects= target.StatusEffects;
            foreach(StatusEffect status in target.StatusEffects)
            {
                
                status.ParentCard = created;
                
            }
            foreach(Equipable equipable in target.GetAllEquipables())
            {
                created.CreateAndEquipCard(equipable.Id, false);
            }
            if(target.MyGameCard.HasParent)
            {
                GameCard parent = target.MyGameCard.Parent;
                target.MyGameCard.RemoveFromParent();
                created.MyGameCard.SetParent(parent);
                
            }
            if(target.MyGameCard.HasChild)
            {
                GameCard child = target.MyGameCard.Child;
                child.RemoveFromParent();
                created.MyGameCard.SetChild(child);

            }
            target.MyGameCard.DestroyCard(false, false);
            created.SetMoverStatus();
            
        }
        public IEnumerator RevealCoroutine(Combatable combatable, Action onComplete, bool halflife = true)
        {
            bool InCutscene = GameCanvas.instance.CurrentScreen == GameCanvas.instance.CutsceneScreen;
          
            GameCamera.instance.TargetPositionOverride = combatable.MyGameCard.transform.position;
            yield return new WaitForSeconds(1f);
            if (!InCutscene)
            {
                GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            }
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_thing_reveal");
            yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
            Cutscenes.Text = "";
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal"], UnityEngine.Random.Range(0.6f, 1.5f), UnityEngine.Random.Range(0.15f, 0.3f));

            List<Equipable> allEquipables = combatable.GetAllEquipables();
            combatable.MyGameCard.DestroyCard(spawnSmoke: true);
            TheThing thing = combatable as TheThing;
            Enemy monster = (Enemy)WorldManager.instance.CreateCard(combatable.MyGameCard.transform.position, "amongus_the_thing"+thing.Stage, faceUp: true, checkAddToStack: false);
            foreach (Equipable item in allEquipables)
            {
                monster.CreateAndEquipCard(item.Id, false);
            }
            monster.HealthPoints = halflife ? monster.ProcessedCombatStats.MaxHealth / 2 : monster.ProcessedCombatStats.MaxHealth; 
            if (!InCutscene)
            {
                GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            }
            yield return new WaitForSeconds(1f);
            GameCamera.instance.TargetPositionOverride = null;
            
            onComplete?.Invoke();

        }
        public static IEnumerator RevealAllCoroutine(List<TheThing> things)
        {
            

            GameCanvas.instance.SetScreen(GameCanvas.instance.CutsceneScreen);
            AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal_all"], 0.7f, 0.2f);
            Cutscenes.Title = SokLoc.Translate("label_amongus_cs_thing_invasion_title");
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_thing_invasion_text");
            yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
            Cutscenes.Text = SokLoc.Translate("label_amongus_cs_things_reveal_all");
            foreach(TheThing thing in things) 
            {
                GameCamera.instance.TargetPositionOverride = thing.MyGameCard.transform.position;
                yield return new WaitForSeconds(1f);
                AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_reveal"], UnityEngine.Random.Range(0.6f, 1.5f), UnityEngine.Random.Range(0.15f, 0.3f));
                List<Equipable> allEquipables = thing.GetAllEquipables();
                thing.MyGameCard.DestroyCard(spawnSmoke: true, false);
                Enemy monster = (Enemy)WorldManager.instance.CreateCard(thing.MyGameCard.transform.position, "amongus_the_thing" + thing.Stage, faceUp: true, checkAddToStack: false, false);
                foreach (Equipable item in allEquipables)
                {
                    monster.CreateAndEquipCard(item.Id, false);
                }
                monster.HealthPoints = monster.ProcessedCombatStats.MaxHealth;
                yield return new WaitForSeconds(1f);
            }
            Cutscenes.Title = "";
            Cutscenes.Text = "";
            GameCamera.instance.TargetPositionOverride = null;
            yield return new WaitForSeconds(1f);
            GameCanvas.instance.SetScreen(GameCanvas.instance.GameScreen);
            AccessTools.Property(typeof(Cutscenes), "currentAnimation").SetValue(typeof(Cutscenes), null);




        }

    }
    public class StatusEffect_contaminate : StatusEffect
    {
        [ExtraData("contaminate_timer")]
        public float SpawnTimer;
        [ExtraData("contaminate_strength")]
        public int ContaminateStrength;
        protected override string TermId => "amongus_SE_contaminate";
        public override Color ColorA => new Color(0.388f,0.369f, 0.259f);
        public override Color ColorB => new Color(0.286f, 0.106f, 0.106f);

        public override Sprite Sprite => AmongUs.MySprites["contaminate"];
        string[] SpawnIds = { "amongus_fangeux", "amongus_vermine", "amongus_grouillant" };
        public override void Update()
        {


            Animal animal = base.ParentCard as Animal;
            animal.CreateTimer = 0;
            float SpawnTargetTimer = 70-(ContaminateStrength*10);
            FillAmount = 1f - SpawnTimer / SpawnTargetTimer;
            if (!animal.InConflict)
            { 
                SpawnTimer += Time.deltaTime * WorldManager.instance.TimeScale; 
            }
            if(animal.HealthPoints <=0)
            {
                CardData card = WorldManager.instance.CreateCard(animal.MyGameCard.transform.position, "amongus_the_thing1", true, false, false);
                card.MyGameCard.SendIt();
                base.ParentCard.RemoveStatusEffect(this);
            }
            if (SpawnTimer >= SpawnTargetTimer)
            {
                
                string tospanw = SpawnIds[UnityEngine.Random.Range(0, 2)];
                CardData card = WorldManager.instance.CreateCard(animal.MyGameCard.transform.position, tospanw, true, false, false);
                card.MyGameCard.SendIt();
                SpawnTimer = 0f;
                
            }

            base.Update();
        }

    }
    public class StatusEffect_Fire : StatusEffect
    {
        [ExtraData("fire_timer")]
        public float FireTimer = 5f;
        public float DamageTimer;
        [ExtraData("fire_init")]
        public bool Init = false;
        protected override string TermId => Firefighter != null ? "amongus_SE_FireFighter" : "amongus_SE_Fire";
        public override Color ColorA =>  Firefighter != null? ColorManager.instance.StatusEffect_Invulnerable_A : ColorManager.instance.StatusEffect_Bleeding_A;
        public override Color ColorB => Color.black;

        public override Sprite Sprite => AmongUs.MySprites["fire"];
        
        public Villager Firefighter => base.ParentCard.MyGameCard.HasChild && base.ParentCard.MyGameCard.Child.CardData is Villager villager? villager : null;
        public override void Update()
        {
            
            


            CardData building = base.ParentCard;
            building.MyGameCard.CancelAnyTimer();
            building.Value = -1;
            if (!Init)
            {
                if(building.MyGameCard.HasChild)
                {
                    GameCard child = building.MyGameCard.Child;
                    child.RemoveFromParent();
                    List<GameCard> childstack = child.GetAllCardsInStack();
                    foreach(GameCard card in childstack)
                    {
                        card.RemoveFromStack();
                        card.SendIt();
                    }


                }
                    Init = true;
            }


            float StillTargetTimer = 30f;
            FillAmount = 1f - FireTimer / StillTargetTimer;
            float num = Firefighter != null ? -1 : 1;
            FireTimer += (Time.deltaTime * WorldManager.instance.TimeScale)*num;
            if (FireTimer >= StillTargetTimer)
            {

                FireTimer = 0f;
                base.ParentCard.RemoveStatusEffect(this);
                AudioManager.me.PlaySound2D(AmongUs.MyAudioClips["SL_fire_end"], UnityEngine.Random.Range(0.7f, 1f), 0.4f);
                base.ParentCard.MyGameCard.DestroyCard(true, false);
            }
            if (FireTimer < 0)
            {

                FireTimer = 0f;
                base.ParentCard.RemoveStatusEffect(this);
                int num2 = building.IsFoil ? 5 : 1;
                building.Value = WorldManager.instance.GameDataLoader.GetCardFromId(building.Id).Value*num2;
            }
            if (Firefighter != null)
            {
                DamageTimer += Time.deltaTime * WorldManager.instance.TimeScale;
                if (DamageTimer >= 3)
                {
                    Firefighter.Damage(1);
                    Firefighter.CreateHitText("1", PrefabManager.instance.HitTextPrefab);
                    AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, UnityEngine.Random.Range(0.8f, 1.2f), 0.2f);
                    DamageTimer = 0;
                }
            }

            base.Update();
        }

    }
    public enum ThingType
    {
        None,
        Assimilator,
        Disruptor,
        Saboteur,
        Aggressor,
        Contaminator
    }
}
