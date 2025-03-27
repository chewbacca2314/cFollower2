﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.BotFramework;
using DreamPoeBot.Common;
using DreamPoeBot.Framework;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.NativeWrappers;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Message = DreamPoeBot.Loki.Bot.Message;

namespace cFollower
{
    public class PartyHandler : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            if (!LokiPoe.IsInGame)
            {
                //Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            var partyStatus = PartyHelper.GetPartyStatus();
            if (partyStatus == PartyStatus.PartyMember)
            {
                return false;
            }

            if (partyStatus == PartyStatus.None)
            {
                return true;
            }

            if (partyStatus == PartyStatus.PartyLeader) // set actual party leader if we're in party
            {
                var leader = cFollower.Leader.LeaderParty;
                Log.Debug($"[{Name}] Leader found: {leader != null}");
                if (leader != null)
                {
                    var ctxResult = LokiPoe.InGameState.PartyHud.OpenContextMenu(settings.LeaderName);
                    await Wait.SleepSafe(200, 300);
                    Log.Debug($"[{Name}] OpenContextMenu result: {ctxResult}");

                    if (LokiPoe.InGameState.ContextMenu.IsOpened)
                    {
                        var promoteResult = LokiPoe.InGameState.ContextMenu.PromoteToPartyLeader();
                        await Wait.SleepSafe(200, 300);
                        Log.Debug($"[{Name}] Promote result: {promoteResult}");

                        if (promoteResult == LokiPoe.InGameState.ContextMenuResult.None)
                            return false;
                    }
                }

                return true;
            }

            var invites = LokiPoe.InstanceInfo.PendingPartyInvites;
            if (invites.Count == 0)
            {
                return false;
            }

            var validInvite = invites.First(x => x.PartyMembers.Any(y => y.CharacterName == settings.LeaderName));

            if (validInvite != null)
            {
                Log.Debug($"[{Name}] Party invite found");

                if (!await Utility.OpenSocialPanel())
                {
                    Log.Debug($"[{Name}] Failed to open Social Panel");
                }
                await Wait.SleepSafe(200, 300);
                var partyAcceptResult = LokiPoe.InGameState.SocialUi.HandlePendingPartyInviteNew(settings.LeaderName);
                await Wait.SleepSafe(200, 300);
                Log.Debug($"[{Name}] Party accept result: {partyAcceptResult}");
                if (partyAcceptResult == LokiPoe.InGameState.HandlePendingPartyInviteResut.Accepted)
                {
                    await Coroutines.CloseBlockingWindows();
                    return false;
                }

                await Coroutines.CloseBlockingWindows();
            }
            else
            {
                Log.Debug($"[{Name}] Valid party invite not found");
            }

            await Wait.SleepSafe(200, 300);
            return true;
        }

        #region skip

        public string Name => "Party Handler";
        public string Description => "Handles party composition";
        public string Author => "chewbacca";
        public string Version => "0.0.0.1";
        private static cFollowerSettings settings = cFollowerSettings.Instance;

        public void Start()
        {
            Log.InfoFormat($"[{Name}] Task Loaded.");
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