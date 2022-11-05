using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace HelpTab;

public class HelpDetailSectionHelper
{
    public static List<DefStringTriplet> BuildDefStringTripletList(List<Def> defs, string[] prefixes = null,
        string[] suffixes = null)
    {
        bool hasPrefix = false, hasSuffix = false;
        if (prefixes != null)
        {
            if (prefixes.Length != defs.Count)
            {
                throw new Exception("Prefix array length does not match Def list length.");
            }

            hasPrefix = true;
        }

        if (suffixes != null)
        {
            if (suffixes.Length != defs.Count)
            {
                throw new Exception("Suffix array length does not match Def list length.");
            }

            hasSuffix = true;
        }

        // prepare list of unique indices, filter out duplicates.
        var seen = new List<Def>();
        var unique = new List<int>();

        for (var i = 0; i < defs.Count; i++)
        {
            var i1 = i;
            if (seen.Count(def => def == defs[i1]) != 0)
            {
                continue;
            }

            unique.Add(i);
            seen.Add(defs[i]);
        }

        var ret = new List<DefStringTriplet>();
        foreach (var i in unique)
        {
            ret.Add(new DefStringTriplet(defs[i], hasPrefix ? prefixes[i] : null, hasSuffix ? suffixes[i] : null));
        }

        return ret;
    }

    public static void DrawText(ref Vector2 cur, float width, string text)
    {
        var height = Text.CalcHeight(text, width);
        var rect = new Rect(cur.x, cur.y, width, height);
        Widgets.Label(rect, text);
        cur.y += height - 6f; // offset to make lineheights fit better
    }

    public static void DrawLink(ref Vector2 cur, Rect container, Def def)
    {
    }

    public static List<StringDescTriplet> BuildStringDescTripletList(string[] stringDescs, string[] prefixes,
        string[] suffixes)
    {
        bool hasPrefix = false, hasSuffix = false;
        if (prefixes != null)
        {
            if (prefixes.Length != stringDescs.Length)
            {
                throw new Exception("Prefix array length does not match stringDescs length.");
            }

            hasPrefix = true;
        }

        if (suffixes != null)
        {
            if (suffixes.Length != stringDescs.Length)
            {
                throw new Exception("Suffix array length does not match stringDescs length.");
            }

            hasSuffix = true;
        }

        var ret = new List<StringDescTriplet>();
        for (var i = 0; i < stringDescs.Length; i++)
        {
            ret.Add(new StringDescTriplet(stringDescs[i], hasPrefix ? prefixes[i] : null,
                hasSuffix ? suffixes[i] : null));
        }

        return ret;
    }
}