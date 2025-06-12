using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerLength : NetworkBehaviour
{
    [SerializeField] private GameObject tailPrefab;
    public NetworkVariable<ushort> lenght = new(1, NetworkVariableReadPermission.Everyone,
                                                   NetworkVariableWritePermission.Server);

    [CanBeNull] public static event System.Action<ushort> ChangedLengthEvent;

    private List<GameObject> _tails;
    private Transform _lastTail;
    private Collider2D _collider2D;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTail = transform;
        _collider2D = GetComponent<Collider2D>();
        if (!IsServer) lenght.OnValueChanged += LengthChangedEvent;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        DestroyTails();
    }

    private void DestroyTails()
    {
        while (_tails.Count != 0)
        {
            GameObject tail = _tails[0];
            _tails.RemoveAt(0);
            Destroy(tail);
        }
    }


    // This will be called by the server
    public void AddLength()
    {
        lenght.Value += 1;

        LengthChanged();
    }

    private void LengthChanged()
    {
        InstantiateTail();

        if (!IsOwner) return;
        ChangedLengthEvent?.Invoke(lenght.Value);
        ClientMusicPlayer.Instance.PlayEatAudioClip();
    }

    private void LengthChangedEvent(ushort previousValue, ushort newValue)
    {
        Debug.Log("LengthChanged Callback");

        LengthChanged();
    }

    private void InstantiateTail()
    {
        GameObject tailGameObj = Instantiate(tailPrefab, transform.position, Quaternion.identity);
        tailGameObj.GetComponent<SpriteRenderer>().sortingOrder = -lenght.Value;
        if (tailGameObj.TryGetComponent(out Tail tail))
        {
            tail.networkOwner = transform;
            tail.followTransform = _lastTail;
            _lastTail = tailGameObj.transform;
            Physics2D.IgnoreCollision(tailGameObj.GetComponent<Collider2D>(), _collider2D);
        }
        _tails.Add(tailGameObj);
    }
}
