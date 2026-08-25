using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using ApexArena.Gameplay;

namespace ApexArena.AI
{
    /// <summary>
    /// متحكم الذكاء الاصطناعي - يدير سلوك الأعداء
    /// AI Controller - manages enemy behavior
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(WeaponSystem))]
    public class AIController : MonoBehaviour, Gameplay.IDamageable
    {
        [Header("AI Type")]
        [SerializeField] private AIType aiType = AIType.Cyborg;

        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private float detectionRange = 50f;
        [SerializeField] private float attackRange = 30f;
        [SerializeField] private float moveSpeed = 3.5f;

        [Header("Behavior")]
        [SerializeField] private float aggression = 0.7f; // 0-1
        [SerializeField] private float reactionTime = 0.5f;

        [Header("Components")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private Transform target;

        private AIState currentState = AIState.Patrol;
        private Vector3 patrolCenter;
        private float lastTargetCheck;
        private bool isAlive = true;

        public bool IsAlive => isAlive;
        public AIType AiType => aiType;

        public event System.Action OnAIDeath;

        private void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (weaponSystem == null) weaponSystem = GetComponent<WeaponSystem>();

            agent.speed = moveSpeed;
            currentHealth = maxHealth;
            patrolCenter = transform.position;
        }

        private void Update()
        {
            if (!isAlive) return;

            UpdateState();
            ExecuteState();
        }

        private void UpdateState()
        {
            // البحث عن هدف
            if (Time.time - lastTargetCheck > reactionTime)
            {
                lastTargetCheck = Time.time;
                FindTarget();
            }

            // تحديد الحالة
            if (target == null)
            {
                currentState = AIState.Patrol;
            }
            else if (currentHealth < maxHealth * 0.3f && aggression < 0.5f)
            {
                currentState = AIState.Retreat;
            }
            else if (Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                currentState = AIState.Attack;
            }
            else
            {
                currentState = AIState.Chase;
            }
        }

        private void ExecuteState()
        {
            switch (currentState)
            {
                case AIState.Patrol:
                    Patrol();
                    break;
                case AIState.Chase:
                    Chase();
                    break;
                case AIState.Attack:
                    Attack();
                    break;
                case AIState.Retreat:
                    Retreat();
                    break;
            }
        }

        private void FindTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, LayerMask.GetMask("Player"));
            float closestDist = float.MaxValue;
            Transform closest = null;

            foreach (var hit in hits)
            {
                var player = hit.GetComponent<Gameplay.PlayerController>();
                if (player != null && player.IsAlive)
                {
                    float dist = Vector3.Distance(transform.position, player.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = player.transform;
                    }
                }
            }

            target = closest;
        }

        private void Patrol()
        {
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * 20f;
                randomPoint.y = patrolCenter.y;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        private void Chase()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }

        private void Attack()
        {
            if (target != null)
            {
                // التوقف والتصويب
                agent.isStopped = true;

                // التوجه نحو الهدف
                Vector3 lookPos = target.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);

                // إطلاق النار
                weaponSystem?.Fire();
            }
        }

        private void Retreat()
        {
            if (target != null)
            {
                Vector3 retreatDir = (transform.position - target.position).normalized;
                Vector3 retreatPos = transform.position + retreatDir * 30f;

                if (NavMesh.SamplePosition(retreatPos, out NavMeshHit hit, 30f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        public void TakeDamage(int damage, Gameplay.DamageType damageType)
        {
            if (!isAlive) return;

            currentHealth -= damage;

            // زيادة العدوانية عند تلقي الضرر
            aggression = Mathf.Min(1f, aggression + 0.1f);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isAlive = false;
            agent.isStopped = true;

            // TODO: Drop loot
            // TODO: Play death animation

            OnAIDeath?.Invoke();

            // Destroy after delay
            Destroy(gameObject, 3f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    public enum AIType
    {
        Cyborg,
        Drone,
        Beast
    }

    public enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Retreat,
        Dead
    }
}
