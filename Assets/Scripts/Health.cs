using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Refs")]
    public HmdFreeze hmdFreeze;
    public EnemyChase1 enemyChase;
    public TextMeshProUGUI stateText;

    [Header("Player")]
    public int playerMaxHits = 5;
    public int playerHitsTaken = 0;
    public bool playerDead = false;
    public int swordDmgHead = 5;
    public int swordDmgHeadless = 20;

    [Header("Enemy")]
    public int enemyMaxHealth = 100;
    public int enemyHealth = 100;
    public bool enemyDead = false;
    public int enemyHitsTaken = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip playerHit;
    public AudioClip enemyHit1;
    public AudioClip enemyHit2;

    void Start()
    {
        playerHitsTaken = 0;
        playerDead = false;

        enemyHealth = enemyMaxHealth;
        enemyDead = false;

        if(stateText != null)
        {
            stateText.text = "";
        }

    }

    private void Update()
    {
        if (stateText == null) return;

        if (playerDead)
        {
            stateText.text = "DEFEAT!";
        } 
        else if (enemyDead)
        {
            stateText.text = "VICTORY!";
        }
        else
        {
            stateText.text = "";
        }
    }


    public void DamagePlayerFromEnemy()
    {
        if (playerDead) return;

        bool headless = hmdFreeze != null && hmdFreeze.headless;

        // Player is invulnerable in headless mode
        if (headless)
            return;

        playerHitsTaken++;

        PlayClip(playerHit);

        Debug.Log("Player hit. Hits taken: " + playerHitsTaken + " / " + playerMaxHits);

        if (playerHitsTaken >= playerMaxHits)
        {
            playerDead = true;
            Debug.Log("Player died.");

            if (enemyChase != null)
            {
                enemyChase.TriggerVictory();

            }
                
        }
    }

    public void DamageEnemyFromSword()
    {
        if (enemyDead) return;

        bool headless = hmdFreeze != null && hmdFreeze.headless;
        int damage = headless ? swordDmgHeadless : swordDmgHead;

        enemyHealth -= damage;
        enemyHitsTaken++;

        Debug.Log("Enemy hit " + enemyHitsTaken + " times. Remaining HP: " + enemyHealth + " / " + enemyMaxHealth);

        if (enemyHealth <= 0)
        {
            enemyHealth = 0;
            enemyDead = true;

            Debug.Log("Enemy died after " + enemyHitsTaken + " hits.");

            if (enemyChase != null)
                enemyChase.Die();

            return;
        }

        if (headless)
        {
            PlayClip(enemyHit2);
            if (enemyChase != null) enemyChase.TriggerHit2();
        }
        else
        {
            PlayClip(enemyHit1);
            if (enemyChase != null) enemyChase.TriggerHit();
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}