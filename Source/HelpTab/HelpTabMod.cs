using Verse;

namespace HelpTab;

public class HelpTabMod : Mod
{
    public static bool SearchMods;

    public HelpTabMod(ModContentPack mcp) : base(mcp)
    {
        LongEventHandler.QueueLongEvent(HelpBuilder.ResolveImpliedDefs, "BuildingHelpDatabase", false, null);
    }
}