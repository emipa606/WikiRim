using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HelpTab;

[StaticConstructorOnStartup]
public static class BuildableDef_Extensions
{
    extension(BuildableDef buildableDef)
    {
        public bool HasResearchRequirement()
        {
            // Can't entirely rely on this one check as it's state may change mid-game
            return buildableDef.researchPrerequisites != null &&
                   buildableDef.researchPrerequisites.Any(def => def != null);
            // Easiest check, do it first
            // Check for an advanced research unlock
        }

        public List<Def> GetResearchRequirements()
        {
            var researchDefs = new List<Def>();

            if (buildableDef.researchPrerequisites != null)
            {
                researchDefs.AddRangeUnique(buildableDef.researchPrerequisites.ConvertAll<Def>(def => def));
            }

            // Return the list of research required
            return researchDefs;
        }

        public List<RecipeDef> GetRecipeDefs()
        {
            return
                DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.products.Any(tc => tc.thingDef == buildableDef as ThingDef)).ToList();
        }
    }
}