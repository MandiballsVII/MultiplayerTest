using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BeholderRay : MonoBehaviour
{
    public float lifeTime = 4f;
    public float damage = 10f;

    Vector2 direction;
    public float speed = 10f;
    float born;

    public Faction faction;

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

        if (Time.time > born + lifeTime)
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
            // efecto aleatorio
            int effect = Random.Range(0, 4);
            Vector2 hit = transform.position;

            switch (effect)
            {
                case 0: status.ApplySlow(0.5f, 3f); break;
                case 1: status.ApplyStun(2f); break;
                case 2: status.ApplyDamage(damage, hit); break;
                case 3: status.ApplySilence(3f); break;
            }
            Destroy(gameObject);
            return;
        }
        else if (col.TryGetComponent<Health>(out var h))
        {
            // NO tiene efectos de estado -> aplicar daño directo
            h.TakeDamage(damage, transform.position);
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
