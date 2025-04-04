﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using cFollower.cMover;
using DreamPoeBot.Loki.Bot.Pathfinding;
using Message = DreamPoeBot.Loki.Bot.Message;
using DreamPoeBot.Loki.FilesInMemory;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Game.GameData;

namespace cFollower
{
    public class FollowTask : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            if (!LokiPoe.IsInGame)
            {
                //Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            Player leader = cFollower.Leader.LeaderPlayer;
            if (leader == null)
            {
                Log.Debug($"[{Name}] Leader not found. Do nothing");
                return false;
            }

            float distanceToLeader = leader.Distance;

            if (distanceToLeader < cFollowerSettings.Instance.FollowDistance && !leader.IsMoving && leader?.CurrentAction?.Skill?.InternalName != "TempestFlurry")
            {
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                Log.Debug($"[{Name}] Standing. Distance to leader: {(distanceToLeader)}");
                return false;
            }

            // Join arena if map has boss
            if (LokiPoe.LocalData.MapMods.ContainsKey(StatTypeGGG.MapContainsMapBoss) && LokiPoe.LocalData.MapMods[StatTypeGGG.MapContainsMapBoss] == 1)
            {
                if (LokiPoe.ObjectManager.GetObjectsByType<AreaTransition>().Any(x => x.IsTargetable))
                {
                    await ZoneHelper.InteractWithNearestTransition();
                }
            }

            var leaderPosition = leader.Position;
            Vector2i fastWalkable = ExilePather.FastWalkablePositionFor(leaderPosition);
            Vector2i lastSeenLeaderPos = Vector2i.Zero;
            var myPos = LokiPoe.Me.Position;

            // If path exists -> move, else find transition
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

                    await ZoneHelper.InteractWithNearestTransition();
                    await Wait.SleepSafe(100, 200);
                }
            }

            if (cFollowerSettings.Instance.FollowType == MoverHelper.MoveType.ToCursor)
            {
                Vector2i leaderDest = Vector2i.Zero;

                if (leader?.CurrentMoveAction != null)
                {
                    leaderDest = leader.CurrentMoveAction.Destination;
                }

                if (leader?.CurrentAction?.Skill?.InternalId == "tempest_flurry")
                {
                    leaderDest = leader.CurrentAction.Destination;
                }

                if (leaderDest != Vector2i.Zero)
                {
                    var _fastWalkable = ExilePather.FastWalkablePositionFor(leaderDest);
                    if (_fastWalkable != Vector2i.Zero)
                        fastWalkable = ExilePather.FastWalkablePositionFor(_fastWalkable);
                }
            }
            
            PlayerMoverManager.Current.MoveTowards(fastWalkable);

            await Wait.SleepSafe(20, 40);
            return true;
        }

        #region skip

        public async Task<LogicResult> Logic(Logic logic)
        {
            return await Task.FromResult(LogicResult.Unprovided);
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Processed;
        }

        public string Name => "Follow Task";
        public string Description => "Task to follow leader";
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

        #endregion skip
    }
}