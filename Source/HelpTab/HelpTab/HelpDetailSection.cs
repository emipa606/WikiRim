﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace HelpTab;

public class HelpDetailSection
{
    private const string DefaultInsetString = "\t";
    private const float DefaultInset = 30f;
    public static readonly float _columnMargin = 8f;
    private readonly float Inset;
    private readonly string InsetString;
    private readonly List<DefStringTriplet> KeyDefs;
    private readonly string Label;
    private readonly List<StringDescTriplet> StringDescs;
    public bool Align;
    private Vector3 ColumnWidths = Vector3.zero;
    private bool WidthsSet;

    public HelpDetailSection(string label,
        string[] stringDescs,
        string[] prefixes,
        string[] suffixes,
        bool align = true)
    {
        Label = label;
        KeyDefs = null;
        StringDescs = stringDescs != null
            ? HelpDetailSectionHelper.BuildStringDescTripletList(stringDescs, prefixes, suffixes)
            : null;
        Align = align;
        if (label != null)
        {
            InsetString = DefaultInsetString;
            Inset = DefaultInset;
        }
        else
        {
            InsetString = string.Empty;
            Inset = 0f;
        }
    }

    public HelpDetailSection(string label,
        List<Def> keyDefs,
        string[] defPrefixes,
        string[] defSuffixes,
        string[] stringDescs,
        string[] descPrefixes,
        string[] descSuffixes,
        bool align = true)
    {
        Label = label;
        KeyDefs = keyDefs != null
            ? HelpDetailSectionHelper.BuildDefStringTripletList(keyDefs, defPrefixes, defSuffixes)
            : null;
        StringDescs = stringDescs != null
            ? HelpDetailSectionHelper.BuildStringDescTripletList(stringDescs, descPrefixes, descSuffixes)
            : null;
        Align = align;
        if (label != null)
        {
            InsetString = DefaultInsetString;
            Inset = DefaultInset;
        }
        else
        {
            InsetString = string.Empty;
            Inset = 0f;
        }
    }

    public HelpDetailSection(string label, List<DefStringTriplet> defStringTriplets,
        List<StringDescTriplet> stringDescTriplets, bool align = true)
    {
        Label = label;
        KeyDefs = defStringTriplets;
        StringDescs = stringDescTriplets;
        Align = align;
        if (label != null)
        {
            InsetString = DefaultInsetString;
            Inset = DefaultInset;
        }
        else
        {
            InsetString = string.Empty;
            Inset = 0f;
        }
    }

    public HelpDetailSection(string label, List<Def> keyDefs, string[] prefixes = null, string[] suffixes = null,
        bool align = true)
    {
        Label = label;
        KeyDefs = keyDefs != null
            ? HelpDetailSectionHelper.BuildDefStringTripletList(keyDefs, prefixes, suffixes)
            : null;
        StringDescs = null;
        Align = align;
        if (label != null)
        {
            InsetString = DefaultInsetString;
            Inset = DefaultInset;
        }
        else
        {
            InsetString = string.Empty;
            Inset = 0f;
        }
    }

    public void Draw(ref Vector2 cur, float width, IHelpDefView window = null)
    {
        // Draw section header, if any
        if (!Label.NullOrEmpty())
        {
            var labelRect = new Rect(cur.x, cur.y, width, 20f);
            Widgets.Label(labelRect, Label);
            cur.y += 20f - MainTabWindow_ModHelp.LineHeigthOffset;
        }

        // respect tabs!
        cur.x += Inset;

        // make sure column widths have been calculated
        if (!WidthsSet)
        {
            SetColumnWidths(width - Inset);
        }

        // draw lines one by one
        if (!StringDescs.NullOrEmpty())
        {
            foreach (var triplet in StringDescs)
            {
                triplet.Draw(ref cur, ColumnWidths);
            }
        }

        if (!KeyDefs.NullOrEmpty())
        {
            foreach (var triplet in KeyDefs)
            {
                triplet.Draw(ref cur, ColumnWidths, window);
            }
        }

        // add some extra space, reset inset
        cur.y += MainTabWindow_ModHelp.ParagraphMargin;
        cur.x -= Inset;

        // done!
    }

    /// <summary>
    ///     Take all defined help sections and calculate 'optimal' column widths.
    /// </summary>
    /// <param name="width">Total width</param>
    private void SetColumnWidths(float width)
    {
        // leave some margin
        width -= 2 * _columnMargin;

        // build lists of all strings in this section
        var prefixes = new List<string>();
        var suffixes = new List<string>();
        var descs = new List<string>();
        var defs = new List<Def>();

        if (StringDescs != null)
        {
            prefixes.AddRange(StringDescs.Select(s => s.Prefix));
            suffixes.AddRange(StringDescs.Select(s => s.Suffix));
            descs.AddRange(StringDescs.Select(s => s.StringDesc));
        }

        if (KeyDefs != null)
        {
            prefixes.AddRange(KeyDefs.Select(k => k.Prefix));
            suffixes.AddRange(KeyDefs.Select(k => k.Suffix));
            defs.AddRange(KeyDefs.Select(k => k.Def));
        }

        // fetch length of all strings, select largest for each column
        var requestedWidths = Vector3.zero;

        // make sure wrapping is off, so we get a true idea of the length
        var WW = Text.WordWrap;
        Text.WordWrap = false;
        if (!prefixes.NullOrEmpty())
        {
            requestedWidths.x = prefixes.Select(s => Text.CalcSize(s).x).Max();
        }

        if (!descs.NullOrEmpty())
        {
            requestedWidths.y = descs.Select(s => Text.CalcSize(s).x).Max();
        }

        if (!defs.NullOrEmpty())
        {
            requestedWidths.y = Mathf.Max(requestedWidths.y, defs.Select(d => d.StyledLabelAndIconSize()).Max());
        }

        if (!suffixes.NullOrEmpty())
        {
            requestedWidths.z = suffixes.Select(s => Text.CalcSize(s).x).Max();
        }

        Text.WordWrap = WW;

        if (requestedWidths.Sum() < width)
        {
            // expand right-most column (even if it was zero)
            requestedWidths.z += width - requestedWidths.Sum();

            // done
            ColumnWidths = requestedWidths;
        }
        else
        {
            // if size overflow is < 30% of largest column width, scale that down.
            if (requestedWidths.Sum() - width < .3f * requestedWidths.Max())
            {
                for (var i = 0; i < 3; i++)
                {
                    if (requestedWidths[i] != requestedWidths.Max())
                    {
                        continue;
                    }

                    requestedWidths[i] -= requestedWidths.Sum() - width;
                    break;
                }
            }
            else // scale everything down, with a minimum width of 15% per column
            {
                var shrinkableWidth = requestedWidths.Subtract(width * .15f);
                var scalingFactor = width / shrinkableWidth.Sum();
                for (var i = 0; i < 3; i++)
                {
                    requestedWidths[i] -= shrinkableWidth[i] * scalingFactor;
                }
            }

            // done
            ColumnWidths = requestedWidths;
        }

        // set done flag.
        WidthsSet = true;
    }

    /// <summary>
    ///     Build the display string for a HelpDetailSection
    /// </summary>
    public string GetString()
    {
        var s = new StringBuilder();
        if (Label != null)
        {
            s.AppendLine($"{Label.CapitalizeFirst()}:");
        }

        if (StringDescs != null)
        {
            foreach (var stringDesc in StringDescs)
            {
                s.Append(InsetString);
                s.AppendLine(stringDesc.ToString());
            }
        }

        if (KeyDefs == null)
        {
            return s.ToString();
        }

        foreach (var def in KeyDefs)
        {
            s.Append(InsetString);
            s.AppendLine(def.ToString());
        }

        return s.ToString();
    }
}