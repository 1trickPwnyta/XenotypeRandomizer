using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace XenotypeRandomizer
{
    public static class XenotypeRandomizer
    {
        public static List<GeneDef> DefaultGenePool => GeneUtility.GenesInOrder.Where(g => g.biostatArc <= 0).ToList();

        public static bool IsNonviolent(this GeneDef gene) => (gene.disabledWorkTags & WorkTags.Violent) != WorkTags.None;

        private static bool IsAllowed(GeneDef gene, bool? overrideAllowNonviolent, bool? overrideAllowInbred, bool desperate = false)
        {
            bool skip = false;
            bool overrideSkip = false;
            if (gene.IsNonviolent() && overrideAllowNonviolent.HasValue)
            {
                overrideSkip |= overrideAllowNonviolent.Value;
            }
            if (gene == GeneDefOf.Inbred && overrideAllowInbred.HasValue)
            {
                overrideSkip |= overrideAllowInbred.Value;
            }
            if (!overrideSkip && !desperate && XenotypeRandomizerSettings.DisallowedGenes.Contains(gene))
            {
                skip = true;
            }

            return !skip;
        }

        private static bool IsPrerequisite(GeneDef gene)
        {
            bool isPrereq = false;
            foreach (GeneDef requiringGene in DefDatabase<GeneDef>.AllDefs)
            {
                if (requiringGene.prerequisite == gene)
                {
                    isPrereq = true;
                    break;
                }
            }
            return isPrereq;
        }

        private static int GetTotalComplexity(this List<GeneDef> genes) => genes.Sum(g => g.biostatCpx);

        private static int GetTotalMetabolicEfficiency(this List<GeneDef> genes) => genes.Sum(g => g.biostatMet);

        private static void RemoveRandomGenes(List<GeneDef> genes, int numberToRemove = -1, IntRange? metRange = null)
        {
            if (numberToRemove < 0)
            {
                numberToRemove = Rand.Range(Mathf.Max(genes.Count - 20, 0), genes.Count);
            }

            for (int i = 0; i < numberToRemove; i++)
            {
                GeneDef gene = genes.RandomElement();
                if (!IsPrerequisite(gene))
                {
                    if (!metRange.HasValue || (gene.biostatMet >= metRange.Value.min && gene.biostatMet <= metRange.Value.max))
                    {
                        genes.Remove(gene);
                    }
                }
            }
        }

        private static XenotypeIconDef GetSuitableIcon(List<GeneDef> genes)
        {
            XenotypeIconDef iconDef;
            if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Hair_LongOnly")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Crown");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Headbone_CenterHorn")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Horn");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Ears_Pig")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Ears");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Skin_Green")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Frown");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Body_Hulk")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Skull");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Head_Gaunt")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("SkullThin");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Ears_Cat")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Crescent");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Ears_Floppy")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Lop");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Beauty_VeryUgly")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Rect");
            }
            else if (genes.Contains(DefDatabase<GeneDef>.GetNamed("Furskin")))
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Furred");
            }
            else
            {
                iconDef = DefDatabase<XenotypeIconDef>.GetNamed("Basic");
            }
            return iconDef;
        }

        public static void Randomize(List<GeneDef> genes, ref XenotypeIconDef iconDef, bool? overrideAllowNonviolent = null, bool? overrideAllowInbred = null)
        {
            // clear the list of genes
            genes.Clear();

            List<GeneDef> genesToRemove;
            GeneDef previousGene = null;
            int positionInGroup = 1;

            foreach (GeneDef gene in DefaultGenePool.Where(g => XenotypeRandomizerSettings.MaxComplexity < 0 || g.biostatCpx <= XenotypeRandomizerSettings.MaxComplexity))
            {
                // keep track of the gene's position in conflicting groups of genes
                if (previousGene == null || !gene.ConflictsWith(previousGene))
                {
                    positionInGroup = 1;
                } 
                else
                {
                    positionInGroup++;
                }

                // determine if this gene is a prerequisite for another gene
                bool isPrereq = IsPrerequisite(gene);

                // calculate obscurity stat
                int obscurity = 
                    gene.biostatCpx + 
                    Mathf.Abs(gene.biostatMet) + 
                    positionInGroup * 2 + 
                    (gene.prerequisite != null ? 20 : 0) + 
                    (isPrereq ? 20 : 0);

                if (IsAllowed(gene, overrideAllowNonviolent, overrideAllowInbred))
                {
                    // chance to add gene is lower with higher obscurity
                    if (Random.Range(0, obscurity) == 0)
                    {
                        // add the gene if it isn't already included and any prerequisite is allowed
                        if (!genes.Contains(gene) && (gene.prerequisite == null || IsAllowed(gene.prerequisite, overrideAllowNonviolent, overrideAllowInbred)))
                        {
                            genes.Add(gene);

                            // add any prerequisite gene
                            if (gene.prerequisite != null)
                            {
                                if (!genes.Contains(gene.prerequisite))
                                {
                                    genes.Add(gene.prerequisite);
                                }
                            }

                            // remove any conflicting genes
                            genesToRemove = new List<GeneDef>();
                            foreach (GeneDef otherGene in genes)
                            {
                                if (gene != otherGene && gene.ConflictsWith(otherGene))
                                {
                                    genesToRemove.Add(otherGene);
                                }
                            }
                            foreach (GeneDef geneToRemove in genesToRemove)
                            {
                                genes.Remove(geneToRemove);
                            }
                        }
                    }
                }

                previousGene = gene;
            }

            // remove random genes so that the list isn't always so long
            RemoveRandomGenes(genes);

            int retries = 1000;

            // remove random genes until max complexity is satisfied
            while (retries-- > 0 && XenotypeRandomizerSettings.MaxComplexity >= 0 && genes.GetTotalComplexity() > XenotypeRandomizerSettings.MaxComplexity)
            {
                RemoveRandomGenes(genes);
            }

            // ensure minimum and maximum metabolic efficiency are met
            int totalMetabolicEfficiency = genes.GetTotalMetabolicEfficiency();
            while (retries > 0 && (totalMetabolicEfficiency < XenotypeRandomizerSettings.MetabolismRange.min || totalMetabolicEfficiency > XenotypeRandomizerSettings.MetabolismRange.max))
            {
                // remove genes at random until minimum is met
                while (retries-- > 0 && totalMetabolicEfficiency < XenotypeRandomizerSettings.MetabolismRange.min)
                {
                    RemoveRandomGenes(genes, 1, new IntRange(int.MinValue, -1));
                    totalMetabolicEfficiency = genes.GetTotalMetabolicEfficiency();
                }
                // remove genes at random until maximum is met
                while (retries-- > 0 && totalMetabolicEfficiency > XenotypeRandomizerSettings.MetabolismRange.max)
                {
                    RemoveRandomGenes(genes, 1, new IntRange(1, int.MaxValue));
                    totalMetabolicEfficiency = genes.GetTotalMetabolicEfficiency();
                }
            }

            if (retries <= 0)
            {
                genes.Clear();
            }

            // if xenotype is empty, desperately add one gene
            if (!genes.Any())
            {
                if (DefaultGenePool.Where(g => IsAllowed(g, overrideAllowNonviolent, overrideAllowInbred, true) && g.prerequisite == null).TryRandomElement(out GeneDef onlyGene))
                {
                    genes.Add(onlyGene);
                }
                else
                {
                    throw new System.Exception("Failed to create random xenotype.");
                }
            }

            // warn if not within parameters
            totalMetabolicEfficiency = genes.GetTotalMetabolicEfficiency();
            if (genes.Any(g => !IsAllowed(g, overrideAllowNonviolent, overrideAllowInbred)) || (XenotypeRandomizerSettings.MaxComplexity >= 0 && genes.GetTotalComplexity() > XenotypeRandomizerSettings.MaxComplexity) || totalMetabolicEfficiency < XenotypeRandomizerSettings.MetabolismRange.min || totalMetabolicEfficiency > XenotypeRandomizerSettings.MetabolismRange.max)
            {
                Messages.Message("XenotypeRandomizer_CantMakeXenotype".Translate(), MessageTypeDefOf.RejectInput, false);
            }

            // find a suitable icon
            iconDef = GetSuitableIcon(genes);
        }
    }
}
