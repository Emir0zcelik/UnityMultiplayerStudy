using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;

    [CanBeNull] public static event System.Action GameOverEvent;

    private Camera _mainCamera;
    private Vector3 _mouseInput = Vector3.zero;
    private PlayerLength _playerLength;
    private bool _canCollide = true;

    private readonly ulong[] _targetClientsArray = new ulong[1];



    private void Initialize()
    {
        _mainCamera = Camera.main;
        _playerLength = GetComponent<PlayerLength>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void Update()
    {
        if (!IsOwner || !Application.isFocused) return;

        _mouseInput.x = Input.mousePosition.x;
        _mouseInput.y = Input.mousePosition.y;
        _mouseInput.z = _mainCamera.nearClipPlane;
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint((Vector3)_mouseInput);
        mouseWorldPos.z = 0f;
        transform.position = Vector3.MoveTowards(transform.position, mouseWorldPos, Time.deltaTime * speed);

        if (mouseWorldPos != transform.position)
        {
            Vector3 targetDir = mouseWorldPos - transform.position;
            targetDir.z = 0;
            transform.up = targetDir;
        }


    }

    [ServerRpc]
    private void DetermineCollisonWinnerServerRpc(PlayerData player1, PlayerData player2)
    {
        if (player1.Length > player2.Length)
        {
            WinInformationServerRpc(player1.Id, player2.Id);
        }
        else if (player2.Length > player1.Length)
        {
            WinInformationServerRpc(player2.Id, player1.Id);
        }
        else
        {
            
        }
    }

    [ServerRpc]
    private void WinInformationServerRpc(ulong winner, ulong loser)
    {
        _targetClientsArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _targetClientsArray
            }
        };
        AtePlayerClientRpc(clientRpcParams);

        _targetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientsArray;

        GameOverClientRpc(clientRpcParams);

    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You ate a player!");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You Lose!");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }

    private IEnumerator CollisionCheckCoroutine()
    {
        _canCollide = false;
        yield return new WaitForSeconds(0.5f);
        _canCollide = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player Collision");

        if (!collision.gameObject.CompareTag("Player")) return;
        if (!IsOwner) return;
        if (!_canCollide) return;

        StartCoroutine(CollisionCheckCoroutine());

        if (collision.gameObject.TryGetComponent(out PlayerLength playerLength))
        {
            Debug.Log("Head collision");
            var player1 = new PlayerData()
            {
                Id = OwnerClientId,
                Length = _playerLength.lenght.Value
            };
            var player2 = new PlayerData()
            {
                Id = playerLength.OwnerClientId,
                Length = playerLength.lenght.Value
            };

            DetermineCollisonWinnerServerRpc(player1, player2);
        }

        else if (collision.gameObject.TryGetComponent(out Tail tail))
        {
            Debug.Log("Tail collision");

            WinInformationServerRpc(tail.networkOwner.GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        }
    }

    struct PlayerData : INetworkSerializable
    {
        public ulong Id;
        public ushort Length;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Length);
        }
    }
}
