using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;

namespace cFollower.cMover
{
    public class cMoverSettings : JsonSettings
    {
        private static cMoverSettings _instance;
        public static cMoverSettings Instance => _instance ?? (_instance = new cMoverSettings());

        private cMoverSettings() : base(GetSettingsFilePath(Configuration.Instance.Name, "cMoverSettings.json"))
        {
        }

        private int _pathRefreshRate;
        private int _moveRange;
        private bool _randomizeMove;
        private bool _avoidWallHugging;
        private int _singleUseDistance;

        [DefaultValue(true)]
        public bool RandomizeMove
        {
            get { return _randomizeMove; }
            set
            {
                if (value == _randomizeMove) return;
                _randomizeMove = value;
                NotifyPropertyChanged(() => RandomizeMove);
            }
        }

        [DefaultValue(true)]
        public bool AvoidWallHugging
        {
            get { return _avoidWallHugging; }
            set
            {
                if (value == _avoidWallHugging) return;
                _avoidWallHugging = value;
                NotifyPropertyChanged(() => AvoidWallHugging);
            }
        }

        [DefaultValue(30)]
        public int PathRefreshRate
        {
            get { return _pathRefreshRate; }
            set
            {
                if (value == _pathRefreshRate) return;
                _pathRefreshRate = value;
                NotifyPropertyChanged(() => PathRefreshRate);
            }
        }

        [DefaultValue(35)]
        public int MoveRange
        {
            get { return _moveRange; }
            set
            {
                if (value == _moveRange) return;
                _moveRange = value;
                NotifyPropertyChanged(() => MoveRange);
            }
        }

        [DefaultValue(18)]
        public int SingleUseDistance
        {
            get { return _singleUseDistance; }
            set
            {
                if (value == _singleUseDistance) return;
                _singleUseDistance = value;
                NotifyPropertyChanged(() => SingleUseDistance);
            }
        }
    }
}