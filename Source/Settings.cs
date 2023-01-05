using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResearchCostFactorAdjustment
{
    public class RCFA_Settings : ModSettings
    {
        public TechLevel threshold = TechLevel.Ultra;
        public float factor = 0.5f;
        public bool exponential = false;
        public bool ratioInversion = false;
        public bool overrideTA = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref threshold, "threshold", TechLevel.Ultra);
            Scribe_Values.Look(ref factor, "factor", 0.5f);
            Scribe_Values.Look(ref exponential, "exponential", false);
            Scribe_Values.Look(ref ratioInversion, "ratioInversion", false);
        }
    }

    class RCFA_Mod : Mod
    {
        public static RCFA_Settings settings;
        public RCFA_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RCFA_Settings>();
        }

        public override string SettingsCategory() => "Research CostFactor Adjustment";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float columnHeight = 164f;
            Listing_Standard ls = new Listing_Standard();

            // Left column
            Rect leftColumn = new Rect(inRect.x, inRect.y, inRect.width / 2f, columnHeight);
            ls.Begin(leftColumn.ContractedBy(5f));
            Listing_Standard ls_left = ls.BeginSection(columnHeight - 18f);
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls_left.Label("RqRCFA_Label_TechLevelThreshold".Translate(), tooltip: "RqRCFA_Label_TechLevelThresholdDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            for (int i = 2; i < Enum.GetValues(typeof(TechLevel)).Length - 1; i++)
            {
                TechLevel current = (TechLevel)i;
                if (ls_left.RadioButton($"   {current.ToStringHuman().CapitalizeFirst()}", settings.threshold == current))
                {
                    settings.threshold = current;
                }
            }
            ls.EndSection(ls_left);
            ls.End();
            // Right column
            Rect rightColumn = new Rect(inRect.width / 2f, inRect.y, inRect.width / 2f, columnHeight);
            ls.Begin(rightColumn.ContractedBy(5f));
            Listing_Standard ls_right = ls.BeginSection(columnHeight - 18f);
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls_right.Label($"{"RqRCFA_Label_TechLevelFactor".Translate()} {settings.factor}", tooltip: "RqRCFA_Label_TechLevelFactorDesc".Translate());
            //settings.factor = Widgets.HorizontalSlider(ls_right.GetRect(22f), settings.factor, 0.5f, 5f, roundTo: 0.1f);
            settings.factor = Widgets.HorizontalSlider_NewTemp(ls_right.GetRect(22f), settings.factor, 0.5f, 5f, roundTo: 0.1f);
            ls_right.Label("RqRCFA_Label_CostIncrease".Translate(), tooltip: "RqRCFA_Label_CostIncreaseDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            if (ls_right.RadioButton($"   {"RqRCFA_RB_Linear".Translate()}", !settings.exponential))
                settings.exponential = false;
            if (ls_right.RadioButton($"   {"RqRCFA_RB_Exponential".Translate()}", settings.exponential))
                settings.exponential = true;
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls_right.Gap(4f);
            ls_right.CheckboxLabeled("RqRCFA_RatioInversion".Translate(), ref settings.ratioInversion, "RqRCFA_RatioInversionDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            ls.EndSection(ls_right);
            ls.End();
            // Table rect
            Rect table = new Rect(inRect.x, columnHeight + 20f, inRect.width, inRect.height - columnHeight - 50f);
            DrawTable(table);
            // Buttons rect
            Rect bottom = new Rect(inRect.x, inRect.height, inRect.width, 30f);
            if (ResearchCostFactorAdjustment.TechAdvancingIsActive)
            {
                ls.Begin(bottom.LeftPartPixels(2 * bottom.width / 3 - 8f));
                Color guiColor = GUI.color;
                GUI.color = Color.Lerp(Color.yellow, Color.grey, 0.7f);
                Text.CurFontStyle.fontStyle = FontStyle.Italic;
                ls.Label("RqRCFA_OverrideTechAdvancing".Translate(), tooltip: "RqRCFA_OverrideTechAdvancingDesc".Translate());
                GUI.color = guiColor;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                ls.End();
            }
            if (Widgets.ButtonText(bottom.RightPartPixels(bottom.width / 3), "RqRCFA_Button_GameDefault".Translate()))
            {
                settings.factor = 0.5f;
                settings.threshold = TechLevel.Industrial;
                settings.exponential = false;
                settings.ratioInversion = false;
            }
            settings.Write();
        }
        private void DrawTable(Rect rect)
        {
            ResearchProjectDef researchProjectDef = new ResearchProjectDef();
            researchProjectDef.baseCost = 1000f;
            Color guiColor = GUI.color;
            TextAnchor textAnchor = Text.Anchor;
            GUI.BeginGroup(rect);
            Rect tableRect = new Rect(30f, 65f, rect.width - 40f, rect.height - 90f);

            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect vertLabel = new Rect(tableRect.xMin - Text.LineHeight, tableRect.yMax, tableRect.height / 1.2f, Text.LineHeight * 0.95f);
            UI.RotateAroundPivot(-90.01f, vertLabel.position);
            Widgets.Label(vertLabel, "RqRCFA_Label_TechLevelResearcher".Translate());
            UI.RotateAroundPivot(90.01f, vertLabel.position);

            Text.Anchor = TextAnchor.MiddleRight;
            Rect horLabel = new Rect(tableRect.x + tableRect.width / 6f, tableRect.y - Text.LineHeight, tableRect.width / 1.2f, Text.LineHeight);
            Widgets.Label(horLabel, "RqRCFA_Label_TechLevelProject".Translate());

            Rect tableDesc = new Rect(tableRect.x, tableRect.yMax, tableRect.width, Text.LineHeight);
            Widgets.Label(tableDesc, "RqRCFA_Label_TableDesc".Translate());

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.BeginGroup(tableRect);
            Widgets.DrawBox(new Rect(0f, 0f, tableRect.width, tableRect.height));
            Widgets.DrawLine(new Vector2(0f, 0f), new Vector2(tableRect.width / 6f, tableRect.height / 6f), Color.grey, 0.5f);
            string label;
            for (int i = 0; i < 6; i++)
            {
                Widgets.DrawLineVertical(tableRect.width * i / 6f, 0f, tableRect.height);
                Widgets.DrawLineHorizontal(0f, tableRect.height * i / 6f, tableRect.width);
            }
            GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            for (int i = 1; i < 6; i++)
            {
                label = $"{((TechLevel)(i + 1)).ToStringHuman().CapitalizeFirst().Truncate(tableRect.width / 6f - 4f)}";
                Widgets.Label(new Rect(tableRect.width * i / 6f, (tableRect.height / 6f - Text.LineHeight) / 2f, tableRect.width / 6f, Text.LineHeight), label);
                Widgets.Label(new Rect(0f, tableRect.height * i / 6f + (tableRect.height / 6f - Text.LineHeight) / 2f, tableRect.width / 6f, Text.LineHeight), label);
            }
            for (int i = 1; i < 6; i++)
            {
                researchProjectDef.techLevel = (TechLevel)(i + 1);
                for (int j = 1; j < 6; j++)
                {
                    var value = (researchProjectDef.baseCost * researchProjectDef.CostFactor((TechLevel)(j + 1)));
                    if (value == researchProjectDef.baseCost)
                        GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    else if (value > researchProjectDef.baseCost)
                    {
                        float c = 1 / researchProjectDef.CostFactor((TechLevel)(j + 1));
                        GUI.color = Color.Lerp(Color.red, Color.white, c);
                    }
                    else
                    {
                        float c = 1.1f - researchProjectDef.CostFactor((TechLevel)(j + 1));
                        GUI.color = Color.Lerp(Color.white, Color.green, c);
                    }
                    Widgets.Label(new Rect(tableRect.width * i / 6f, tableRect.height * j / 6f + (tableRect.height / 6f - Text.LineHeight) / 2f, tableRect.width / 6f, Text.LineHeight), value.ToString());
                }
            }
            Text.Anchor = textAnchor;
            GUI.color = guiColor;
            GUI.EndGroup();
            GUI.EndGroup();
        }
    }
}