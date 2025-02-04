using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ATrinityScore : MonoBehaviour
{
    public static System.Action<ETrinityScore> OnVictory;
    //public float TimeToDamageScoreWeight = .5f;
    
    [Header("Time")]
    [SerializeField]
    private float BestTime = 120f;
    [SerializeField]
    private float WorstTime = 240f;
    private float Timer;
    public float NormalizedTimeScore = 0f;

    [Header("Damage Taken")]
    [SerializeField]
    private float BestDamageTaken = 0f;
    [SerializeField]
    private float WorstDamageTaken = 100f;
    private float DamageTaken;
    public float NormalizedDamageTakenScore = 0f;

    public void Start()
    {
        ATrinityGameManager.SetScore(this);
    }

    public void Update()
    {
        CheckForVictory();
    }
    
    
    
    private void CheckForVictory()
    {
        bool bBossExists = false;
        bool bBossAlive = false;
        
        foreach (IEnemyController ec in ATrinityGameManager.GetEnemyControllers())
        {
            if (ec)
            {
                bBossExists = true;
                
                if (ec.EnemyStatus.Health.Current > 0f)
                {
                    bBossAlive = true;
                }
            }
        }

        if (bBossExists && !bBossAlive)
        {
            OnVictory?.Invoke(GetScore());
        }
    }
    
    public ETrinityScore GetScore()
    {
        return MapScoreToETS(CalculateScore());
    }
    
    private ETrinityScore MapScoreToETS(float score)
    {
        return (ETrinityScore)Mathf.CeilToInt(score);
    }

    private float CalculateScore()
    {
        NormalizedTimeScore = NormalizeTime(ClampTime(Timer));
        NormalizedDamageTakenScore = NormalizeDamageTaken(ClampDamageTaken(DamageTaken));

        return (NormalizedTimeScore + NormalizedDamageTakenScore) * 10f; //multiply to map to ETS properly
    }

    private float NormalizeTime(float clampedTime)
    {
        return Mathf.Clamp01(1f - (clampedTime - BestTime) / BestTime);
    }
    
    private float NormalizeDamageTaken(float clampedDamageTaken)
    {
        return Mathf.Clamp01(1f - (clampedDamageTaken - BestDamageTaken) / BestDamageTaken);
    }
    
    private float ClampTime(float timer)
    {
        return Mathf.Clamp(timer, BestTime, WorstTime);
    }

    private float ClampDamageTaken(float damageTaken)
    {
        return Mathf.Clamp(damageTaken, BestDamageTaken, WorstDamageTaken);
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
