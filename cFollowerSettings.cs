using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;

namespace cFollower
{
    internal class cFollowerSettings : JsonSettings
    {
        private static cFollowerSettings _instance;
        public static cFollowerSettings Instance => _instance ?? (_instance = new cFollowerSettings());
        private cFollowerSettings() : base(GetSettingsFilePath(Configuration.Instance.Name, "cFollowerSettings.json"))
        {
            if (ItemFilterList == null)
            {
                ItemFilterList = SetupDefaultItems();
            }
                
        }
        private string _leaderName;
        private MoverHelper.MoveType _moveType;
        private int _followDistance;
        private bool _lootEnabled;
        private bool _depositEnabled;
        private bool _tradeEnabled;
        private int _distanceToLeaderLoot;
        private ObservableCollection<ItemFilter> _itemFilterList;
        private string _depositTabName;
        private int _stashDepositDelay;
        private int _guildStashDepositDelay;
        private int _tradeDepositDelay;
        private int _radiusLeaderLoot;
        private int _radiusPlayerLoot;
        private int _groundItemsRefreshRate;
        public static List<MoverHelper.MoveType> MoveTypeOptions = new List<MoverHelper.MoveType>
            {
                MoverHelper.MoveType.ToLeader,
                MoverHelper.MoveType.ToCursor
            };
        [DefaultValue(MoverHelper.MoveType.ToLeader)]
        public MoverHelper.MoveType FollowType
        {
            get { return _moveType; }
            set
            {
                if (value == _moveType) return;
                _moveType = value;
                NotifyPropertyChanged(() => FollowType);
            }
        }
        public ObservableCollection<ItemFilter> ItemFilterList
        {
            get => _itemFilterList;//?? (_defensiveSkills = new ObservableCollection<DefensiveSkillsClass>());
            set
            {
                _itemFilterList = value;
                NotifyPropertyChanged(() => ItemFilterList);
            }
        }
        [DefaultValue("tab1,tab2,tab3")]
        public string DepositTabNames
        {
            get { return _depositTabName; }
            set
            {
                if (value == _depositTabName) return;
                _depositTabName = value;
                NotifyPropertyChanged(() => DepositTabNames);
            }
        }
        [DefaultValue("leadername")]
        public string LeaderName
        {
            get { return _leaderName; }
            set
            {
                if (value == _leaderName) return;
                _leaderName = value;
                NotifyPropertyChanged(() => LeaderName);
            }
        }

        [DefaultValue(true)]
        public bool LootEnabled
        {
            get { return _lootEnabled; }
            set
            {
                if (value == _lootEnabled) return;
                _lootEnabled = value;
                NotifyPropertyChanged(() => LootEnabled);
            }
        }
        [DefaultValue(true)]
        public bool DepositEnabled
        {
            get { return _depositEnabled; }
            set
            {
                if (value == _depositEnabled) return;
                _depositEnabled = value;
                NotifyPropertyChanged(() => DepositEnabled);
            }
        }
        [DefaultValue(true)]
        public bool TradeEnabled
        {
            get { return _tradeEnabled; }
            set
            {
                if (value == _tradeEnabled) return;
                _tradeEnabled = value;
                NotifyPropertyChanged(() => TradeEnabled);
            }
        }
        [DefaultValue(35)]
        public int FollowDistance
        {
            get { return _followDistance; }
            set
            {
                if (value == _followDistance) return;
                _followDistance = value;
                NotifyPropertyChanged(() => FollowDistance);
            }
        }
        [DefaultValue(55)]
        public int DistanceToLeaderLoot
        {
            get { return _distanceToLeaderLoot; }
            set
            {
                if (value == _distanceToLeaderLoot) return;
                _distanceToLeaderLoot = value;
                NotifyPropertyChanged(() => DistanceToLeaderLoot);
            }
        }
        [DefaultValue(50)]
        public int StashDepositDelay
        {
            get { return _stashDepositDelay; }
            set
            {
                if (value == _stashDepositDelay) return;
                _stashDepositDelay = value;
                NotifyPropertyChanged(() => StashDepositDelay);
            }
        }
        [DefaultValue(200)]
        public int GuildStashDepositDelay
        {
            get { return _guildStashDepositDelay; }
            set
            {
                if (value == _guildStashDepositDelay) return;
                _guildStashDepositDelay = value;
                NotifyPropertyChanged(() => GuildStashDepositDelay);
            }
        }
        [DefaultValue(50)]
        public int TradeDepositDelay
        {
            get { return _tradeDepositDelay; }
            set
            {
                if (value == _tradeDepositDelay) return;
                _tradeDepositDelay = value;
                NotifyPropertyChanged(() => TradeDepositDelay);
            }
        }

        [DefaultValue(40)]
        public int RadiusLeaderLoot
        {
            get { return _radiusLeaderLoot; }
            set
            {
                if (value == _radiusLeaderLoot) return;
                _radiusLeaderLoot = value;
                NotifyPropertyChanged(() => RadiusLeaderLoot);
            }
        }

        [DefaultValue(40)]
        public int RadiusPlayerLoot
        {
            get { return _radiusPlayerLoot; }
            set
            {
                if (value == _radiusPlayerLoot) return;
                _radiusPlayerLoot = value;
                NotifyPropertyChanged(() => RadiusPlayerLoot);
            }
        }
        [DefaultValue(80)]
        public int GroundItemsRefreshRate
        {
            get { return _groundItemsRefreshRate; }
            set
            {
                if (value == _groundItemsRefreshRate) return;
                _groundItemsRefreshRate = value;
                NotifyPropertyChanged(() => GroundItemsRefreshRate);
            }
        }

        private ObservableCollection<ItemFilter> SetupDefaultItems()
        {
            ObservableCollection<ItemFilter> items = new ObservableCollection<ItemFilter>();

            items.Add(new ItemFilter(true, "Headhunter", "Art/2DItems/Belts/Headhunter"));
            items.Add(new ItemFilter(true, "Mageblood", "Art/2DItems/Belts/InjectorBelt"));
            items.Add(new ItemFilter(true, "Mirror", "Art/2DItems/Currency/CurrencyDuplicate"));
            return items;
        }


    }
}
