﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using Message = DreamPoeBot.Loki.Bot.Message;
using log4net;
using DreamPoeBot.Loki.Game.GameData;

namespace cFollower
{
    public class ZoneHandler : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Stopwatch zoneSW = Stopwatch.StartNew();
        private Vector2i lastSeenLeaderPos;
        private bool blockedTransition = false;
        private List<TriggerableBlockage> arenaBlockages;
        private AreaTransition areaTransition = null;

        public async Task<bool> Run()
        {
            if (!LokiPoe.IsInGame)
            {
                //Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            if (ZoneHelper.IsInSameZoneWithLeader())
            {
                var leader = Utility.GetLeaderPlayer();
                var leaderPos = leader.Position;
                var myPos = LokiPoe.Me.Position;
                var currentWorldArea = LokiPoe.CurrentWorldArea.Name;
                //Utility.TransitionCheckAreas.ContainsKey(currentWorldArea)
                if (!blockedTransition && LokiPoe.LocalData.MapMods[StatTypeGGG.MapContainsMapBoss] == 1)
                {
                    var blockages = LokiPoe.ObjectManager.GetObjectsByType<TriggerableBlockage>().Where(x => Utility.TransitionCheckAreas.Contains(x.Metadata)).ToList();
                    if (blockages.Count() > 0)
                    {
                        arenaBlockages = blockages;
                        foreach (var blockage in blockages)
                        {
                            Log.Debug($"[{Name}] Found arena transition, blocking it");
                            Utility.AddObstacle(blockage);
                            blockedTransition = true;
                        }
                    }
                }

                var fastWalkable = ExilePather.FastWalkablePositionFor(leaderPos);
                if (ExilePather.PathExistsBetween(myPos, fastWalkable))
                    lastSeenLeaderPos = fastWalkable;
                else
                {
                    if (lastSeenLeaderPos != Vector2i.Zero)
                    {
                        while (lastSeenLeaderPos.Distance(LokiPoe.Me.Position) > 60)
                        {
                            Log.Debug($"[{Name}] No path to leader. Moving to last seen pos at {lastSeenLeaderPos}.");
                            PlayerMoverManager.Current.MoveTowards(lastSeenLeaderPos);
                            await Wait.SleepSafe(50, 70);
                        }

                        if (areaTransition == null)
                        {
                            areaTransition = LokiPoe.ObjectManager.GetObjectsByType<AreaTransition>()
                                .OrderBy(x => x.Position.Distance(LokiPoe.Me.Position))
                                .FirstOrDefault(x => ExilePather.PathExistsBetween(myPos, ExilePather.FastWalkablePositionFor(x.Position, 20)));
                        }

                        var interactionResult = await Coroutines.InteractWith(areaTransition);
                        Log.Debug($"[{Name}] Interacting with area transition {areaTransition.Name} at {areaTransition.Position}. Succesful?: {interactionResult}");
                        await Wait.SleepSafe(100, 200);

                        if (arenaBlockages?.Any() == true)
                        {
                            foreach (var x in arenaBlockages)
                            {
                                Log.Debug($"[{Name}] We transitioned to arena. Now unblocking it at {x.Position}");
                                Utility.RemoveObstacle(x);
                            }
                        }
                    }
                }
                //Log.Debug($"[{Name}] We're in same zone with leader. Returning false");
                return false;
            }
            else
            {
                if (ZoneHelper.IsLeaderOnMap())
                {
                    Log.Debug($"[{Name}] Leader on map. Trying to find portals");
                    var portals = LokiPoe.ObjectManager.InTownPortals.OrderBy(x => x.Distance).ToList();
                    var portalsCount = portals.Count;
                    Log.Debug($"[{Name}] Leader on map. Found {portalsCount} portals");

                    if (portals.Count > 0)
                    {
                        await Coroutines.InteractWith(portals.First());
                        await Wait.LatencySleep();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (ZoneHelper.IsLeaderInLab() && (LokiPoe.CurrentWorldArea.IsLabyrinthArea || LokiPoe.CurrentWorldArea.Name == "Aspirants' Plaza")) // add interaction with lab trial later on
                    {
                        Log.Debug($"[{Name}] Leader in lab. Trying to find transition");
                        var leaderArea = Utility.GetLeaderPartyMember().Area;
                        var leaderTransition = LokiPoe.ObjectManager.AreaTransition(leaderArea.Name);
                        if (leaderTransition != null)
                        {
                            Log.Debug($"[{Name}] Leader in lab. Transition found at {leaderTransition.Position}");
                            await Coroutines.InteractWith(leaderTransition);
                            await Wait.LatencySleep();
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!ZoneHelper.IsLeaderInLab() && (ZoneHelper.IsLeaderInCombatArea() || ZoneHelper.IsLeaderInHideout() || ZoneHelper.IsLeaderInTown()))
                        {
                            Log.Debug($"[{Name}] Leader is in combat area/hideout. Swirling");
                            LokiPoe.InGameState.PartyHud.FastGoToZone(Utility.GetLeaderPartyMember().Name);
                            await Wait.SleepSafe(1000, 1100);
                        }
                    }
                }
            }
            return true;
        }

        #region skip

        public string Name => "Zone Handler";
        public string Description => "Class to handle zone transition/swirl";
        public string Author => "chewbacca";
        public string Version => "0.0.0.1";

        public void Start()
        {
            Log.Info($"[{Name}] Task Loaded.");
        }

        public void Stop()
        {
        }

        public void Tick()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Processed;
        }

        #endregion skip
    }
}