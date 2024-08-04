namespace Caps.RPG.CombatEngine.Attributes
{
    public class AttributeSet
    {
        private Strength str;
        private Agility agi;
        private Constitution con;
        private Intellect ilt;
        private Arcana arc;
        private Wisdom wis;
        private Presence pre;
        private Charisma cha;

        public AttributeSet()
        {
            str = new Strength();
            agi = new Agility();
            con = new Constitution();
            ilt = new Intellect();
            arc = new Arcana();    
            wis = new Wisdom();
            pre = new Presence();
            cha = new Charisma();
        }
        public AttributeSet(int str, int agi, int con, int ilt, int arc, int wis, int pre, int cha)
        {
            this.str = new Strength(str);
            this.agi = new Agility(agi);
            this.con = new Constitution(con);
            this.ilt = new Intellect(ilt);
            this.arc = new Arcana(arc);
            this.wis = new Wisdom(wis);
            this.pre = new Presence(pre);
            this.cha = new Charisma(cha);
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
