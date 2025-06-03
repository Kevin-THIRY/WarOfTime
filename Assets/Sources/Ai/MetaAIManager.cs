
using System.Collections.Generic;
public static class BotList
{
    public static List<BotOption> Bots = new List<BotOption>();
}

public class BotOption : PlayerInfos
{
    public BotDifficulty botDifficulty { get; set; }
    public List<Unit> BotUnits = new List<Unit>();
}