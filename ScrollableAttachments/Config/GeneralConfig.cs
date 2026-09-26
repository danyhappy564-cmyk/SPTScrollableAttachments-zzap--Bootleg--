using AttachmentScrolling.Attributes;
using AttachmentScrolling.Components;
using BepInEx.Configuration;
using UnityEngine;

namespace AttachmentScrolling.Config;

public enum BetterCompareMode
{
    // 켜둔 기준 중 하나도 나빠지지 않고, 최소 하나는 좋아져야 표시
    NoneWorse,
    // 켜둔 기준 중 하나라도 좋아지면 표시 (다른 게 나빠져도)
    AnyBetter
}

public static class GeneralConfig
{
    private const string ScrollSection = "1. 스크롤";
    private const string HighlightSection = "2. 더 좋은 부착물 표시";

    public static ConfigEntry<bool> ScrollEnabled { get; set; }
    public static ConfigEntry<float> ScrollSpeed { get; set; }
    public static ConfigEntry<int> GridColumns { get; set; }
    public static ConfigEntry<float> ViewHeight { get; set; }

    public static ConfigEntry<bool> HighlightEnabled { get; set; }
    public static ConfigEntry<bool> CompareErgonomics { get; set; }
    public static ConfigEntry<bool> CompareRecoil { get; set; }
    public static ConfigEntry<bool> CompareAccuracy { get; set; }
    public static ConfigEntry<bool> CompareWeight { get; set; }
    public static ConfigEntry<BetterCompareMode> CompareMode { get; set; }
    public static ConfigEntry<Color> BorderColor { get; set; }
    public static ConfigEntry<float> BorderThickness { get; set; }

    public static void Initialize(ConfigFile config)
    {
        ScrollEnabled = config.Bind(ScrollSection, "스크롤 기능 사용", true, new ConfigDescription(
            "끄면 부착물 목록이 원래 게임 방식(스크롤 없이 전부 펼쳐짐)으로 돌아갑니다",
            null,
            new ConfigurationManagerAttributes() {Order = 1000}
        ));

        ScrollSpeed = config.Bind(ScrollSection, "스크롤 속도", 32f, new ConfigDescription(
            "부착물 목록의 스크롤 속도",
            new AcceptableValueRange<float>(0f, 100f),
            new ConfigurationManagerAttributes() {Order = 990}
        ));

        GridColumns = config.Bind(ScrollSection, "목록 열 개수", 6, new ConfigDescription(
            "부착물 목록에 한 줄에 보여줄 칸 수",
            new AcceptableValueRange<int>(1, 16),
            new ConfigurationManagerAttributes() {Order = 980}
        ));

        ViewHeight = config.Bind(ScrollSection, "목록 높이", 420f, new ConfigDescription(
            "부착물 목록 창의 높이",
            new AcceptableValueRange<float>(0f, 1080f),
            new ConfigurationManagerAttributes() {Order = 970}
        ));

        HighlightEnabled = config.Bind(HighlightSection, "더 좋은 부착물 테두리 표시", true, new ConfigDescription(
            "지금 달려 있는 부착물보다 좋은 부착물에 테두리를 칠합니다 (빈 슬롯이면 '아무것도 안 단 상태'와 비교)",
            null,
            new ConfigurationManagerAttributes() {Order = 900}
        ));

        CompareErgonomics = config.Bind(HighlightSection, "비교: 에르고(인체공학)", true, new ConfigDescription(
            "에르고가 높을수록 좋은 것으로 봅니다",
            null,
            new ConfigurationManagerAttributes() {Order = 890}
        ));

        CompareRecoil = config.Bind(HighlightSection, "비교: 반동", true, new ConfigDescription(
            "반동 수치가 낮을수록(마이너스일수록) 좋은 것으로 봅니다",
            null,
            new ConfigurationManagerAttributes() {Order = 880}
        ));

        CompareAccuracy = config.Bind(HighlightSection, "비교: 정확도", false, new ConfigDescription(
            "정확도 수치가 높을수록 좋은 것으로 봅니다",
            null,
            new ConfigurationManagerAttributes() {Order = 870}
        ));

        CompareWeight = config.Bind(HighlightSection, "비교: 무게", false, new ConfigDescription(
            "부착물 자체 무게가 가벼울수록 좋은 것으로 봅니다 (그 위에 달린 하위 부착물 무게는 제외)",
            null,
            new ConfigurationManagerAttributes() {Order = 860}
        ));

        CompareMode = config.Bind(HighlightSection, "판정 방식", BetterCompareMode.NoneWorse, new ConfigDescription(
            "NoneWorse: 켜둔 기준이 하나도 나빠지지 않고 하나 이상 좋아질 때만 표시\n" +
            "AnyBetter: 켜둔 기준 중 하나라도 좋아지면 표시",
            null,
            new ConfigurationManagerAttributes() {Order = 850}
        ));

        BorderColor = config.Bind(HighlightSection, "테두리 색", new Color(1f, 0.85f, 0f, 1f), new ConfigDescription(
            "테두리 색 (기본: 노란색)",
            null,
            new ConfigurationManagerAttributes() {Order = 840}
        ));

        BorderThickness = config.Bind(HighlightSection, "테두리 두께", 2f, new ConfigDescription(
            "테두리 두께 (픽셀)",
            new AcceptableValueRange<float>(1f, 6f),
            new ConfigurationManagerAttributes() {Order = 830}
        ));

        ScrollEnabled.SettingChanged += (_, _) => AttachmentScrollComponent.ForEach(c => c.SetScrollEnabled(ScrollEnabled.Value));
        ScrollSpeed.SettingChanged += (_, _) => AttachmentScrollComponent.ForEach(c => c.SetScrollSpeed(ScrollSpeed.Value));
        GridColumns.SettingChanged += (_, _) => AttachmentScrollComponent.ForEach(c => c.SetGridColumns(GridColumns.Value));
        ViewHeight.SettingChanged += (_, _) => AttachmentScrollComponent.ForEach(c => c.SetViewHeight(ViewHeight.Value));

        HighlightEnabled.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        CompareErgonomics.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        CompareRecoil.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        CompareAccuracy.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        CompareWeight.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        CompareMode.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        BorderColor.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
        BorderThickness.SettingChanged += (_, _) => BetterAttachmentHighlighter.RefreshOpenMenu();
    }
}
