using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HelpTab;

public static class ThingDef_Extensions
{
    // Dummy for functions needing a ref list
    private static List<Def> nullDefs;

    extension(ThingDef thingDef)
    {
        public void RecacheRecipes(Map map, bool validateBills)
        {
            thingDef.allRecipesCached = null;

            if (!validateBills || Current.ProgramState != ProgramState.MapInitializing)
            {
                return;
            }

            // Get the recached recipes
            var recipes = thingDef.AllRecipes;

            // Remove bill on any table of this def using invalid recipes
            var buildings = map.listerBuildings.AllBuildingsColonistOfDef(thingDef);
            foreach (var building in buildings)
            {
                if (building is not IBillGiver iBillGiver)
                {
                    continue;
                }

                for (var i = 0; i < iBillGiver.BillStack.Count; ++i)
                {
                    var bill = iBillGiver.BillStack[i];
                    if (recipes.Exists(r => bill.recipe == r))
                    {
                        continue;
                    }

                    Log.Message($"Removing {bill.recipe.defName} as it is invalid");
                    iBillGiver.BillStack.Delete(bill);
                }
            }
        }

        public bool IsFoodMachine()
        {
            return typeof(Building_NutrientPasteDispenser).IsAssignableFrom(thingDef.thingClass);
        }

        private bool IsIngestible()
        {
            return thingDef.ingestible != null;
        }

        public bool IsDrug()
        {
            return thingDef.IsIngestible() &&
                   (thingDef.ingestible.drugCategory == DrugCategory.Hard ||
                    thingDef.ingestible.drugCategory == DrugCategory.Social);
        }

        public bool IsImplant()
        {
            // Return true if a recipe exist implanting this thing def
            return
                DefDatabase<RecipeDef>.AllDefsListForReading.Exists(r =>
                    r.addsHediff != null &&
                    r.IsIngredient(thingDef)
                );
        }

        public RecipeDef GetImplantRecipeDef()
        {
            // Get recipe for implant
            return
                DefDatabase<RecipeDef>.AllDefsListForReading.Find(r =>
                    r.addsHediff != null &&
                    r.IsIngredient(thingDef)
                );
        }

        public HediffDef GetImplantHediffDef()
        {
            // Get hediff for implant
            var recipeDef = thingDef.GetImplantRecipeDef();
            return recipeDef?.addsHediff;
        }

        public bool EverHasRecipes()
        {
            return
                !thingDef.GetRecipesCurrent().NullOrEmpty() ||
                !thingDef.GetRecipesUnlocked(ref nullDefs).NullOrEmpty()
                ;
        }

        public bool EverHasRecipe(RecipeDef recipeDef)
        {
            return
                thingDef.GetRecipesCurrent().Contains(recipeDef) ||
                thingDef.GetRecipesUnlocked(ref nullDefs).Contains(recipeDef)
                ;
        }

        public List<JoyGiverDef> GetJoyGiverDefsUsing()
        {
            var joyGiverDefs = DefDatabase<JoyGiverDef>.AllDefsListForReading.Where(def =>
                !def.thingDefs.NullOrEmpty() &&
                def.thingDefs.Contains(thingDef)
            ).ToList();
            return joyGiverDefs;
        }

        public List<RecipeDef> GetRecipesUnlocked(ref List<Def> researchDefs)
        {
            // Recipes that are unlocked on thing with research
            var recipeDefs = new List<RecipeDef>();
            researchDefs?.Clear();

            // Look at recipes
            var recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(r =>
                r.researchPrerequisite != null &&
                (
                    r.recipeUsers != null &&
                    r.recipeUsers.Contains(thingDef) ||
                    thingDef.recipes != null &&
                    thingDef.recipes.Contains(r)
                )
            );

            recipeDefs.AddRangeUnique(recipes);

            return recipeDefs;
        }

        private List<RecipeDef> GetRecipesCurrent()
        {
            return thingDef.AllRecipes;
        }

        public List<RecipeDef> GetRecipesAll()
        {
            // Things it is locked on with research
            var recipeDefs = new List<RecipeDef>();

            recipeDefs.AddRangeUnique(thingDef.GetRecipesCurrent());
            recipeDefs.AddRangeUnique(thingDef.GetRecipesUnlocked(ref nullDefs));

            return recipeDefs;
        }
    }
}