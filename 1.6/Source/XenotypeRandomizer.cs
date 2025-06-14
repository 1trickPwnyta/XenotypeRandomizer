using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace XenotypeRandomizer
{
    public static class XenotypeRandomizer
    {
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

        private static int GetTotalMetabolicEfficiency(List<GeneDef> genes)
        {
            int totalMetabolicEfficiency = 0;
            foreach (GeneDef gene in genes)
            {
                totalMetabolicEfficiency += gene.biostatMet;
            }
            return totalMetabolicEfficiency;
        }

        private static void RemoveRandomGenes(List<GeneDef> genes, IntRange metRange)
        {
            int numberToRemove = Rand.Range(0, genes.Count);

            for (int i = 0; i < numberToRemove; i++)
            {
                GeneDef gene = genes.RandomElement();
                if (!IsPrerequisite(gene))
                {
                    if (gene.biostatMet >= metRange.min && gene.biostatMet <= metRange.max)
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

        public static void Randomize(List<GeneDef> genes, ref XenotypeIconDef iconDef, bool allowNonviolent = true, bool allowInbred = false)
        {
            // clear the list of genes
            genes.Clear();

            List<GeneDef> genesToRemove;
            GeneDef previousGene = null;
            int positionInGroup = 1;

            foreach (GeneDef gene in GeneUtility.GenesInOrder.Where(d => d.biostatArc <= 0))
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
                    positionInGroup*2 + 
                    (gene.prerequisite != null? 20 : 0) + 
                    (isPrereq? 20: 0);

                if ((allowNonviolent || (gene.disabledWorkTags & WorkTags.Violent) == WorkTags.None) && (allowInbred || gene != GeneDefOf.Inbred))
                {
                    // chance to add gene is lower with higher obscurity
                    if (Random.Range(0, obscurity) == 0)
                    {
                        // add the gene if it isn't already included
                        if (!genes.Contains(gene))
                        {
                            genes.Add(gene);
                        }

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

                previousGene = gene;
            }

            // remove random genes so that the list isn't always so long
            RemoveRandomGenes(genes, new IntRange(int.MinValue, int.MaxValue));

            // ensure minimum and maximum metabolic efficiency are met
            int totalMetabolicEfficiency = GetTotalMetabolicEfficiency(genes);
            while (totalMetabolicEfficiency < -5 || totalMetabolicEfficiency > 5)
            {
                // remove genes at random until minimum is met
                while (totalMetabolicEfficiency < -5)
                {
                    RemoveRandomGenes(genes, new IntRange(int.MinValue, -1));
                    totalMetabolicEfficiency = GetTotalMetabolicEfficiency(genes);
                }
                // remove genes at random until maximum is met
                while (totalMetabolicEfficiency > 5)
                {
                    RemoveRandomGenes(genes, new IntRange(1, int.MaxValue));
                    totalMetabolicEfficiency = GetTotalMetabolicEfficiency(genes);
                }
            }

            // find a suitable icon
            iconDef = GetSuitableIcon(genes);
        }
    }
}
