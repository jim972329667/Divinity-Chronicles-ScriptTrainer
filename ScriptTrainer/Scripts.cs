using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using JTW;
using DV;
using System.Linq;
using System.Collections;
using HarmonyLib;
using System.IO;
using static System.Collections.Specialized.BitVector32;

namespace ScriptTrainer
{
    public class Scripts : MonoBehaviour
    {
        public static void UpgradeCard()
        {
            if (Game.Get().GetPlayer() == null)
                return;
            if (!Game.Get().CheckInputDisabled())
            {
                Game.Get().DisableInput();
            }
            CardsCallback cardSelectedCallback = delegate (List<Card> picked)
            {
                if (picked.Count == 0)
                {
                    Game.Get().EnableInput();
                    return;
                }
                BasicCallback finishCallback = delegate ()
                {
                    Game.Get().EnableInput();
                };
                EngineWrapper.Get().CreateAndUpgradeCards(picked, CardModifier.ValidLength.PERMANENT, finishCallback, null);
            };
            EngineWrapper.Get().PickCardsFromDeckForUpgrade(10, EngineWrapper.Get().GetLocalizedString("Camp/IDS_TEXT_UPGRADE_1_CARD"), cardSelectedCallback, null, false);
        }
        public static void RemoveCard()
        {
            if (Game.Get().GetPlayer() == null)
                return;
            if (!Game.Get().CheckInputDisabled())
            {
                Game.Get().DisableInput();
            }
            CardsCallback cardSelectedCallback = delegate (List<Card> picked)
            {
                if (picked.Count == 0)
                {
                    Game.Get().EnableInput();
                    return;
                }
                BasicCallback finishCallback = delegate ()
                {
                    Game.Get().EnableInput();
                };
                foreach (Card card in picked)
                {
                    Game.Get().GetPlayer().Deck.RemoveCard(card);
                }
                
                EngineWrapper.Get().RemoveCards(picked, delegate ()
                {
                }, false, 0.6f);
            };

            EngineWrapper.Get().PickCards(Game.Get().GetPlayer().Deck.GetCards(), 10, EngineWrapper.Get().GetLocalizedString("Merchant/IDS_TEXT_REMOVE_1_CARD"), cardSelectedCallback, null, null, false);
        }
        public static void UpgradeCompanion()
        {
            if (Game.Get().GetPlayer() == null)
                return;
            if (!Game.Get().CheckInputDisabled())
            {
                Game.Get().DisableInput();
            }
            List<Companion> companions = Game.Get().GetPlayer().CompanionSet.GetCompanions();
            CompanionsCallback callback = delegate (List<Companion> picked)
            {
                if (picked.Count == 0)
                {
                    return;
                }
                foreach (Companion companion in picked)
                {
                    companion.Upgrade(true, true);
                }
            };
            CompanionPickerConstraint constraint = delegate (Companion companion, List<Companion> picked, ref string reason)
            {
                if (!companion.CanUpgrade())
                {
                    reason = EngineWrapper.Get().GetLocalizedString("Camp/IDS_CAMP_Upgrade_Companion_R0");
                    return false;
                }
                //if (!companion.CanUpgradeInChapter())
                //{
                //    reason = EngineWrapper.Get().GetLocalizedString("Camp/IDS_CAMP_Upgrade_Companion_R1");
                //    return false;
                //}
                return true;
            };
            EngineWrapper.Get().PickCompanions(companions, 1, EngineWrapper.Get().GetLocalizedString("Generic/IDS_CAMP_UI_PICK_ONE_COMPANION_TO_UPGRADE"), callback, true, constraint, false);
        }
        private static List<Companion> GetCompanions(Player player, int count)
        {
            #region 添加同伴
            //Debug.Log("ZG:开始添加同伴");
            //CompanionSetInLocation companionSetInLocation = new CompanionSetInLocation();
            //companionSetInLocation.Add("Fire Fox", new FireFox(), 100);
            //companionSetInLocation.Add("Shadow Fairy", new ShadowFairy(), 100);
            //companionSetInLocation.Add("Black Tortoise", new BlackTortoise(), 100);
            //companionSetInLocation.Add("White Tiger", new WhiteTiger(), 100);
            //companionSetInLocation.Add("Azure Dragon", new AzureDragon(), 100);
            //companionSetInLocation.Add("Vermilion Bird", new VermilionBird(), 100);
            //companionSetInLocation.Add("Dark Unicorn", new DarkUnicorn(), 100);
            //companionSetInLocation.Add("Evil Monk", new HolyMonkEvil(), 100);
            //companionSetInLocation.Add("Water Dragon", new WaterDragon(), 100);
            //companionSetInLocation.Add("Hairy Monkey", new HairyMonkey(), 100);
            //companionSetInLocation.Add("Fat Pig", new FatPig(), 100);

            //companionSetInLocation.Add("Celestial of Fortune", new CelestialOfFortune(), 100);
            //companionSetInLocation.Add("Celestial of Wealth", new CelestialOfWealth(), 100);
            //companionSetInLocation.Add("Celestial of Longevity", new CelestialOfLongevity(), 100);
            //companionSetInLocation.Add("Peach Fairy", new PeachFairy(), 100);
            //companionSetInLocation.Add("Vanquisher of Evil", new VanquisherOfEvil(), 100);
            //companionSetInLocation.Add("Haunting Beauty", new HauntingBeauty(), 100);
            //companionSetInLocation.Add("Heaven Guardian", new HeavenGuardian(), 100);
            //companionSetInLocation.Add("Good Monk", new HolyMonkGood(), 100);
            //companionSetInLocation.Add("Ice Dragon", new IceDragon(), 100);
            //companionSetInLocation.Add("Mighty Monkey", new MightyMonkey(), 100);
            //companionSetInLocation.Add("Hungry Pig", new HungryPig(), 100);

            //List<Companion> list = companionSetInLocation.GetCompanions();
            //Debug.Log("ZG:结束添加同伴");
            #endregion

            List<Companion> companions = new List<Companion>();
           
            List<Companion> list = new List<Companion>();
            List<Type> players = new List<Type>();
            IEnumerable<Type> enumerable = from t in Assembly.GetAssembly(typeof(Companion)).GetTypes()
                                           where t.IsSubclassOf(typeof(Companion))
                                           select t;
            foreach(var x in player.CompanionSet.GetCompanions())
            {
                players.Add(x.GetType());
            }
            foreach(Type t in enumerable)
            {
                Companion companion = (Companion)Activator.CreateInstance(t);
                if(!companion.CrucialCompanion)
                {
                    if(!players.Contains(t))
                        list.Add(companion);
                }
            }
            Debug.Log($"companion ：{list.Count}");
            for (int i = 0; i < count; i++)
            {
                int index = RandomInt.Next(list.Count);
                if (list[index].CheckValidToAppear(player))
                {
                    list[index].InitHP();
                    list[index].GenerateCombatsToLeaveCount();
                    list[index].PriceToBuy = 0;
                    companions.Add(list[index]);
                    list.RemoveAt(index);
                }
                else
                {
                    i--;
                }
                if (list.Count == 0)
                {
                    break;
                }
            }
            return companions;
        }
        
        public static void HireMercenary()
        {
            Player player = Game.Get().GetPlayer();
            if (player == null)
                return;
            if(player.CompanionSet.GetCompanions().Count >= 4)
                return;
            List<Companion> companions = GetCompanions(player, 8);

            Debug.Log("同伴数量" + companions.Count.ToString());
            CompanionsCallback callback = delegate (List<Companion> picked)
            {
                if (picked.Count == 0)
                {
                    return;
                }
                foreach (Companion companion in picked)
                {
                    Game.Get().GetPlayer().AddCompanion(companion, true, null);
                }
            };
            EngineWrapper.Get().PickCompanions(companions, 1,EngineWrapper.Get().GetLocalizedString("Generic/IDS_PICKER_HIRE_COMPANION"), callback, true);
        }
        public static void AddMoney(int count)
        {
            Player player = Game.Get().GetPlayer();
            if(player != null) 
                player.Gold += count;
        }
        public static void AddTalentPoint(int count)
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
                player.TalentPoint += count;
        }
        public static void ChangeHp()
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
                player.ChangeTeamHP(player.MaxHP);
            Combat combat = Combat.Get();
            if (combat != null)
            {
                PlayerObject playerob = combat.m_playerObjects.Last();
                playerob.ChangeHP(playerob.MaxHP, null);
            }
        }
        public static void IncreasePotionContainerSize()
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
                player.IncreasePotionContainerSize(1);
        }
        public static void IncreaseHandSize()
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
                player.IncreaseHandSize(1);
        }
        public static void ZeroEnergyCost(bool state)
        {
            ScriptPatch.ZeroEnergyCost = state;
        }
        public static void IncreaseCompanionCapacity()
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
                player.CompanionSet.Capacity += 1;
        }
        public static void InfinityCompass(bool state)
        {
            ScriptPatch.InfinityCompass = state;
            if(state)
                EngineWrapperUnity.Get().UpdateMapStatuses();
        }

        public static void PickCardsFromDeckForSummonNPC()
        {
            if (!Game.Get().CheckInputDisabled())
            {
                Game.Get().DisableInput();
            }
            CardsCallback cardSelectedCallback = delegate (List<Card> picked)
            {
                Game.Get().EnableInput();
            };
            EngineWrapperUnity.Get().PickCardsFromDeckForSummonNPC(1, cardSelectedCallback);
           
        }
        #region[冥想卡牌]
        public static void MeditateCard()
        {
            Player player = Game.Get().GetPlayer();
            if (player != null)
            {
                if(player.Score.NPCKilled.Count > 0)
                {
                    CardsCallback cardSelectedCallback = delegate (List<Card> picked)
                    {
                        Game.Get().EnableInput();
                    };

                    if (HasShapeShiftingCard())
                    {
                        Game.Get().DisableInput();
                        EngineWrapper.Get().PickCardsFromDeckForTransform(player.ShapeShiftingMemoryLimit, player.AllowMeditateAllNPCs, cardSelectedCallback);
                    }
                    if (HasShapeShiftingCardSK())
                    { 
                        Game.Get().DisableInput();
                        EngineWrapper.Get().PickCardsFromDeckForTransformSK(player.ShapeShiftingMemoryLimit, player.AllowMeditateAllNPCs, cardSelectedCallback);
                    }
                }
            }
        }
        private static bool HasShapeShiftingCard()
        {
            Player player = Context.Get().GetValue<Player>("Current Human Player", null);
            using (List<Card>.Enumerator enumerator = player.Deck.GetCards().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is CardShapeShiftingBase)
                    {
                        return player.ShapeShiftingMemoryLimit != 0;
                    }
                }
            }
            return false;
        }
        private static bool HasShapeShiftingCardSK()
        {
            Player player = Context.Get().GetValue<Player>("Current Human Player", null);
            using (List<Card>.Enumerator enumerator = player.Deck.GetCards().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is CardShapeShiftingSK)
                    {
                        return player.ShapeShiftingMemoryLimit != 0;
                    }
                }
            }
            return false;
        }
        #endregion

        #region[附加卡牌属性]
        private static readonly Dictionary<CombatAction.CombatActionType, string> ModifierFormatStrings = new Dictionary<CombatAction.CombatActionType, string>()
        {
            { CombatAction.CombatActionType.NONE,"选择{0}张任意牌,{1}"},
            { CombatAction.CombatActionType.ATTACK,"选择{0}张攻击牌,{1}"},
            { CombatAction.CombatActionType.SKILL,"选择{0}张技能牌,{1}"}
        };
        private static string CurrentDesc = string.Empty;
        public static void ModifierCard()
        {
            Player player = Game.Get().GetPlayer();
            if (player == null)
                return;
            Game.Get().EnterAsyncComputation();
            Game.Get().DisableInput();

            List<Card> list = player.Deck.GetCards();
            
            if (list.Count == 0)
            {
                Game.Get().EnableInput();
                Game.Get().LeaveAsyncComputation();
                return;
            }
            CardsCallback callback = delegate (List<Card> picked)
            {
                if (picked.Count != 0)
                {
                    CardCallback halfway = delegate (Card card)
                    {
                        ModifierCard(card);

                        //card.AddModifier(GetModifier());
                    };
                    EngineWrapper.Get().CreateAndFlipCards(picked, halfway, delegate
                    {
                        Game.Get().EnableInput();
                        Game.Get().LeaveAsyncComputation();
                    });
                    return;
                }
                Game.Get().EnableInput();
                Game.Get().LeaveAsyncComputation();
            };

            CardConstraint constraint = delegate (Card card, ref string reason)
            {
                var type = GetActionType();
                bool flag = false;
                if (MainWindow.SelectModifier == 1)
                {
                    flag = card.CheckKeyword(Card.Keyword.EXHAUST);
                    if (!flag)
                    {
                        reason = string.Format(EngineWrapper.Get().GetLocalizedString("Relic/IDS_RELIC_Fantasy_Stone_R0"), card.GetDisplayName());
                    }
                    return flag;
                }
                else
                {
                    if (GetActionType() == CombatAction.CombatActionType.NONE)
                        return true;
                    flag = card.GetActionType(false) == type;
                    if (!flag)
                    {
                        if (type == CombatAction.CombatActionType.ATTACK)
                            reason = string.Format(EngineWrapper.Get().GetLocalizedString("Relic/IDS_RELIC_Phoenix_Feather_Fan_R0"), card.GetDisplayName());
                        else if (type == CombatAction.CombatActionType.SKILL)
                        {
                            reason = string.Format(EngineWrapper.Get().GetLocalizedString("Relic/IDS_RELIC_Ice_Worm_Web_R0"), card.GetDisplayName());
                            if(card.GetActionType(false) == CombatAction.CombatActionType.POWER)
                            {
                                flag = true;
                            }
                        }
                    }
                    return flag;
                }  
            };


            EngineWrapper.Get().PickCards(list, MainWindow.ModifierCardCount, string.Format(ModifierFormatStrings[GetActionType()], MainWindow.ModifierCardCount, CurrentDesc), callback, null, constraint, false);

        }

        public static void Test()
        {
            Debug.Log(string.Format(EngineWrapper.Get().GetLocalizedString("Relic/IDS_CM_PhoenixFeatherFan_DESC"), UITheme.kStatusTextColor));//移除词缀

            
        }
        public static void ModifierCard(Card card)
        {
            int count = MainWindow.ModifierCardIncrease;
            if(MainWindow.SelectModifier == 0)//移除1能量
            {
                var value = new CMChangeCardEnergyCost(CardModifier.ValidLength.PERMANENT)
                {
                    Change = -count
                };
                CurrentDesc = value.GetDescription();
                card.AddModifier(value);
                return;
            }
            else if(MainWindow.SelectModifier == 1)//移除消耗词缀
            {
                var value1 = new CMRemoveKeyword(CardModifier.ValidLength.PERMANENT)
                {
                    Keyword = Card.Keyword.EXHAUST
                };
                CurrentDesc = value1.GetDescription();
                card.AddModifier(value1);
                return;
            }


            ATAddStatus aTAddStatus = card.Action.ActionInternal.AddAction<ATAddStatus>(null);
            switch (MainWindow.SelectModifier)
            {
                default:
                case 2://中毒
                    aTAddStatus.Status = new STPoison
                    {
                        Count = count
                    };
                    break;
                case 3://流血
                    aTAddStatus.Status = new STBleeding
                    {
                        Count = count
                    };
                    break;
                case 4://燃烧
                    aTAddStatus.Status = new STBurning
                    {
                        Count = count
                    };
                    break;
                case 5://潮湿
                    aTAddStatus.Status = new STWet
                    {
                        Count = count
                    };
                    break;
                case 6://内伤
                    aTAddStatus.Status = new STRupture
                    {
                        Count = count
                    };
                    break;
                case 7://熟睡
                    aTAddStatus.Status = new STSleeping
                    {
                        Count = count
                    };
                    break;
                case 8://诅咒
                    aTAddStatus.Status = new STCursed
                    {
                        Count = count
                    };
                    break;
                case 9://缠绕
                    aTAddStatus.Status = new STEntangled();
                    break;
                case 10://失明
                    aTAddStatus.Status = new STBlind() { Length = count };
                    break;
                case 11://震惊
                    aTAddStatus.Status = new STShocked();
                    break;
                case 12://冰冻
                    aTAddStatus.Status = new STFreezing() { Length = count };
                    break;
                case 13://虚弱
                    aTAddStatus.Status = new STWeak() { Length = count };
                    break;
                case 14://脆弱
                    aTAddStatus.Status = new STFrail() { Length = count };
                    break;
                case 15://易伤
                    aTAddStatus.Status = new STVulnerable() { Length = count };
                    break;
                case 16://困惑
                    aTAddStatus.Status = new STConfused() { Length = count };
                    break;
                case 17://眩晕
                    aTAddStatus.Status = new STStunned();
                    break;
                case 18://凶蚀
                    aTAddStatus.Status = new STApparition() { Count = count };
                    break;
                case 19://善
                    aTAddStatus.Status = new STGood() { Count = count };
                    break;
                case 20://恶
                    aTAddStatus.Status = new STEvil() { Count = count };
                    break;
                case 21://能量吸收
                    aTAddStatus.Status = new STEnergyDrain() { Count = count };
                    break;
                case 22://诱惑
                    aTAddStatus.Status = new STCharmed() { Length = count };
                    break;
                case 23://绞杀
                    
                    CMSnakeSkinBelt value3 = new CMSnakeSkinBelt();
                    STStrangled sT = value3.Status as STStrangled;
                    sT.Count = count;
                    aTAddStatus.Status = sT;
                    break;
                case 24://回复
                    aTAddStatus.Status = new STRegeneration() { Count = count };
                    break;
                case 25://活体护甲
                    aTAddStatus.Status = new STLivingArmor() { Count = count };
                    break;
                case 26://闪避
                    aTAddStatus.Status = new STEvasion() { Count = count };
                    break;
                case 27://祝福
                    aTAddStatus.Status = new STBlessed() { Count = count };
                    break;
                case 28://飞行
                    aTAddStatus.Status = new STFlying() { Count = count };
                    break;
                case 29://霸体
                    aTAddStatus.Status = new STInvinciblePerDamage() { Count = count };
                    break;
                case 30://隐身
                    aTAddStatus.Status = new STInvisible() { Length = count };
                    break;
                case 31://守护
                    aTAddStatus.Status = new STArtifact() { Count = count };
                    break;
                case 32://力量
                    aTAddStatus.Status = new STStrength() { Count = count };
                    break;
                case 33://坚韧
                    aTAddStatus.Status = new STConstitution() { Count = count };
                    break;
            }
            if(aTAddStatus.Status != null)
                CurrentDesc = aTAddStatus.GetDescription().Text;
        }
        private static CardModifier GetModifier()
        {
            CMACAAddStatus status = new CMACAAddStatus(CardModifier.ValidLength.PERMANENT);
            int count = MainWindow.ModifierCardIncrease;
            switch (MainWindow.SelectModifier)
            {
                case 0://移除1能量
                    var value = new CMChangeCardEnergyCost(CardModifier.ValidLength.PERMANENT)
                    {
                        Change = -count
                    };
                    CurrentDesc = $"移除卡牌<b>{count}</b><color=yellow><b>能量</b></color>。";
                    return value;
                case 1://移除消耗词缀
                    var value1 = new CMRemoveKeyword(CardModifier.ValidLength.PERMANENT)
                    {
                        Keyword = Card.Keyword.EXHAUST
                    };
                    CurrentDesc = $"移除卡牌<color=yellow><b>消耗</b></color>词缀。";
                    return value1;
                case 2://中毒
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>中毒</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>中毒</b></color>。";
                    status.Status = new STPoison
                    {
                        Count = count
                    };
                    
                    return status;
                case 3://流血
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>流血</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>流血</b></color>。";
                    status.Status = new STBleeding
                    {
                        Count = count
                    };
                    return status;
                case 4://燃烧
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>燃烧</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>燃烧</b></color>。";
                    status.Status = new STBurning
                    {
                        Count = count
                    };
                    return status;
                case 5://潮湿
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>潮湿</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>潮湿</b></color>。";
                    status.Status = new STWet
                    {
                        Count = count
                    };
                    return status;
                case 6://内伤
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>内伤</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>内伤</b></color>。";
                    status.Status = new STRupture
                    {
                        Count = count
                    };
                    return status;
                case 7://熟睡
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>熟睡</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>熟睡</b></color>。";
                    status.Status = new STSleeping
                    {
                        Count = count
                    };
                    return status;
                case 8://诅咒
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>诅咒</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>诅咒</b></color>。";
                    status.Status = new STCursed
                    {
                        Count = count
                    };
                    return status;
                case 9://缠绕
                    CurrentDesc = $"向目标附加<color=yellow><b>缠绕</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<color=yellow><b>缠绕</b></color>。";
                    status.Status = new STEntangled();
                    return status;
                case 10://失明
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>失明</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>失明</b></color>。";
                    status.Status = new STBlind() { Length = count};
                    return status;
                case 11://震惊
                    CurrentDesc = $"向目标附加<color=yellow><b>震惊</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<color=yellow><b>震惊</b></color>。";
                    status.Status = new STShocked();
                    return status;
                case 12://冰冻
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>冰冻</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>冰冻</b></color>。";
                    status.Status = new STFreezing() { Length = count};
                    return status;
                case 13://虚弱
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>虚弱</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>虚弱</b></color>。";
                    status.Status = new STWeak() { Length = count };
                    return status;
                case 14://脆弱
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>脆弱</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>脆弱</b></color>。";
                    status.Status = new STFrail() { Length = count };
                    return status;
                case 15://易伤
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>易伤</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>易伤</b></color>。";
                    status.Status = new STVulnerable() { Length = count };
                    return status;
                case 16://困惑
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>困惑</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>困惑</b></color>。";
                    status.Status = new STConfused() { Length = count };
                    return status;
                case 17://眩晕
                    CurrentDesc = $"向目标附加<color=yellow><b>眩晕</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<color=yellow><b>眩晕</b></color>。";
                    status.Status = new STStunned();
                    return status;
                case 18://凶蚀
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>凶蚀</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>凶蚀</b></color>。";
                    status.Status = new STApparition() { Count = count };
                    return status;
                case 19://善
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>善</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>善</b></color>。";
                    status.Status = new STGood() { Count = count };
                    return status;
                case 20://恶
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>恶</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>恶</b></color>。";
                    status.Status = new STEvil() { Count = count };
                    return status;
                case 21://能量吸收
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>能量吸收</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>能量吸收</b></color>。";
                    status.Status = new STEnergyDrain() { Count = count };
                    return status;
                case 22://诱惑
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>诱惑</b></color>。";
                    status.m_addtionalDesc = $"向目标附加<b>{count}</b><color=yellow><b>诱惑</b></color>。";
                    status.Status = new STCharmed() { Length = count };
                    return status;
                case 23://绞杀
                    CurrentDesc = $"向目标附加<b>{count}</b><color=yellow><b>绞杀</b></color>。";
                    CMSnakeSkinBelt value3 = new CMSnakeSkinBelt();
                    STStrangled sT = value3.Status as STStrangled;
                    sT.Count = count;
                    value3.Status = sT;
                    return value3;


                case 24://回复
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>回复</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>回复</b></color>。";
                    //status.Status = new STRegeneration() { Count = count };
                    status.Status = new STRegeneration() { Count = count };
                    return status;
                case 25://活体护甲
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>活体护甲</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>活体护甲</b></color>。";
                    //status.Status = new STLivingArmor() { Count = count };
                    status.Status = new STLivingArmor() { Count = count };
                    return status;
                case 26://闪避
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>闪避</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>闪避</b></color>。";
                    //status.Status = new STEvasion() { Count = count };
                    status.Status = new STEvasion() { Count = count };
                    return status;
                case 27://祝福
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>祝福</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>祝福</b></color>。";
                    //status.Status = new STBlessed() { Count = count };
                    status.Status = new STBlessed() { Count = count };
                    return status;
                case 28://飞行
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>飞行</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>飞行</b></color>。";
                    //status.Status = new STFlying() { Count = count };
                    status.Status = new STFlying() { Count = count };
                    return status;
                case 29://霸体
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>霸体</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>霸体</b></color>。";
                    //status.Status = new STInvinciblePerDamage() { Count = count };
                    status.Status = new STInvinciblePerDamage() { Count = count };
                    return status;
                case 30://隐身
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>隐身</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>隐身</b></color>。";
                    //status.Status = new STInvisible() { Length = count };
                    status.Status = new STInvisible() { Length = count };
                    return status;
                case 31://守护
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>守护</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>守护</b></color>。";
                    //status.Status = new STArtifact() { Count = count };
                    status.Status = new STArtifact() { Count = count };
                    return status;
                case 32://力量
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>力量</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>力量</b></color>。";
                    status.Status = new STStrength() { Count = count };
                    return status;
                case 33://坚韧
                    CurrentDesc = $"向自己附加<b>{count}</b><color=yellow><b>坚韧</b></color>。";
                    status.m_addtionalDesc = $"向自己附加<b>{count}</b><color=yellow><b>坚韧</b></color>。";
                    status.Status = new STConstitution() { Count = count};
                    return status;

                default:
                    CurrentDesc = $"移除卡牌<b>{count}</b><color=yellow><b>能量</b></color>。";
                    var valueD = new CMChangeCardEnergyCost(CardModifier.ValidLength.PERMANENT)
                    {
                        Change = -count
                    };
                    return valueD;

            }

        }
        private static CombatAction.CombatActionType GetActionType()
        {
            if(MainWindow.ModifierActionType.Count <= MainWindow.SelectModifier)
                return CombatAction.CombatActionType.NONE;
            else
                return MainWindow.ModifierActionType[MainWindow.SelectModifier];
        }
        
        #endregion
    }
}
