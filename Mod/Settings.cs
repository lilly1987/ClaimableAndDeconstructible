using Verse;

namespace Lilly.ClaimableAndDeconstructible
{
    public class Settings : ModSettings
    {
        public static bool onDebug = true;
        public static bool onPatch = true;

        public override void ExposeData()
        {
            MyLog.Message($"<color=#00FF00FF>{Scribe.mode}</color>");
            base.ExposeData();
            Scribe_Values.Look(ref onDebug, "onDebug", false);
            Scribe_Values.Look(ref onPatch, "onPatch", true);

            Patch.OnPatch(true);
        }
    }
}
