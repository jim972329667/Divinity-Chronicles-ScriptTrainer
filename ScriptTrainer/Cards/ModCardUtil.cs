using DV;
using JTW;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptTrainer.Cards
{
    public interface IModCard
    {
        void SetModInfo(ModCardInfo info);
        ModCardInfo GetModInfo();
        ModCardValue AddValue(string key, object value);
        ModCardValue GetValue(string key);
        List<string> GetValueKeys();
        DV.Action GetAction(string key);
        void SetActionActive(string key, bool active);
        int GetActionCount();
    }
    public static class ModCardUtil
    {
        public const string UpgradeAdditon = "_UpgradeValue";
        public static void ModCardUpgrade(this IModCard card, bool Upgrade = true)
        {
            var upgradeKeys = card.GetValueKeys().Where(key => key.EndsWith(UpgradeAdditon)).ToList();

            foreach (var ukey in upgradeKeys)
            {
                var newkey = ukey.Replace(UpgradeAdditon, "");

                var uValue = card.GetValue(ukey);
                var oValue = card.GetValue(newkey);

                ATCombat value2 = (ATCombat)card.GetAction(newkey);
                if (oValue != null)
                {
                    if (newkey.StartsWith("F:"))
                    {
                        if (Upgrade)
                            card.AddValue(newkey, oValue.FloatValue + uValue.FloatValue);
                        else
                            card.AddValue(newkey, oValue.FloatValue - uValue.FloatValue);
                    }
                    else
                    {
                        if (Upgrade)
                            card.AddValue(newkey, oValue.IntValue + uValue.IntValue);
                        else
                            card.AddValue(newkey, oValue.IntValue - uValue.IntValue);
                    }
                }
                else if(value2 != null)
                {
                    if (uValue.StringValue.ToLower() == "false")
                    {
                        value2.ConditionFunc = ((DV.Action _addPotions) => false);
                    }
                    else if (uValue.StringValue.ToLower() == "true")
                    {
                        value2.ConditionFunc = null;
                    }
                    else
                    {
                        if (value2 is ATAttack)
                        {
                            value2.ChangeModCardUpgradeProperty("Damage", uValue, Upgrade);
                        }
                        else if (value2 is ATBlock)
                        {
                            value2.ChangeModCardUpgradeProperty("BlockValue", uValue, Upgrade);
                        }
                        else if (value2 is ATDrawCards cards)
                        {
                            value2.ChangeModCardUpgradeProperty("Count", uValue, Upgrade);
                        }
                        else if (value2 is ATChangeHPs || value2 is ATChangeEnergy)
                        {
                            value2.ChangeModCardUpgradeProperty("Change", uValue, Upgrade);
                        }
                        else if (value2 is ATAddStatus aTAddStatus)
                        {
                            var statu = aTAddStatus.Status;

                            statu.ChangeModCardUpgradeProperty("Count", uValue, Upgrade);
                            statu.ChangeModCardUpgradeProperty("Length", uValue, Upgrade);
                            statu.ChangeModCardUpgradeProperty("MaxCount", uValue, Upgrade);
                            statu.ChangeModCardUpgradeProperty("Cap", uValue, Upgrade);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"{ukey}:卡牌升级失败，没有获取到{newkey}参数！");
                }
            }
        }
        public static void ChangeModCardUpgradeProperty(this object obj, string property, ModCardValue value, bool upgrade)
        {
            var CountInfo = obj.GetType().GetProperty(property);
            if (CountInfo != null)
            {
                var countValue = CountInfo.GetValue(obj);
                if (countValue.GetType() == typeof(int))
                {
                    if (upgrade)
                        CountInfo.SetValue(obj, (int)countValue + value.IntValue);
                    else
                        CountInfo.SetValue(obj, (int)countValue - value.IntValue);
                }
                else if (countValue.GetType() == typeof(float))
                {
                    if (upgrade)
                        CountInfo.SetValue(obj, (float)countValue + value.FloatValue);
                    else
                        CountInfo.SetValue(obj, (float)countValue - value.FloatValue);
                }
            }
        }
        public static CombatObject RandomPickAllTarget(bool hasSelf)
        {
            CombatObject self = CombatAction.GetCurrentSource();
            STTaunted status = self.GetStatus<STTaunted>("Taunted");
            if (status != null)
            {
                return status.Target;
            }
            List<CombatObject> list = new List<CombatObject>();

            List<CombatObject> old = new List<CombatObject>();
            old.AddRange(Combat.Get().GetAllies(self, false));
            old.AddRange(Combat.Get().GetEnemies(self, false));
            if(!hasSelf)
                old.Remove(self);

            foreach (CombatObject combatObject in old)
            {
                if (!combatObject.CheckDeadOrEscaping())
                {
                    List<Status> statuses = combatObject.GetStatuses(typeof(STProtected));
                    bool flag = false;
                    using (List<Status>.Enumerator enumerator2 = statuses.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (((STProtected)enumerator2.Current).Protector == self)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag && combatObject.CheckCanDirectTarget())
                    {
                        list.Add(combatObject);
                    }
                }
            }
            if (list.Count != 0)
            {
                int index = RandomInt.Next(list.Count);
                return list[index];
            }
            return null;
        }
        public static CombatObject RandomPickAlliesTarget(bool hasSelf)
        {
            CombatObject self = CombatAction.GetCurrentSource();
            STTaunted status = self.GetStatus<STTaunted>("Taunted");
            if (status != null)
            {
                return status.Target;
            }
            List<CombatObject> list = new List<CombatObject>();

            List<CombatObject> old = new List<CombatObject>();
            old.AddRange(Combat.Get().GetAllies(self, false));
            if (!hasSelf)
                old.Remove(self);

            foreach (CombatObject combatObject in old)
            {
                if (!combatObject.CheckDeadOrEscaping())
                {
                    List<Status> statuses = combatObject.GetStatuses(typeof(STProtected));
                    bool flag = false;
                    using (List<Status>.Enumerator enumerator2 = statuses.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (((STProtected)enumerator2.Current).Protector == self)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag && combatObject.CheckCanDirectTarget())
                    {
                        list.Add(combatObject);
                    }
                }
            }
            if (list.Count != 0)
            {
                int index = RandomInt.Next(list.Count);
                return list[index];
            }
            return null;
        }
    }
}
