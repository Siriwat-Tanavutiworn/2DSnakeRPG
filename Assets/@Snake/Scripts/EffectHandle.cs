using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectHandle : MonoBehaviour
{
    public static EffectHandle current;
    public List<EffectList> effectLists = new List<EffectList>();
    private void Awake()
    {
        current = this;
    }

    public void PlaySelectEffect(string effectName, Vector3 pos, Transform parent = null)
    {
        //Debug.Log("@PlayEffect : " + effectName);
        foreach (EffectList e in effectLists)
        {
            if (effectName.Equals(e.name))
            {
                GameObject effect = Instantiate(e.effect, pos, Quaternion.identity, parent);
                Destroy(effect.gameObject, e.destroyTime);
                break;
            }
        }
    }
    
    public GameObject GetEffectObject(string effectName)
    {
        foreach (EffectList e in effectLists)
        {
            if (effectName.Equals(e.name))
            {
                return e.effect;
            }
        }

        return null;
    }
}

[System.Serializable]
public class EffectList
{
    public string name;
    public float destroyTime;
    public GameObject effect;
}

