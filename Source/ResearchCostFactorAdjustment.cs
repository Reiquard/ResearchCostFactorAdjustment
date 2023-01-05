using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ResearchCostFactorAdjustment
{
    [StaticConstructorOnStartup]
    internal static class ResearchCostFactorAdjustment
    {
        public static bool TechAdvancingIsActive { get; private set; }
        static ResearchCostFactorAdjustment()
        {
            new Harmony("reiquard.researchcostfactoradjustment").PatchAll();
            if (ModLister.GetActiveModWithIdentifier("GHXX.TechAdvancing") != null)
            {
                TechAdvancingIsActive = true;
            }
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef))]
    [HarmonyPatch("CostFactor")]
    static class CostFactorRedefining
    {
        private static SimpleCurve _curvePoints = new SimpleCurve
        {
            new CurvePoint(-20f, 0.4f),
            new CurvePoint(-4f, 0.6f),
            new CurvePoint(0f, 1f)
        };
        [HarmonyAfter(new string[] { "com.ghxx.rimworld.techadvancing" })]
        static void Postfix(TechLevel researcherTechLevel, ResearchProjectDef __instance, ref float __result)
        {
            float techLevel = (float)Mathf.Min((int)__instance.techLevel, (int)RCFA_Mod.settings.threshold);
            float dif = techLevel - (float)researcherTechLevel;
            float difInv = (float)__instance.techLevel - (float)researcherTechLevel;
            float result = 1f;
            if (dif > 0)
            {
                if (RCFA_Mod.settings.exponential)
                {
                    result = (float)Math.Round((1f + RCFA_Mod.settings.factor) * (Math.Pow(dif * 1.442f, dif * 0.5f)), 0);
                }
                else
                {
                    result = (float)Math.Round(1f + dif * RCFA_Mod.settings.factor, 1);
                }
            }
            else if (difInv < 0 && RCFA_Mod.settings.ratioInversion)
            {
                result = (float)Math.Round(_curvePoints.Evaluate(difInv * (0.5f + RCFA_Mod.settings.factor)), 2);
            }
            __result = result;
        }
    }
}