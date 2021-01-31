using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicSystem : MonoBehaviour
{
    [SerializeField] AudioSource mainLoopSource;
    [SerializeField] AudioSource SongVox;
    [SerializeField] AudioSource SongGuitar;
    [SerializeField] bool vocalMusicInterrupt = false;
    [SerializeField] float delayForFinalChorus = 180f;

    public void Start()
    {
        SongVox.Play();
        SongGuitar.Play();
        mainLoopSource.PlayDelayed(SongVox.clip.length);
        Invoke("QueueVocalsMusic", delayForFinalChorus);
    }

    public void QueueVocalsMusic()
    {
        mainLoopSource.loop = false;
        vocalMusicInterrupt = true;
    }

    public void Update()
    {
        if (vocalMusicInterrupt)
        {
            if (!mainLoopSource.isPlaying)
            {
                SongVox.Play();
                SongGuitar.Play();
                mainLoopSource.PlayDelayed(SongVox.clip.length);
                mainLoopSource.loop = true;
                vocalMusicInterrupt = false;
            }
        }
    }

}
