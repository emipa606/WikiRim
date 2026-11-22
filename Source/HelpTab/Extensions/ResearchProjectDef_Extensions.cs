using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HelpTab;

[StaticConstructorOnStartup]
public static class ResearchProjectDef_Extensions
{
    private static readonly Dictionary<ushort, List<Pair<Def, string>>> _unlocksCache;

    static ResearchProjectDef_Extensions()
    {
        _unlocksCache = new Dictionary<ushort, List<Pair<Def, string>>>();
    }

    extension(ResearchProjectDef research)
    {
        public List<ResearchProjectDef> ExclusiveDescendants()
        {
            var descendants = new List<ResearchProjectDef>();

            // recursively go through all children
            // populate initial queue
            var queue = new Queue<ResearchProjectDef>(
                DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(res =>
                    res.prerequisites.Contains(research)));

            // for each item in queue, determine if there's something unlocking it
            // if not, add to the list, and queue up children.
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (descendants.Contains(current))
                {
                    continue;
                }

                descendants.Add(current);
                foreach (var descendant in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(res =>
                             res.prerequisites.Contains(current)))
                {
                    queue.Enqueue(descendant);
                }
            }

            return descendants;
        }

        public List<ResearchProjectDef> GetPrerequisitesRecursive()
        {
            var result = new List<ResearchProjectDef>();
            if (research.prerequisites.NullOrEmpty())
            {
                return result;
            }

            var stack = new Stack<ResearchProjectDef>(research.prerequisites.Where(parent => parent != research));

            while (stack.Count > 0)
            {
                var parent = stack.Pop();
                result.Add(parent);

                if (parent.prerequisites.NullOrEmpty())
                {
                    continue;
                }

                foreach (var grandparent in parent.prerequisites.Where(grandparent => grandparent != parent))
                {
                    stack.Push(grandparent);
                }
            }

            return result.Distinct().ToList();
        }

        public List<Pair<Def, string>> GetUnlockDefsAndDescs()
        {
            if (_unlocksCache.TryGetValue(research.shortHash, out var descs))
            {
                return descs;
            }

            var unlocks = new List<Pair<Def, string>>();

            // dumps recipes/plants unlocked, because of the peculiar way CCL helpdefs are done.
            var dump = new List<ThingDef>();

            unlocks.AddRange(research.GetThingsUnlocked()
                .Where(d => d.IconTexture() != null)
                .Select(d => new Pair<Def, string>(d, "AllowsBuildingX".Translate(d.LabelCap))));
            unlocks.AddRange(research.GetTerrainUnlocked()
                .Where(d => d.IconTexture() != null)
                .Select(d => new Pair<Def, string>(d, "AllowsBuildingX".Translate(d.LabelCap))));
            unlocks.AddRange(research.GetRecipesUnlocked(ref dump)
                .Where(d => d.IconTexture() != null)
                .Select(d => new Pair<Def, string>(d, "AllowsCraftingX".Translate(d.LabelCap))));
            var sowTags = string.Join(" and ", research.GetSowTagsUnlocked(ref dump).ToArray());
            unlocks.AddRange(dump.Where(d => d.IconTexture() != null)
                .Select(d => new Pair<Def, string>(d, "AllowsSowingXinY".Translate(d.LabelCap, sowTags))));

            _unlocksCache.Add(research.shortHash, unlocks);
            return unlocks;
        }

        public List<ThingDef> GetThingsUnlocked()
        {
            // Buildings it unlocks
            var thingsOn = new List<ThingDef>();
            var researchThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(t =>
            {
                var r = t.GetResearchRequirements();
                return r != null && r.Contains(research);
            }).ToList();

            if (!researchThings.NullOrEmpty())
            {
                thingsOn.AddRangeUnique(researchThings);
            }

            return thingsOn;
        }

        public List<TerrainDef> GetTerrainUnlocked()
        {
            // Buildings it unlocks
            var thingsOn = new List<TerrainDef>();
            var researchThings = DefDatabase<TerrainDef>.AllDefsListForReading.Where(t =>
            {
                var r = t.GetResearchRequirements();
                return r != null && r.Contains(research);
            }).ToList();

            if (!researchThings.NullOrEmpty())
            {
                thingsOn.AddRangeUnique(researchThings);
            }

            return thingsOn;
        }

        public List<RecipeDef> GetRecipesUnlocked(ref List<ThingDef> thingDefs)
        {
            // Recipes on buildings it unlocks
            var recipes = new List<RecipeDef>();
            thingDefs?.Clear();

            // Add all recipes using this research projects
            var researchRecipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(d =>
                d.researchPrerequisite == research
            ).ToList();

            if (!researchRecipes.NullOrEmpty())
            {
                recipes.AddRangeUnique(researchRecipes);
            }

            if (thingDefs == null)
            {
                return recipes;
            }

            // Add buildings for those recipes
            foreach (var r in recipes)
            {
                if (!r.recipeUsers.NullOrEmpty())
                {
                    thingDefs.AddRangeUnique(r.recipeUsers);
                }

                var recipeThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(d =>
                    !d.recipes.NullOrEmpty() &&
                    d.recipes.Contains(r)
                ).ToList();
                if (!recipeThings.NullOrEmpty())
                {
                    thingDefs.AddRangeUnique(recipeThings);
                }
            }

            return recipes;
        }

        public List<string> GetSowTagsUnlocked(ref List<ThingDef> thingDefs)
        {
            var sowTags = new List<string>();
            thingDefs?.Clear();

            // Add all plants using this research project
            var researchPlants = DefDatabase<ThingDef>.AllDefsListForReading.Where(d =>
                d.plant != null &&
                !d.plant.sowResearchPrerequisites.NullOrEmpty() &&
                d.plant.sowResearchPrerequisites.Contains(research)
            ).ToList();

            if (researchPlants.NullOrEmpty())
            {
                return sowTags;
            }

            foreach (var plant in researchPlants)
            {
                sowTags.AddRangeUnique(plant.plant.sowTags);
            }

            thingDefs?.AddRangeUnique(researchPlants);

            return sowTags;
        }

        public List<Def> GetResearchRequirements()
        {
            var researchDefs = new List<Def>();

            if (research.prerequisites != null)
            {
                researchDefs.AddRangeUnique(research.prerequisites.ConvertAll<Def>(def => def));
            }

            // Return the list of research required
            return researchDefs;
        }

        public List<Def> GetResearchUnlocked()
        {
            var researchDefs = new List<Def>();
            researchDefs.AddRangeUnique(DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(rd =>
                !rd.prerequisites.NullOrEmpty() &&
                rd.prerequisites.Contains(research)
            ).ToList().ConvertAll<Def>(def => def));
            return researchDefs;
        }
    }
}