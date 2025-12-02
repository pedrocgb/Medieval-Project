namespace CharactersStats
{
    public class StatModifier
    {
        public readonly float Value;
        public readonly StatModType StatType;
        public readonly int Order;
        public readonly object Source;

        public StatModifier(float m_value, StatModType m_type, int m_order, object m_source)
        {
            Value = m_value;
            StatType = m_type;
            Order = m_order;
            Source = m_source;
        }

        public StatModifier(float m_value, StatModType m_type) : this(m_value, m_type, (int)m_type, null) { }

        public StatModifier(float m_value, StatModType m_type, int m_order) : this(m_value, m_type, m_order, null) { }

        public StatModifier(float m_value, StatModType m_type, object m_source) : this(m_value, m_type, (int)m_type, m_source) { }
    }

    public enum StatModType
    {
        Flat = 100,
        PercentAdd = 200,
        PercentMulti = 300
    }
}
