namespace Visvang.Core
{
    public enum GamePhase
    {
        MainMenu,
        DamSelection,
        GearSetup,
        Fishing,
        FishFight,
        FishCaught,
        FishLost,
        ChaosEvent,
        Results,
        Paused
    }

    public enum FishingState
    {
        Idle,
        PreparingBait,
        Casting,
        Waiting,
        BiteDetected,
        SettingHook,
        Fighting,
        Landing,
        Caught,
        Lost,
        RodPulledIn
    }

    public enum TimeOfDay
    {
        EarlyMorning,
        Morning,
        Midday,
        Afternoon,
        Evening,
        Night
    }

    public enum Weather
    {
        Clear,
        Cloudy,
        Rainy,
        Windy,
        Stormy
    }

    public enum FishSpecies
    {
        // Carp
        CommonCarp,
        MirrorCarp,
        LeatherCarp,
        GhostCarp,
        WildSmallCarp,
        BoknesGoldenCarp,

        // Barbel / Catfish
        Barbel,
        FlatNoseRiverBarber,

        // Mudfish
        Mudfish,

        // Other
        Kurper,
        Tilapia,
        Bass,
        Graskarp,
        Yellowfish,
        Eel
    }

    public enum FishRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum DipCategory
    {
        BarbelAttractor,
        MudfishAttractor,
        MudfishRepellent,
        CarpSpecialist,
        AllRounder
    }

    public enum RodTier
    {
        Entry,
        Mid,
        High,
        Legendary
    }

    public enum MultiplayerMode
    {
        None,
        BarberBattle,
        MudfishMadness,
        CarpOnlyLeague
    }

    public enum QTEType
    {
        Tap,
        Hold,
        GripStrength,
        DirectionalSwipe,
        SlimeWash
    }
}
