using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CharactersStats
{
    [Serializable]
    public class CharacterStat
    {
        #region Variables and Properties
        public float BaseValue = 0f;

        protected float m_value;
        public virtual float Value
        {
            get
            {
                if (changedRecently || BaseValue != lastBaseValue)
                {
                    lastBaseValue = BaseValue;
                    m_value = CalculateFinalValue();
                    changedRecently = false;
                }
                return m_value;
            }
        }
        protected float lastBaseValue = float.MinValue;

        protected bool changedRecently = true;

        protected readonly List<StatModifier> statModifiers;
        public readonly ReadOnlyCollection<StatModifier> StatModifiers;
        #endregion

        // ----------------------------------------------------------------------

        #region Constructor
        public CharacterStat()
        {
            statModifiers = new List<StatModifier>();
            StatModifiers = statModifiers.AsReadOnly();
        }

        public CharacterStat(float m_baseValue) : this()
        {
            BaseValue = m_baseValue;

        }
        #endregion

        // ----------------------------------------------------------------------

        #region Add Modifiers
        public virtual void AddModifier(StatModifier m_newMod)
        {
            changedRecently = true;
            statModifiers.Add(m_newMod);
            statModifiers.Sort(CompareModifierOrder);
        }
        protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order)
                return -1;
            else if (a.Order > b.Order)
                return 1;
            return 0;
        }
        #endregion

        // ----------------------------------------------------------------------

        #region Manipulate Modifiers
        public virtual bool RemoveModifier(StatModifier m_modToRemove)
        {
            if (statModifiers.Remove(m_modToRemove))
            {
                changedRecently = true;
                return true;
            }
            return false;
        }

        public virtual bool RemoveAllModifiersFromSource(object m_source)
        {
            bool finishedRemoving = false;

            for (int i = statModifiers.Count - 1; i >= 0; i--)
            {
                if (statModifiers[i].Source == m_source)
                {
                    changedRecently = true;
                    finishedRemoving = true;
                    statModifiers.RemoveAt(i);
                }
            }

            return finishedRemoving;
        }
        #endregion

        // ----------------------------------------------------------------------

        #region Calculate Final Value
        protected virtual float CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < statModifiers.Count; i++)
            {
                if (statModifiers[i].StatType == StatModType.Flat)
                    finalValue += statModifiers[i].Value;
                else if (statModifiers[i].StatType == StatModType.PercentAdd)
                {
                    sumPercentAdd += statModifiers[i].Value;

                    if (i + 1 >= statModifiers.Count || statModifiers[i + 1].StatType != StatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                }
                else if (statModifiers[i].StatType == StatModType.PercentMulti)
                    finalValue *= 1 + statModifiers[i].Value;

            }

            return (float)Math.Round(finalValue, 4);
        }
        #endregion
    }
}
