using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField] private float delayTime = 0.1f;
    [SerializeField] private float distance = 0.3f;
    [SerializeField] private float moveStep = 10f;

    public Transform followTransform;
    public Transform networkOwner;

    private Vector3 _targetPos;

    private void Update()
    {
        _targetPos = followTransform.position - followTransform.forward * distance;
        _targetPos += (transform.position - _targetPos) * delayTime;
        _targetPos.z = 0f;

        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * moveStep);
    }
}
