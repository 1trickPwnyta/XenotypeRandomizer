using HarmonyLib;
using Verse;

namespace XenotypeRandomizer
{
    public class XenotypeRandomizerMod : Mod
    {
        public const string PACKAGE_ID = "xenotyperandomizer.1trickPwnyta";
        public const string PACKAGE_NAME = "Xenotype Randomizer";

        public XenotypeRandomizerMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}
