using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace HelpTab;

public struct StringDescTriplet(string stringDesc, string prefix = null, string suffix = null)
{
    public readonly string StringDesc = stringDesc;
    public readonly string Prefix = prefix;
    public readonly string Suffix = suffix;

    private float _height = 0f;
    private bool _heightSet = false;

    public override string ToString()
    {
        var s = new StringBuilder();
        if (Prefix != "")
        {
            s.Append($"{Prefix} ");
        }

        s.Append(StringDesc);
        if (Suffix != "")
        {
            s.Append($" {Suffix}");
        }

        return s.ToString();
    }

    public void Draw(ref Vector2 cur, Vector3 colWidths)
    {
        if (!_heightSet)
        {
            var heights = new List<float>();
            if (!Prefix.NullOrEmpty())
            {
                heights.Add(Text.CalcHeight(Prefix, colWidths.x));
            }

            heights.Add(Text.CalcHeight(StringDesc, colWidths.y));
            if (!Suffix.NullOrEmpty())
            {
                heights.Add(Text.CalcHeight(Suffix, colWidths.z));
            }

            _height = heights.Max();
            _heightSet = true;
        }

        if (!Prefix.NullOrEmpty())
        {
            var prefixRect = new Rect(cur.x, cur.y, colWidths.x, _height);
            Widgets.Label(prefixRect, Prefix);
        }

        if (!Suffix.NullOrEmpty())
        {
            var suffixRect = new Rect(cur.x + colWidths.x + colWidths.y + (2 * HelpDetailSection._columnMargin),
                cur.y, colWidths.z, _height);
            Widgets.Label(suffixRect, Suffix);
        }

        var labelRect =
            new Rect(cur.x + colWidths.x + (Prefix.NullOrEmpty() ? 0f : HelpDetailSection._columnMargin),
                cur.y,
                colWidths.y,
                _height);

        Widgets.Label(labelRect, StringDesc);
        cur.y += _height - MainTabWindow_ModHelp.LineHeigthOffset;
    }
}