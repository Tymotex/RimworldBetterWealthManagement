using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.Noise;

namespace BetterWealthManagement
{
    [StaticConstructorOnStartup]
    public class MyMod
    {
        static MyMod()
        {
            Log.Message("Hello world! Now supporting v1.5.");
            
        }
    }

    public abstract class WindowWithTable : MainTabWindow
    {
        private PawnTable table;

        protected virtual float ExtraBottomSpace => 53f;

        protected virtual float ExtraTopSpace => 0f;

        protected abstract PawnTableDef PawnTableDef { get; }

        protected override float Margin => 6f;

        // Sets the actual window UI's dimensions.
        public override Vector2 RequestedTabSize
        {
            get
            {
                if (table == null)
                {
                    return Vector2.zero;
                }
                return new Vector2(table.Size.x + Margin * 2f, table.Size.y + ExtraBottomSpace + ExtraTopSpace + Margin * 2f);
            }
        }

        protected virtual IEnumerable<Pawn> Pawns => Find.CurrentMap.mapPawns.FreeColonists;

        public override void PostOpen()
        {
            base.PostOpen();
            if (table == null)
            {
                table = CreateTable();
            }
            SetDirty();
        }

        public override void DoWindowContents(Rect rect)
        {
            table.PawnTableOnGUI(new Vector2(rect.x, rect.y + ExtraTopSpace));
        }

        public void Notify_PawnsChanged()
        {
            SetDirty();
        }

        public override void Notify_ResolutionChanged()
        {
            table = CreateTable();
            base.Notify_ResolutionChanged();
        }

        private PawnTable CreateTable()
        {
            return (PawnTable)Activator.CreateInstance(PawnTableDef.workerClass, PawnTableDef, (Func<IEnumerable<Pawn>>)(() => Pawns), UI.screenWidth - (int)(Margin * 2f), (int)((float)(UI.screenHeight - 35) - ExtraBottomSpace - ExtraTopSpace - Margin * 2f));
        }

        protected void SetDirty()
        {
            table.SetDirty();
            SetInitialSizeAndPosition();
        }
    }

    [StaticConstructorOnStartup]
    public class WealthWindow : WindowWithTable
    {
        private List<Thing> tmpThings = new List<Thing>();

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.Wildlife;

        protected override IEnumerable<Pawn> Pawns
        {
            get
            {
                Log.Message("Ran Find function.");
                float wealthItems = CalculateWealthItems();
                Log.Message("CalculateWealthItems: ");
                Log.Message(wealthItems);
                return Find.CurrentMap.mapPawns.AllPawns.Where((Pawn p) => true);
            }
        }
        private float CalculateWealthItems()
        {
            tmpThings.Clear();
            Map map = Find.CurrentMap;
            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, allowUnreal: false, WealthWatcher.WealthItemsFilter);
            float num = 0f;
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (tmpThings[i].SpawnedOrAnyParentSpawned && !tmpThings[i].PositionHeld.Fogged(map))
                {
                    float currThingValue = tmpThings[i].MarketValue * (float)tmpThings[i].stackCount;
                    num += currThingValue;
                    Log.Message(String.Format("Counting this thing: {0}, which has value: ${1}.", tmpThings[i].def.label, currThingValue));
                }
            }

            tmpThings.Clear();
            return num;
        }
    }
}

