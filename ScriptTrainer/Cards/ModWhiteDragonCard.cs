﻿using DV;
using JTW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptTrainer.Cards
{
    public class ModWhiteDragonCard : CardWhiteDragon, IModCard
    {
        public ModWhiteDragonCard() : base("Card/IDS_CARD_NAME_ZGTestCard", CombatAction.CombatActionType.SKILL, CombatAction.ActionTargetType.ALLY_ALL)
        {
            Rarity = Rarity.RARE;
            //RARE,COMMON,UNCOMMON

            EnergyCost = 1;
        }
        public override void OnDowngrade()
        {
            this.ModCardUpgrade(false);
        }
        public override void OnUpgrade()
        {
            this.ModCardUpgrade(true);
        }
        public override string GetDisplayName()
        {
            if (CardInfo != null && !String.IsNullOrEmpty(CardInfo.CardDisplayName))
                return CardInfo.CardDisplayName;
            return base.GetDisplayName();
        }

        #region[IModCard]
        private Dictionary<string, ModCardValue> Values = new Dictionary<string, ModCardValue>();
        private ModCardInfo CardInfo;
        public ModCardValue AddValue(string key, object value)
        {
            Values[key] = new ModCardValue(value);
            return Values[key];
        }
        public ModCardValue GetValue(string key)
        {
            if (Values.TryGetValue(key, out ModCardValue value))
            {
                return value;
            }
            return null;
        }
        public DV.Action GetAction(string key)
        {
            foreach (var action in Action.ActionInternal.GetActions())
            {
                if (action.Name == key) return action;
            }
            return null;
        }
        public int GetActionCount()
        {
            return Action.ActionInternal.GetActions().Count;
        }
        public void SetActionActive(string key, bool active)
        {
            var x = GetAction(key);
            if (x != null)
            {
                if (active)
                {
                    x.ConditionFunc = null;
                }
                else
                {
                    x.ConditionFunc = ((DV.Action _action) => false);
                }
            }
        }
        public List<string> GetValueKeys() => Values.Keys.ToList();
        public void SetModInfo(ModCardInfo info)
        {
            CardInfo = info;
        }
        public ModCardInfo GetModInfo()
        {
            return CardInfo;
        }
        #endregion

        #region[保存和读取]
        public override void Copy(Card other)
        {
            base.Copy(other);
            if (other is IModCard modCard)
            {
                CardInfo = modCard.GetModInfo();
                if (CardInfo == null)
                    return;
                Action.ActionInternal.Cancel();
                Action = new CombatAction(CardInfo.CardName, CardInfo.CardCombatType, CardInfo.CardTargetType);
                Values.Clear();
                Action.ActionInternal = DynamicCardCreator.CreatActionList(CardInfo, this);
            }
        }
        public override CardSaveData GenerateSaveData()
        {
            CardSaveData cardSaveData = base.GenerateSaveData();
            cardSaveData.Data.Add("CardInfo", CardInfo.GetBytes());
            return cardSaveData;
        }
        public override void OnLoadSaveData(CardSaveData data)
        {
            base.OnLoadSaveData(data);
            if (!data.Data.ContainsKey("CardInfo"))
                return;
            CardInfo = ModCardInfo.ReadFromBytes((byte[])data.Data["CardInfo"]);
            if (CardInfo != null)
            {
                Name = CardInfo.CardName;
                Action.ActionInternal.Cancel();
                Action = new CombatAction(CardInfo.CardName, CardInfo.CardCombatType, CardInfo.CardTargetType);
                Values.Clear();
                Action.ActionInternal = DynamicCardCreator.CreatActionList(CardInfo, this);
            }
        }
        #endregion

    }
}
