namespace Visvang.Core
{
    public static class Constants
    {
        // Fishing States
        public const float MAX_LINE_TENSION = 100f;
        public const float MIN_LINE_TENSION = 0f;
        public const float LINE_SNAP_THRESHOLD = 95f;
        public const float HOOK_SET_WINDOW = 1.5f;

        // Slime System
        public const float MAX_SLIME_METER = 100f;
        public const float SLIME_PER_MUDFISH = 25f;
        public const float SLIME_WASH_TIME = 1f;
        public const float SLIME_REEL_PENALTY = 0.4f;

        // Grip System
        public const float MAX_GRIP_STRENGTH = 100f;
        public const float GRIP_DECAY_RATE = 5f;
        public const float GRIP_RECOVERY_RATE = 15f;

        // XP Rewards
        public const int XP_CARP_BASE = 50;
        public const int XP_BARBEL_BASE = 200;
        public const int XP_MUDFISH_BASE = 10;
        public const int XP_KURPER_BASE = 20;
        public const int XP_TILAPIA_BASE = 25;
        public const int XP_BASS_BASE = 40;
        public const int XP_GRASKARP_BASE = 60;
        public const int XP_YELLOWFISH_BASE = 80;
        public const int XP_EEL_BASE = 100;
        public const int XP_LEGENDARY_MULTIPLIER = 10;

        // Chaos Events
        public const float CHAOS_CHECK_INTERVAL = 30f;
        public const float CHAOS_BASE_CHANCE = 0.15f;
        public const float BARBEL_AMBUSH_NIGHT_CHANCE = 0.3f;
        public const float MUDFISH_SWARM_DURATION = 60f;
        public const int MUDFISH_STREAK_PENALTY_THRESHOLD = 3;

        // Cast System
        public const float MAX_CAST_POWER = 100f;
        public const float MAX_CAST_DISTANCE = 120f;

        // Fight Timing
        public const float BARBEL_DEATH_ROLL_DURATION = 4f;
        public const float BARBEL_UI_DISORIENT_DURATION = 3f;
        public const float QTE_TAP_WINDOW = 0.5f;
        public const float QTE_GRIP_DURATION = 3f;

        // Pap System
        public const float PAP_BUCKET_MAX = 100f;
        public const float PAP_PER_CAST = 5f;
        public const float PAP_BARBEL_DESTROY_AMOUNT = 30f;
        public const float PAP_MUDFISH_RUIN_AMOUNT = 10f;

        // Multiplayer
        public const float BARBER_BATTLE_DURATION = 300f;
        public const float MUDFISH_MADNESS_DURATION = 300f;

        // Player Level
        public const int MAX_PLAYER_LEVEL = 50;
        public const int BASE_XP_PER_LEVEL = 100;
        public const float XP_LEVEL_SCALING = 1.5f;
    }
}
