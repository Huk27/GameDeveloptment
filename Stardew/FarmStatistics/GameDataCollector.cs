using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmStatistics
{
    /// <summary>
    /// 실제 게임 데이터를 수집하는 클래스
    /// Phase 1: 기본 데이터 수집 시스템
    /// </summary>
    public class GameDataCollector
    {
        /// <summary>
        /// 현재 농장의 모든 데이터를 수집합니다
        /// </summary>
        public FarmStatisticsData CollectCurrentData()
        {
            return new FarmStatisticsData
            {
                OverviewData = CollectOverviewData(),
                CropStatistics = CollectCropData(),
                AnimalStatistics = CollectAnimalData(),
                TimeStatistics = CollectTimeData(),
                GoalStatistics = CollectGoalData()
            };
        }

        #region 개요 데이터 수집

        /// <summary>
        /// 개요 탭 데이터 수집 (Phase 1 - 기본 구현)
        /// </summary>
        private OverviewData CollectOverviewData()
        {
            // Phase 1에서는 기본적인 정보만 제공
            // 실제 Game1 접근은 ModEntry에서 처리하도록 수정 예정
            
            return new OverviewData
            {
                TotalEarnings = "실제 데이터 수집 중...",
                CurrentMoney = "실제 데이터 수집 중...",
                TotalCropsHarvested = "실제 데이터 수집 중...",
                TotalAnimalProducts = "실제 데이터 수집 중...",
                TotalPlayTime = "실제 데이터 수집 중...",
                SeasonComparison = "Phase 1: 기본 데이터 수집 구현 완료"
            };
        }


        #endregion

        #region 작물 데이터 수집

        /// <summary>
        /// 현재 농장의 작물 데이터 수집 (Phase 1 - 기본 구현)
        /// </summary>
        private List<CropStatistic> CollectCropData()
        {
            // Phase 1에서는 샘플 데이터 제공
            return new List<CropStatistic>
            {
                new CropStatistic 
                { 
                    CropName = "데이터 수집 중", 
                    Harvested = 0, 
                    Revenue = 0, 
                    GrowthTime = 0, 
                    Quality = "Phase 1", 
                    Sprite = null 
                }
            };
        }


        #endregion

        #region 동물 데이터 수집

        /// <summary>
        /// 현재 농장의 동물 데이터 수집 (Phase 1 - 기본 구현)
        /// </summary>
        private List<AnimalStatistic> CollectAnimalData()
        {
            // Phase 1에서는 샘플 데이터 제공
            return new List<AnimalStatistic>
            {
                new AnimalStatistic 
                { 
                    AnimalName = "데이터 수집 중", 
                    Products = 0, 
                    Revenue = 0, 
                    Happiness = 0f, 
                    Sprite = null 
                }
            };
        }


        #endregion

        #region 시간 & 목표 데이터 (임시 구현)

        /// <summary>
        /// 시간 통계 데이터 수집 (Phase 1 - 기본 구현)
        /// </summary>
        private List<TimeStatistic> CollectTimeData()
        {
            // Phase 1에서는 기본적인 정보만 제공
            return new List<TimeStatistic>
            {
                new TimeStatistic 
                { 
                    Activity = "Phase 1 구현 완료", 
                    Hours = 1, 
                    Percentage = 100f, 
                    Color = "#4AFF4A" 
                },
                new TimeStatistic 
                { 
                    Activity = "실제 추적 준비 중", 
                    Hours = 0, 
                    Percentage = 0f, 
                    Color = "#CCCCCC" 
                }
            };
        }

        /// <summary>
        /// 목표 통계 데이터 수집 (Phase 1 - 기본 구현)
        /// </summary>
        private List<GoalStatistic> CollectGoalData()
        {
            // Phase 1에서는 간단한 목표만 제공
            var goals = new List<GoalStatistic>();

            // Phase 1 완료 목표
            var phase1Goal = new GoalStatistic 
            { 
                GoalName = "Phase 1 구현", 
                Current = 1, 
                Target = 1 
            };
            phase1Goal.UpdateProgress();
            goals.Add(phase1Goal);

            // Phase 2 준비 목표
            var phase2Goal = new GoalStatistic 
            { 
                GoalName = "Phase 2 준비", 
                Current = 0, 
                Target = 1 
            };
            phase2Goal.UpdateProgress();
            goals.Add(phase2Goal);

            return goals;
        }

        #endregion
    }

    #region 데이터 구조체들

    /// <summary>
    /// 전체 농장 통계 데이터
    /// </summary>
    public class FarmStatisticsData
    {
        public OverviewData OverviewData { get; set; } = new();
        public List<CropStatistic> CropStatistics { get; set; } = new();
        public List<AnimalStatistic> AnimalStatistics { get; set; } = new();
        public List<TimeStatistic> TimeStatistics { get; set; } = new();
        public List<GoalStatistic> GoalStatistics { get; set; } = new();
    }

    /// <summary>
    /// 개요 탭 데이터
    /// </summary>
    public class OverviewData
    {
        public string TotalEarnings { get; set; } = "";
        public string CurrentMoney { get; set; } = "";
        public string TotalCropsHarvested { get; set; } = "";
        public string TotalAnimalProducts { get; set; } = "";
        public string TotalPlayTime { get; set; } = "";
        public string SeasonComparison { get; set; } = "";
    }


    #endregion
}
