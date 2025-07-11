using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace XenotypeRandomizer
{
    [StaticConstructorOnStartup]
    public class XenotypeRandomizerSettings : ModSettings
    {
        private static readonly Texture2D optionsTex = ContentFinder<Texture2D>.Get("UI/Icons/Options/OptionsGeneral");

        static XenotypeRandomizerSettings()
        {
            XenotypeRandomizerMod.Settings = XenotypeRandomizerMod.Mod.GetSettings<XenotypeRandomizerSettings>();
        }

        private static bool AllowNonviolent = true;
        private static bool AllowInbred = false;
        public static HashSet<GeneDef> DisallowedGenes = new HashSet<GeneDef>() { GeneDefOf.Inbred };

        private static Vector2 genesScrollPosition;
        private static float genesHeight;
        private static readonly QuickSearchWidget search = new QuickSearchWidget();

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);

            DoGenes(listing);

            listing.End();
        }

        private static void DoGenes(Listing_Standard listing)
        {
            Rect genesHeaderRect = listing.GetRect(30f);
            Widgets.Label(genesHeaderRect.LeftPartPixels(genesHeaderRect.width - 200f), "XenotypeRandomizer_AllowedGenes".Translate());
            Rect buttonsRect = genesHeaderRect.RightPartPixels(230f);
            Rect allowButtonsRect = buttonsRect.LeftPartPixels(200f);
            if (Widgets.ButtonText(allowButtonsRect.LeftHalf(), "XenotypeRandomizer_AllowAll".Translate()))
            {
                DisallowedGenes.Clear();
            }
            if (Widgets.ButtonText(allowButtonsRect.RightHalf(), "XenotypeRandomizer_DisallowAll".Translate()))
            {
                DisallowedGenes.AddRange(XenotypeRandomizer.DefaultGenePool);
            }
            if (Widgets.ButtonImage(buttonsRect.RightPartPixels(30f), optionsTex))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
                {
                    new FloatMenuOption("XenotypeRandomizer_DisallowNonviolent".Translate(), DisallowNonviolentGenes)
                }));
            }

            listing.Gap();

            Rect genesOutRect = listing.GetRect(300f);

            listing.Gap();

            Rect searchRect = listing.GetRect(Window.QuickSearchSize.y).LeftPartPixels(Window.QuickSearchSize.x);
            search.OnGUI(searchRect);

            Rect genesViewRect = new Rect(0f, 0f, genesOutRect.width - 20f, genesHeight);
            genesHeight = 0f;
            Widgets.BeginScrollView(genesOutRect, ref genesScrollPosition, genesViewRect);
            bool doAlternate = true;
            foreach (GeneDef gene in XenotypeRandomizer.DefaultGenePool.Where(g => search.filter.Matches(g.label)))
            {
                Rect geneRect = new Rect(0f, genesHeight, genesViewRect.width, 30f);
                if (doAlternate = !doAlternate)
                {
                    Widgets.DrawRectFast(geneRect, Widgets.MenuSectionBGFillColor);
                }
                Widgets.DrawTextureFitted(geneRect.LeftPartPixels(geneRect.height), gene.Icon, 1f);
                bool geneAllowed = !DisallowedGenes.Contains(gene);
                Widgets.CheckboxLabeled(geneRect.RightPartPixels(geneRect.width - geneRect.height - Window.StandardMargin), gene.LabelCap, ref geneAllowed, paintable: true);
                if (geneAllowed)
                {
                    DisallowedGenes.Remove(gene);
                }
                else
                {
                    DisallowedGenes.Add(gene);
                }
                genesHeight += geneRect.height;
            }
            Widgets.EndScrollView();
        }

        private static void DisallowNonviolentGenes()
        {
            DisallowedGenes.AddRange(XenotypeRandomizer.DefaultGenePool.Where(g => g.IsNonviolent()));
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref AllowNonviolent, "AllowNonviolent");
            Scribe_Values.Look(ref AllowInbred, "AllowInbred");
            Scribe_Collections.Look(ref DisallowedGenes, "DisallowedGenes", LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (DisallowedGenes == null)
                {
                    DisallowedGenes = new HashSet<GeneDef>() { GeneDefOf.Inbred };
                }
                if (!AllowNonviolent)
                {
                    DisallowNonviolentGenes();
                }
                if (AllowInbred)
                {
                    DisallowedGenes.Remove(GeneDefOf.Inbred);
                }
            }
        }
    }
}
