﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HelpTab;

public static class HelpBuilder
{
    //[Unsaved]

    private static readonly string HelpPostFix = "_HelpCategoryDef",

        // items
        ApparelHelp = $"Apparel{HelpPostFix}",
        BodyPartHelp = $"BodyPart{HelpPostFix}",
        DrugHelp = $"Drug{HelpPostFix}",
        MealHelp = $"Meal{HelpPostFix}",
        WeaponHelp = $"Weapon{HelpPostFix}",
        StuffHelp = $"Stuff{HelpPostFix}",

        // flora and fauna
        TerrainHelp = $"Terrain{HelpPostFix}",
        Plants = $"Plants{HelpPostFix}",
        Animals = $"Animals{HelpPostFix}",
        Humanoids = $"Humanoids{HelpPostFix}",
        Mechanoids = $"Mechanoids{HelpPostFix}",
        Biomes = $"Biomes{HelpPostFix}",

        // recipes and research
        RecipeHelp = $"Recipe{HelpPostFix}",
        ResearchHelp = $"Research{HelpPostFix}";

    public static void ResolveImpliedDefs()
    {
        // Items
        ResolveApparel();
        ResolveBodyParts();
        ResolveDrugs();
        ResolveMeals();
        ResolveWeapons();

        ResolveStuff();
        // TODO: Add workTypes
        // TODO: Add capacities
        // TODO: Add skills

        // The below are low priority  (as considered by Fluffy)
        // TODO: Add needs
        // TODO: Add building resources
        // TODO: Add factions
        // TODO: Add hediffs

        // The below are really low priority (as considered by Fluffy)
        // TODO: Add traders
        // TODO: Add tradertags

        // Buildings
        ResolveBuildings();
        ResolveMinifiableOnly();

        // Terrain
        ResolveTerrain();

        // flora and fauna
        ResolvePlants();
        ResolvePawnkinds();
        ResolveBiomes();

        // Recipes
        ResolveRecipes();

        // Research
        ResolveResearch();

        // Rebuild help caches
        ResolveReferences();
    }

    private static void ResolveReferences()
    {
        foreach (var helpCategory in DefDatabase<HelpCategoryDef>.AllDefsListForReading)
        {
            helpCategory.Recache();
        }

        MainTabWindow_ModHelp.Recache();
    }

    private static void ResolveApparel()
    {
        // Get list of things
        var thingDefs =
            DefDatabase<ThingDef>.AllDefsListForReading.Where(t =>
                t.thingClass == typeof(Apparel)
            ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(ApparelHelp, ResourceBank.String.AutoHelpSubCategoryApparel,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveBodyParts()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where typeof(ThingWithComps).IsAssignableFrom(thing.thingClass) && thing.IsImplant()
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(BodyPartHelp, ResourceBank.String.AutoHelpSubCategoryBodyParts,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveDrugs()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.IsIngestible && thing.IsDrug
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(DrugHelp, ResourceBank.String.AutoHelpSubCategoryDrugs,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveMeals()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.IsNutritionGivingIngestible
                  && !thing.IsDrug && thing.category != ThingCategory.Plant
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(MealHelp, ResourceBank.String.AutoHelpSubCategoryMeals,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveWeapons()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.IsWeapon
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(WeaponHelp, ResourceBank.String.AutoHelpSubCategoryWeapons,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveStuff()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.IsStuff
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(StuffHelp, ResourceBank.String.AutoHelpSubCategoryStuff,
            ResourceBank.String.AutoHelpCategoryItems);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveBuildings()
    {
        // Go through buildings by designation categories
        foreach (var designationCategoryDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading)
        {
            // Get list of things
            var thingDefs = (
                from thing in DefDatabase<ThingDef>.AllDefsListForReading
                where thing.designationCategory == designationCategoryDef
                select thing
            ).ToList();

            if (thingDefs.NullOrEmpty())
            {
                continue;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey($"{designationCategoryDef.defName}_Building{HelpPostFix}",
                designationCategoryDef.label, ResourceBank.String.AutoHelpCategoryBuildings);

            // Scan through all possible buildable defs and auto-generate help
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }
    }

    private static void ResolveMinifiableOnly()
    {
        // Get list of things
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.Minifiable && thing.designationCategory == null
            select thing
        ).ToList();

        if (thingDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey($"Special_Building{HelpPostFix}",
            ResourceBank.String.AutoHelpSubCategorySpecial, ResourceBank.String.AutoHelpCategoryBuildings);

        // Scan through all possible buildable defs and auto-generate help
        ResolveDefList(
            thingDefs,
            helpCategoryDef
        );
    }

    private static void ResolveTerrain()
    {
        // Get list of terrainDefs without designation category that occurs as a byproduct of mining (rocky),
        // or is listed in biomes (natural terrain). This excludes terrains that are not normally visible (e.g. Underwall).
        var rockySuffixes = new[] { "_Rough", "_Smooth", "_RoughHewn" };

        var terrainDefs =
            DefDatabase<TerrainDef>.AllDefsListForReading
                .Where(
                    // not buildable
                    t => t.designationCategory == null
                         && (
                             // is a type generated from rock
                             rockySuffixes.Any(s => t.defName.EndsWith(s))

                             // or is listed in any biome
                             || DefDatabase<BiomeDef>.AllDefsListForReading.Any(
                                 b => b.AllTerrainDefs().Contains(t))
                         ))
                .ToList();

        if (!terrainDefs.NullOrEmpty())
        {
            // Get help category
            var helpCategoryDef = HelpCategoryForKey(TerrainHelp, ResourceBank.String.AutoHelpSubCategoryTerrain,
                ResourceBank.String.AutoHelpCategoryTerrain);

            // resolve the defs
            ResolveDefList(terrainDefs, helpCategoryDef);
        }

        // Get list of buildable floors per designation category
        foreach (var categoryDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading)
        {
            terrainDefs =
                DefDatabase<TerrainDef>.AllDefsListForReading.Where(t => t.designationCategory == categoryDef)
                    .ToList();

            if (terrainDefs.NullOrEmpty())
            {
                continue;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey(categoryDef.defName + HelpPostFix, categoryDef.LabelCap,
                ResourceBank.String.AutoHelpCategoryTerrain);

            // resolve the defs
            ResolveDefList(terrainDefs, helpCategoryDef);
        }
    }

    private static void ResolvePlants()
    {
        // plants
        var plants = DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.plant != null).ToList();
        var category = HelpCategoryForKey(Plants, ResourceBank.String.AutoHelpSubCategoryPlants,
            ResourceBank.String.AutoHelpCategoryFloraAndFauna);

        ResolveDefList(plants, category);
    }

    private static void ResolvePawnkinds()
    {
        // animals
        var pawnkinds =
            DefDatabase<PawnKindDef>.AllDefsListForReading.Where(t => t.race.race.Animal).ToList();
        var category = HelpCategoryForKey(Animals, ResourceBank.String.AutoHelpSubCategoryAnimals,
            ResourceBank.String.AutoHelpCategoryFloraAndFauna);
        ResolveDefList(pawnkinds, category);

        // mechanoids
        pawnkinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(t => t.race.race.IsMechanoid).ToList();
        category = HelpCategoryForKey(Mechanoids, ResourceBank.String.AutoHelpSubCategoryMechanoids,
            ResourceBank.String.AutoHelpCategoryFloraAndFauna);
        ResolveDefList(pawnkinds, category);

        // humanoids - old version based on pawnkind, can get real messy with faction mods.
        //pawnkinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(t => !t.race.race.Animal && !t.race.race.IsMechanoid).ToList();
        //category = HelpCategoryForKey(Humanoids, ResourceBank.String.AutoHelpSubCategoryHumanoids,
        //                               ResourceBank.String.AutoHelpCategoryFloraAndFauna);
        //ResolveDefList(pawnkinds, category);

        // humanoids - new version based on race.
        var races = new List<ThingDef>();
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            bool? humanlike;
            if (def == null)
            {
                humanlike = null;
            }
            else
            {
                var race = def.race;
                humanlike = race != null ? new bool?(race.Humanlike) : null;
            }

            if ((humanlike ?? false) && !races.Contains(def))
            {
                races.Add(def);
            }
        }

        category = HelpCategoryForKey(Humanoids, ResourceBank.String.AutoHelpSubCategoryHumanoids,
            ResourceBank.String.AutoHelpCategoryFloraAndFauna);

        ResolveDefList(races, category);
    }

    private static void ResolveBiomes()
    {
        var biomes = DefDatabase<BiomeDef>.AllDefsListForReading;
        var category = HelpCategoryForKey(Biomes, ResourceBank.String.AutoHelpSubCategoryBiomes,
            ResourceBank.String.AutoHelpCategoryFloraAndFauna);
        ResolveDefList(biomes, category);
    }

    private static void ResolveRecipes()
    {
        // Get the thing database of things which ever have recipes
        var thingDefs = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.EverHasRecipes()
                  && !typeof(Corpse).IsAssignableFrom(thing.thingClass)
                  && thing.category != ThingCategory.Pawn
            select thing
        ).ToList();

        // Get help database
        var helpDefs = DefDatabase<HelpDef>.AllDefsListForReading;

        // Scan through defs and auto-generate help
        foreach (var thingDef in thingDefs)
        {
            var recipeDefs = thingDef.GetRecipesAll();
            if (recipeDefs.NullOrEmpty())
            {
                continue;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey($"{thingDef.defName}_{RecipeHelp}", thingDef.label,
                ResourceBank.String.AutoHelpCategoryRecipes);

            foreach (var recipeDef in recipeDefs)
            {
                // Find an existing entry
                var helpDef = helpDefs.Find(h =>
                    h.keyDef == recipeDef &&
                    h.secondaryKeyDef == thingDef
                );

                if (helpDef != null)
                {
                    continue;
                }

                // Make a new one
                //Log.Message( "Help System :: " + recipeDef.defName );
                helpDef = HelpForRecipe(thingDef, recipeDef, helpCategoryDef);

                // Inject the def
                if (helpDef != null)
                {
                    helpDefs.Add(helpDef);
                }
            }
        }
    }

    private static void ResolveResearch()
    {
        // Get research database
        var researchProjectDefs =
            DefDatabase<ResearchProjectDef>.AllDefsListForReading.ToList();

        if (researchProjectDefs.NullOrEmpty())
        {
            return;
        }

        // Get help category
        var helpCategoryDef = HelpCategoryForKey(ResearchHelp, ResourceBank.String.AutoHelpSubCategoryProjects,
            ResourceBank.String.AutoHelpCategoryResearch);

        // filter duplicates and create helpdefs
        ResolveDefList(researchProjectDefs, helpCategoryDef);
    }

    private static void ResolveDefList<T>(IEnumerable<T> defs, HelpCategoryDef category) where T : Def
    {
        // Get help database
        var processedDefs =
            new HashSet<Def>(DefDatabase<HelpDef>.AllDefsListForReading.Select(h => h.keyDef));

        // Scan through defs and auto-generate help
        foreach (var def in defs)
        {
            // Check if the def doesn't already have a help entry
            if (processedDefs.Contains(def))
            {
                continue;
            }

            // Make a new one
            HelpDef helpDef = null;
            try
            {
                helpDef = HelpForDef(def, category);
                if (helpDef?.keyDef?.modContentPack?.Name != null)
                {
                    var modName = helpDef.keyDef.modContentPack.Name;
                    if (helpDef.description == null)
                    {
                        helpDef.description = $"({modName})";
                    }
                    else
                    {
                        if (!helpDef.description.Contains(modName))
                        {
                            helpDef.description += $"\n({modName})";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning($"HelpTab :: Failed to build help for: {def}\n\t{e}");
            }

            // Inject the def
            if (helpDef != null)
            {
                DefDatabase<HelpDef>.Add(helpDef);
            }
        }
    }

    private static HelpCategoryDef HelpCategoryForKey(string key, string label, string modname)
    {
        // Get help category
        var helpCategoryDef = DefDatabase<HelpCategoryDef>.GetNamed(key, false);

        if (helpCategoryDef != null)
        {
            return helpCategoryDef;
        }

        // Create new designation help category
        helpCategoryDef = new HelpCategoryDef
        {
            defName = key,
            keyDef = key,
            label = label,
            ModName = modname
        };

        DefDatabase<HelpCategoryDef>.Add(helpCategoryDef);

        return helpCategoryDef;
    }

    private static HelpDef HelpForDef<T>(T def, HelpCategoryDef category) where T : Def
    {
        if (category.keyDef == Humanoids)
        {
            return HelpForHumanoid(def as ThingDef, category);
        }

        // both thingdefs (buildings, items) and terraindefs (floors) are derived from buildableDef
        if (def is BuildableDef buildableDef)
        {
            return HelpForBuildable(buildableDef, category);
        }

        if (def is ResearchProjectDef projectDef)
        {
            return HelpForResearch(projectDef, category);
        }

        if (def is PawnKindDef kindDef)
        {
            return HelpForPawnKind(kindDef, category);
        }

        if (def is RecipeDef)
        {
            return null;
        }

        if (def is BiomeDef biomeDef)
        {
            return HelpForBiome(biomeDef, category);
        }

        return null;
    }

    private static HelpDef HelpForBuildable(BuildableDef buildableDef, HelpCategoryDef category)
    {
        // we need the thingdef in several places
        var thingDef = buildableDef as ThingDef;

        // set up empty helpdef
        var helpDef = new HelpDef
        {
            defName = $"{buildableDef.defName}_BuildableDef_Help",
            keyDef = buildableDef,
            label = buildableDef.label,
            category = category,
            description = buildableDef.description
        };

        var statParts = new List<HelpDetailSection>();
        var linkParts = new List<HelpDetailSection>();

        if (!buildableDef.statBases.NullOrEmpty())
        {
            // Look at base stats
            var baseStats = new HelpDetailSection(
                null,
                buildableDef.statBases.Select(sb => sb.stat).ToList().ConvertAll(def => (Def)def),
                null,
                buildableDef.statBases.Select(sb =>
                    {
                        var numberSense = ToStringNumberSense.Absolute;
                        if (sb.stat.defName.Contains("Multiplier"))
                        {
                            numberSense = ToStringNumberSense.Factor;
                        }

                        return sb.stat.ValueToString(sb.value, numberSense);
                    })
                    .ToArray());

            statParts.Add(baseStats);
        }

        // Stuff stats
        if (thingDef?.IsStuff == true && thingDef.stuffProps.statFactors != null)
        {
            var statFactors = new HelpDetailSection(
                null,
                thingDef.stuffProps.statFactors.Select(sb => sb.stat).ToList().ConvertAll(def => (Def)def),
                null,
                thingDef.stuffProps.statFactors
                    .Select(sb => sb.stat.ValueToString(sb.value, ToStringNumberSense.Factor)).ToArray());

            statParts.Add(statFactors);
        }

        // Add list of required research
        var researchDefs = buildableDef.GetResearchRequirements();
        if (!researchDefs.NullOrEmpty())
        {
            var reqResearch = new HelpDetailSection(
                ResourceBank.String.AutoHelpListResearchRequired,
                researchDefs.ConvertAll(def => def));
            linkParts.Add(reqResearch);
        }

        // specific thingdef costs (terrainDefs are buildable with costlist, but do not have stuff cost (oddly)).
        if (!buildableDef.costList.NullOrEmpty())
        {
            var costs = new HelpDetailSection(
                ResourceBank.String.AutoHelpCost,
                buildableDef.costList.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                buildableDef.costList.Select(tc => tc.count.ToString()).ToArray());

            linkParts.Add(costs);
        }

        if (thingDef != null)
        {
            if (thingDef.equippedStatOffsets?.Any() == true)
            {
                var equippedOffsets = new HelpDetailSection(
                    ResourceBank.String.AutoHelpListStatOffsets,
                    thingDef.equippedStatOffsets.Select(so => so.stat).ToList().ConvertAll(def => (Def)def),
                    null,
                    thingDef.equippedStatOffsets
                        .Select(so => so.stat?.ValueToString(so.value, so.stat.toStringNumberSense))
                        .ToArray());

                statParts.Add(equippedOffsets);
            }

            if (thingDef.Verbs?.Any() == true)
            {
                foreach (var thingDefVerb in thingDef.Verbs)
                {
                    var listOfPropertyNames = new List<string>();
                    var listOfPropertyValues = new List<string>();

                    if (thingDefVerb.label != null)
                    {
                        listOfPropertyNames.Add("type".Translate());
                        listOfPropertyValues.Add(thingDefVerb.label.CapitalizeFirst());
                    }

                    if (thingDefVerb.range > 0)
                    {
                        listOfPropertyNames.Add("range".Translate());
                        listOfPropertyValues.Add(thingDefVerb.range.ToString("F1"));
                        if (thingDefVerb.minRange > 0)
                        {
                            listOfPropertyNames.Add("minRange".Translate());
                            listOfPropertyValues.Add(thingDefVerb.minRange.ToString("F1"));
                        }
                    }

                    if (thingDefVerb.ForcedMissRadius > 0)
                    {
                        listOfPropertyNames.Add("forcedMissRadius".Translate());
                        listOfPropertyValues.Add(thingDefVerb.ForcedMissRadius.ToString("F1"));
                    }

                    if (thingDefVerb.warmupTime > 0)
                    {
                        listOfPropertyNames.Add("warmupTime".Translate());
                        listOfPropertyValues.Add(thingDefVerb.warmupTime.ToString("F2"));
                    }

                    if (thingDefVerb.burstShotCount > 1)
                    {
                        listOfPropertyNames.Add("burstShotCount".Translate());
                        listOfPropertyValues.Add(thingDefVerb.burstShotCount.ToString());
                        if (thingDefVerb.ticksBetweenBurstShots > 0)
                        {
                            listOfPropertyNames.Add("ticksBetweenBurstShots".Translate());
                            listOfPropertyValues.Add(thingDefVerb.ticksBetweenBurstShots.ToString());
                        }
                    }

                    if (thingDefVerb.defaultProjectile != null)
                    {
                        listOfPropertyNames.Add("defaultProjectile".Translate());
                        listOfPropertyValues.Add(thingDefVerb.defaultProjectile.LabelCap);
                        if (thingDefVerb.defaultProjectile.projectile is { damageDef: not null } &&
                            thingDefVerb.defaultProjectile.projectile.GetDamageAmount(1f) > 0)
                        {
                            listOfPropertyNames.Add("projectileDamage".Translate());
                            listOfPropertyValues.Add(thingDefVerb.defaultProjectile.projectile?.GetDamageAmount(1f)
                                .ToString("F1"));
                        }

                        if (thingDefVerb.defaultProjectile.projectile?.StoppingPower > 0)
                        {
                            listOfPropertyNames.Add("projectileStoppingPower".Translate());
                            listOfPropertyValues.Add(
                                thingDefVerb.defaultProjectile.projectile?.StoppingPower.ToString("F1"));
                        }

                        if (thingDefVerb.defaultProjectile.projectile?.explosionRadius > 0)
                        {
                            listOfPropertyNames.Add("projectileExplosionRadius".Translate());
                            listOfPropertyValues.Add(
                                thingDefVerb.defaultProjectile.projectile?.explosionRadius.ToString("F1"));
                        }
                    }


                    statParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListVerbs,
                        listOfPropertyNames.ToArray(), null, listOfPropertyValues.ToArray()));
                }
            }

            if (thingDef.tools?.Any() == true)
            {
                foreach (var tool in thingDef.tools)
                {
                    var listOfPropertyNames = new List<string>();
                    var listOfPropertyValues = new List<string>();

                    if (tool.label != null)
                    {
                        listOfPropertyNames.Add("type".Translate());
                        listOfPropertyValues.Add(tool.LabelCap);
                    }

                    if (tool.power > 0)
                    {
                        listOfPropertyNames.Add("power".Translate());
                        listOfPropertyValues.Add(tool.power.ToString("F1"));
                    }

                    if (tool.cooldownTime > 0)
                    {
                        listOfPropertyNames.Add("cooldownTime".Translate());
                        listOfPropertyValues.Add(tool.cooldownTime.ToString("F2"));
                    }

                    if (tool.armorPenetration > 0)
                    {
                        listOfPropertyNames.Add("armorPenetration".Translate());
                        listOfPropertyValues.Add(tool.armorPenetration.ToString("F1"));
                    }

                    statParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListTools,
                        listOfPropertyNames.ToArray(), null, listOfPropertyValues.ToArray()));
                }
            }

            // What stuff can it be made from?
            if (
                thingDef.costStuffCount > 0 &&
                !thingDef.stuffCategories.NullOrEmpty()
            )
            {
                linkParts.Add(new HelpDetailSection(
                    "AutoHelpStuffCost".Translate(thingDef.costStuffCount.ToString()),
                    thingDef.stuffCategories.ToList().ConvertAll(def => (Def)def)));
            }

            var recipeDefs = buildableDef.GetRecipeDefs();
            if (!recipeDefs.NullOrEmpty())
            {
                var recipes = new HelpDetailSection(
                    ResourceBank.String.AutoHelpListRecipes,
                    recipeDefs.ConvertAll(def => (Def)def));
                linkParts.Add(recipes);

                // TODO: Figure out why this fails on a few select recipes (e.g. MVP's burger recipes and Apparello's Hive Armor), but works when called directly in these recipe's helpdefs.
                var tableDefs = recipeDefs.SelectMany(r => r.GetRecipeUsers())
                    .ToList()
                    .ConvertAll(def => def as Def);

                if (!tableDefs.NullOrEmpty())
                {
                    var tables = new HelpDetailSection(
                        ResourceBank.String.AutoHelpListRecipesOnThingsUnlocked, tableDefs);
                    linkParts.Add(tables);
                }
            }

            // Look at base stats
            if (thingDef.IsIngestible)
            {
                // only show Joy if it's non-zero
                var needDefs = new List<Def>
                {
                    NeedDefOf.Food
                };
                if (Math.Abs(thingDef.ingestible.joy) > 1e-3)
                {
                    needDefs.Add(DefDatabase<NeedDef>.GetNamedSilentFail("Joy"));
                }

                var suffixes = new List<string>
                {
                    thingDef.ingestible.CachedNutrition.ToString("0.###")
                };
                if (Math.Abs(thingDef.ingestible.joy) > 1e-3)
                {
                    suffixes.Add(thingDef.ingestible.joy.ToString("0.###"));
                }

                // show different label for plants to show we're talking about the actual plant, not the grown veggie/fruit/etc.
                var statLabel = ResourceBank.String.AutoHelpListNutrition;
                if (thingDef.plant != null)
                {
                    statLabel = ResourceBank.String.AutoHelpListNutritionPlant;
                }

                statParts.Add(
                    new HelpDetailSection(statLabel, needDefs, null, suffixes.ToArray()));
            }

            if (!thingDef.thingCategories.NullOrEmpty() &&
                thingDef.thingCategories.Contains(ThingCategoryDefOf.BodyParts) &&
                thingDef.IsImplant())
            {
                var hediffDef = thingDef.GetImplantHediffDef();

                if (hediffDef.addedPartProps != null)
                {
                    statParts.Add(new HelpDetailSection(ResourceBank.String.BodyPartEfficiency,
                        [hediffDef.addedPartProps.partEfficiency.ToString("P0")], null, null));
                }

                if (!hediffDef.stages.NullOrEmpty() &&
                    hediffDef.stages.Exists(stage =>
                        !stage.capMods.NullOrEmpty()
                    )
                   )
                {
                    var capacityMods = new HelpDetailSection(
                        ResourceBank.String.AutoHelpListCapacityModifiers,
                        hediffDef.stages.Where(s => !s.capMods.NullOrEmpty())
                            .SelectMany(s => s.capMods)
                            .Select(cm => cm.capacity)
                            .ToList()
                            .ConvertAll(def => (Def)def),
                        null,
                        hediffDef.stages
                            .Where(s => !s.capMods.NullOrEmpty())
                            .SelectMany(s => s.capMods)
                            .Select(
                                cm => (cm.offset > 0 ? "+" : "") + cm.offset.ToString("P0"))
                            .ToArray());

                    statParts.Add(capacityMods);
                }


                /*
                #region Components (Melee attack)

                if ((!hediffDef.comps.NullOrEmpty()) &&
                    (hediffDef.comps.Exists(p => (
                      (p.compClass == typeof(HediffComp_VerbGiver))
                  )))
                )
                {
                    foreach (var comp in hediffDef.comps)
                    {
                        if (comp.compClass == typeof(HediffComp_VerbGiver))
                        {
                            if (!comp.verbs.NullOrEmpty())
                            {
                                foreach (var verb in comp.verbs)
                                {
                                    if (verb.verbClass == typeof(Verb_MeleeAttack))
                                    {
                                        statParts.Add(new HelpDetailSection(
                                                "MeleeAttack".Translate(verb.meleeDamageDef.label),
                                                new[]
                                                {
                                                    ResourceBank.String.MeleeWarmupTime,
                                                    ResourceBank.String.StatsReport_MeleeDamage
                                                },
                                                null,
                                                new[]
                                                {
                                                    verb.defaultCooldownTicks.ToString(),
                                                    verb.meleeDamageBaseAmount.ToString()
                                                }
                                            ));
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion
                */
                var recipeDef = thingDef.GetImplantRecipeDef();
                if (!recipeDef.appliedOnFixedBodyParts.NullOrEmpty())
                {
                    linkParts.Add(new HelpDetailSection(
                        ResourceBank.String.AutoHelpSurgeryFixOrReplace,
                        recipeDef.appliedOnFixedBodyParts.ToList().ConvertAll(def => (Def)def)));
                }
            }

            // Get list of recipes
            recipeDefs = thingDef.AllRecipes;
            if (!recipeDefs.NullOrEmpty())
            {
                var recipes = new HelpDetailSection(
                    ResourceBank.String.AutoHelpListRecipes,
                    recipeDefs.ConvertAll(def => (Def)def));
                linkParts.Add(recipes);
            }

            // Build help for unlocked recipes associated with building
            recipeDefs = thingDef.GetRecipesUnlocked(ref researchDefs);
            if (
                !recipeDefs.NullOrEmpty() &&
                !researchDefs.NullOrEmpty()
            )
            {
                var unlockRecipes = new HelpDetailSection(
                    ResourceBank.String.AutoHelpListRecipesUnlocked,
                    recipeDefs.ConvertAll<Def>(def => def));
                var researchBy = new HelpDetailSection(
                    ResourceBank.String.AutoHelpListResearchBy,
                    researchDefs.ConvertAll(def => def));
                linkParts.Add(unlockRecipes);
                linkParts.Add(researchBy);
            }

            var powerSectionList = new List<StringDescTriplet>();

            // Get power required or generated
            var compPowerTrader = thingDef.GetCompProperties<CompProperties_Power>();
            if (compPowerTrader != null)
            {
                if (compPowerTrader.basePowerConsumption > 0)
                {
                    powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpRequired, null,
                        compPowerTrader.basePowerConsumption.ToString()));

                    /*
                    var compPowerIdle = thingDef.GetCompProperties<CompProperties_LowIdleDraw>();
                    if (compPowerIdle != null)
                    {
                        int idlePower;
                        if (compPowerIdle.idlePowerFactor < 1.0f)
                        {
                            idlePower = (int)(compPowerTrader.basePowerConsumption * compPowerIdle.idlePowerFactor);
                        }
                        else
                        {
                            idlePower = (int)compPowerIdle.idlePowerFactor;
                        }
                        powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpIdlePower, null, idlePower.ToString()));
                    }
                    */
                }
                else if (compPowerTrader.basePowerConsumption < 0)
                {
                    // A14 - check this!
                    if (thingDef.HasComp(typeof(CompPowerPlantWind)))
                    {
                        powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpGenerates, null,
                            "1700"));
                    }
                    else
                    {
                        var basePowerConsumption = (int)-compPowerTrader.basePowerConsumption;
                        powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpGenerates, null,
                            basePowerConsumption.ToString()));
                    }
                }
            }

            var compBattery = thingDef.GetCompProperties<CompProperties_Battery>();
            if (compBattery != null)
            {
                var stored = (int)compBattery.storedEnergyMax;
                var efficiency = (int)(compBattery.efficiency * 100f);
                powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpStores, null,
                    stored.ToString()));
                powerSectionList.Add(new StringDescTriplet(ResourceBank.String.AutoHelpEfficiency, null,
                    $"{efficiency}%"));
            }

            if (!powerSectionList.NullOrEmpty())
            {
                var powerSection = new HelpDetailSection(
                    ResourceBank.String.AutoHelpPower,
                    null,
                    powerSectionList);
                statParts.Add(powerSection);
            }

            // Get list of buildings effected by it
            var facilityProperties = thingDef.GetCompProperties<CompProperties_Facility>();
            if (facilityProperties != null)
            {
                var effectsBuildings = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(f =>
                    {
                        var compProps = f.GetCompProperties<CompProperties_AffectedByFacilities>();
                        return compProps is { linkableFacilities: not null } &&
                               compProps.linkableFacilities.Contains(f);
                    }).ToList();
                if (!effectsBuildings.NullOrEmpty())
                {
                    var facilityDefs = new List<DefStringTriplet>();
                    var facilityStrings = new List<StringDescTriplet>
                    {
                        new StringDescTriplet(ResourceBank.String.AutoHelpMaximumAffected, null,
                            facilityProperties.maxSimultaneous.ToString())
                    };

                    // Look at stats modifiers if there is any
                    if (!facilityProperties.statOffsets.NullOrEmpty())
                    {
                        foreach (var stat in facilityProperties.statOffsets)
                        {
                            facilityDefs.Add(new DefStringTriplet(stat.stat, null,
                                $": {stat.stat.ValueToString(stat.value, stat.stat.toStringNumberSense)}"));
                        }
                    }

                    var facilityDetailSection = new HelpDetailSection(
                        ResourceBank.String.AutoHelpFacilityStats,
                        facilityDefs, facilityStrings);

                    var facilitiesAffected = new HelpDetailSection(
                        ResourceBank.String.AutoHelpListFacilitiesAffected,
                        effectsBuildings.ConvertAll<Def>(def => def));

                    statParts.Add(facilityDetailSection);
                    linkParts.Add(facilitiesAffected);
                }
            }

            // Get valid joy givers
            var joyGiverDefs = thingDef.GetJoyGiverDefsUsing();

            if (!joyGiverDefs.NullOrEmpty())
            {
                foreach (var joyGiverDef in joyGiverDefs)
                {
                    // Get job driver stats
                    if (joyGiverDef.jobDef == null)
                    {
                        continue;
                    }

                    var defs = new List<DefStringTriplet>();
                    var strings = new List<StringDescTriplet>
                    {
                        new StringDescTriplet(joyGiverDef.jobDef.reportString),
                        new StringDescTriplet(joyGiverDef.jobDef.joyMaxParticipants.ToString(),
                            ResourceBank.String.AutoHelpMaximumParticipants)
                    };
                    defs.Add(new DefStringTriplet(joyGiverDef.jobDef.joyKind,
                        ResourceBank.String.AutoHelpJoyKind));
                    if (joyGiverDef.jobDef.joySkill != null)
                    {
                        defs.Add(new DefStringTriplet(joyGiverDef.jobDef.joySkill,
                            ResourceBank.String.AutoHelpJoySkill));
                    }

                    linkParts.Add(new HelpDetailSection(
                        ResourceBank.String.AutoHelpListJoyActivities,
                        defs, strings));
                }
            }
        }

        if (
            thingDef is { plant: not null }
        )
        {
            HelpPartsForPlant(thingDef, ref statParts, ref linkParts);
        }

        if (buildableDef is TerrainDef terrainDef)
        {
            HelpPartsForTerrain(terrainDef, ref statParts, ref linkParts);
        }

        helpDef.HelpDetailSections.AddRange(statParts);
        helpDef.HelpDetailSections.AddRange(linkParts);

        return helpDef;
    }

    private static HelpDef HelpForRecipe(ThingDef thingDef, RecipeDef recipeDef, HelpCategoryDef category)
    {
        var helpDef = new HelpDef
        {
            keyDef = recipeDef,
            secondaryKeyDef = thingDef
        };
        helpDef.defName = $"{helpDef.keyDef}_RecipeDef_Help";
        helpDef.label = recipeDef.label;
        helpDef.category = category;
        helpDef.description = recipeDef.description;

        helpDef.HelpDetailSections.Add(new HelpDetailSection(null,
            [recipeDef.WorkAmountTotal(null).ToStringWorkAmount()],
            [$"{ResourceBank.String.WorkAmount} : "],
            null));

        if (!recipeDef.skillRequirements.NullOrEmpty())
        {
            try
            {
                helpDef.HelpDetailSections.Add(new HelpDetailSection(
                    ResourceBank.String.MinimumSkills,
                    recipeDef.skillRequirements.Select(sr => sr.skill).ToList().ConvertAll(sd => (Def)sd),
                    null,
                    recipeDef.skillRequirements.Select(sr => sr.minLevel.ToString("####0")).ToArray()));
            }
            catch
            {
                Log.Warning(
                    $"[WikiRim]: Failed to read the skillRequirements when creating the help definition for recipe {recipeDef.defName}");
            }
        }

        // List of ingredients
        if (!recipeDef.ingredients.NullOrEmpty())
        {
            // TODO: find the actual thingDefs of ingredients, so we can use defs instead of strings.
            var ingredients = new HelpDetailSection(
                ResourceBank.String.Ingredients,
                recipeDef.ingredients
                    .Select(ic => recipeDef.IngredientValueGetter.BillRequirementsDescription(recipeDef, ic))
                    .ToArray(), null, null);

            helpDef.HelpDetailSections.Add(ingredients);
        }

        // List of products
        if (!recipeDef.products.NullOrEmpty())
        {
            var products = new HelpDetailSection(
                ResourceBank.String.AutoHelpListRecipeProducts,
                recipeDef.products.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                recipeDef.products.Select(tc => tc.count.ToString()).ToArray());

            helpDef.HelpDetailSections.Add(products);
        }

        // Add things it's on
        var thingDefs = recipeDef.GetRecipeUsers();
        if (!thingDefs.NullOrEmpty())
        {
            var billgivers = new HelpDetailSection(
                ResourceBank.String.AutoHelpListRecipesOnThings,
                thingDefs.ConvertAll<Def>(def => def));

            helpDef.HelpDetailSections.Add(billgivers);
        }

        // Add research required
        var researchDefs = recipeDef.GetResearchRequirements();
        if (!researchDefs.NullOrEmpty())
        {
            var requiredResearch = new HelpDetailSection(
                ResourceBank.String.AutoHelpListResearchRequired,
                researchDefs);

            helpDef.HelpDetailSections.Add(requiredResearch);
        }

        // What things is it on after research
        thingDefs = recipeDef.GetThingsUnlocked(ref researchDefs);
        if (thingDefs.NullOrEmpty())
        {
            return helpDef;
        }

        var recipesOnThingsUnlocked = new HelpDetailSection(
            ResourceBank.String.AutoHelpListRecipesOnThingsUnlocked,
            thingDefs.ConvertAll<Def>(def => def));

        helpDef.HelpDetailSections.Add(recipesOnThingsUnlocked);

        if (researchDefs.NullOrEmpty())
        {
            return helpDef;
        }

        var researchBy = new HelpDetailSection(
            ResourceBank.String.AutoHelpListResearchBy,
            researchDefs.ConvertAll(def => def));

        helpDef.HelpDetailSections.Add(researchBy);

        return helpDef;
    }

    private static HelpDef HelpForResearch(ResearchProjectDef researchProjectDef, HelpCategoryDef category)
    {
        var helpDef = new HelpDef
        {
            defName = $"{researchProjectDef.defName}_ResearchProjectDef_Help",
            keyDef = researchProjectDef,
            label = researchProjectDef.label,
            category = category,
            description = researchProjectDef.description
        };

        var totalCost = new HelpDetailSection(null,
            [researchProjectDef.baseCost.ToString()],
            [ResourceBank.String.AutoHelpTotalCost],
            null);
        helpDef.HelpDetailSections.Add(totalCost);

        // Add research required
        var researchDefs = researchProjectDef.GetResearchRequirements();
        if (!researchDefs.NullOrEmpty())
        {
            var researchRequirements = new HelpDetailSection(
                ResourceBank.String.AutoHelpListResearchRequired,
                researchDefs.ConvertAll(def => def));

            helpDef.HelpDetailSections.Add(researchRequirements);
        }

        // Add research unlocked
        //CCL_Log.Message(researchProjectDef.label, "getting unlocked research");
        researchDefs = researchProjectDef.GetResearchUnlocked();
        if (!researchDefs.NullOrEmpty())
        {
            var reseachUnlocked = new HelpDetailSection(
                ResourceBank.String.AutoHelpListResearchLeadsTo,
                researchDefs.ConvertAll(def => def));

            helpDef.HelpDetailSections.Add(reseachUnlocked);
        }

        // Add buildables unlocked (items, buildings and terrain)
        var buildableDefs = new List<Def>();

        // items and buildings
        buildableDefs.AddRange(researchProjectDef.GetThingsUnlocked().ConvertAll<Def>(def => def));

        // terrain
        buildableDefs.AddRange(researchProjectDef.GetTerrainUnlocked().ConvertAll<Def>(def => def));

        // create help section
        if (!buildableDefs.NullOrEmpty())
        {
            var thingsUnlocked = new HelpDetailSection(
                ResourceBank.String.AutoHelpListThingsUnlocked,
                buildableDefs);

            helpDef.HelpDetailSections.Add(thingsUnlocked);
        }

        // filter down to thingdefs for recipes etc.
        var thingDefs =
            buildableDefs.Where(def => def is ThingDef)
                .ToList()
                .ConvertAll(def => (ThingDef)def);

        // Add recipes it unlocks
        var recipeDefs = researchProjectDef.GetRecipesUnlocked(ref thingDefs);
        if (
            !recipeDefs.NullOrEmpty() &&
            !thingDefs.NullOrEmpty()
        )
        {
            var recipesUnlocked = new HelpDetailSection(
                ResourceBank.String.AutoHelpListRecipesUnlocked,
                recipeDefs.ConvertAll<Def>(def => def));

            helpDef.HelpDetailSections.Add(recipesUnlocked);

            var recipesOnThingsUnlocked = new HelpDetailSection(
                ResourceBank.String.AutoHelpListRecipesOnThingsUnlocked,
                thingDefs.ConvertAll<Def>(def => def));

            helpDef.HelpDetailSections.Add(recipesOnThingsUnlocked);
        }

        // Look in advanced research to add plants and sow tags it unlocks
        var sowTags = researchProjectDef.GetSowTagsUnlocked(ref thingDefs);
        if (sowTags.NullOrEmpty() || thingDefs.NullOrEmpty())
        {
            return helpDef;
        }

        var plantsUnlocked = new HelpDetailSection(
            ResourceBank.String.AutoHelpListPlantsUnlocked,
            thingDefs.ConvertAll<Def>(def => def));

        helpDef.HelpDetailSections.Add(plantsUnlocked);

        var plantsIn = new HelpDetailSection(
            ResourceBank.String.AutoHelpListPlantsIn,
            sowTags.ToArray(), null, null);

        helpDef.HelpDetailSections.Add(plantsIn);

        return helpDef;
    }

    private static HelpDef HelpForBiome(BiomeDef biomeDef, HelpCategoryDef category)
    {
        var helpDef = new HelpDef
        {
            keyDef = biomeDef
        };
        helpDef.defName = $"{helpDef.keyDef}_RecipeDef_Help";
        helpDef.label = biomeDef.label;
        helpDef.category = category;
        helpDef.description = biomeDef.description;

        // we can't get to these stats. They seem to be hardcoded in RimWorld.Planet.WorldGenerator_Grid.BiomeFrom()
        // hacky solution would be to reverse-engineer them by taking a loaded world and 5th and 95th percentiles from worldsquares with this biome.
        // however, that requires a world to be loaded.

        var diseases = (
            from incident in DefDatabase<IncidentDef>.AllDefsListForReading
            where incident.diseaseBiomeRecords != null
            from record in incident.diseaseBiomeRecords
            where record.biome == biomeDef && record.commonality > 0
            select incident
        ).ToList();

        if (diseases.Count > 0)
        {
            var defs = new List<Def>(diseases.Count);
            var chances = new List<string>(diseases.Count);

            foreach (var disease in diseases)
            {
                var diseaseCommonality = biomeDef.CommonalityOfDisease(disease) /
                                         (biomeDef.diseaseMtbDays * GenDate.DaysPerYear);

                chances.Add(diseaseCommonality.ToStringPercent());
                defs.Add(disease.diseaseIncident);
            }

            helpDef.HelpDetailSections.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListBiomeDiseases,
                defs, null, chances.ToArray()));
        }

        var terrains = biomeDef.AllTerrainDefs().ConvertAll(def => (Def)def);
        // commonalities unknown
        if (!terrains.NullOrEmpty())
        {
            helpDef.HelpDetailSections.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListBiomeTerrain,
                terrains));
        }

        var plants = (
            from thing in DefDatabase<ThingDef>.AllDefsListForReading
            where thing.plant is { wildBiomes: not null }
            from record in thing.plant.wildBiomes
            where record.biome == biomeDef && record.commonality > 0
            select thing as Def
        ).ToList();

        if (!plants.NullOrEmpty())
        {
            helpDef.HelpDetailSections.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListBiomePlants,
                plants));
        }

        var animals = (
            from pawnKind in DefDatabase<PawnKindDef>.AllDefs
            where pawnKind.RaceProps is { wildBiomes: not null }
            from record in pawnKind.RaceProps.wildBiomes
            where record.biome == biomeDef && record.commonality > 0
            select pawnKind as Def
        ).ToList();

        if (!animals.NullOrEmpty())
        {
            helpDef.HelpDetailSections.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListBiomeAnimals,
                animals));
        }

        return helpDef;
    }

    private static HelpDef HelpForPawnKind(PawnKindDef kindDef, HelpCategoryDef category)
    {
        // we need the thingdef in several places
        var raceDef = kindDef.race;

        // set up empty helpdef
        var helpDef = new HelpDef
        {
            defName = $"{kindDef.defName}_PawnKindDef_Help",
            keyDef = kindDef,
            label = kindDef.label,
            category = category,
            description = raceDef.description
        };

        var statParts = new List<HelpDetailSection>();
        var linkParts = new List<HelpDetailSection>();

        if (!raceDef.statBases.NullOrEmpty())
        {
            // Look at base stats
            var baseStats = new HelpDetailSection(
                null,
                raceDef.statBases.Select(sb => sb.stat).ToList().ConvertAll(def => (Def)def),
                null,
                raceDef.statBases.Select(sb => sb.stat.ValueToString(sb.value, sb.stat.toStringNumberSense))
                    .ToArray());

            statParts.Add(baseStats);
        }

        HelpPartsForAnimal(kindDef, ref statParts, ref linkParts);

        helpDef.HelpDetailSections.AddRange(statParts);
        helpDef.HelpDetailSections.AddRange(linkParts);

        return helpDef;
    }

    private static HelpDef HelpForHumanoid(ThingDef def, HelpCategoryDef category)
    {
        // set up empty helpdef
        var helpDef = new HelpDef
        {
            defName = $"{def.defName}_PawnKindDef_Help",
            keyDef = def,
            label = def.label,
            category = category,
            description = def.description
        };

        var statParts = new List<HelpDetailSection>();
        var linkParts = new List<HelpDetailSection>();

        if (!def.statBases.NullOrEmpty())
        {
            // Look at base stats
            var baseStats = new HelpDetailSection(
                null,
                def.statBases.Select(sb => sb.stat).ToList().ConvertAll(thing => (Def)thing),
                null,
                def.statBases.Select(sb => sb.stat.ValueToString(sb.value, sb.stat.toStringNumberSense))
                    .ToArray());

            statParts.Add(baseStats);
        }

        HelpPartsForHumanoid(def, ref statParts, ref linkParts);

        helpDef.HelpDetailSections.AddRange(statParts);
        helpDef.HelpDetailSections.AddRange(linkParts);

        return helpDef;
    }

    private static void HelpPartsForTerrain(TerrainDef terrainDef, ref List<HelpDetailSection> statParts,
        ref List<HelpDetailSection> linkParts)
    {
        statParts.Add(new HelpDetailSection(null,
            [
                terrainDef.fertility.ToStringPercent(),
                terrainDef.pathCost.ToString()
            ],
            [
                $"{ResourceBank.String.AutoHelpListFertility}:",
                $"{ResourceBank.String.AutoHelpListPathCost}:"
            ],
            null));

        // wild biome tags
        var biomes = DefDatabase<BiomeDef>.AllDefsListForReading
            .Where(b => b.AllTerrainDefs().Contains(terrainDef))
            .ToList();
        if (!biomes.NullOrEmpty())
        {
            linkParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListAppearsInBiomes,
                biomes.Select(r => r as Def).ToList()));
        }
    }

    private static void HelpPartsForPlant(ThingDef thingDef, ref List<HelpDetailSection> statParts,
        ref List<HelpDetailSection> linkParts)
    {
        var plant = thingDef.plant;

        // non-def stat part
        statParts.Add(new HelpDetailSection(null,
            [
                plant.growDays.ToString(),
                plant.fertilityMin.ToStringPercent(),
                $"{plant.growMinGlow.ToStringPercent()} - {plant.growOptimalGlow.ToStringPercent()}"
            ],
            [
                ResourceBank.String.AutoHelpGrowDays,
                ResourceBank.String.AutoHelpMinFertility,
                ResourceBank.String.AutoHelpLightRange
            ],
            null));

        if (plant.Harvestable)
        {
            // yield
            linkParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListPlantYield,
                [..new[] { plant.harvestedThingDef }],
                [plant.harvestYield.ToString()]
            ));
        }

        // sowtags
        if (plant.Sowable)
        {
            linkParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListCanBePlantedIn,
                plant.sowTags.ToArray(), null, null));
        }

        // biomes
        if (plant.wildBiomes.NullOrEmpty())
        {
            return;
        }

        var biomes = (
            from record in plant.wildBiomes
            where record.commonality > 0
            select record.biome as Def
        ).ToList();

        linkParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListAppearsInBiomes, biomes));
    }

    private static void HelpPartsForAnimal(PawnKindDef kindDef, ref List<HelpDetailSection> statParts,
        ref List<HelpDetailSection> linkParts)
    {
        var race = kindDef.race.race;

        var maxSize = race.lifeStageAges.Select(lsa => lsa.def.bodySizeFactor * race.baseBodySize).Max();

        // set up vars
        var defs = new List<Def>();
        var stringDescs = new List<string>();
        var prefixes = new List<string>();
        var suffixes = new List<string>();

        statParts.Add(new HelpDetailSection(null,
            [
                (race.baseHealthScale * race.lifeStageAges.Last().def.healthScaleFactor).ToStringPercent(),
                race.lifeExpectancy.ToStringApproxAge(),
                race.ResolvedDietCategory.ToStringHuman(),
                race.trainability?.ToString() ?? "Sentient"
            ],
            [
                ResourceBank.String.AutoHelpHealthScale,
                ResourceBank.String.AutoHelpLifeExpectancy,
                ResourceBank.String.AutoHelpDiet,
                ResourceBank.String.AutoHelpIntelligence
            ],
            null));

        if (race.Animal)
        {
            var DST = new List<DefStringTriplet>();
            foreach (var def in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                // skip if explicitly disallowed
                if (!race.untrainableTags.NullOrEmpty() &&
                    race.untrainableTags.Any(tag => def.MatchesTag(tag)))
                {
                    continue;
                }

                // explicitly allowed tags.
                if (!race.trainableTags.NullOrEmpty() &&
                    race.trainableTags.Any(tag => def.MatchesTag(tag)) &&
                    maxSize >= def.minBodySize)
                {
                    DST.Add(new DefStringTriplet(def));
                    continue;
                }

                // A17 TODO: Check TrainableIntelligance
                // normal proceedings
                if (maxSize >= def.minBodySize
                    //   && race.TrainableIntelligence >= def.requiredTrainableIntelligence
                    && def.defaultTrainable)
                {
                    DST.Add(new DefStringTriplet(def));
                }
            }

            if (DST.Count > 0)
            {
                linkParts.Add(new HelpDetailSection(
                    ResourceBank.String.AutoHelpListTrainable,
                    DST, null));
            }

            defs.Clear();
        }

        var ages = race.lifeStageAges.Select(age => age.minAge).ToList();
        for (var i = 0; i < race.lifeStageAges.Count; i++)
        {
            defs.Add(race.lifeStageAges[i].def);
            // final lifestage
            suffixes.Add(i == race.lifeStageAges.Count - 1
                ? $"{ages[i].ToStringApproxAge()} - ~{race.lifeExpectancy.ToStringApproxAge()}"
                // other lifestages
                : $"{ages[i].ToStringApproxAge()} - {ages[i + 1].ToStringApproxAge()}");
        }

        // only print if interesting (i.e. more than one lifestage).
        if (defs.Count > 1)
        {
            statParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListLifestages,
                defs,
                null,
                suffixes.ToArray()));
        }

        defs.Clear();
        suffixes.Clear();

        var eggComp = kindDef.race.GetCompProperties<CompProperties_EggLayer>();
        if (eggComp != null)
        {
            // egglayers
            var range = eggComp.eggCountRange.min == eggComp.eggCountRange.max
                ? eggComp.eggCountRange.min.ToString()
                : eggComp.eggCountRange.ToString();

            stringDescs.Add("AutoHelpEggLayer".Translate(range,
                (eggComp.eggLayIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear).ToStringApproxAge()));

            statParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListReproduction,
                stringDescs.ToArray(), null, null));
            stringDescs.Clear();
        }
        else if (race.hasGenders && race.gestationPeriodDays > 0 &&
                 race.lifeStageAges.Any(lsa => lsa.def.reproductive))
        {
            // mammals
            var SDT = new List<StringDescTriplet>
            {
                new StringDescTriplet(
                    (race.gestationPeriodDays * GenDate.TicksPerDay / GenDate.TicksPerYear).ToStringApproxAge(),
                    ResourceBank.String.AutoHelpGestationPeriod)
            };

            if (race.litterSizeCurve is { PointsCount: >= 3 })
            {
                // if size is three, there is actually only one option (weird boundary restrictions by Tynan require a +/- .5 min/max)
                if (race.litterSizeCurve.PointsCount == 3)
                {
                    SDT.Add(new StringDescTriplet(race.litterSizeCurve[1].x.ToString(),
                        ResourceBank.String.AutoHelpLitterSize));
                }

                // for the same reason, if more than one choice, indeces are second and second to last.
                else
                {
                    SDT.Add(new StringDescTriplet(
                        $"{race.litterSizeCurve[1].x} - {race.litterSizeCurve[race.litterSizeCurve.PointsCount - 2].x}",
                        ResourceBank.String.AutoHelpLitterSize));
                    stringDescs.Add(ResourceBank.String.AutoHelpLitterSize);
                }
            }
            else
            {
                // if litterSize is not defined in XML, it's always 1
                SDT.Add(new StringDescTriplet("1", ResourceBank.String.AutoHelpLitterSize));
            }

            statParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListReproduction, null, SDT));
        }

        if (race.Animal)
        {
            var kinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(t => t.race == kindDef.race);
            foreach (var kind in kinds)
            {
                foreach (var biome in DefDatabase<BiomeDef>.AllDefsListForReading)
                {
                    if (biome.AllWildAnimals.Contains(kind))
                    {
                        defs.Add(biome);
                    }
                }
            }

            defs = defs.Distinct().ToList();

            if (!defs.NullOrEmpty())
            {
                linkParts.Add(new HelpDetailSection(
                    ResourceBank.String.AutoHelpListAppearsInBiomes,
                    defs));
            }

            defs.Clear();
        }

        if (race.IsFlesh)
        {
            // fleshy pawns ( meat + leather )
            defs.Add(race.meatDef);

            prefixes.Add($"~{maxSize * StatDefOf.MeatAmount.defaultBaseValue}");

            if (race.leatherDef != null)
            {
                defs.Add(race.leatherDef);

                prefixes.Add(
                    $"~{maxSize * kindDef.race.statBases.Find(sb => sb.stat == StatDefOf.LeatherAmount)?.value}");
            }

            statParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListButcher,
                defs,
                prefixes.ToArray()));
        }
        else if (
            race.IsMechanoid &&
            !kindDef.race.butcherProducts.NullOrEmpty()
        )
        {
            // metallic pawns ( mechanoids )
            linkParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListDisassemble,
                kindDef.race.butcherProducts.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                kindDef.race.butcherProducts.Select(tc => tc.count.ToString()).ToArray()));
        }

        defs.Clear();
        prefixes.Clear();

        // Need to handle subclasses (such as CompMilkableRenameable)
        if (kindDef.race.HasComp(typeof(CompMilkable)))
        {
            if (kindDef.race.comps.Find(c =>
                    c.compClass == typeof(CompMilkable) || c.compClass.IsSubclassOf(typeof(CompMilkable))) is
                CompProperties_Milkable milkComp)
            {
                defs.Add(milkComp.milkDef);
                prefixes.Add(milkComp.milkAmount.ToString());
                suffixes.Add("AutoHelpEveryX".Translate(
                    ((float)milkComp.milkIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear)
                    .ToStringApproxAge()));

                linkParts.Add(new HelpDetailSection(
                    ResourceBank.String.AutoHelpListMilk,
                    defs,
                    prefixes.ToArray(),
                    suffixes.ToArray()));
            }

            defs.Clear();
            prefixes.Clear();
            suffixes.Clear();
        }

        // Need to handle subclasses (such as CompShearableRenameable)
        if (!kindDef.race.HasComp(typeof(CompShearable)))
        {
            return;
        }

        if (kindDef.race.comps.Find(c =>
                c.compClass == typeof(CompShearable) || c.compClass.IsSubclassOf(typeof(CompShearable))) is
            CompProperties_Shearable shearComp)
        {
            defs.Add(shearComp.woolDef);
            prefixes.Add(shearComp.woolAmount.ToString());
            suffixes.Add("AutoHelpEveryX".Translate(
                ((float)shearComp.shearIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear)
                .ToStringApproxAge()));

            linkParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListShear,
                defs,
                prefixes.ToArray(),
                suffixes.ToArray()));
        }

        defs.Clear();
        prefixes.Clear();
        suffixes.Clear();
    }

    private static void HelpPartsForHumanoid(ThingDef raceDef, ref List<HelpDetailSection> statParts,
        ref List<HelpDetailSection> linkParts)
    {
        var race = raceDef.race;
        var maxSize = race.lifeStageAges.Select(lsa => lsa.def.bodySizeFactor * race.baseBodySize).Max();

        // set up vars
        var defs = new List<Def>();
        var stringDescs = new List<string>();
        var prefixes = new List<string>();
        var suffixes = new List<string>();


        statParts.Add(new HelpDetailSection(null,
            [
                (race.baseHealthScale * race.lifeStageAges.Last().def.healthScaleFactor).ToStringPercent(),
                race.lifeExpectancy.ToStringApproxAge(),
                race.ResolvedDietCategory.ToStringHuman(),
                "Sentient"
            ],
            [
                ResourceBank.String.AutoHelpHealthScale,
                ResourceBank.String.AutoHelpLifeExpectancy,
                ResourceBank.String.AutoHelpDiet,
                ResourceBank.String.AutoHelpIntelligence
            ],
            null));

        var ages = race.lifeStageAges.Select(age => age.minAge).ToList();
        for (var i = 0; i < race.lifeStageAges.Count; i++)
        {
            defs.Add(race.lifeStageAges[i].def);
            // final lifestage
            suffixes.Add(i == race.lifeStageAges.Count - 1
                ? $"{ages[i].ToStringApproxAge()} - ~{race.lifeExpectancy.ToStringApproxAge()}"
                // other lifestages
                : $"{ages[i].ToStringApproxAge()} - {ages[i + 1].ToStringApproxAge()}");
        }

        // only print if interesting (i.e. more than one lifestage).
        if (defs.Count > 1)
        {
            statParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListLifestages,
                defs,
                null,
                suffixes.ToArray()));
        }

        defs.Clear();
        suffixes.Clear();


        var eggComp = raceDef.GetCompProperties<CompProperties_EggLayer>();
        if (eggComp != null)
        {
            // egglayers
            var range = eggComp.eggCountRange.min == eggComp.eggCountRange.max
                ? eggComp.eggCountRange.min.ToString()
                : eggComp.eggCountRange.ToString();

            stringDescs.Add("AutoHelpEggLayer".Translate(range,
                (eggComp.eggLayIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear).ToStringApproxAge()));

            statParts.Add(new HelpDetailSection(
                ResourceBank.String.AutoHelpListReproduction,
                stringDescs.ToArray(), null, null));
            stringDescs.Clear();
        }
        else if (race.hasGenders && race.lifeStageAges.Any(lsa => lsa.def.reproductive))
        {
            // mammals
            var SDT = new List<StringDescTriplet>
            {
                new StringDescTriplet(
                    (race.gestationPeriodDays * GenDate.TicksPerDay / GenDate.TicksPerYear).ToStringApproxAge(),
                    ResourceBank.String.AutoHelpGestationPeriod)
            };

            if (race.litterSizeCurve is { PointsCount: >= 3 })
            {
                // if size is three, there is actually only one option (weird boundary restrictions by Tynan require a +/- .5 min/max)
                if (race.litterSizeCurve.PointsCount == 3)
                {
                    SDT.Add(new StringDescTriplet(race.litterSizeCurve[1].x.ToString(),
                        ResourceBank.String.AutoHelpLitterSize));
                }

                // for the same reason, if more than one choice, indeces are second and second to last.
                else
                {
                    SDT.Add(new StringDescTriplet(
                        $"{race.litterSizeCurve[1].x} - {race.litterSizeCurve[race.litterSizeCurve.PointsCount - 2].x}",
                        ResourceBank.String.AutoHelpLitterSize));
                    stringDescs.Add(ResourceBank.String.AutoHelpLitterSize);
                }
            }
            else
            {
                // if litterSize is not defined in XML, it's always 1
                SDT.Add(new StringDescTriplet("1", ResourceBank.String.AutoHelpLitterSize));
            }

            statParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListReproduction, null, SDT));
        }


        if (race.IsMechanoid && !raceDef.butcherProducts.NullOrEmpty())
        {
            // metallic pawns ( mechanoids )
            linkParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListDisassemble,
                raceDef.butcherProducts.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                raceDef.butcherProducts.Select(tc => tc.count.ToString()).ToArray()));
        }
        else
        {
            // fleshy pawns ( meat + leather )
            defs.Add(race.meatDef);
            prefixes.Add($"~{maxSize * StatDefOf.MeatAmount.defaultBaseValue}");

            if (race.leatherDef != null)
            {
                //defs.Add(race.leatherDef);
                //prefixes.Add("~" + (maxSize * raceDef.statBases.Find(sb => sb.stat == StatDefOf.LeatherAmount).value));
                var statModifier = raceDef.statBases.Find(sb => sb.stat == StatDefOf.LeatherAmount);
                if (statModifier != null)
                {
                    defs.Add(race.leatherDef);
                    prefixes.Add($"~{maxSize * statModifier.value}");
                }
            }

            statParts.Add(new HelpDetailSection(ResourceBank.String.AutoHelpListButcher, defs, prefixes.ToArray()));
        }

        defs.Clear();
        prefixes.Clear();
    }
}