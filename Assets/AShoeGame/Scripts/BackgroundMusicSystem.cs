using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicSystem : MonoBehaviour
{
    [SerializeField] AudioSource mainLoopSource;
    [SerializeField] AudioSource SongVox;
    [SerializeField] AudioSource SongGuitar;
    [SerializeField] AudioSource finalBell;
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
                finalBell.PlayDelayed(SongVox.clip.length + 1f);
                mainLoopSource.loop = true;
                //Invoke something to end the game here, some kind of score check?
                Invoke("EndGameSomewhere", SongVox.clip.length + finalBell.clip.length - 10.5f);
                mainLoopSource.PlayDelayed(SongVox.clip.length + finalBell.clip.length);
                vocalMusicInterrupt = false;
            }
        }
    }

    private void EndGameSomewhere()
    {
        //Somewhere the end game logic should be called.
        NpcManager.Instance.checkWinCondition();
    }

}
