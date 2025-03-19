using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Components;
using DreamPoeBot.Loki.FilesInMemory;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.NativeWrappers;
using DreamPoeBot.Loki.Game.Objects;
using log4net;

namespace cFollower
{
    public class ZoneHelper
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public static bool IsLeaderInHideout()
        {
            var leader = Utility.GetLeaderPartyMember();
            if (leader != null)
            {
                return leader.Area.IsHideoutArea;
            }

            return false;
        }

        public static bool IsLeaderOnMap()
        {
            var leader = Utility.GetLeaderPartyMember();
            if (leader != null)
            {
                return leader.Area.IsMap;
            }

            return false;
        }

        public static bool IsLeaderInCombatArea()
        {
            var leader = Utility.GetLeaderPartyMember();
            if (leader != null)
            {
                return leader.Area.IsCombatArea;
            }

            return false;
        }

        public static bool IsLeaderInLab()
        {
            var leader = Utility.GetLeaderPartyMember();
            if (leader != null)
            {
                return leader.Area.IsLabyrinthArea;
            }

            return false;
        }

        public static bool IsLeaderInTown()
        {
            var leader = Utility.GetLeaderPartyMember();

            if (leader != null)
            {
                return leader.Area.IsTown;
            }

            return false;
        }

        public static bool IsInSameZoneWithLeader()
        {
            var leader = Utility.GetLeaderPartyMember();
            if (leader != null)
            {
                return LokiPoe.InGameState.PartyHud.IsInSameZone(leader.Name);
            }

            return false;
        }

        public static async Task<bool> InteractWithNearestTransition()
        {
            var myPos = LokiPoe.Me.Position;
            var areaTransition = LokiPoe.ObjectManager.GetObjectsByType<DreamPoeBot.Loki.Game.Objects.AreaTransition>()
                    .OrderBy(x => x.Position.Distance(myPos))
                    .FirstOrDefault(x => x.IsTargetable && ExilePather.PathExistsBetween(myPos, ExilePather.FastWalkablePositionFor(x.Position, 20)));

            var interactionResult = await Coroutines.InteractWith(areaTransition);
            Log.Debug($"[InteractWithNearestTransition] Interacting with area transition {areaTransition?.Name} at {areaTransition?.Position}. Succesful?: {interactionResult}");
            return interactionResult;
        }
    }
}