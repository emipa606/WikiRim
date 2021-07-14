using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HelpTab
{
    public static class BiomeDef_Extensions
    {
        // TODO: This is a nasty method, please get rid of it. Reason: Poor performance.
        public static List<TerrainDef> AllTerrainDefs(this BiomeDef biome)
        {
            var ret = new List<TerrainDef>();

            // map terrain
            if (!biome.terrainsByFertility.NullOrEmpty())
            {
                ret.AddRangeUnique(biome.terrainsByFertility.Select(t => t.terrain));
            }

            // patch maker terrain
            if (biome.terrainPatchMakers.NullOrEmpty())
            {
                return ret;
            }

            foreach (var patchMaker in biome.terrainPatchMakers)
            {
                ret.AddRangeUnique(patchMaker.thresholds.Select(t => t.terrain));
            }

            return ret;
        }
    }
}