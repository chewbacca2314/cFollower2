using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DreamPoeBot.BotFramework;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Implementation.Content.SkillBlacklist;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using log4net;
using MahApps.Metro.Controls;
using Message = DreamPoeBot.Loki.Bot.Message;
using UserControl = System.Windows.Controls.UserControl;

namespace cFollower.cMover
{
    public class cMover : IPlayerMover
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static Stopwatch sw = Stopwatch.StartNew();
        private static Skill moveSkillOnPanel;
        private static System.Windows.Forms.Keys moveSkillOnPanelKey;
        private static int moveSkillOnPanelSlot;

        public bool MoveTowards(Vector2i position, params dynamic[] user)
        {
            if (!LokiPoe.IsInGame)
            {
                Log.Info($"[{Name}] LokiPoe.IsInGame = {LokiPoe.IsInGame}");
                return false;
            }

            if (position == Vector2i.Zero)
            {
                Log.Error($"[{Name}.MoveTowards] Recieved [0, 0] as position, return false");
                return false;
            }

            var myPosition = LokiPoe.MyPosition;

            if (_cmd == null || // No command yet
                _cmd.Path == null ||
                _cmd.EndPoint != position || // Moving to a new position
                LokiPoe.CurrentWorldArea.IsTown || // In town, always generate new paths
                (sw.IsRunning && sw.ElapsedMilliseconds > cMoverSettings.Instance.PathRefreshRate) || // New paths on interval
                _cmd.Path.Count <= 2 || // Not enough points
                _cmd.Path.All(p => myPosition.Distance(p) > 7))
            // Try and find a better path to follow since we're off course
            {
                _cmd = new PathfindingCommand(myPosition, position, 3, cMoverSettings.Instance.AvoidWallHugging);
                if (!ExilePather.FindPath(ref _cmd))
                {
                    sw.Restart();
                    Log.ErrorFormat("[Alcor75PlayerMoverSettings.MoveTowards] ExilePather.FindPath failed from {0} to {1}.",
                        myPosition, position);
                    return false;
                }
                sw.Restart();
                // Signal 'FindPath_Result' tp PatherExplorer.
                //Utility.BroadcastMessage(null, "FindPath_Result", _cmd);
            }

            while (_cmd.Path.Count > 1)
            {
                if (ExilePather.PathDistance(_cmd.Path[0], myPosition) < settings.MoveRange)
                {
                    //Log.Debug($"[{Name}] Deleting first point");
                    _cmd.Path.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            //Log.Debug($"[{Name}] Move Range: {settings.MinMoveDistance}");
            var point = _cmd.Path[0];

            if (settings.RandomizeMove)
            {
                //Log.Debug($"[{Name}] Randomize movement enabled");
                point += new Vector2i(LokiPoe.Random.Next(-2, 3), LokiPoe.Random.Next(-2, 3));
                _cmd.Path[0] = ExilePather.FastWalkablePositionFor(point);
            }

            if (cMoverSettings.Instance.BlinkToggle)
            {
                var blinkSkill = LokiPoe.Me.GetSkillByName("blink");

                if (blinkSkill == null)
                {
                    Log.ErrorFormat("[cMover.BasicMove] Blink is null. Enable blink or toggle it off in settings");

                    BotManager.Stop();

                    return false;
                }

                Vector2i raycastHit = Vector2i.Zero;
                if (!blinkSkill.IsOnCooldown && myPosition.Distance(position) > cMoverSettings.Instance.BlinkDistance && !ExilePather.Raycast(myPosition, position, out raycastHit))
                {
                    Log.Debug($"[cMover.BasicMove] Using blink at {position}.");
                    MouseManager.SetMousePos("cMover.BasicMove.Blink", point);
                    LokiPoe.Input.SimulateKeyEvent(Keys.Space);

                    return true;
                }
            }

            return BasicMove(myPosition, point);
        }

        public override string ToString() => $"{Name}: {Description}";

        private static bool BasicMove(Vector2i myPosition, Vector2i point)
        {
            // Signal 'Next_Selected_Walk_Position' tp PatherExplorer.
            //Utility.BroadcastMessage(null, "Next_Selected_Walk_Position", point);

            var move = LokiPoe.InGameState.SkillBarHud.LastBoundMoveSkill;
            if (move == null)
            {
                Log.ErrorFormat("[cMover.BasicMove] Please assign the \"Move\" skill to your skillbar, do not use mouse button, use q,w,e,r,t!");

                BotManager.Stop();

                return false;
            }

            var pointDist = myPosition.Distance(point);

            // In this example we check the current state of the key assigned to the move skill.
            if ((LokiPoe.ProcessHookManager.GetKeyState(move.BoundKeys.Last()) & 0x8000) != 0 &&
                LokiPoe.Me.HasCurrentAction && LokiPoe.Me.CurrentAction.Skill != null && LokiPoe.Me.CurrentAction.Skill.InternalId.Equals("Move"))
            {
                // If the key is pressed, but next moving point is less that SingleUseDistance setted, we need to reset keys and press the move skill once.
                if (myPosition.Distance(point) <= cMoverSettings.Instance.SingleUseDistance)
                {
                    LokiPoe.ProcessHookManager.ClearAllKeyStates();
                    LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
                    Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
                }
                // Otherwise we just move the mouse to the next moving position, for a fast and smooth natural movement.
                else
                {
                    MouseManager.SetMousePos("Alcor75PlayerMoverSettings.MoveTowards", point, false);
                    Log.WarnFormat("[SetMousePos] {0}", point);
                }
            }
            else
            {
                // If the key was not pressed, we clear all keys state (we newer know what keys other component might have pressed but for sure we now just want to move!)
                // Then decide if to perform a single key press of to press the key and keep it pressed based on next position distance and configuration.
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                if (myPosition.Distance(point) <= cMoverSettings.Instance.SingleUseDistance)
                {
                    LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
                    Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
                }
                else
                {
                    LokiPoe.InGameState.SkillBarHud.BeginUseAt(move.Slots.Last(), false, point);
                    Log.WarnFormat("[BeginUseAt] {0}", point);
                }
            }

            return true;
        }

        public void ClearLMB()
        {
            for (int i = 1; i < 3; i++)
            {
                if (i == 1 && LokiPoe.InGameState.SkillBarHud.Slot(1).InternalName != "Melee")
                {
                    LokiPoe.InGameState.SkillBarHud.ClearSlot(i);
                    Log.Debug($"[{Name}] Clear slot {i}");
                }
                if (LokiPoe.InGameState.SkillBarHud.Slot(i) != null && LokiPoe.InGameState.SkillBarHud.Slot(i).InternalName == "Move")
                {
                    LokiPoe.InGameState.SkillBarHud.ClearSlot(i);
                    Log.Debug($"[{Name}] Clear slot {i}");
                }
            }

            Log.Debug($"[{Name}] LMB is clear");
        }

        #region skip

        public string Name => "cMover";
        public string Description => "Mover for cFollower";
        public string Author => "chewbacca";
        public string Version => "0.0.0.1";
        private cMoverGUI _gui;
        public UserControl Control => _gui ?? (_gui = new cMoverGUI());

        public JsonSettings Settings => cMoverSettings.Instance;
        public cMoverSettings settings = cMoverSettings.Instance;

        private PathfindingCommand _cmd;
        public PathfindingCommand CurrentCommand => _cmd;

        public void Initialize()
        {
            Log.Info($"Initializing {Name} - {Description} by {Author}, version {Version}");
        }

        public void Deinitialize()
        {
            Log.Info($"Deinitializing {Name} - {Description} by {Author}, version {Version}");
        }

        public void Start()
        {
            Log.Info($"[{Name}][Start] Loaded.");
            ClearLMB();
            moveSkillOnPanel = LokiPoe.InGameState.SkillBarHud.LastBoundMoveSkill;
            if (moveSkillOnPanel != null)
            {
                moveSkillOnPanelKey = moveSkillOnPanel.BoundKey;
                moveSkillOnPanelSlot = moveSkillOnPanel.Slot;
                Log.Debug($"[{Name}] Move skill found at \"{moveSkillOnPanelKey}\"");
            }
        }

        public void Stop()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return await Task.FromResult(LogicResult.Unprovided);
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public void Tick()
        {
        }

        #endregion skip
    }
}