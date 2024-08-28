using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace XenotypeRandomizer
{
    [HarmonyPatch(typeof(Dialog_CreateXenotype))]
    [HarmonyPatch("DoBottomButtons")]
    public static class Patch_Dialog_CreateXenotype_DoBottomButtons
    {
        public static void Postfix(
            Dialog_CreateXenotype __instance, 
            Rect rect, 
            ref List<GeneDef> ___selectedGenes, ref string ___xenotypeName, ref bool ___xenotypeNameLocked, ref XenotypeIconDef ___iconDef)
        {
            if (Widgets.ButtonText(new Rect(rect.x + rect.width/2 - 150f/2, rect.y, 150f, 38f), "XenotypeRandomizer_Randomize".Translate()))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);

                XenotypeRandomizer.Randomize(___selectedGenes, ref ___iconDef, XenotypeRandomizerSettings.AllowNonviolent, XenotypeRandomizerSettings.AllowInbred);

                if (!___xenotypeNameLocked)
                {
                    ___xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(___selectedGenes);
                }

                MethodInfo onGenesChanged = AccessTools.Method(typeof(Dialog_CreateXenotype), "OnGenesChanged");
                onGenesChanged.Invoke(__instance, new object[] { });
            }
        }
    }
}
