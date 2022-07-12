using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class score : MonoBehaviour
{
    [SerializeField] attack AT;
    [SerializeField] ai AI;
    [SerializeField] CPScal cps;
    internal int AIscore=0;
    int playAtFreq, spam;
    float aiAcc, playAcc;

    public int calculate()
    {
        aiAcc = AI.aiAccuracy;
        playAcc = AT.hitAcc;
        playAtFreq = AT.press.value;
        spam = cps.spam;
        if (playAtFreq > 15)
            playAtFreq = 15;
        AIscore= Mathf.RoundToInt(-25 * aiAcc - 25 * spam / 10 + 25* playAtFreq / 15 + 25 *  playAcc);
        return AIscore;
    }
 /*   void facingscore()
    {
        if (an.GetInteger("Score") <= 0)
        {
            if (is_facing)
            {
                an.SetInteger("Score", 0);
                play?.SendMessage("scorealert");
            }
            else if (!is_facing && (Time.time - decisiont > 3 || an.GetInteger("Score") == 0))
            {
                an.SetInteger("Score", -20);
                play?.SendMessage("scorealert");
            }
        }
    }


    public void scoree(int score)
    {
        if (is_facing)
        {
            tmp = an.GetInteger("Score") + score;
            tmp = Mathf.Clamp(tmp, 0, 50);
            an.SetInteger("Score", tmp);
            play?.SendMessage("scorealert");
        }
        else
        {
            decisiont = Time.time;
            tmp = -50;
            an.SetInteger("Score", -50);
            play?.SendMessage("scorealert");
        }
    }

    public void set_score(int score)
    {
        an.SetInteger("Score", score);
    }
    public void set_tmp_score()
    {
        an.SetInteger("Score", tmp);
        play.SendMessage("scorealert");
    }*/
}
