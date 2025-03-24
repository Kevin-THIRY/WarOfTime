using UnityEngine;
using System.Collections.Generic;

public enum BotDifficulty{
    Easy,
    Medium,
    Hard
}

public class PlayerOption
{
    public string Name { get; set;}
    public Color Color { get; set;}
}

public class BotOption
{
    public BotDifficulty botDifficulty{ get; set;}
}

public class GameData
{
    public static List<PlayerOption> playerList = new List<PlayerOption>();
    public static List<BotOption> botList = new List<BotOption>();
}