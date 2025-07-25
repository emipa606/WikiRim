using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HelpTab;

public static class Def_Extensions
{
    /// <summary>
    ///     hold a cached list of icons per def
    /// </summary>
    private static readonly Dictionary<Def, Texture2D> _cachedDefIcons = new();

    private static readonly Dictionary<Def, Color> _cachedIconColors = new();

    /// <summary>
    ///     hold a cached list of def -> helpdef links
    /// </summary>
    private static readonly Dictionary<Def, HelpDef> _cachedDefHelpDefLinks = new();

    /// <summary>
    ///     Get the label, capitalized and given appropriate styling  bold if def has a helpdef, italic if def has no helpdef
    ///     but does have description.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public static string LabelStyled(this Def def)
    {
        if (def.label.NullOrEmpty())
        {
            return string.Empty;
        }

        if (!def.description.NullOrEmpty())
        {
            return "<i>" + def.LabelCap + "</i>";
        }

        return def.LabelCap;
    }

    public static void DrawColouredIcon(this Def def, Rect canvas)
    {
        GUI.color = def.IconColor();
        GUI.DrawTexture(canvas, def.IconTexture(), ScaleMode.ScaleToFit);
        GUI.color = Color.white;
    }

    /// <summary>
    ///     Gets an appropriate drawColor for this def.
    ///     Will use a default stuff or DrawColor, if defined.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    private static Color IconColor(this Def def)
    {
        // check cache
        if (_cachedIconColors.TryGetValue(def, out var color))
        {
            return color;
        }

        // otherwise try to determine icon
        var tdef = def as ThingDef;

        // get product color for recipes
        if (def is RecipeDef rdef)
        {
            if (!rdef.products.NullOrEmpty())
            {
                _cachedIconColors.Add(def, rdef.products.First().thingDef.IconColor());
                return _cachedIconColors[def];
            }
        }

        // get color from final lifestage for pawns
        if (def is PawnKindDef pdef)
        {
            _cachedIconColors.Add(def, pdef.lifeStages.Last().bodyGraphicData.color);
            return _cachedIconColors[def];
        }

        if (def is not BuildableDef bdef)
        {
            // if we reach this point, def.IconTexture() would return null. Just store and return white to make sure we don't get weird errors down the line.
            _cachedIconColors.Add(def, Color.white);
            return _cachedIconColors[def];
        }

        // built def != listed def
        if (
            tdef is { entityDefToBuild: not null }
        )
        {
            _cachedIconColors.Add(def, tdef.entityDefToBuild.IconColor());
            return _cachedIconColors[def];
        }

        // graphic.color set?
        if (bdef.graphic != null)
        {
            _cachedIconColors.Add(def, bdef.graphic.color);
            return _cachedIconColors[def];
        }

        // stuff used?
        if (
            tdef is { MadeFromStuff: true }
        )
        {
            var stuff = GenStuff.DefaultStuffFor(tdef);
            _cachedIconColors.Add(def, stuff.stuffProps.color);
            return _cachedIconColors[def];
        }

        // all else failed.
        _cachedIconColors.Add(def, Color.white);
        return _cachedIconColors[def];
    }

    /// <summary>
    ///     Get a texture for the def, where defined.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public static Texture2D IconTexture(this Def def)
    {
        // check cache
        if (_cachedDefIcons.TryGetValue(def, out var texture))
        {
            return texture;
        }

        // recipes will be passed icon of first product, if defined.
        if (
            def is RecipeDef rdef &&
            !rdef.products.NullOrEmpty()
        )
        {
            _cachedDefIcons.Add(def, rdef.products.First().thingDef.IconTexture());
            return _cachedDefIcons[def];
        }

        // animals need special treatment ( this will still only work for animals, pawns are a whole different can o' worms ).
        if (def is PawnKindDef pdef)
        {
            try
            {
                _cachedDefIcons.Add(def,
                    (pdef.lifeStages.Last().bodyGraphicData.Graphic.MatSouth.mainTexture as Texture2D).Crop());
                return _cachedDefIcons[def];
            }
            catch
            {
                // ignored
            }
        }

        // if not buildable it probably doesn't have an icon.
        if (def is not BuildableDef bdef)
        {
            _cachedDefIcons.Add(def, null);
            return null;
        }

        // if def built != def listed.
        if (
            def is ThingDef tdef
        )
        {
            if (tdef.entityDefToBuild != null)
            {
                _cachedDefIcons.Add(def, tdef.entityDefToBuild.IconTexture().Crop());
                return _cachedDefIcons[def];
            }

            // corpses don't have icon
            if (tdef.IsCorpse)
            {
                return null;
            }
        }

        _cachedDefIcons.Add(def, bdef.uiIcon.Crop());
        return bdef.uiIcon.Crop();
    }

    public static float StyledLabelAndIconSize(this Def def)
    {
        var WW = Text.WordWrap;
        Text.WordWrap = false;
        var width = Text.CalcSize(def.LabelStyled()).x + (def.IconTexture() == null ? 0 : 20);
        Text.WordWrap = WW;
        return width;
    }

    /// <summary>
    ///     Get the helpdef associated with the current def, or null if none exists.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public static HelpDef GetHelpDef(this Def def)
    {
        if (_cachedDefHelpDefLinks.TryGetValue(def, out var helpDef))
        {
            return helpDef;
        }

        _cachedDefHelpDefLinks.Add(def,
            DefDatabase<HelpDef>.AllDefsListForReading.FirstOrDefault(hd => hd.keyDef == def));
        return _cachedDefHelpDefLinks[def];
    }
}