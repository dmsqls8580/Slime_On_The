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
        public const float SPIKE_DELAY_NOTBERSERKED = 0.5f;	 		// 채력 50퍼 이상 사이 딜레이
        public const float SPIKE_DELAY_BERSERKED = 0.3f;			// 체력 50퍼 이하 사이 딜레이
        public const int SPAWN_SPIKE_1 = 24;						// 첫번째 스파이크 소환 수
        public const int SPAWN_SPIKE_2 = 21;						// 두번째 스파이크 소환 수
        public const int SPAWN_SPIKE_3 = 18;						// 세번째 스파이크 소환 수
        public const float SPAWN_SPIKE_RADIUS_1 = 8f;				// 첫번째 스파이크 범위
        public const float SPAWN_SPIKE_RADIUS_2 = 6f;				// 두번째 스파이크 범위
        public const float SPAWN_SPIKE_RADIUS_3 = 4f;				// 세번째 스파이크 범위
        
        // BabyTurtle Leaf
        public const float SPAWN_LEAF_RADIUS = 3f;				    // 폭발 반경
        public const float SPAWN_LEAF_DELAY_NOTBERSERKED = 0.2f;	// 채력 50퍼 이상 패턴 사이 딜레이
        public const float SPAWN_LEAF_DELAY_BERSERKED = 0.1f;		// 체력 50퍼 이하 패턴 사이 딜레이
        public const float LEAF_SHAKE_DURATION = 0.2f;				// 폭발 전 흔들림 지속 시간
        public const float LEAF_SHAKE_MAGNITUDE = 0.1f;				// 폭발 전 흔들림 크기
        
        // BabyTurtle Tentacle
        public const float SPAWN_TENTACLE_DISTANCE = 3f;			// 촉수 소환 거리(플레이어 기준)
        public const float TENTACLE_SPEED_NOTBERSERKED = 1.0f;		// 체력 50퍼 이상 촉수 시전 속도
        public const float TENTACLE_SPEED_BERSERKED = 1.2f;			// 체력 50퍼 이하 촉수 시전 속도
    }
}
