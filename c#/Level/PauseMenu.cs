using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject self;
    public void resume()
    {
        Time.timeScale = 1f;
        self.SetActive(false);
    }
    public void quit()
    {
        Application.Quit();
    }
    public void Menu()
    {
        SceneManager.LoadScene("Thismenu");
    }
    public void Retry()
    {
        SceneManager.LoadScene("Level1");
    }
}
