using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    private const int maxPrefabCount = 50;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

    private void SpawnFoodStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;

        for (int i = 0; i < 30; i++)
        {
            SpawnFood();
        }

        StartCoroutine(SpawnOverTime());

    }

    private IEnumerator SpawnOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            yield return new WaitForSeconds(2f);
            if (NetworkObjectPool.Singleton.GetCurrentPrefabCount(_prefab) < maxPrefabCount)

            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(_prefab, GetRandomPositionOnMap(), Quaternion.identity);

        obj.GetComponent<Food>().prefab = _prefab;
        if(!obj.IsSpawned) obj.Spawn(true);
    }

    private Vector3 GetRandomPositionOnMap()
    {
        return new Vector3(Random.Range(-9f, 9f), Random.Range(-5f, 5f), 0f);
    }
}
