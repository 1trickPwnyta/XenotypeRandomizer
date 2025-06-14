using RimWorld;
using UnityEngine;
using Verse;

namespace XenotypeRandomizer
{
    public class XenotypeRandomizerSettings : ModSettings
    {
        public static bool AllowNonviolent = true;
        public static bool AllowInbred = false;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("XenotypeRandomizer_AllowNonviolent".Translate(), ref AllowNonviolent);
            listingStandard.CheckboxLabeled("XenotypeRandomizer_AllowInbred".Translate(), ref AllowInbred);

            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref AllowNonviolent, "AllowNonviolent", true);
            Scribe_Values.Look(ref AllowInbred, "AllowInbred", false);
        }
    }
}
