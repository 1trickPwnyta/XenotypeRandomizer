using HarmonyLib;
using UnityEngine;
using Verse;

namespace XenotypeRandomizer
{
    public class XenotypeRandomizerMod : Mod
    {
        public const string PACKAGE_ID = "xenotyperandomizer.1trickPwnyta";
        public const string PACKAGE_NAME = "Xenotype Randomizer";

        public static XenotypeRandomizerMod Mod { get; private set; }
        public static XenotypeRandomizerSettings Settings;

        public XenotypeRandomizerMod(ModContentPack content) : base(content)
        {
            Mod = this;

            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }

        public override string SettingsCategory() => PACKAGE_NAME;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            XenotypeRandomizerSettings.DoSettingsWindowContents(inRect);
        }
    }
}
