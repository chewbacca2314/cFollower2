using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Controllers;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Elements;
using DreamPoeBot.Loki.Game;
using Message = DreamPoeBot.Loki.Bot.Message;
using DreamPoeBot.Loki.Game.Objects;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using log4net;
using System.Security.Principal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DreamPoeBot.BotFramework;
using DreamPoeBot.Common;

namespace cFollower
{
    public class LootTask : ITask
    {
        private float distanceToLeader;
        private Player leader;
        private Stopwatch lootSw = Stopwatch.StartNew();
        private HashSet<int> trashItems = new HashSet<int>();
        private HashSet<WorldItem> lootTable = new HashSet<WorldItem>();

        public async Task<bool> Run()
        {
            if (!cFollowerSettings.Instance.LootEnabled)
                return false;

            if (!LokiPoe.IsInGame)
            {
                //Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            leader = cFollower.Leader.LeaderPlayer;
            //if (leader.Distance > cFollowerSettings.Instance.DistanceToLeaderLoot)
            //{
            //    Log.Debug($"[{Name}] {leader.Distance} > {cFollowerSettings.Instance.DistanceToLeaderLoot}");
            //    return false;
            //}
            if (lootSw.IsRunning && lootSw.ElapsedMilliseconds > cFollowerSettings.Instance.GroundItemsRefreshRate)
            {
                lootTable = GetNearbyItems();
                lootSw.Restart();
            }

            Log.Debug($"[{Name}] Found {lootTable.Count} items to loot");
            if (lootTable.Count == 0)
            {
                return false;
            }

            var invControl = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;
            foreach (var item in lootTable)
            {
                if (leader.Distance > cFollowerSettings.Instance.DistanceToLeaderLoot)
                {
                    //Log.Debug($"[{Name}] {leader.Distance} > {cFollowerSettings.Instance.DistanceToLeaderLoot}");
                    return false;
                }

                var itemPos = item.Position;
                if (LokiPoe.MyPosition.Distance(itemPos) > cFollowerSettings.Instance.RadiusPlayerLoot
                    || leader.Position.Distance(itemPos) > cFollowerSettings.Instance.RadiusLeaderLoot)
                {
                    continue;
                }

                if (!item.HasVisibleHighlightLabel)
                {
                    continue;
                }

                if (invControl.Inventory.CanFitItem(item.Item))
                {
                    if (item.IsHighlightable && await Coroutines.InteractWith(item))
                    {
                        Log.Debug($"[{Name}] Item {item.Name} looted");
                    }
                };
            }

            return false;
        }

        public HashSet<WorldItem> GetNearbyItems()
        {
            //var metadataObjects = LokiPoe.ObjectManager.GetObjectsByMetadata("123");
            var worldItems = LokiPoe.ObjectManager.Objects.OfType<WorldItem>();
            HashSet<WorldItem> resultWorldItems = new HashSet<WorldItem>();

            foreach (var wi in worldItems)
            {
                if (wi == null
                    || !wi.IsValid
                    || trashItems.Contains(wi.Id)
                    || !wi.HasVisibleHighlightLabel)
                {
                    continue;
                }

                if (wi.IsAllocatedToOther
                    || !IsItemToLoot(wi))
                {
                    trashItems.Add(wi.Id);
                    continue;
                }

                var wiPos = wi.Position;

                if (LokiPoe.MyPosition.Distance(wiPos) > cFollowerSettings.Instance.RadiusPlayerLoot
                    || leader.Position.Distance(wiPos) > cFollowerSettings.Instance.RadiusLeaderLoot)
                {
                    continue;
                }

                resultWorldItems.Add(wi);
            }

            return resultWorldItems;
        }

        public bool IsItemToLoot(WorldItem wi)
        {
            if (wi == null)
            {
                return false;
            }

            if (wi.Item.Rarity != DreamPoeBot.Loki.Game.GameData.Rarity.Unique)
            {
                return true;
            }

            string renderArt = wi.Item.RenderArt;

            return cFollowerSettings.Instance.ItemFilterList.Any(x => renderArt.ToLower().Contains(x.RenderItem.ToLower()));
        }

        #region skip

        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public string Name => "Loot task";
        public string Description => "Task to handle looting";
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

        #endregion
    }
}