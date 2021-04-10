using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

public class HelpCon : MonoBehaviour/*,IPointerClickHandler*/
{
    public static HelpCon current;
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
#if UNITY_STANDALONE
        StartCoroutine(LoadImgHelp(Path.Combine(Application.streamingAssetsPath, nameFolder + "_PC")));
#else
        StartCoroutine(LoadImgHelp((Application.platform == RuntimePlatform.Android ? "" : "file://") + $"{Application.streamingAssetsPath}/{nameFolder}_M"));
#endif
        defaultSizeImg = imgHelp.rectTransform.rect.size;
        imgHelp.rectTransform.anchorMin = new Vector2(.5f, .5f);
        imgHelp.rectTransform.anchorMax = new Vector2(.5f, .5f);
    }
    public bool stopTimeScale;
    public List<Texture> textures;
    public string nameFolder = "Help";
    public string prefix = "";
    public string subfix = "";
    public RawImage imgHelp;
    public CanvasGroup canvasGroup;
    public Button btnNext;
    public Button btnPrevious;
    int index = 0;
    Vector2 defaultSizeImg;
    IEnumerator LoadImgHelp(string path)
    {
        int countFile = 0;
        while (true)
        {
            UnityWebRequest load = UnityWebRequestTexture.GetTexture($"{path}/{prefix}{++countFile}{subfix}.png");
            yield return load.SendWebRequest();
            while (!load.isDone)
            {
                yield return null;
            }
            if (load.responseCode == 200)
            {
                textures.Add(DownloadHandlerTexture.GetContent(load));
            }
            else
            {
                break;
            }
        }
    }
    //public void ChangeImg()
    //{
    //    if (count < textures.Count)
    //    {
    //        imgHelp.texture = textures[count];
    //        count++;
    //    }
    //    else
    //    {
    //        CloseHelp();

    //    }
    //}
    public void OpenHelp()
    {
        if (stopTimeScale)
            Time.timeScale = 0;
        SetImage(0);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }
    public void CloseHelp()
    {
        if (stopTimeScale)
            Time.timeScale = 1;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        imgHelp.texture = null;
    }
    public void Next() => SetImage(index += 1);
    public void Previous() => SetImage(index -= 1);

    void SetImage(int i)
    {
        print(i);
        index = Mathf.Clamp(i, 0, textures.Count - 1);
        if (textures[index].width > textures[index].height)
        {
            float aspect = (float)textures[index].height / (float)textures[index].width;
            imgHelp.rectTransform.sizeDelta = new Vector2(defaultSizeImg.x, defaultSizeImg.x * aspect);
        }
        else
        {
            float aspect = (float)textures[index].width / (float)textures[index].height;
            imgHelp.rectTransform.sizeDelta = new Vector2(defaultSizeImg.y * aspect, defaultSizeImg.y);
        }
        imgHelp.texture = textures[index];
        btnNext.gameObject.SetActive(!(index == textures.Count - 1));
        btnPrevious.gameObject.SetActive(!(index == 0));
    }
}
