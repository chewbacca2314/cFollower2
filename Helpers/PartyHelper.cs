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
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.NativeWrappers;
using DreamPoeBot.Loki.Game.Objects;
using log4net;

namespace cFollower
{
    public static class PartyHelper
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public static PartyStatus GetPartyStatus()
        {
            if (!LokiPoe.InstanceInfo.PartyMembers.Any())
            {
                return PartyStatus.None;
            }
            else
            {
                if (LokiPoe.InstanceInfo.PartyLeaderName == LokiPoe.Me.Name)
                {
                    return PartyStatus.PartyLeader;
                }
                return PartyStatus.PartyMember;
            }
        }
    }
}