using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

public class NavMesh2DManager : SceneOnlySingleton<NavMesh2DManager>
{
    public NavMeshSurface Surface2D;

    private void Start()
    {
        Surface2D = GetComponent<NavMeshSurface>();
        Surface2D.BuildNavMesh();
    }
    void Update()
    {
        // UpdateNavMesh();
    }

    // 오브젝트(자원, 건물, 등) 스폰, 설치, 파괴, 틱(30초) 당 호출
    public void UpdateNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }
}
