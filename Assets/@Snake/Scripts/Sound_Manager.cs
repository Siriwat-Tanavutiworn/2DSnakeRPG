using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Option_Menu;

public class Sound_Manager : MonoBehaviour
{
    public static Sound_Manager instance;
    public List<AudioClip> Play_Now  = new List<AudioClip>();
    public List<Sound_Data> Sounds = new List<Sound_Data>();

    //public List<AudioClip> PlayScene = new List<AudioClip>();

    void Awake()
    {
        if (Sound_Manager.instance)
        {
            Destroy(Sound_Manager.instance.gameObject);
        }
        
        instance = this;
    }

    void Start()
    {        
        if (OptionMenu.current.audioSourceEffect.isPlaying) OptionMenu.current.audioSourceEffect.Stop();
        if (Play_Now.Count > 0){
            if (OptionMenu.current.audioSourceBGM.isPlaying) OptionMenu.current.audioSourceBGM.Stop();
            if (OptionMenu.current.audioSourceEffect.isPlaying) OptionMenu.current.audioSourceEffect.Stop();

            /*if(PlayScene.Count > 0)
            {
                Play_Now[0] = PlayScene[Random.Range(0, PlayScene.Count)];
            }*/
            OptionMenu.current.audioSourceBGM.clip = Play_Now[0];
            OptionMenu.current.audioSourceBGM.Play();
            for(int i = 1 ; i < Play_Now.Count; i++){
                OptionMenu.current.audioSourceEffect.clip = Play_Now[i];
                OptionMenu.current.audioSourceEffect.Play();
            }
        }
    }

    public void Play_SelectSound(string name){

        foreach(Sound_Data s in Sounds){
            if(s.Name.Equals(name)){
                switch(s.Type){
                    case Sound_Type.EFX:
                        OptionMenu.current.audioSourceEffect.PlayOneShot(s.Clip);
                    break;
                    case Sound_Type.QA:
                        OptionMenu.current.audioSourceQA.PlayOneShot(s.Clip);
                    break;
                    case Sound_Type.BGM:
                        OptionMenu.current.audioSourceBGM.clip = s.Clip;
                        OptionMenu.current.audioSourceBGM.Play();
                        break;
                }
            }
        }
    }

    public void StopPlayBGM()
    {
        if (OptionMenu.current.audioSourceBGM.isPlaying) OptionMenu.current.audioSourceBGM.Stop();
    }

}

[System.Serializable]
public class Sound_Data{

    public string Name;
    public AudioClip Clip;
    public Sound_Type Type;

}

public enum Sound_Type{
    EFX,QA,BGM
}
