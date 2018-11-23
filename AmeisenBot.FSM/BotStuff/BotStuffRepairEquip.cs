﻿using AmeisenBot.Character;
using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM.Actions;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using AmeisenBotUtilities.Enums;
using AmeisenBotUtilities.Objects;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenBotFSM.BotStuff
{
    public class BotStuffRepairEquip : ActionMoving
    {
        private const int MAMMOTH_SPELL = 61425;

        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }
        private AmeisenCharacterManager AmeisenCharacterManager { get; set; }
        public bool IGotTheDamnMammoth => AmeisenCore.IsSpellKnown(MAMMOTH_SPELL);

        private Me Me => AmeisenDataHolder.Me;
        private Unit Target => AmeisenDataHolder.Target;

        public BotStuffRepairEquip(
            AmeisenDataHolder ameisenDataHolder, 
            AmeisenDBManager ameisenDBManager,
            AmeisenCharacterManager ameisenCharacterManager)
            : base(ameisenDataHolder, ameisenDBManager)
        {
            AmeisenDBManager = ameisenDBManager;
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenCharacterManager = ameisenCharacterManager;
        }

        public override void Start()
        {
            base.Start();
            FindClosestRepairNpc();
        }

        private void FindClosestRepairNpc()
        {
            if (!IGotTheDamnMammoth)
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Searching for Units to repair equipment...", this);
                List<RememberedUnit> possibleUnits = AmeisenDBManager.GetRememberedUnits(UnitTrait.REPAIR);
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Found {possibleUnits.Count} Units to repair at", this);

                RememberedUnit closestUnit = null;

                double lastDistance = 100000;
                foreach (RememberedUnit unit in possibleUnits)
                {
                    double currentDistance = Utils.GetDistance(Me.pos, unit.Position);
                    if (currentDistance < lastDistance)
                    {
                        closestUnit = unit;
                        lastDistance = currentDistance;
                    }
                }

                if (closestUnit != null)
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Unit to repair equip found: {closestUnit.Name}", this);
                    GoToUnitAndRepair(closestUnit);
                }
                else
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, "No Unit to repair equip found...", this);
                }
            }
            else
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "I got the mammoth to repair...", this);

                if (AmeisenCore.IsOutdoors)
                {
                    AmeisenCore.CastSpellById(MAMMOTH_SPELL);
                }

                Thread.Sleep(2000);
                RepairEquipmentAtUnit(null, true);
            }
        }

        private void GoToUnitAndRepair(RememberedUnit closestUnit)
        {
            if (!WaypointQueue.Contains(closestUnit.Position))
            {
                WaypointQueue.Enqueue(closestUnit.Position);
            }
        }

        private void RepairEquipmentAtUnit(RememberedUnit unit, bool mammoth = false)
        {
            if (mammoth)
            {
                AmeisenCore.TargetNpcByName("Gnimo");
            }
            else
            {
                AmeisenCore.TargetNpcByName(unit.Name);
            }

            Target.Update();
            AmeisenCore.InteractWithGUID(Target.pos, Target.Guid, InteractionType.INTERACT);
            Thread.Sleep(500);
            AmeisenCore.RepairAllItems();
            AmeisenCore.SellAllGrayItems();
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Repaired all items and sold all gray stuff", this);
        }
    }
}