using UnityEngine;
using System.Collections.Generic;

public enum BotDifficulty{
    Easy,
    Medium,
    Hard
}

public class PlayerInfos
{
    public string Name { get; set; }
    public Color Color { get; set; }
    public int Team { get; set; }
    public bool isBot { get; set; }
    public int id { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not PlayerInfos other) return false;
        return Name == other.Name &&
            Color.Equals(other.Color) &&
            Team == other.Team &&
            isBot == other.isBot &&
            id == other.id;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
            hash = hash * 23 + Color.GetHashCode();
            hash = hash * 23 + Team.GetHashCode();
            hash = hash * 23 + isBot.GetHashCode();
            hash = hash * 23 + id.GetHashCode();
            return hash;
        }
    }
}

public class GameData
{
    public static PlayerInfos playerInfos = new PlayerInfos();
}