using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

public class NavMesh2DManager : SceneOnlySingleton<NavMesh2DManager>
{
    public NavMeshSurface Surface2D;

    protected override void Awake()
    {
        base.Awake();
        Surface2D = GetComponent<NavMeshSurface>();
    }

    // 외부에서 사용할 수 있도록 public으로 제작한 메서드
    public void BakeNavMesh()
    {
        StartCoroutine(DelayBakeNavMesh());
    }

    // 배이크 프레임 딜레이
    private IEnumerator DelayBakeNavMesh()
    {
        yield return null;
        Logger.Log("Baking NavMesh");
        Surface2D.BuildNavMesh();
    }

    
    // 오브젝트(자원, 건물, 등) 스폰, 설치, 파괴, 틱(30초) 당 호출
    public void UpdateThisNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }
}
