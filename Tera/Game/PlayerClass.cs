using System;

namespace Tera.Game
{
    [Flags]
    public enum PlayerClass
    {
        Warrior = 1,
        Lancer = 2,
        Slayer = 3,
        Berserker = 4,
        Sorcerer = 5,
        Archer = 6,
        Priest = 7,
        Mystic = 8,
        Reaper = 9,
        Gunner = 10,
        Brawler = 11,
        Ninja = 12,

        Common = 255
    }

    public static class PlayerClassHelper
    {
        public static bool IsHeal(PlayerClass playerClass)
        {
            return playerClass == PlayerClass.Mystic || playerClass == PlayerClass.Priest;
        }
    }
}