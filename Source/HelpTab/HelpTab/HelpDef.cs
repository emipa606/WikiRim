using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace HelpTab;

public class HelpDef : Def, IComparable
{
    public readonly List<HelpDetailSection> HelpDetailSections = [];
    public HelpCategoryDef category;

    [Unsaved] public Def keyDef;
    public Def secondaryKeyDef;

    public string Description
    {
        get
        {
            var s = new StringBuilder();
            s.AppendLine(description);
            foreach (var section in HelpDetailSections)
            {
                s.AppendLine(section.GetString());
            }

            return s.ToString();
        }
    }

    public bool ShouldDraw { get; private set; }

    public int CompareTo(object obj)
    {
        return
            obj is HelpDef d
                ? string.CompareOrdinal(d.label, label) * -1
                : 1;
    }

    public void Filter(string filter, bool force = false)
    {
        ShouldDraw = force || MatchesFilter(filter);
    }

    public bool MatchesFilter(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return true;
        }

        if (string.IsNullOrEmpty(label))
        {
            return false;
        }

        return label.ToLower().Contains(filter.ToLower()) || HelpTabMod.SearchMods &&
            keyDef.modContentPack?.Name?.ToLower().Contains(filter.ToLower()) == true;
    }
}