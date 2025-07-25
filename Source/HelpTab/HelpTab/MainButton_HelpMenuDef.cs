using RimWorld;
using UnityEngine;

namespace HelpTab;

public class MainButton_HelpMenuDef : MainButtonDef
{
    public readonly float listWidth = MainTabWindow_ModHelp.MinListWidth;
    public readonly bool pauseGame = false;

    public Vector2 windowSize = new(MainTabWindow_ModHelp.MinWidth, MainTabWindow_ModHelp.MinHeight);
}