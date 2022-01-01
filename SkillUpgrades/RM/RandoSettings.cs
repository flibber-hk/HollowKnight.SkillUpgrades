namespace SkillUpgrades.RM
{
    public class RandoSettings
    {
        public bool ExtraAirDash;
        public bool DirectionalDash;
        public bool WallClimb;
        public bool VerticalSuperdash;
        public bool TripleJump;
        public bool DownwardFireball;
        public bool HorizontalDive;
        public bool SpiralScream;

        [Newtonsoft.Json.JsonIgnore]
        public bool Any =>
            ExtraAirDash
            || DirectionalDash
            || WallClimb
            || VerticalSuperdash
            || TripleJump
            || DownwardFireball
            || HorizontalDive
            || SpiralScream;
    }
}
