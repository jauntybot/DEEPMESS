using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloatedBulb : GridElement {

    public override event OnElementUpdate ElementDestroyed;

    public override void Init(Grid g, Vector2 c) {
        base.Init(g, c);
        string name = gfxAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        gfxAnim.Play(name, 0, Util.Remap(8 + (int)coord.x - (int)coord.y, 1, 15, 1, 18));
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) 
            destroyed = true;

        ElementDestroyed?.Invoke(this);
        ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        
        
        gfxAnim.SetTrigger("Destroy");
        PlaySound(destroyedSFX);
        float timer = 0f;
        while (timer < 1f) {
            yield return null;
            timer += Time.deltaTime;
        }

        if (gameObject != null)
            Destroy(gameObject);
    }

}
