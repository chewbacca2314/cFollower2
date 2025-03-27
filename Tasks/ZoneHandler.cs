using System.Diagnostics;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Message = DreamPoeBot.Loki.Bot.Message;

namespace cFollower
{
    public class ZoneHandler : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            if (!LokiPoe.IsInGame)
            {
                //Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            if (cFollower.Leader.LeaderPlayer != null)
            {
                //Log.Debug($"[{Name}] We're in same zone with leader. Returning false");
                return false;
            }

            var leader = cFollower.Leader.LeaderParty;

            if (ZoneHelper.IsLeaderOnMap(leader))
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
                if (ZoneHelper.IsLeaderInLab(leader) && (LokiPoe.CurrentWorldArea.IsLabyrinthArea || LokiPoe.CurrentWorldArea.Name == "Aspirants' Plaza")) // add interaction with lab trial later on
                {
                    Log.Debug($"[{Name}] Leader in lab. Trying to find transition");
                    var leaderArea = cFollower.Leader.LeaderParty.Area;
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
                    if (!ZoneHelper.IsLeaderInLab(leader) && (ZoneHelper.IsLeaderInCombatArea(leader) || ZoneHelper.IsLeaderInHideout(leader) || ZoneHelper.IsLeaderInTown(leader)))
                    {
                        Log.Debug($"[{Name}] Leader is in combat area/hideout. Swirling");
                        LokiPoe.InGameState.PartyHud.FastGoToZone(cFollower.Leader.LeaderParty.Name);
                        await Wait.SleepSafe(1000, 1100);
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
            return await Task.FromResult(LogicResult.Unprovided);
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Processed;
        }

        #endregion skip
    }
}