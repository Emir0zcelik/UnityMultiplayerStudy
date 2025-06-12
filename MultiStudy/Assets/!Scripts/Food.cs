using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public GameObject prefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (NetworkManager.Singleton.IsServer)
        {
            if (collision.TryGetComponent(out PlayerLength playerLength))
            {
                playerLength.AddLength();
            }

            else if (collision.TryGetComponent(out Tail tail))
            {
                tail.networkOwner.GetComponent<PlayerLength>().AddLength();
            }
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        NetworkObject.Despawn();
    }
}
