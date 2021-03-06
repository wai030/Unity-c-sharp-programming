using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
public class take_damage : MonoBehaviour
{
    //require deadfunction implemented
    [SerializeField] GameObject self, endG;
    [SerializeField] Text text;
    [SerializeField] string dead_func, Endword;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Material  white;
    [SerializeField] float whiteT;
    [SerializeField] public int maxH;
    [HideInInspector] public int currentH;
    Material original;
    [HideInInspector] [SerializeField] internal bool aiNeeded;
    [HideInInspector] [SerializeField] internal string determine_func;
    [HideInInspector] [SerializeField] internal GameObject enemyai;
    [SerializeField] healthBar Health;
    
    void Start()
    {
        original = sr.material;
        currentH = maxH;
        Health?.setbar(currentH / maxH);
    }

    public void takedamage(int damage, bool turnwhite=true)
    {
        currentH -= damage;
        if (currentH <= 0)
        {
            die();
            endG.SetActive(true);
            text.text = Endword;
        }
        if(turnwhite)
            StartCoroutine(turn_white(whiteT, 4));

        enemyai?.GetComponent<ai>()?.hitalert();
        Health?.setbar((float)currentH / maxH);

    }
    IEnumerator turn_white(float t, int blink)
    {
        Time.timeScale = 0;
        float pauseT = Time.realtimeSinceStartup + t;
        bool b = true;

        while (Time.realtimeSinceStartup < pauseT)
        {
            if (b)
            {
                b = false;
                sr.material = white;
                yield return new WaitForSecondsRealtime(t / blink);
            }
            else
            {
                b = true;
                sr.material = original;
                yield return new WaitForSecondsRealtime(t / blink);
            }
        }
        Time.timeScale = 1f;
    }
    void die()
    {
        self?.SendMessage(dead_func, SendMessageOptions.DontRequireReceiver);
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(take_damage))]
[CanEditMultipleObjects]// custom editor
public class take_damageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        take_damage dmg = target as take_damage;
        dmg.aiNeeded = EditorGUILayout.Toggle("aiNeeded", dmg.aiNeeded);
        if (dmg.aiNeeded)
        {
            dmg.enemyai = (GameObject)EditorGUILayout.ObjectField("enemyai", dmg.enemyai, typeof(GameObject), true);
            dmg.determine_func = EditorGUILayout.TextField("determine_func", dmg.determine_func);
        }
        else
        {
            dmg.enemyai = null;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
