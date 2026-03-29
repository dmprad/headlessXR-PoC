using UnityEngine;
using UnityEngine.AI;

public class EnemyChase1 : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Animator anim;
    public Health health;
    public Collider col;


    [Header("Targets")]
    public Transform normalTarget;
    public Transform editorTarget;
    public Transform frozenAnchor;
    public HmdFreeze hmdFreeze;
    public bool inEditor = false;

    [Header("Chase")]
    public float repathRate = 0.2f;
    public float faceTargetSpeed = 6f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;

    [Header("Headless Behaviour")]
    public float headlessIdleSeconds = 2f;

    [Header("Animation Durations")]
    public float attackDuration = 1.0f;
    public float hitDuration = 0.6f;

    [Header("State")]
    public bool isDead = false;
    public bool inVictory = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip victory;
    public AudioClip defeat;

    float repathTimer;
    float attackCooldownTimer;

    bool lastHeadless;
    bool inHeadlessIdle;
    float headlessIdleTimer;

    float stateLockTimer;
    int lockedMovingValue = -1;

    bool damageAppliedThisAttack = false;


    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        SetIdle();
    }

    void Update()
    {
        if (agent == null || anim == null)
            return;

        if (isDead)
        {
            agent.isStopped = true;
            return;
        }

        if (inVictory)
        {
            agent.isStopped = true;
            anim.SetInteger("battle", 1);
            anim.SetInteger("moving", 6); // victory
            return;
        }



        bool headless = hmdFreeze != null && hmdFreeze.headless;

        if (headless && !lastHeadless)
            StartHeadlessIdle();

        lastHeadless = headless;

        attackCooldownTimer -= Time.deltaTime;
        stateLockTimer -= Time.deltaTime;

        Transform target = inEditor ? editorTarget : (headless ? frozenAnchor : normalTarget);

        

        if (target == null)
        {
            SetIdle();
            return;
        }

        if (inHeadlessIdle)
        {
            headlessIdleTimer -= Time.deltaTime;
            agent.isStopped = true;
            SetIdleBattle();

            if (headlessIdleTimer <= 0f)
            {
                inHeadlessIdle = false;
                agent.isStopped = false;
                repathTimer = repathRate;
            }

            return;
        }

        // attack / hit lock
        if (stateLockTimer > 0f)
        {
            agent.isStopped = true;
            anim.SetInteger("moving", lockedMovingValue);
            return;
        }

        Vector3 enemyFlat = transform.position;
        Vector3 targetFlat = target.position;
        enemyFlat.y = 0f;
        targetFlat.y = 0f;

        float distance = Vector3.Distance(enemyFlat, targetFlat);
        bool inAttackRange = distance <= attackRange;

        if (!inAttackRange)
        {
            agent.isStopped = false;

            repathTimer += Time.deltaTime;
            if (repathTimer >= repathRate)
            {
                repathTimer = 0f;
                agent.SetDestination(target.position);
            }

            SetWalk();
        }
        else
        {
            agent.isStopped = true;

            Vector3 dir = target.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRot,
                    faceTargetSpeed * Time.deltaTime
                );
            }

            if (!isDead && !inVictory && stateLockTimer <= 0f && attackCooldownTimer <= 0f)
            {
                attackCooldownTimer = attackCooldown;
                LockMovingState(5, attackDuration); // attack3
            }
            else
            {
                SetIdleBattle();
            }
        }

        CheckAttackDamage();
    }

    public void DealDamageToPlayer()
    {
        if (health != null)
            health.DamagePlayerFromEnemy();
    }

    void CheckAttackDamage()
    {
        if (isDead || inVictory)
        {
            damageAppliedThisAttack = false;
            return;
        }

        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("attack 3"))
        {
            if (!damageAppliedThisAttack && state.normalizedTime >= 0.67f)
            {
                DealDamageToPlayer();
                damageAppliedThisAttack = true;
            }
        }
        else
        {
            damageAppliedThisAttack = false;
        }
    }


    void StartHeadlessIdle()
    {
        inHeadlessIdle = true;
        headlessIdleTimer = headlessIdleSeconds;
        attackCooldownTimer = Mathf.Max(attackCooldownTimer, 0.25f);
    }

    void LockMovingState(int movingValue, float duration)
    {
        lockedMovingValue = movingValue;
        stateLockTimer = duration;
        anim.SetInteger("moving", movingValue);
    }

    void SetIdle()
    {
        anim.SetInteger("battle", 0); // idle
        anim.SetInteger("moving", 0);
    }

    void SetIdleBattle()
    {
        anim.SetInteger("battle", 1); // idle battl
        anim.SetInteger("moving", 0);
    }

    void SetWalk()
    {
        anim.SetInteger("battle", 0); // battle mode off
        anim.SetInteger("moving", 1); // walk
    }

    public void TriggerHit()
    {
        if (isDead || inVictory) return;

        if (stateLockTimer > 0f && lockedMovingValue == 13)         //neu
        {
            Debug.Log("Hit animation already playing, ignoring duplicate");
            return;
        }

        Debug.Log($"TriggerHit - starting hit animation at {Time.time}");
        agent.isStopped = true;

        agent.ResetPath();                  //neu
        stateLockTimer = 0f;
        attackCooldownTimer = 0f;

        anim.SetInteger("battle", 1);
        lockedMovingValue = 13;               //neu
        stateLockTimer = hitDuration;
        anim.SetInteger("moving", 13);
        //LockMovingState(13, hitDuration); // hit1

        Debug.Log($"Hit animation started, timer set to {hitDuration}");
    }

    public void TriggerHit2()
    {
        if (isDead || inVictory) return;

        agent.isStopped = true;
        anim.SetInteger("battle", 1);
        LockMovingState(14, hitDuration); // hit2, falls 14 korrekt ist
    }


    public void Die()
    {
        if (isDead) return;

        Debug.Log("Die wird ausgeführt!");
        isDead = true;
        inVictory = false;
        agent.isStopped = true;
        agent.ResetPath();

        stateLockTimer = 0f;
        lockedMovingValue = -1;
        damageAppliedThisAttack = false;

        anim.SetInteger("battle", 1);
        anim.SetInteger("moving", 9); // death1

        if(col != null)
        {
            col.enabled = false;
        }

        if(victory != null)
        {
            audioSource.volume = 0.5f;
            PlayClip(victory);
        }

    }

    public void TriggerVictory()
    {
        if (isDead) return;

        Debug.Log("Player defeated.");

        inVictory = true;
        stateLockTimer = 0f;
        lockedMovingValue = -1;

        if (col != null)
        {
            col.enabled = false;
        }


        agent.isStopped = true;
        anim.SetInteger("battle", 1);
        anim.SetInteger("moving", 6); // victory

        if(defeat != null)
        {
            audioSource.volume = 0.5f;
            PlayClip(defeat);
        }

    }

    public void SetHeadless(bool value)
    {
        /*headless = value;
        if (headless && !lastHeadless)
            StartHeadlessIdle();
        lastHeadless = headless;   */
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

}