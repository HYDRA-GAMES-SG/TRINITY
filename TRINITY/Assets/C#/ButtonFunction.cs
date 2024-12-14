using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunction : MonoBehaviour
{
    public string Menu = "MenuScene";
    public string CrabBossStage = "CrabBossDungeon";
    public string FlyingBossStage = "FlyingBossStageScene";
    public string HumanoidBossStage = "HumanoidBossStageScene";

    public void OnMenu()
    {
        SceneManager.LoadScene(Menu);
    }public void OnCrabBoss()
    {
        SceneManager.LoadScene(CrabBossStage);
    }
    public void OnFlyingBoss()
    {
        SceneManager.LoadScene(FlyingBossStage);
    }
    public void OnHumanoid()
    {
        SceneManager.LoadScene(HumanoidBossStage);
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
