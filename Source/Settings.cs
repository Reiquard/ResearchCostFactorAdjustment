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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref threshold, "threshold", TechLevel.Ultra);
            Scribe_Values.Look(ref factor, "factor", 0.5f);
            Scribe_Values.Look(ref exponential, "exponential", false);
        }
    }

    class RCFA_Mod : Mod
    {
        public static RCFA_Settings settings;
        private static ResearchProjectDef researchProjectDef = new ResearchProjectDef();
        TextAnchor textAnchor;

        public RCFA_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RCFA_Settings>();
        }

        public override string SettingsCategory() => "Research CostFactor Adjustment";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            textAnchor = Text.Anchor;
            researchProjectDef.baseCost = 1000f;

            Rect leftColumn = new Rect(inRect.x, inRect.y, inRect.width / 2f, inRect.height / 2.6f);
            ls.Begin(leftColumn.ContractedBy(10f));
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls.Label("RqRCFA_Label_TechLevelThreshold".Translate(), tooltip: "RqRCFA_Label_TechLevelThresholdDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            for (int i = 2; i < Enum.GetValues(typeof(TechLevel)).Length - 1; i++)
            {
                TechLevel current = (TechLevel)i;
                if (ls.RadioButton_NewTemp($"   {current.ToStringHuman().CapitalizeFirst()}", settings.threshold == current))
                {
                    settings.threshold = current;
                }
            }
            ls.Gap();
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls.Label($"{"RqRCFA_Label_TechLevelFactor".Translate()} {settings.factor}", tooltip: "RqRCFA_Label_TechLevelFactorDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            settings.factor = Widgets.HorizontalSlider(ls.GetRect(22f), settings.factor, 0.5f, 5f, roundTo: 0.1f);
            ls.End();

            Rect rightColumn = new Rect(inRect.width / 2f, inRect.y, inRect.width / 2f, inRect.height / 2.7f);
            ls.Begin(rightColumn.ContractedBy(10f));
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls.Label("RqRCFA_Label_CostIncrease".Translate(), tooltip: "RqRCFA_Label_CostIncreaseDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            if (ls.RadioButton_NewTemp($"   {"RqRCFA_RB_Linear".Translate()}", !settings.exponential))
                settings.exponential = false;
            if (ls.RadioButton_NewTemp($"   {"RqRCFA_RB_Exponential".Translate()}", settings.exponential))
                settings.exponential = true;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.CurFontStyle.fontStyle = FontStyle.Italic;
            ls.Label(settings.exponential ? "R = B * (1 + D * D * F)" : "R = B * (1 + D * F)", tooltip: "RqRCFA_Label_CostFactorExplanationDesc".Translate());
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            Text.Anchor = textAnchor;
            Text.Font = GameFont.Small;
            ls.End();

            Rect bottomButtons = new Rect(inRect.width / 2f, 170f, inRect.width / 2f, 90f);
            ls.Begin(bottomButtons.ContractedBy(10f));
            if (ls.ButtonText("RqRCFA_Button_GameDefault".Translate()))
            {
                settings.factor = 0.5f;
                settings.threshold = TechLevel.Industrial;
                settings.exponential = false;
            }
            if (ls.ButtonText("RqRCFA_Button_ModsDefault".Translate()))
            {
                settings.factor = 0.5f;
                settings.threshold = TechLevel.Ultra;
                settings.exponential = false;
            }
            ls.End();

            Rect bottom = new Rect(inRect.x, inRect.height / 2.8f, inRect.width, inRect.height / 1.5f);
            DrawTable(bottom);
            settings.Write();
        }
        private void DrawTable(Rect rect)
        {
            Color guiColor = GUI.color;
            GUI.BeginGroup(rect);
            Rect tableRect = new Rect(30f, 75f, rect.width - 40f, rect.height - 100f);

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = Color.gray;
            Rect tableDesc = new Rect(tableRect.x, tableRect.yMax, tableRect.width, Text.LineHeight);
            Widgets.Label(tableDesc, "RqRCFA_Label_TableDesc".Translate());

            GUI.color = Color.grey;
            Text.Font = GameFont.Medium;
            Rect vertLabel = new Rect(tableRect.xMin - Text.LineHeight, tableRect.yMax, tableRect.height / 1.2f, Text.LineHeight * 0.95f);
            UI.RotateAroundPivot(-90, vertLabel.position);
            Widgets.Label(vertLabel, "RqRCFA_Label_TechLevelResearcher".Translate());
            UI.RotateAroundPivot(90, vertLabel.position);

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.grey;
            Rect horLabel = new Rect(tableRect.x + tableRect.width / 6f, tableRect.y - Text.LineHeight, tableRect.width / 1.2f, Text.LineHeight);
            Widgets.Label(horLabel, "RqRCFA_Label_TechLevelProject".Translate());
            Text.Font = GameFont.Small;
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
                label = $"{((TechLevel)(i + 1)).ToStringHuman().CapitalizeFirst()}";
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
                        GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    else
                    {
                        float c = 1f - researchProjectDef.CostFactor((TechLevel)6) / (researchProjectDef.CostFactor((TechLevel)(j + 1)));
                        GUI.color = Color.Lerp(Color.white, Color.red, c);
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