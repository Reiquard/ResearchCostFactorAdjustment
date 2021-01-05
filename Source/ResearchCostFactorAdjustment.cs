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
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("reiquard.researchcostfactoradjustment").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef))]
    [HarmonyPatch("CostFactor")]
    static class CostFactorRedefining
    {
        [HarmonyPrefix]
        static bool CostFactor_Prefix(TechLevel researcherTechLevel, ResearchProjectDef __instance, ref float __result)
        {
            TechLevel techLevel = (TechLevel)Mathf.Min((int)__instance.techLevel, (int)RCFA_Mod.settings.threshold);
            if ((int)researcherTechLevel >= (int)techLevel)
            {
                __result = 1f;
                return false;
            }
            int num = techLevel - researcherTechLevel;
            __result = (float)Math.Round(1f + (float)num * (RCFA_Mod.settings.exponential ? (float)num : 1f) * RCFA_Mod.settings.factor, 1);
            return false;
        }
    }
}