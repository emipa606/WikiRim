using UnityEngine;
using Verse;

namespace HelpTab;

public static class Texture2D_Extensions
{
    extension(Texture2D tex)
    {
        // </summary>
        // <param name="tex"></param>
        // <returns></returns>
        public Texture2D Crop()
        {
            // see note above
            return tex;
        }

        public void DrawFittedIn(Rect rect)
        {
            var rectProportion = rect.width / rect.height;
            var texProportion = tex.width / (float)tex.height;

            if (texProportion > rectProportion)
            {
                var wider = new Rect(rect.xMin, 0f, rect.width, rect.width / texProportion).CenteredOnYIn(rect)
                    .CenteredOnXIn(rect);
                GUI.DrawTexture(wider, tex);
                return;
            }

            if (texProportion < rectProportion)
            {
                var taller = new Rect(0f, rect.yMin, rect.height * texProportion, rect.height).CenteredOnXIn(rect)
                    .CenteredOnXIn(rect);
                GUI.DrawTexture(taller, tex);
                return;
            }

            GUI.DrawTexture(rect, tex);
        }
    }
}