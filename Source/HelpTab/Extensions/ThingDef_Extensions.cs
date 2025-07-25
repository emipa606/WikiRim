﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HelpTab;

public static class ThingDef_Extensions
{
    // Dummy for functions needing a ref list
    private static List<Def> nullDefs;

    public static void RecacheRecipes(this ThingDef thingDef, Map map, bool validateBills)
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

    public static bool IsFoodMachine(this ThingDef thingDef)
    {
        return typeof(Building_NutrientPasteDispenser).IsAssignableFrom(thingDef.thingClass);
    }

    private static bool IsIngestible(this ThingDef thingDef)
    {
        return thingDef.ingestible != null;
    }

    public static bool IsDrug(this ThingDef thingDef)
    {
        return thingDef.IsIngestible() &&
               (thingDef.ingestible.drugCategory == DrugCategory.Hard ||
                thingDef.ingestible.drugCategory == DrugCategory.Social);
    }

    public static bool IsImplant(this ThingDef thingDef)
    {
        // Return true if a recipe exist implanting this thing def
        return
            DefDatabase<RecipeDef>.AllDefsListForReading.Exists(r =>
                r.addsHediff != null &&
                r.IsIngredient(thingDef)
            );
    }

    public static RecipeDef GetImplantRecipeDef(this ThingDef thingDef)
    {
        // Get recipe for implant
        return
            DefDatabase<RecipeDef>.AllDefsListForReading.Find(r =>
                r.addsHediff != null &&
                r.IsIngredient(thingDef)
            );
    }

    public static HediffDef GetImplantHediffDef(this ThingDef thingDef)
    {
        // Get hediff for implant
        var recipeDef = thingDef.GetImplantRecipeDef();
        return recipeDef?.addsHediff;
    }

    public static bool EverHasRecipes(this ThingDef thingDef)
    {
        return
            !thingDef.GetRecipesCurrent().NullOrEmpty() ||
            !thingDef.GetRecipesUnlocked(ref nullDefs).NullOrEmpty()
            ;
    }

    public static bool EverHasRecipe(this ThingDef thingDef, RecipeDef recipeDef)
    {
        return
            thingDef.GetRecipesCurrent().Contains(recipeDef) ||
            thingDef.GetRecipesUnlocked(ref nullDefs).Contains(recipeDef)
            ;
    }

    public static List<JoyGiverDef> GetJoyGiverDefsUsing(this ThingDef thingDef)
    {
        var joyGiverDefs = DefDatabase<JoyGiverDef>.AllDefsListForReading.Where(def =>
            !def.thingDefs.NullOrEmpty() &&
            def.thingDefs.Contains(thingDef)
        ).ToList();
        return joyGiverDefs;
    }


    public static List<RecipeDef> GetRecipesUnlocked(this ThingDef thingDef, ref List<Def> researchDefs)
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

    private static List<RecipeDef> GetRecipesCurrent(this ThingDef thingDef)
    {
        return thingDef.AllRecipes;
    }

    public static List<RecipeDef> GetRecipesAll(this ThingDef thingDef)
    {
        // Things it is locked on with research
        var recipeDefs = new List<RecipeDef>();

        recipeDefs.AddRangeUnique(thingDef.GetRecipesCurrent());
        recipeDefs.AddRangeUnique(thingDef.GetRecipesUnlocked(ref nullDefs));

        return recipeDefs;
    }
}