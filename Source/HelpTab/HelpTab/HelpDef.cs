using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace HelpTab
{
    public class HelpDef : Def, IComparable
    {
        public HelpCategoryDef category;

        public List<HelpDetailSection> HelpDetailSections = new List<HelpDetailSection>();

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

        public bool ShouldDraw { get; set; }

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
            return filter == "" || LabelCap.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}