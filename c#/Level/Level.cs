using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Level : MonoBehaviour
{
    public void Load()
    {
        SceneManager.LoadScene("Level1");
    }
    public void quit()
    {
        Application.Quit();
    }
}
