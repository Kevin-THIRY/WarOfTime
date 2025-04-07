using UnityEngine;
using System.Collections.Generic;

public enum BotDifficulty{
    Easy,
    Medium,
    Hard
}

public class PlayerInfos
{
    public string Name { get; set;}
    public Color Color { get; set;}
    public int localPlayerIndex { get; set;}
}

public class BotOption
{
    public BotDifficulty botDifficulty{ get; set;}
}

public class GameData
{
    public static PlayerInfos playerInfos = new PlayerInfos();
    public static List<BotOption> botList = new List<BotOption>();
}