
using System.Collections.Generic;
public static class BotList
{
    public static List<BotOption> AllBots = new List<BotOption>();
}

public class BotOption : PlayerInfos
{
    public BotDifficulty botDifficulty { get; set; }
    public static List<Unit> BotUnits = new List<Unit>();
}