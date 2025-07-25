using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HelpTab;

[StaticConstructorOnStartup]
public static class BuildableDef_Extensions
{
    public static bool HasResearchRequirement(this BuildableDef buildableDef)
    {
        // Can't entirely rely on this one check as it's state may change mid-game
        return buildableDef.researchPrerequisites != null &&
               buildableDef.researchPrerequisites.Any(def => def != null);
        // Easiest check, do it first
        // Check for an advanced research unlock
    }

    public static List<Def> GetResearchRequirements(this BuildableDef buildableDef)
    {
        var researchDefs = new List<Def>();

        if (buildableDef.researchPrerequisites != null)
        {
            researchDefs.AddRangeUnique(buildableDef.researchPrerequisites.ConvertAll<Def>(def => def));
        }

        // Return the list of research required
        return researchDefs;
    }

    public static List<RecipeDef> GetRecipeDefs(this BuildableDef buildableDef)
    {
        return
            DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.products.Any(tc => tc.thingDef == buildableDef as ThingDef)).ToList();
    }
}