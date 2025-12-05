using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BeholderRay : MonoBehaviour
{
    Vector2 direction;
    public float speed = 10f;
    float born;

    public Faction faction;

    public SpellData spellData;

    public void Init(Vector2 dir, float spd, Faction casterFaction)
    {
        direction = dir.normalized;
        speed = spd;
        born = Time.time;
        faction = casterFaction;

        IgnoreSameFactionCollisions();
    }
    void IgnoreSameFactionCollisions()
    {
        var myCol = GetComponent<Collider2D>();
        var allies = FactionManager.Instance.GetColliders(faction);
        foreach (var allyCol in allies)
        {
            print("BeholderRay ignoring collision with ally " + allyCol.name);
            if (allyCol != null && myCol != null)
                Physics2D.IgnoreCollision(myCol, allyCol, true);
        }
    }



    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Time.time > born + spellData.duration)
        {
            //print("BeholderRay expired");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // No impactar aliados
        var fac = col.GetComponent<IFactionMember>();
        if (fac != null && fac.Faction == faction)
            return;
        Vector2 hitPoint = gameObject.transform.position;
        //print("BeholderRay hit " + col.name);
        // ajusta la comprobación según tu Player script / tags
        if (col.TryGetComponent<StatusEffectHandler>(out var status))
        {
            status.ApplyDamage(spellData.power, transform.position);
            SpellEffectApplier.ApplyRandomEffect(spellData, col, transform.position);
            Destroy(gameObject);
            return;
        }
        else if (col.TryGetComponent<Health>(out var h))
        {
            // NO tiene efectos de estado -> aplicar daño directo
            h.TakeDamage(spellData.power, transform.position, spellData);
            Destroy(gameObject);
            return;
        }

        // destruye si golpea un muro u otra cosa (opcional)
        if (col.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Destroy(gameObject);
        }
    }

}
