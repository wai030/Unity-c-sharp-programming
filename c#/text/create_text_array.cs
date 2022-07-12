using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class create_text_array : MonoBehaviour
{

    TextAsset[] txt;
    public Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

    private void Awake()
    {
        txt = Resources.LoadAll<TextAsset>("txt");
        foreach(var i in txt)
        {
            List<string> words = new List<string>();// how many text file
            var line = i.text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var l in line)// l is mean each line of word
            {
                words.Add(l);
            }
            dict.Add(i.name, words);
            
        }
    }
    public string gettext(string textname, int i)
    {
        return dict[textname][i];
    }
  
    public int getlength(string txt)
    {
        return dict[txt].Count;
    }
}
