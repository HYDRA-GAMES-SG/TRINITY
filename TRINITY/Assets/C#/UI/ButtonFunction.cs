using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunction : MonoBehaviour
{
    public string menu = "MenuScene";
    public string crabBossStage = "CrabBossDungeon";
    public string flyingBossStage = "FlyingBossStageScene";
    public string humanoidBossStage = "HumanoidBossStageScene";

    public void OnMenu()
    {
        SceneManager.LoadScene(menu);
    }public void OnCrabBoss()
    {
        SceneManager.LoadScene(crabBossStage);
    }
    public void OnFlyingBoss()
    {
        SceneManager.LoadScene(flyingBossStage);
    }
    public void OnHumanoid()
    {
        SceneManager.LoadScene(humanoidBossStage);
    }
    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnQuit()
    {
        Application.Quit();
    }
}
