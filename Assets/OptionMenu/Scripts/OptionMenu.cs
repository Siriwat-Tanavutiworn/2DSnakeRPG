using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Option_Menu
{

    public class OptionMenu : MonoBehaviour
    {
        public static OptionMenu current;
        public bool syncValue;
        public AudioMixer mixer;
        public Canvas canvas;
        public CanvasGroup canvasGroup;
        public GameObject help;

        [Range(0f, 1f)]
        public float defaultValue = 1;

        bool isOpen;

        [Header("Use")]
        public bool _BGM;
        public bool _EFFECT;
        public bool _QA;
        public bool _UsetimeScale;
        [Header("Scence")]
        public string scenceHome;
        public string[] scencePlay;
        [Header("SliderValue")]
        public Slider sliderMaster;
        public Slider sliderBGM;
        public Slider sliderEffect;
        public Slider sliderQA;
        [Header("AudioSource")]
        public AudioSource audioSourceBGM;
        public AudioSource audioSourceEffect;
        public AudioSource audioSourceQA;
        public Button buttonHome;
        public Button buttonRestart;

        private float mindB = -40;
        private float maxdB = 0;
        private void Awake()
        {
            if (current)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                current = this;
            }


            SceneManager.sceneLoaded += CheckScence;
            isOpen = true;

            State();
        }

        void Init()
        {
            if (!PlayerPrefs.HasKey("MasterRatio"))
                PlayerPrefs.SetFloat("MasterRatio", defaultValue);
            if (!PlayerPrefs.HasKey("BGMRatio"))
                PlayerPrefs.SetFloat("BGMRatio", defaultValue);
            if (!PlayerPrefs.HasKey("EffectRatio"))
                PlayerPrefs.SetFloat("EffectRatio", defaultValue);
            if (!PlayerPrefs.HasKey("QARatio"))
                PlayerPrefs.SetFloat("QARatio", defaultValue);

            Debug.Log("MasterRatio " + PlayerPrefs.GetFloat("MasterRatio"));
            sliderMaster.value = PlayerPrefs.GetFloat("MasterRatio");
            mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterRatio") == 0 ? -80 : (PlayerPrefs.GetFloat("MasterRatio") * (maxdB - mindB)) + mindB);

            Debug.Log("BGMRatio " + PlayerPrefs.GetFloat("BGMRatio"));
            sliderBGM.value = syncValue ? sliderMaster.value * PlayerPrefs.GetFloat("BGMRatio") : PlayerPrefs.GetFloat("BGMRatio");
            mixer.SetFloat("BGM", PlayerPrefs.GetFloat("BGMRatio") == 0 ? -80 : (PlayerPrefs.GetFloat("BGMRatio") * (maxdB - mindB)) + mindB);

            Debug.Log("EffectRatio " + PlayerPrefs.GetFloat("EffectRatio"));
            sliderEffect.value = syncValue ? sliderMaster.value * PlayerPrefs.GetFloat("EffectRatio") : PlayerPrefs.GetFloat("EffectRatio");
            mixer.SetFloat("Effect", PlayerPrefs.GetFloat("EffectRatio") == 0 ? -80 : (PlayerPrefs.GetFloat("EffectRatio") * (maxdB - mindB)) + mindB);

            Debug.Log("QARatio " + PlayerPrefs.GetFloat("QARatio"));
            sliderQA.value = syncValue ? sliderMaster.value * PlayerPrefs.GetFloat("QARatio") : PlayerPrefs.GetFloat("QARatio");
            mixer.SetFloat("QA", PlayerPrefs.GetFloat("QARatio") == 0 ? -80 : (PlayerPrefs.GetFloat("QARatio") * (maxdB - mindB)) + mindB);

            sliderMaster.onValueChanged.AddListener((_) =>
            {
                SyncVolumn(_, "Master", sliderMaster);
            });
            sliderBGM.onValueChanged.AddListener((_) =>
            {
                SyncVolumn(_, "BGM", sliderBGM);
            });
            sliderEffect.onValueChanged.AddListener((_) =>
            {
                SyncVolumn(_, "Effect", sliderEffect);
            });
            sliderQA.onValueChanged.AddListener((_) =>
            {
                SyncVolumn(_, "QA", sliderQA);
            });

        }
        void SyncVolumn(float value, string key, Slider slider)
        {
            if (!EventSystem.current.currentSelectedGameObject || EventSystem.current.currentSelectedGameObject.GetComponent<Slider>() != slider)
                return;
            Debug.Log(key);
            switch (key)
            {
                case "Master":
                    PlayerPrefs.SetFloat("MasterRatio", value);
                    if (syncValue)
                    {
                        sliderBGM.value = value * PlayerPrefs.GetFloat("BGMRatio");
                        sliderEffect.value = value * PlayerPrefs.GetFloat("EffectRatio");
                        sliderQA.value = value * PlayerPrefs.GetFloat("QARatio");
                    }
                    break;

                default:
                    if (value > PlayerPrefs.GetFloat("MasterRatio") && syncValue)
                    {
                        if (key + "Ratio" != "BGMRatio")
                            PlayerPrefs.SetFloat("BGMRatio", PlayerPrefs.GetFloat("BGMRatio") * PlayerPrefs.GetFloat("MasterRatio") / value);

                        if (key + "Ratio" != "EffectRatio")
                            PlayerPrefs.SetFloat("EffectRatio", PlayerPrefs.GetFloat("EffectRatio") * PlayerPrefs.GetFloat("MasterRatio") / value);

                        if (key + "Ratio" != "QARatio")
                            PlayerPrefs.SetFloat("QARatio", PlayerPrefs.GetFloat("QARatio") * PlayerPrefs.GetFloat("MasterRatio") / value);

                        PlayerPrefs.SetFloat("MasterRatio", value);
                        sliderMaster.value = value;
                    }
                    PlayerPrefs.SetFloat(key + "Ratio", syncValue ? value / PlayerPrefs.GetFloat("MasterRatio") : value);
                    break;
            }
            Debug.Log("In " + key + " " + (value == 0 ? -80 : (value * (maxdB - mindB)) + mindB));
            mixer.SetFloat(key, (value == 0 ? -80 : (value * (maxdB - mindB)) + mindB));
        }
        private void Start()
        {
            Init();
        }
        void CheckScence(Scene s, LoadSceneMode m)
        {
            canvas.worldCamera = Camera.main;
            buttonHome.gameObject.SetActive(scenceHome != s.name);
            buttonRestart.gameObject.SetActive(scencePlay.Contains(s.name));

        }
        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    State();
            //}
            CheckQAPlay();
        }
        void CheckQAPlay()
        {
            if (audioSourceQA.isPlaying)
            {
                mixer.SetFloat("BGM", sliderBGM.value == 0 ? -80f : ((0.1f > sliderBGM.value ? sliderBGM.value : 0.1f) * (maxdB - mindB)) + mindB);
                mixer.SetFloat("Effect", sliderEffect.value == 0 ? -80f : ((0.1f > sliderEffect.value ? sliderEffect.value : 0.1f) * (maxdB - mindB)) + mindB);
            }
            else
            {
                mixer.SetFloat("BGM", sliderBGM.value == 0 ? -80f : (sliderBGM.value * (maxdB - mindB)) + mindB);
                mixer.SetFloat("Effect", sliderEffect.value == 0 ? -80f : (sliderEffect.value * (maxdB - mindB)) + mindB);
            }
        }
        public void State()
        {
            isOpen = !isOpen;
            canvasGroup.alpha = isOpen ? 1 : 0;
            canvasGroup.blocksRaycasts = isOpen;
            if (_UsetimeScale)
                Time.timeScale = isOpen ? 0 : 1;
        }
        public void Home()
        {
            if (!string.IsNullOrEmpty(scenceHome))
                SceneManager.LoadScene(scenceHome);
            else
                SceneManager.LoadScene(0);
            State();
        }
        public void Restart()
        {
            if (scencePlay.Contains(SceneManager.GetActiveScene().name) || scencePlay.Length == 0)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else
                SceneManager.LoadScene(scencePlay[0]);
            State();
        }
        public void Help()
        {
            HelpCon.current.OpenHelp();
            //State();
        }
        public void Exit()
        {
            Application.Quit();
        }
        private void OnValidate()
        {
            sliderBGM.transform.parent.gameObject.SetActive(_BGM);
            sliderEffect.transform.parent.gameObject.SetActive(_EFFECT);
            sliderQA.transform.parent.gameObject.SetActive(_QA);
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= CheckScence;
        }

        public static void PlayEffect(AudioClip clip) => current.audioSourceEffect.PlayOneShot(clip);
        public static void PlayQA(AudioClip clip) => current.audioSourceQA.PlayOneShot(clip);
        public static void PlayBGM(AudioClip clip)
        {
            current.audioSourceBGM.clip = clip;
            current.audioSourceBGM.Play();
        }
    }
}