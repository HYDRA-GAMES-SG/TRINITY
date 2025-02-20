using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ATrinityScore : MonoBehaviour
{
    private static ATrinityScore Instance;

    public List<IEnemyController> DefeatedBosses;
    
    public System.Action<ETrinityScore> OnVictory;
    //public float TimeToDamageScoreWeight = .5f;
    
    private float Timer;
    private float DamageTaken;
    
    [HideInInspector]
    public float NormalizedTimeScore = 0f;
    [HideInInspector]
    public float NormalizedDamageTakenScore = 0f;

    private bool bInvokedVictoryThisScene = false;

    
    public static FScoreLimits GetScoreLimits()
    {
        FScoreLimits newLimits = new FScoreLimits();
        
        switch (ATrinityGameManager.CurrentScene)
        {
            case "CrabBossDungeon":
                newLimits.SceneName = ATrinityGameManager.CurrentScene;
                newLimits.BestTime = 120; // seconds
                newLimits.WorstTime = 240; // seconds
                newLimits.BestDamageTaken = 0; //percent
                newLimits.WorstDamageTaken = 120; //percent
                return newLimits;
            case "DevourerSentinelBossDungeon":
                newLimits.SceneName = ATrinityGameManager.CurrentScene;
                newLimits.BestTime = 120; // seconds
                newLimits.WorstTime = 240; // seconds
                newLimits.BestDamageTaken = 0; //percent
                newLimits.WorstDamageTaken = 120; //percent
                return newLimits;
            default:
                newLimits.SceneName = "";
                print("WARNING:NO SCORE LIMITS!");
                return newLimits;
        }
    }
    
    public void Awake()
    {
        List<ATrinityScore> CurrentInstances = FindObjectsOfType<ATrinityScore>().ToList();
        DefeatedBosses = new List<IEnemyController>();
        
        if (CurrentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        ATrinityGameManager.SetScore(this);
        ATrinityGameManager.OnSceneChanged += Reset;
    }
    public void Start()
    {
        
    }
    
    

    public void Update()
    {
        if (ATrinityGameManager.CurrentScene != "PORTAL")
        {
            Timer += Time.deltaTime;
        }
        
        CheckForVictory();
    }

    public void AddDamageTaken(float healthDamage)
    {
        DamageTaken += healthDamage;
    }


    private void Reset()
    {
        bInvokedVictoryThisScene = false;
        DamageTaken = 0;
        Timer = 0;
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

        if (ATrinityGameManager.GetEnemyControllers().Count > 0 && !bBossAlive && !bInvokedVictoryThisScene)
        {
            FScoreLimits scoreLimits = GetScoreLimits();
            OnVictory?.Invoke(GetETS(scoreLimits));
            bInvokedVictoryThisScene = true;

            switch (ATrinityGameManager.CurrentScene)
            {
                case "CrabBossDungeon":
                    ATrinityGameManager.bCrabDefeated = true;
                    break;
                case "DevourerSentinelBossDungeon":
                    ATrinityGameManager.bDevourerSentinelDefeated = true;
                    break;
                default:
                    print("ATrinityScore: Defeated Boss Scene Name Not Found Line 127");
                    break;
            }

            DefeatedBosses.Add(ATrinityGameManager.GetEnemyControllers()[0]);
        }
    }

    public ETrinityScore GetETS(FScoreLimits scoreLimits)
    {
        float clampedTime = Mathf.Clamp(GetTimer(), scoreLimits.BestTime, scoreLimits.WorstTime);
        float clampedDamageTaken = Mathf.Clamp(GetDamageTaken(), scoreLimits.BestDamageTaken, scoreLimits.WorstDamageTaken);

        NormalizedTimeScore = Mathf.Clamp01((scoreLimits.WorstTime - clampedTime) / (clampedTime / Mathf.Clamp(scoreLimits.BestTime, 0.000001f, 1)));  //ensure no divide by 0

        NormalizedDamageTakenScore = Mathf.Clamp01(1f - (clampedDamageTaken / Mathf.Clamp(scoreLimits.WorstDamageTaken, 0.00001f, scoreLimits.WorstDamageTaken)));  //ensure no divide by 0

        float etsFloat = Mathf.Clamp((NormalizedTimeScore + NormalizedDamageTakenScore) * 10f, 0, 20); //multiply by 10 and clamp to ETS range to map to ETS properly
        
        return (ETrinityScore)Mathf.FloorToInt(etsFloat);
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
