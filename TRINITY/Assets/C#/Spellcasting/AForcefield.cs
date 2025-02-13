using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AForcefield : ASpell
{
    public GameObject ImpactFX;
    public float DamageAbsorbedPerMana = 1f;
    private GameObject ForcefieldEffect;
    
    public override void Initialize()
    {
        if (ForcefieldEffect == null)
        {
            ForcefieldEffect = Instantiate(SpellPrefab, ATrinityGameManager.GetPlayerController().Position, Quaternion.identity);
            ForcefieldEffect.transform.parent = ATrinityGameManager.GetPlayerController().transform;
            ForcefieldEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);
            ForcefieldEffect.SetActive(false);
        }
        ATrinityGameManager.GetSpells().ManaComponent.OnOutOfMana += Release;
        ATrinityGameManager.GetPlayerController().OnForcefieldHit += SpawnSparks;
    }
    
    public override void CastStart()
    {
        if (ATrinityGameManager.GetSpells().ManaComponent.Current <= 0 || ATrinityGameManager.GetBrain().bIsStunned)
        {
            return;
        }

        ATrinityGameManager.GetBrain().bForcefieldActive = true;
        ForcefieldEffect.SetActive(true);
    }
    
    public override void CastUpdate()
    {
        
    }
    
    public override void CastEnd()
    {
        ATrinityGameManager.GetAnimator().AnimComponent.Play("Null", 1, 0f);
        ATrinityGameManager.GetBrain().bForcefieldActive = false;
        ForcefieldEffect.SetActive(false);
    }

    public void SpawnSparks(FHitInfo hitInfo)
    {
        if (hitInfo.CollisionData == null)
        {
            return;
        }
        
        if (ATrinityGameManager.GetBrain().bForcefieldActive)
        {
            GameObject sparks = Instantiate(ImpactFX, hitInfo.CollisionData.GetContact(0).point, Quaternion.identity);
            Destroy(sparks, 2f);
        }
    }
}
