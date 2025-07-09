using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static class Enemy
    {
        
    }

    public static class Boss
    {
        // BabyTurtle spike
        public const float SPIKE_DELAY_NOTBERSERKED = 0.5f;
        public const float SPIKE_DELAY_BERSERKED = 0.3f;
        public const int SPAWN_SPIKE_1 = 24;
        public const int SPAWN_SPIKE_2 = 21;
        public const int SPAWN_SPIKE_3 = 18;
        public const float SPAWN_SPIKE_RADIUS_1 = 8f;
        public const float SPAWN_SPIKE_RADIUS_2 = 6f;
        public const float SPAWN_SPIKE_RADIUS_3 = 4f;
        
        // BabyTurtle Leaf
        public const float SPAWN_LEAF_RADIUS = 3f;
        public const float SPAWN_LEAF_DELAY_NOTBERSERKED = 0.2f;
        public const float SPAWN_LEAF_DELAY_BERSERKED = 0.1f;
        public const float LEAF_SHAKE_DURATION = 0.2f;
        public const float LEAF_SHAKE_MAGNITUDE = 0.1f;
        
        // BabyTurtle Tentacle
        public const float SPAWN_TENTACLE_DISTANCE = 3f;
        public const float TENTACLE_SPEED_NOTBERSERKED = 1.0f;
        public const float TENTACLE_SPEED_BERSERKED = 1.2f;
    }
}
