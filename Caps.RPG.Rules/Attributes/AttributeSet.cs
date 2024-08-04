namespace Caps.RPG.CombatEngine.Attributes
{
    public class AttributeSet
    {
        private Strength str;
        private Agility agi;
        private Constitution con;
        private Intellect itl;
        private Arcana arc;
        private Wisdom wis;
        private Presence pre;
        private Charisma cha;

        public AttributeSet()
        {
            str = new Strength();
            agi = new Agility();
            con = new Constitution();
            itl = new Intellect();
            arc = new Arcana();    
            wis = new Wisdom();
            pre = new Presence();
            cha = new Charisma();
        }
        public AttributeSet(int str, int agi, int con, int itl, int arc, int wis, int pre, int cha)
        {
            this.str = new Strength(str);
            this.agi = new Agility(agi);
            this.con = new Constitution(con);
            this.itl = new Intellect(itl);
            this.arc = new Arcana(arc);
            this.wis = new Wisdom(wis);
            this.pre = new Presence(pre);
            this.cha = new Charisma(cha);
        }

        public int GetStatValue(Stat stat)
        {
            return stat switch
            {
                Strength => str.value,
                Agility => agi.value,
                Constitution => con.value,
                Intellect => itl.value,
                Arcana => arc.value,
                Wisdom => wis.value,
                Presence => pre.value,
                Charisma => cha.value,
                _ => throw new ArgumentException("Invalid Stat type")
            };
        }

        public int GetMaxHealth()
        {
            return con.GetMaxHealth();
        }

        public int InitiativeModifier()
        {
            return agi.InitiativeModifier();
        }

    }
}
