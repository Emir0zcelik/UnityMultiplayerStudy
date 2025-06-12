using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ClientMusicPlayer : Singleton<ClientMusicPlayer>
{
    [SerializeField] private AudioClip _eatingAudioClip;
    private AudioSource _audioSource;


    public override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayEatAudioClip()
    {
        _audioSource.clip = _eatingAudioClip;
        _audioSource.Play();
    }
}
