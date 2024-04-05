﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HelpTab;

public class MainTabWindow_ModHelp : MainTabWindow, IHelpDefView
{
    public enum State
    {
        Expanded,
        Closed,
        Leaf
    }

    public const float WindowMargin = 6f; // 15 is way too much.
    public const float EntryHeight = 30f;
    public const float EntryIndent = 15f;
    public const float ParagraphMargin = 8f;
    public const float LineHeigthOffset = 6f; // CalcSize overestimates required height by roughly this much.

    public const float MinWidth = 800f;
    public const float MinHeight = 600f;
    public const float MinListWidth = 200f;

    // Maximum count of displayed entries.
    // It added to improve perfomance at large search result.
    private const int MaxEntryCount = 200;
    protected static List<ModCategory> CachedHelpCategories;
    protected static Vector2 ArrowImageSize = new Vector2(10f, 10f);

    private static string _filterString = "";
    private bool _filtered;
    private bool _jump;
    private string _lastFilterString = "";
    private int _lastFilterTick;
    public float ContentHeight = 9999f;
    protected Rect DisplayRect;
    protected Vector2 DisplayScrollPos;

    private int entryCounter;
    public HelpDef SelectedHelpDef;
    public float SelectionHeight = 9999f;

    protected Rect SelectionRect;

    protected Vector2 SelectionScrollPos;

    public MainTabWindow_ModHelp()
    {
        layer = WindowLayer.GameUI;
        soundAppear = null;
        soundClose = null;
        doCloseButton = false;
        doCloseX = true;
        closeOnCancel = true;
        forcePause = true;
    }

    private MainButton_HelpMenuDef TabDef => def as MainButton_HelpMenuDef;

    public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;

    public override Vector2 RequestedTabSize
    {
        get
        {
            if (TabDef != null)
            {
                return new Vector2(TabDef.windowSize.x > MinWidth ? TabDef.windowSize.x : MinWidth,
                    TabDef.windowSize.y > MinHeight ? TabDef.windowSize.y : MinHeight);
            }

            return new Vector2(MinWidth, MinHeight);
        }
    }

    public void JumpTo(Def helpDef)
    {
        JumpTo(helpDef.GetHelpDef());
    }

    public void JumpTo(HelpDef helpDef)
    {
        Find.MainTabsRoot.SetCurrentTab(def);
        ResetFilter();
        _jump = true;
        SelectedHelpDef = helpDef;
        var cat = DefDatabase<HelpCategoryDef>.AllDefsListForReading.First(hc => hc.HelpDefs.Contains(helpDef));
        cat.Expanded = true;
        var mod = CachedHelpCategories.First(mc => mc.HelpCategories.Contains(cat));
        mod.Expanded = true;
    }

    public bool Accept(HelpDef helpDef)
    {
        return true;
    }

    public IHelpDefView SecondaryView(HelpDef helpDef)
    {
        return null;
    }

    public override void PreOpen()
    {
        base.PreOpen();

        // Set whether the window forces a pause
        // Not entirely sure why force pause warrants a xml setting? - Fluffy.
        if (TabDef != null)
        {
            forcePause = TabDef.pauseGame;
        }

        // Build the help system
        Recache();

        // set initial Filter
        Filter();
    }

    public static void Recache()
    {
        CachedHelpCategories = [];
        foreach (var helpCategory in DefDatabase<HelpCategoryDef>.AllDefs)
        {
            // parent modcategory does not exist, create it.
            if (CachedHelpCategories.All(t => t.ModName != helpCategory.ModName))
            {
                var mCat = new ModCategory(helpCategory.ModName);
                mCat.AddCategory(helpCategory);
                CachedHelpCategories.Add(mCat);
            }
            // add to existing modcategory
            else
            {
                var mCat = CachedHelpCategories.Find(t => t.ModName == helpCategory.ModName);
                mCat.AddCategory(helpCategory);
            }
        }
    }

    private void FilterUpdate()
    {
        // filter after a short delay.
        // Log.Message(_filterString + " | " + _lastFilterTick + " | " + _filtered);
        if (_filterString != _lastFilterString)
        {
            _lastFilterString = _filterString;
            _lastFilterTick = 0;
            _filtered = false;
        }
        else if (!_filtered)
        {
            if (_lastFilterTick > 60)
            {
                Filter();
            }

            _lastFilterTick++;
        }
    }

    public void Filter()
    {
        foreach (var mc in CachedHelpCategories)
        {
            mc.Filter(_filterString);
        }

        _filtered = true;
    }

    public void ResetFilter()
    {
        _filterString = "";
        _lastFilterString = "";
        Filter();
    }

    public override void DoWindowContents(Rect rect)
    {
        Text.Font = GameFont.Small;

        GUI.BeginGroup(rect);

        var selectionWidth = TabDef != null
            ? TabDef.listWidth >= MinListWidth ? TabDef.listWidth : MinListWidth
            : MinListWidth;
        SelectionRect = new Rect(0f, 0f, selectionWidth, rect.height);
        DisplayRect = new Rect(
            SelectionRect.width + WindowMargin, 0f,
            rect.width - SelectionRect.width - WindowMargin, rect.height
        );

        DrawSelectionArea(SelectionRect);
        DrawDisplayArea(DisplayRect);

        GUI.EndGroup();
    }

    private void DrawDisplayArea(Rect rect)
    {
        Widgets.DrawMenuSection(rect);

        if (SelectedHelpDef == null)
        {
            return;
        }

        Text.Font = GameFont.Medium;
        Text.WordWrap = false;
        var titleWidth = Text.CalcSize(SelectedHelpDef.LabelCap).x;
        var titleRect = new Rect(rect.xMin + WindowMargin, rect.yMin + WindowMargin, titleWidth, 60f);

        if (SelectedHelpDef.keyDef is ResearchProjectDef research)
        {
            var researchRect = new Rect(rect.xMin + WindowMargin, rect.yMin + WindowMargin + 5f, 90f, 50f);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            if (research.IsFinished)
            {
                Widgets.DrawMenuSection(researchRect);
                Widgets.Label(researchRect, ResourceBank.String.Finished);
            }
            else if (research == Find.ResearchManager.currentProj)
            {
                Widgets.DrawMenuSection(researchRect);
                Widgets.Label(researchRect, ResourceBank.String.InProgress);
            }
            else if (!research.CanStartNow)
            {
                Widgets.DrawMenuSection(researchRect);
                Widgets.Label(researchRect, ResourceBank.String.Locked);
            }
            else if (Widgets.ButtonText(researchRect, ResourceBank.String.Research, true, false))
            {
                SoundDef.Named("ResearchStart").PlayOneShotOnCamera();
                Find.ResearchManager.currentProj = research;
                TutorSystem.Notify_Event("StartResearchProject");
            }

            titleRect.x += 100f;
        }
        else if (
            SelectedHelpDef.keyDef != null &&
            SelectedHelpDef.keyDef.IconTexture() != null
        )
        {
            var iconRect = new Rect(titleRect.xMin + WindowMargin, rect.yMin + WindowMargin,
                60f - (2 * WindowMargin), 60f - (2 * WindowMargin));
            titleRect.x += 60f;
            SelectedHelpDef.keyDef.DrawColouredIcon(iconRect);
        }

        if (SelectedHelpDef.keyDef is PawnKindDef kindDef)
        {
            Widgets.InfoCardButton(rect.xMax - WindowMargin - 30f, rect.yMin + WindowMargin, kindDef.race);
        }
        else if (SelectedHelpDef.keyDef is not ResearchProjectDef && SelectedHelpDef.keyDef is not BiomeDef)
        {
            Widgets.InfoCardButton(rect.xMax - WindowMargin - 30f, rect.yMin + WindowMargin, SelectedHelpDef.keyDef);
        }

        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(titleRect, SelectedHelpDef.LabelCap);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;

        var outRect = rect.ContractedBy(WindowMargin);
        outRect.yMin += 60f;
        var viewRect = outRect;
        viewRect.width -= 16f;
        viewRect.height = ContentHeight;

        GUI.BeginGroup(outRect);
        Widgets.BeginScrollView(outRect.AtZero(), ref DisplayScrollPos, viewRect.AtZero());

        var cur = Vector2.zero;

        HelpDetailSectionHelper.DrawText(ref cur, viewRect.width, SelectedHelpDef.description);

        cur.y += ParagraphMargin;

        foreach (var section in SelectedHelpDef.HelpDetailSections)
        {
            section.Draw(ref cur, viewRect.width, this);
        }

        ContentHeight = cur.y;

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    private void DrawSelectionArea(Rect rect)
    {
        entryCounter = 0;
        Widgets.DrawMenuSection(rect);

        FilterUpdate();
        var filterRect = new Rect(rect.xMin + WindowMargin, rect.yMin + WindowMargin,
            rect.width - (3 * WindowMargin) - 30f, 30f);
        var clearRect = new Rect(filterRect.xMax + WindowMargin + 3f, rect.yMin + WindowMargin + 3f, 24f, 24f);
        _filterString = Widgets.TextField(filterRect, _filterString);
        if (_filterString != "")
        {
            if (Widgets.ButtonImage(clearRect, Widgets.CheckboxOffTex))
            {
                _filterString = "";
                Filter();
            }
        }

        var modCheckRect = new Rect(rect.xMin + WindowMargin, rect.yMin + WindowMargin + 30f,
            rect.width - (3 * WindowMargin) - 30f, 30f);
        var startValue = HelpTabMod.SearchMods;
        Widgets.CheckboxLabeled(modCheckRect, "SearchModsAsWell".Translate(), ref HelpTabMod.SearchMods);
        if (HelpTabMod.SearchMods != startValue)
        {
            Filter();
        }

        var outRect = rect;
        outRect.yMin += 70f;
        outRect.xMax -= 2f; // some spacing around the scrollbar

        var viewWidth = SelectionHeight > outRect.height ? outRect.width - 16f : outRect.width;
        var viewRect = new Rect(0f, 0f, viewWidth, SelectionHeight);

        GUI.BeginGroup(outRect);
        Widgets.BeginScrollView(outRect.AtZero(), ref SelectionScrollPos, viewRect);

        if (CachedHelpCategories.Count(mc => mc.ShouldDraw) < 1)
        {
            var messageRect = outRect.AtZero();
            Widgets.Label(messageRect, "NoHelpDefs".Translate());
        }
        else
        {
            var cur = Vector2.zero;

            // This works fine for the current artificial three levels of helpdefs. 
            // Can easily be adapted by giving each category a list of subcategories, 
            // and migrating the responsibility for drawing them and the helpdefs to DrawCatEntry().
            // Would also require some minor adaptations to the filter methods, but nothing major.
            // - Fluffy.
            foreach (var mc in CachedHelpCategories.Where(mc => mc.ShouldDraw))
            {
                DrawModEntry(ref cur, 0, viewRect, mc);

                cur.x += EntryIndent;
                if (!mc.Expanded)
                {
                    continue;
                }

                foreach (var hc in mc.HelpCategories.Where(hc => hc.ShouldDraw))
                {
                    DrawCatEntry(ref cur, 1, viewRect, hc);

                    if (!hc.Expanded)
                    {
                        continue;
                    }

                    foreach (var hd in hc.HelpDefs.Where(hd => hd.ShouldDraw))
                    {
                        if (entryCounter >= MaxEntryCount)
                        {
                            var lastItem = new HelpDef
                            {
                                defName = hd.defName,
                                label = "MaxAmountOfHits".Translate(),
                                category = hd.category
                            };
                            DrawHelpEntry(ref cur, 1, viewRect, lastItem);
                            break;
                        }

                        DrawHelpEntry(ref cur, 1, viewRect, hd);
                    }

                    if (entryCounter >= MaxEntryCount)
                    {
                        break;
                    }
                }

                if (entryCounter >= MaxEntryCount)
                {
                    break;
                }
            }

            SelectionHeight = cur.y;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    /// <summary>
    ///     Generic method for drawing the squares.
    /// </summary>
    /// <param name="cur">Current x,y vector</param>
    /// <param name="nestLevel">Level of nesting for indentation</param>
    /// <param name="view">Size of viewing area (assumed vertically scrollable)</param>
    /// <param name="label">Label to show</param>
    /// <param name="state">State of collapsing icon to show</param>
    /// <param name="selected">For leaf entries, is this entry selected?</param>
    /// <returns></returns>
    public bool DrawEntry(ref Vector2 cur, int nestLevel, Rect view, string label, State state,
        bool selected = false)
    {
        entryCounter++;
        cur.x = nestLevel * EntryIndent;
        var iconOffset = ArrowImageSize.x + (2 * WindowMargin);
        var width = view.width - cur.x - iconOffset - WindowMargin;
        var height = EntryHeight;

        if (Text.CalcHeight(label, width) > EntryHeight)
        {
            Text.Font = GameFont.Tiny;
            var height2 = Text.CalcHeight(label, width);
            height = Mathf.Max(height, height2);
        }

        if (state != State.Leaf)
        {
            var iconRect = new Rect(cur.x + WindowMargin, cur.y + (height / 2) - (ArrowImageSize.y / 2),
                ArrowImageSize.x, ArrowImageSize.y);
            GUI.DrawTexture(iconRect,
                state == State.Expanded
                    ? ResourceBank.Icon.HelpMenuArrowDown
                    : ResourceBank.Icon.HelpMenuArrowRight);
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        var labelRect = new Rect(cur.x + iconOffset, cur.y, width, height);
        Widgets.Label(labelRect, label);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;

        // full viewRect width for overlay and button
        var buttonRect = view;
        buttonRect.yMin = cur.y;
        cur.y += height;
        buttonRect.yMax = cur.y;
        GUI.color = Color.grey;
        Widgets.DrawLineHorizontal(view.xMin, cur.y, view.width);
        GUI.color = Color.white;
        if (selected)
        {
            Widgets.DrawHighlightSelected(buttonRect);
        }
        else
        {
            Widgets.DrawHighlightIfMouseover(buttonRect);
        }

        return Widgets.ButtonInvisible(buttonRect);
    }

    public void DrawModEntry(ref Vector2 cur, int nestLevel, Rect view, ModCategory mc)
    {
        var curState = mc.Expanded ? State.Expanded : State.Closed;
        if (DrawEntry(ref cur, nestLevel, view, mc.ModName, curState))
        {
            mc.Expanded = !mc.Expanded;
        }
    }

    public void DrawCatEntry(ref Vector2 cur, int nestLevel, Rect view, HelpCategoryDef catDef)
    {
        var curState = catDef.Expanded ? State.Expanded : State.Closed;
        if (DrawEntry(ref cur, nestLevel, view, catDef.LabelCap, curState))
        {
            catDef.Expanded = !catDef.Expanded;
        }
    }

    public void DrawHelpEntry(ref Vector2 cur, int nestLevel, Rect view, HelpDef helpDef)
    {
        var selected = SelectedHelpDef == helpDef;
        if (selected && _jump)
        {
            SelectionScrollPos.y = cur.y;
            _jump = false;
        }

        if (DrawEntry(ref cur, nestLevel, view, helpDef.LabelCap, State.Leaf, selected))
        {
            SelectedHelpDef = helpDef;
        }
    }

    public class ModCategory(string modName)
    {
        private readonly List<HelpCategoryDef> _helpCategories = [];

        public readonly string ModName = modName;

        public bool Expanded;

        public List<HelpCategoryDef> HelpCategories => _helpCategories.OrderBy(a => a.label).ToList();

        public bool ShouldDraw { get; set; }

        public bool MatchesFilter(string filter)
        {
            return filter == "" || ModName.ToUpper().Contains(filter.ToUpper());
        }

        public bool ThisOrAnyChildMatchesFilter(string filter)
        {
            return MatchesFilter(filter) || HelpCategories.Any(hc => hc.ThisOrAnyChildMatchesFilter(filter));
        }

        public void Filter(string filter)
        {
            ShouldDraw = ThisOrAnyChildMatchesFilter(filter);
            Expanded = filter != "" && ThisOrAnyChildMatchesFilter(filter);

            foreach (var hc in HelpCategories)
            {
                hc.Filter(filter, MatchesFilter(filter));
            }
        }

        public void AddCategory(HelpCategoryDef def)
        {
            _helpCategories.AddUnique(def);
        }
    }
}