using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class NPC_SpellCaster : NPC_ControllerBase
{
    [Header("SpellCaster")]
    public SpellData spell; // Hechizo asignado a este NPC
    private float spellCooldownTimer = 0f;

    [Header("Summon settings")]
    public float summonDistance = 4f; // distancia a mantener del objetivo al invocar

    protected override void UpdateState()
    {
        if (spell == null) return;

        // Reducir cooldown del hechizo
        spellCooldownTimer -= Time.deltaTime;

        switch (spell.type)
        {
            case SpellType.Projectile:
            case SpellType.Area:
                HandleRangedBehavior();
                break;
            case SpellType.Summon:
                HandleSummonerBehavior();
                break;
            case SpellType.Self:
                HandleSelfSupportBehavior();
                break;
        }
    }

    // ================= RANGED / PROJECTILE =================
    void HandleRangedBehavior()
    {
        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= attackRange)
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 1);
                TryCastSpell(target.position);
            }
            else
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 0);
                ChaseTarget(target.position);
            }
        }
        else if (lastKnownPosition.HasValue)
        {
            currentState = StateMachine.Search;
            animator?.SetInteger("State", 0);
            SearchLastKnown();
        }
        else
        {
            currentState = StateMachine.Patrol;
            animator?.SetInteger("State", 0);
            Patrol();
        }
    }

    // ================= SUMMONER =================
    void HandleSummonerBehavior()
    {
        // Buscar jugadores visibles
        Transform playerTarget = target;

        if (playerTarget != null)
        {
            currentState = StateMachine.Engage;
            animator?.SetInteger("State", 0);

            // Mantener distancia
            float dist = Vector2.Distance(transform.position, playerTarget.position);
            if (dist < summonDistance)
            {
                Vector2 dir = (transform.position - playerTarget.position).normalized;
                transform.position += (Vector3)(dir * speed * Time.deltaTime);
            }

            // Intentar invocar
            if (spellCooldownTimer <= 0f)
            {
                CastSummon();
                spellCooldownTimer = spell.cooldown;
            }
        }
        else
        {
            currentState = StateMachine.Patrol;
            animator?.SetInteger("State", 0);
            Patrol();
        }
    }

    // ================= SELF / SUPPORT =================
    void HandleSelfSupportBehavior()
    {
        // Buscar aliados vivos
        NPC_ControllerBase[] allies = FindObjectsOfType<NPC_ControllerBase>();
        NPC_ControllerBase targetAlly = null;
        float lowestRatio = 1f;

        foreach (var ally in allies)
        {
            if (ally == this) continue;
            if (!ally.health.IsAlive) continue;
            if (ally.faction != this.faction) continue;

            float ratio = ally.health.currentHealth / ally.health.maxHealth;
            if (ratio < lowestRatio)
            {
                lowestRatio = ratio;
                targetAlly = ally;
            }
        }

        if (targetAlly != null)
        {
            // Hacer chase al aliado si está lejos
            float dist = Vector2.Distance(transform.position, targetAlly.transform.position);
            if (dist > 1f)
                ChaseTarget(targetAlly.transform.position);

            // Lanzar spell si cooldown listo
            if (spellCooldownTimer <= 0f)
            {
                TryCastSpell(targetAlly.transform.position);
                spellCooldownTimer = spell.cooldown;
            }
        }
        else if (target != null)
        {
            // No hay aliados, hay enemigos -> huir
            Vector2 dir = (transform.position - target.position).normalized;
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
        }
        else
        {
            // Ni aliados ni enemigos -> patrulla
            currentState = StateMachine.Patrol;
            animator?.SetInteger("State", 0);
            Patrol();
        }
    }

    // ================= MOVIMIENTO =================
    void ChaseTarget(Vector3 destination)
    {
        // Movimiento directo si hay LOS
        if (target != null && HasLineOfSight(target))
        {
            RotateTowards(destination);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            return;
        }

        // Si no hay LOS, usamos pathfinding
        if (currentNode != null && nodeManager != null)
        {
            Node tNode = GetClosestNode(destination);
            path = AStarManager.instance.GeneratePath(currentNode, tNode, radiusInTiles);
        }
    }

    void Patrol()
    {
        if (path.Count == 0)
        {
            Node randomNode = nodeManager.GetRandomNode();
            if (randomNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, randomNode, radiusInTiles);
        }
    }

    void SearchLastKnown()
    {
        if (!lastKnownPosition.HasValue) return;

        currentNode = GetClosestNode(transform.position);

        if (path.Count == 0)
        {
            Node goal = nodeManager.GetClosestNode(lastKnownPosition.Value);
            if (goal != null)
                path = AStarManager.instance.GeneratePath(currentNode, goal, radiusInTiles);
        }
    }

    // ================= SPELL CAST =================
    void TryCastSpell(Vector3 targetPos)
    {
        if (spell == null || spell.prefab == null) return;

        var spellGO = Instantiate(spell.prefab, transform.position, Quaternion.identity);

        // Si es Projectile o Area, apuntar hacia target
        if (spell.type == SpellType.Projectile || spell.type == SpellType.Area)
        {
            Vector2 dir = (targetPos - transform.position).normalized;
            spellGO.transform.right = dir;

            if (spell.type == SpellType.Projectile && spellGO.TryGetComponent<Rigidbody2D>(out var rb))
                rb.velocity = dir * spell.projectileSpeed;
        }

        // Invocados y buffs se manejan según SpellData (puedes añadir Init si hace falta)
    }

    void CastSummon()
    {
        if (spell.summonPrefab == null) return;

        for (int i = 0; i < spell.summonCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            var summon = Instantiate(spell.summonPrefab, spawnPos, Quaternion.identity);

            // Pasar Owner
            var npc = summon.GetComponent<NPC_ControllerBase>();
            if (npc != null)
                npc.Init(transform);

            // Destruir tras duración
            if (spell.summonDuration > 0)
                Destroy(summon, spell.summonDuration);
        }
    }
}
