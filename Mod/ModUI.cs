using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lilly.ClaimableAndDeconstructible
{
    public class ModUI : Mod
    {
        public static ModUI self;
        public static Settings settings;

        public ModUI(ModContentPack content) : base(content)
        {
            self = this;
            MyLog.Message($"ST");

            settings = GetSettings<Settings>();// 주의. StaticConstructorOnStartup 보다 먼저 실행됨      

            MyLog.Message($"ED");
        }

        Vector2 scrollPosition;
        string tmp;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            var rect = new Rect(0, 0, inRect.width - 16, 1000);

            Widgets.BeginScrollView(inRect, ref scrollPosition, rect);

            Listing_Standard listing = new Listing_Standard();

            listing.Begin(rect);

            listing.GapLine();

            // ---------

            listing.CheckboxLabeled($"Debug", ref Settings.onDebug);
            listing.CheckboxLabeled($"on Patch", ref Settings.onPatch);

            // ---------

            listing.GapLine();

            listing.End();

            Widgets.EndScrollView();
        }

        public override string SettingsCategory()
        {
            return "Claimable And Deconstructible".Translate();
        }
    }
}
