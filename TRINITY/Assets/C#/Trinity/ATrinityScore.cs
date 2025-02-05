using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ATrinityScore : MonoBehaviour
{
    public FScoreLimits ScoreLimits;
    public System.Action<ETrinityScore> OnVictory;
    //public float TimeToDamageScoreWeight = .5f;
    
    private float Timer;
    private float DamageTaken;
    
    [HideInInspector]
    public float NormalizedTimeScore = 0f;
    [HideInInspector]
    public float NormalizedDamageTakenScore = 0f;

    public void Awake()
    {
        ATrinityGameManager.SetScore(this);
    }
    public void Start()
    {
        ATrinityGameManager.GetPlayerController().OnHit += AddDamageTaken;
        ATrinityGameManager.OnGameStart += SetScoreLimits;
        ATrinityGameManager.OnSceneChanged += SetScoreLimits;
    }

    private void SetScoreLimits(FScoreLimits newLimits)
    {
        if (newLimits.SceneName != "")
        {
            ScoreLimits = newLimits;
            Timer = 0f;
            DamageTaken = 0f;
        }
    }

    public void Update()
    {
        if (ATrinityGameManager.CurrentScene != "PORTAL")
        {
            Timer += Time.deltaTime;
        }
        
        CheckForVictory();
    }

    public void AddDamageTaken(FHitInfo hitInfo)
    {
        DamageTaken += (hitInfo.Damage / ATrinityGameManager.GetPlayerController().HealthComponent.MAX);
    }



    private void CheckForVictory()
    {
        bool bBossAlive = false;
        
        foreach (IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
        {
            if (ec.EnemyStatus.Health.Current > 0f)
            {
                bBossAlive = true;
            }
        }

        if (ATrinityGameManager.GetEnemyControllers().Count > 0 && !bBossAlive)
        {
            OnVictory?.Invoke(GetETS());
        }
    }

    public ETrinityScore GetETS()
    {
        float clampedTime = Mathf.Clamp(GetTimer(), ScoreLimits.BestTime, ScoreLimits.WorstTime);
        float clampedDamageTaken = Mathf.Clamp(GetDamageTaken(), ScoreLimits.BestDamageTaken, ScoreLimits.WorstDamageTaken);

        NormalizedTimeScore = Mathf.Clamp01((ScoreLimits.WorstTime - clampedTime) / (clampedTime / Mathf.Clamp(ScoreLimits.BestTime, 0.000001f, 1)));  //ensure no divide by 0

        NormalizedDamageTakenScore = Mathf.Clamp01(1f - (clampedDamageTaken / Mathf.Clamp(ScoreLimits.WorstDamageTaken, 0.00001f, ScoreLimits.WorstDamageTaken)));  //ensure no divide by 0

        float etsFloat = Mathf.Clamp((NormalizedTimeScore + NormalizedDamageTakenScore) * 10f, 0, 20); //multiply by 10 and clamp to ETS range to map to ETS properly
        
        return (ETrinityScore)Mathf.CeilToInt(etsFloat);
    }
    
    public float GetTimer()
    {
        return Timer;
    }

    public float GetDamageTaken()
    {
        return DamageTaken;
    }

    public static string GetScoreString(ETrinityScore score)
    {
        switch (score)
        {
            case ETrinityScore.ETS_S_PlusPlusPlus:
                return "S+++";
            case ETrinityScore.ETS_S_PlusPlus:
                return "S++";
            case ETrinityScore.ETS_S_Plus:
                return "S+";
            case ETrinityScore.ETS_S:
                return "S";
            case ETrinityScore.ETS_S_Minus:
                return "S-";
            case ETrinityScore.ETS_A_Plus:
                return "A+";
            case ETrinityScore.ETS_A:
                return "A";
            case ETrinityScore.ETS_A_Minus:
                return "A-";
            case ETrinityScore.ETS_B_Plus:
                return "B+";
            case ETrinityScore.ETS_B:
                return "B";
            case ETrinityScore.ETS_B_Minus:
                return "B-";
            case ETrinityScore.ETS_C_Plus:
                return "C+";
            case ETrinityScore.ETS_C:
                return "C";
            case ETrinityScore.ETS_C_Minus:
                return "C-";
            case ETrinityScore.ETS_D_Plus:
                return "D+";
            case ETrinityScore.ETS_D:
                return "D";
            case ETrinityScore.ETS_D_Minus:
                return "D-";
            case ETrinityScore.ETS_F_Plus:
                return "F+";
            case ETrinityScore.ETS_F:
                return "F";
            case ETrinityScore.ETS_F_Minus:
                return "F-";
            case ETrinityScore.ETS_F_MinusMinus:
                return "F--";
            default:
                return "Invalid Score";
        }
    }
}
