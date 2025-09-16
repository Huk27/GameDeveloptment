using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace FarmStatistics
{

    /// <summary>
    /// 플레이어 정보를 UI에 바인딩하기 위한 ViewModel 클래스
    /// MatrixFishingUI의 FishMenuData.cs를 참고하여 작성
    /// </summary>
    public class PlayerInfoViewModel : INotifyPropertyChanged
    {
        // 테스트용 직접 프로퍼티 추가
        public string TestProperty => "테스트 성공!";
        
        // MatrixFishingUI 방식의 간단한 프로퍼티
        public string HeaderText { get; set; } = "플레이어 정보";

        // 탭 시스템
        public IReadOnlyList<TabData> Tabs { get; set; } = new List<TabData>();
        public string SelectedTab { get; set; } = "overview";
        
        // 탭별 표시 여부를 위한 boolean 프로퍼티들
        public bool ShowOverviewTab => SelectedTab == "overview";
        public bool ShowInventoryTab => SelectedTab == "inventory";
        public bool ShowSkillsTab => SelectedTab == "skills";
        public bool ShowSettingsTab => SelectedTab == "settings";
        
        // 인벤토리 관련 프로퍼티들
        public ParsedItemData[] InventoryItems { get; set; } = new ParsedItemData[0];
        public string InventoryHeaderText => $"인벤토리 ({InventoryItems.Length}개 아이템)";

        private string _playerName = "";
        private int _health = 0;
        private int _energy = 0;
        
        public string PlayerName 
        { 
            get => _playerName; 
            set => SetField(ref _playerName, value); 
        }
        
        public int Health 
        { 
            get => _health; 
            set 
            { 
                if (SetField(ref _health, value))
                {
                    OnPropertyChanged(nameof(HealthText));
                }
            } 
        }
        
        public int Energy 
        { 
            get => _energy; 
            set 
            { 
                if (SetField(ref _energy, value))
                {
                    OnPropertyChanged(nameof(EnergyText));
                }
            } 
        }
        
        // 문자열 변환 프로퍼티 추가
        public string HealthText => _health.ToString();
        public string EnergyText => _energy.ToString();

        /// <summary>
        /// 탭 활성화 처리 메서드
        /// StardewUI 공식 예제와 동일한 로직 사용
        /// </summary>
        public void OnTabActivated(string name)
        {
            SelectedTab = name;
            foreach (var tab in Tabs)
            {
                if (tab.Name != name)
                {
                    tab.Active = false;
                }
                // 공식 예제에서는 선택된 탭을 true로 설정하지 않음
                // StardewUI가 자동으로 처리하는 것으로 보임
            }
            OnPropertyChanged(nameof(SelectedTab));
            
            // 탭 표시 여부 프로퍼티들도 업데이트
            OnPropertyChanged(nameof(ShowOverviewTab));
            OnPropertyChanged(nameof(ShowInventoryTab));
            OnPropertyChanged(nameof(ShowSkillsTab));
            OnPropertyChanged(nameof(ShowSettingsTab));
            
            // 인벤토리 탭이 선택되면 아이템 목록 업데이트
            if (name == "inventory")
            {
                UpdateInventoryItems();
            }
        }

        /// <summary>
        /// 게임 데이터로부터 ViewModel을 생성하는 정적 메서드
        /// </summary>
        public static PlayerInfoViewModel LoadFromGameData()
        {
            var viewModel = new PlayerInfoViewModel();

            if (Game1.player != null)
            {
                viewModel.PlayerName = Game1.player.Name;
                viewModel.Health = Game1.player.health;
                viewModel.Energy = (int)Game1.player.Stamina;
                
                // 디버그 로그 추가
                System.Console.WriteLine($"[SimpleUI] ViewModel 생성: PlayerName={viewModel.PlayerName}, Health={viewModel.Health}, Energy={viewModel.Energy}");
                System.Console.WriteLine($"[SimpleUI] HealthText={viewModel.HealthText}, EnergyText={viewModel.EnergyText}");
            }
            else
            {
                viewModel.PlayerName = "N/A";
                viewModel.Health = 0;
                viewModel.Energy = 0;
                
                System.Console.WriteLine("[SimpleUI] Game1.player가 null입니다. 기본값 사용");
            }

            // 탭 데이터 초기화
            viewModel.InitializeTabs();

            return viewModel;
        }

        /// <summary>
        /// 탭 데이터를 초기화하는 메서드
        /// </summary>
        private void InitializeTabs()
        {
            var tabs = new List<TabData>();
            
            // 기본 스프라이트 사용 (게임 내 기본 아이콘들)
            var mouseCursors = Game1.mouseCursors;
            
            // 개요 탭 (하트 아이콘)
            tabs.Add(new TabData("overview", "개요", mouseCursors, new Rectangle(211, 428, 7, 6)));
            
            // 인벤토리 탭 (가방 아이콘)
            tabs.Add(new TabData("inventory", "인벤토리", mouseCursors, new Rectangle(0, 428, 10, 10)));
            
            // 스킬 탭 (별 아이콘)
            tabs.Add(new TabData("skills", "스킬", mouseCursors, new Rectangle(10, 428, 10, 10)));
            
            // 설정 탭 (톱니바퀴 아이콘)
            tabs.Add(new TabData("settings", "설정", mouseCursors, new Rectangle(60, 428, 10, 10)));
            
            Tabs = tabs;
            
            // 첫 번째 탭을 활성화
            if (Tabs.Count > 0)
            {
                OnTabActivated("overview");
            }
        }

        /// <summary>
        /// 인벤토리 아이템 목록을 업데이트하는 메서드
        /// </summary>
        private void UpdateInventoryItems()
        {
            if (Game1.player?.Items == null)
            {
                InventoryItems = new ParsedItemData[0];
                return;
            }

            var items = new List<ParsedItemData>();
            
            // 플레이어 인벤토리의 모든 아이템을 가져옴
            foreach (var item in Game1.player.Items)
            {
                if (item != null)
                {
                    // 아이템 데이터를 ParsedItemData로 변환
                    var itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
                    if (itemData != null)
                    {
                        items.Add(itemData);
                    }
                }
            }
            
            InventoryItems = items.ToArray();
            OnPropertyChanged(nameof(InventoryItems));
            OnPropertyChanged(nameof(InventoryHeaderText));
        }

        /// <summary>
        /// ViewModel 데이터를 업데이트하는 메서드
        /// </summary>
        public void UpdateData()
        {
            if (Game1.player != null)
            {
                PlayerName = Game1.player.Name;
                Health = Game1.player.health;
                Energy = (int)Game1.player.Stamina;
                
                // 인벤토리 탭이 활성화되어 있으면 아이템 목록도 업데이트
                if (SelectedTab == "inventory")
                {
                    UpdateInventoryItems();
                }
            }
        }

        #region Property Changes

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            System.Console.WriteLine($"[SimpleUI] PropertyChanged 이벤트 발생: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}