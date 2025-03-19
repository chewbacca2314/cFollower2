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

            Player leader = Utility.GetLeaderPlayer();
            if (leader == null)
            {
                Log.Debug($"[{Name}] Leader not found. Do nothing");
                return false;
            }

            if (!ZoneHelper.IsInSameZoneWithLeader())
            {
                return false;
            }

            float distanceToLeader = leader.Distance;
            if (distanceToLeader < cFollowerSettings.Instance.MinDistanceToFollow)
            {
                Log.Debug($"[{Name}] Standing. Distance to leader: {(distanceToLeader)}");
                return false;
            }

            var leaderPosition = leader.Position;
            var fastWalkable = ExilePather.FastWalkablePositionFor(leaderPosition);
            var myPos = LokiPoe.Me.Position;

            if (ExilePather.PathDistance(myPos, fastWalkable) < 45)
                LokiPoe.InGameState.SkillBarHud.UseAt(4, false, fastWalkable, false);
            else
                PlayerMoverManager.Current.MoveTowards(fastWalkable);

            await Wait.SleepSafe(50, 70);
            return true;
        }

        #region skip

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
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