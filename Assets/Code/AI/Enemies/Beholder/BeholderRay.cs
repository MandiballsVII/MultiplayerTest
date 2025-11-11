using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BeholderRay : MonoBehaviour
{
    public float lifeTime = 4f;
    public float damage = 10f;

    Vector2 direction;
    public float speed = 10f;
    float born;

    public void Init(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        born = Time.time;
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
        if (Time.time > born + lifeTime)
        {
            //print("BeholderRay expired");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Vector2 hitPoint = gameObject.transform.position;
        //print("BeholderRay hit " + col.name);
        // ajusta la comprobación según tu Player script / tags
        if (col.TryGetComponent<PlayerStatus>(out var status))
        {
            // efecto aleatorio
            int effect = Random.Range(0, 4);
            switch (effect)
            {
                case 0: status.ApplySlow(0.5f, 3f); break;
                case 1: status.ApplyImmobilize(2f); break;
                case 2: status.ApplyDamage((int)damage, hitPoint); break;
                case 3: status.ApplySilence(3f); break;
            }
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
