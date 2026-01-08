using System.Collections.Generic;
using UnityEngine;

public class TestMobSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] monsterPrefabs;
    [SerializeField] private int monstersPerRoom = 3;

    List<GameObject> spawnedMonsters = new List<GameObject>();

    private TestMapGenerator mapGenerator;

    private void Start()
    {
        mapGenerator = TestMapGenerator.instance;
    }

    public void SpawnMonstersInRooms()
    {
        if (mapGenerator == null || mapGenerator.rooms == null)
        {
            Debug.LogWarning("MonsterSpawner: 맵이 생성되지 않았습니다.");
            return;
        }

        if (spawnedMonsters.Count > 0)
        {
            foreach (var monster in spawnedMonsters)
            {
                Destroy(monster);
            }
            spawnedMonsters.Clear();
        }

        foreach (var room in mapGenerator.rooms)
        {
            SpawnMonstersInRoom(room.roomRect);
        }
    }

    private void SpawnMonstersInRoom(RectInt roomRect)
    {
        if (roomRect.width < 3 || roomRect.height < 3) return;
        for (int i = 0; i < monstersPerRoom; i++)
        {
            int randX = roomRect.x + Random.Range(1, roomRect.width - 1);
            int randY = roomRect.y + Random.Range(1, roomRect.height - 1);

            Vector3Int worldPos = new Vector3Int(
                randX - mapGenerator.MapSize.x / 2,
                randY - mapGenerator.MapSize.y / 2,
                0
            );

            int prefabIndex = Random.Range(0, monsterPrefabs.Length);
            GameObject monster = Instantiate(monsterPrefabs[prefabIndex], worldPos, Quaternion.identity);
            spawnedMonsters.Add(monster);
        }
    }

    private void OnDrawGizmos()
    {
        // 몬스터 위치 표시
        Gizmos.color = Color.magenta;

        foreach (Transform monster in spawnedMonsters.ConvertAll(m => m.transform))
        {
            if (monster == null) continue;
            Gizmos.DrawCube(monster.position + Vector3.forward * 0.1f, Vector3.one * 0.6f);
        }
    }
}
