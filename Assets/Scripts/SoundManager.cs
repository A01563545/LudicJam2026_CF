using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip jumpSound;
    public AudioClip switchSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playJumpSound();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playSwitchSound();
        }
    }

    public void playJumpSound()
    {
        audioSource.PlayOneShot(jumpSound);
    }

    public void playSwitchSound()
    {
        audioSource.PlayOneShot(switchSound);
    }
}