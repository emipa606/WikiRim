using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HelpTab;

public class HelpCategoryDef : Def
{
    public string keyDef;
    public string ModName;

    [field: Unsaved] public List<HelpDef> HelpDefs { get; } = [];

    public bool ShouldDraw { get; set; }

    public bool Expanded { get; set; }

    public bool MatchesFilter(string filter)
    {
        return filter == "" || LabelCap.ToString().ToUpper().Contains(filter.ToUpper());
    }

    public bool ThisOrAnyChildMatchesFilter(string filter)
    {
        return MatchesFilter(filter) || HelpDefs.Any(hd => hd.MatchesFilter(filter));
    }

    public void Filter(string filter, bool force = false)
    {
        ShouldDraw = force || ThisOrAnyChildMatchesFilter(filter);
        Expanded = filter != "" && (force || ThisOrAnyChildMatchesFilter(filter));
        foreach (var hd in HelpDefs)
        {
            hd.Filter(filter, force || MatchesFilter(filter));
        }
    }


    public override void ResolveReferences()
    {
        base.ResolveReferences();
        Recache();
    }

    public void Recache()
    {
        HelpDefs.Clear();
        foreach (var def in
                 from t in DefDatabase<HelpDef>.AllDefs
                 where t.category == this
                 select t)
        {
            HelpDefs.Add(def);
        }

        HelpDefs.Sort();
    }
}