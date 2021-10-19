﻿using System.Collections.Generic;

namespace SkillUpgrades
{
    public class SkillUpgradeSaveData
    {
        public Dictionary<string, bool> EnabledSkills { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();
    }
}
