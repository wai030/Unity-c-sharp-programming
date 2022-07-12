using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;

public class aiTest : MonoBehaviour
{
    [SerializeField] Transform ptr, tr, texttr;
    [SerializeField] Animator selfAnim;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CapsuleCollider2D pcc, kcc;
    [SerializeField] take_damage ad_takeD, kn_takeD;
    [SerializeField] state st = new state();
    [SerializeField] string idleAnim, parryAnim, walkAnim, dieAnim;
    [SerializeField] List<attackType> attackData;
    [Serializable] struct attackType
    {
        public string AnimName;
        public int dmg;
    }
    [SerializeField] GameObject dialogue;
    [SerializeField] bool mockFin;
    internal int direction = 0;
    internal bool inrange, wallcrashed;

    float xdtr, dasht = -3, attackTime = -3, bt = -10, lastattacktime = 0;
    
    int score = 0, i = 0, attack_last_i = 4, ttmpi = 0;
    bool is_dashing = false, invincible = false, dead = false, pause = false, can_attack = true, is_facing, _is_left = false, is_ground, alreadyDead = false, waitismock, isblocking = false;

    bool is_left
    {
        get
        {
            return _is_left;
        }
        set
        {
            if (_is_left == value)
                return;
            _is_left = value;
            lookwhere();
            updatefacing();
        }
    }
    enum mind { aggressive, passive, counter, mock }
    mind mand = new mind();
    mind md
    {
        get { return mand; }
        set
        {
            if (mand == value)
                return;
            mand = value;
            chooseState(value);
            DOit += isground;
        }
    }
    enum state
    {
        attack,
        walk,
        deflect,
        block,
        roll,
        idle,
        dead,
        dash
    }
    CancellationTokenSource cancel, mockaction;
    showcanvas show;
    detectplayer detect;
    event Action DOit;
    RaycastHit2D ra;
    LayerMask mask;
    private void Awake()
    {
        show = GameObject.FindObjectOfType<showcanvas>();
        detect = gameObject.GetComponentInChildren<detectplayer>();
        if (show != null)
            show.sendpause += checkPause;
        mask = LayerMask.GetMask("Platforms");
        cancel = new CancellationTokenSource();
        mockaction = new CancellationTokenSource();

    }// return pause

    void Start()
    {
        st = state.idle;
        DOit = aggressive;
        DOit += isground;
        mockFin = true;
    }
    void Update()
    {
        /*if (!isblocking)
            block();*/
        /*if (!dead)
        {
            if (!pause || !isblocking)
            {
                facing_dir();
                DOit?.Invoke();
            }
            else
            {
                selfAnim.Play(idleAnim);
            }
        }*/

    }

    private void FixedUpdate()
    {

        rb.velocity = new Vector2(-1 * Time.fixedDeltaTime, -9.81f);
    }

    //ai{
    public void determine()
    {
        if (!mockFin && Mathf.Abs(xdtr) < 7)//if mock not yet finish and player come close 
        {
            cancelAndCreate(ref mockaction);
            return;
        }
        if (!mockFin || Time.time < 10)
            return;
        if (ad_takeD.currentH <= 0.5 * ad_takeD.maxH)
            md = mind.aggressive;
        else
        {
            if (kn_takeD.currentH > 0.7 * kn_takeD.maxH)
            {
                if (ad_takeD.currentH > 0.5 * ad_takeD.maxH)
                    md = mind.counter;
            }
            else if (kn_takeD.currentH >= 0.3 * kn_takeD.maxH && kn_takeD.currentH <= 0.7 * kn_takeD.maxH)
                if (ad_takeD.currentH > 0.5 * ad_takeD.maxH)
                {
                    if (score >= 0)
                        md = mind.counter;
                    else if (score < 0)
                        md = mind.aggressive;
                }
                else if (kn_takeD.currentH < 0.3 * kn_takeD.maxH)
                    if (ad_takeD.currentH > 0.5 * ad_takeD.maxH)
                    {
                        if (score >= 0)
                            md = mind.passive;
                        else if (score < 0)
                            md = mind.aggressive;
                    }
        }
    }

    //mock timer{
    public void hitalert()
    {
        cancelAndCreate(ref cancel);
        MockTimer(cancel.Token);
    }

    void cancelAndCreate(ref CancellationTokenSource can)
    {
        can.Cancel();
        can.Dispose();
        can = new CancellationTokenSource();
    }

    async void MockTimer(CancellationToken token)
    {
        try
        {
            waitismock = true;
            await Task.Delay(20000, token);
            while (waitismock)
            {
                if (Mathf.Abs(xdtr) > 7)
                {
                    md = mind.mock;
                    mockFin = false;
                    dialogue.SetActive(true);
                    await Task.Delay(30000, mockaction.Token);
                    mockFin = true;
                    dialogue.SetActive(false);
                    cancelAndCreate(ref cancel);
                    waitismock = false;
                }
                await Task.Delay(1000);
            }
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)//cancel when got hitted
        {
            dialogue.SetActive(false);
            waitismock = false;
        }
        catch (System.OperationCanceledException) when (mockaction.IsCancellationRequested)//cancel if closer than 7 unit
        {
            dialogue.SetActive(false);
            waitismock = false;
            mockFin = true;
            cancelAndCreate(ref mockaction);
        }
    }
    //} end of mock
    void aggressive()
    {
        if (can_attack && !inrange)
        {
            if (st == state.block)
            {
                bt = -10;
                st = state.walk;
                selfAnim.Play(idleAnim);
            }
            if (can_attack && Mathf.Abs(xdtr) > 7 && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(1, 1));//dash attack
            }
            else if (can_attack)
            {
                walk(2f, 1f, 1, 1);
            }
        }
        else if (can_attack && inrange)
        {
            if (Time.time - attackTime > 3)
                attack();
            else
                kangoidle();
        }
    }

    void passive()
    {
        if (can_attack && !inrange)
        {
            if (selfAnim.GetBool("wall"))
            {
                tr.localScale = new Vector3(tr.localScale.x * -1 * 5, 1 * 5, 1);
                texttr.localScale = new Vector3(tr.localScale.x * -1, 1, 1);
                md = mind.counter;
            }
            if (can_attack && Mathf.Abs(xdtr) >= 13)
            {
                kangoidle();
                Debug.Log("heal");
            }
            else if (can_attack && Mathf.Abs(xdtr) < 4 && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(-1, -1));
            }
            else if (can_attack && Mathf.Abs(xdtr) < 13)
                walk(2, 1, -1, -1);


        }
        else if (can_attack && inrange)
        {
            if (can_attack && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(-1, -1));//dash away asap
            }
            else if (Mathf.Abs(xdtr) > 7)
                walk(2, 1, -1, -1); //walk away
        }
    }

    void counter()
    {
        if (can_attack && !inrange)
        {
            walk(1, 0.5f, 1, 1);
        }
        else if (can_attack && inrange)
        { // jump or
            if (Time.time - attackTime > 3 && is_facing)
            {
                can_attack = false;
                selfAnim.Play(parryAnim);
                attackTime = Time.time;
            }
            else
                kangoidle();
        }
    }

    void mocking()
    {
        Debug.Log("mock");
    }
    //}ai

    void walk(float speed, float playback, int walkforward, int lookToward) // 1 is true -1 is false
    {
        if (!inrange)
        {
            selfAnim.SetFloat("speed", 1 * playback);
            selfAnim.Play(walkAnim);
            st = state.walk;
            /*rb.velocity = new Vector2(walkforward * direction * Time.deltaTime * speed, 0);*/
            /*tr.Translate(walkforward * direction * Time.deltaTime * speed, 0, 0);*/
            tr.localScale = is_left ? new Vector3(lookToward * -1*5, 1 * 5, 1) : new Vector3(lookToward * 1 * 5, 1 * 5, 1);
            texttr.localScale = new Vector3(tr.localScale.x * -1, 1, 1);
        }
        else
        {
            kangoidle();
        }
    }

    void chooseState(mind a)
    {
        switch (a)
        {
            case aiTest.mind.aggressive:
                DOit = aggressive;
                break;
            case aiTest.mind.counter:
                DOit = counter;
                break;
            case aiTest.mind.passive:
                DOit = passive;
                break;
                /*case ai.mind.mock:
                    DOit = mocking;
                    break;*/
        }
    }

    //player facing, ai facing direction{
    void facing_dir()
    {
        xdtr = tr.position.x - ptr.position.x;
        is_left = xdtr > 0;
    }
    void lookwhere() //check which side ai should be looking at
    {
        direction = is_left ? -1 : 1;
    }

    public void updatefacing() //check whether player is facing ai
    {
        is_facing = is_left ? (ptr.localScale.x > 0 ? true : false) : (ptr.localScale.x > 0 ? false : true);
    }
    //}
    void checkPause()
    {
        pause = Time.timeScale == 0 ? true : false;
    }
    public void isground()
    {
        ra = Physics2D.CircleCast(new Vector2(tr.position.x, tr.position.y), 0.1f, Vector2.down * .1f, 1f, mask);
        is_ground = ra.collider != null;
    }


    //what can knight do{
    IEnumerator dash(int tow, int attack)
    {
        st = state.dash;
        can_attack = false;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(1000000f * direction * tow, 0f), ForceMode2D.Impulse);
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(0.2f);
        rb.gravityScale = gravity;
        rb.velocity = new Vector2(0f, 0f);
        can_attack = true;
        if (attack != 1)
            kangoidle();
    }

    IEnumerator roll()
    {
        selfAnim.Play("roll");
        Physics2D.IgnoreCollision(pcc, kcc, true);
        st = state.roll;
        can_attack = false;
        if (is_left)
            tr.localScale = new Vector3(-1, 1, 1);
        else
            tr.localScale = new Vector3(1, 1, 1);
        st = state.roll;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(130000f * direction, 0f), ForceMode2D.Impulse);
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(0.7f);
        rb.gravityScale = gravity;
        rb.velocity = new Vector2(0f, 0f);
        Physics2D.IgnoreCollision(pcc, kcc, false);
        can_attack = true;
        kangoidle();
    }

    void attack()
    {
        i = UnityEngine.Random.Range(0, 3);
        if (attack_last_i != i)
            attack_last_i = i;
        else
        {
            switch (i)
            {
                case 0:
                    i = UnityEngine.Random.Range(1, 3);
                    break;
                case 1:
                    ttmpi = UnityEngine.Random.Range(0, 2);
                    i = ttmpi == 0 ? 0 : 2;
                    break;
                case 2:
                    i = UnityEngine.Random.Range(0, 2);
                    break;
            }
            attack_last_i = i;
        }
        selfAnim.Play(attackData[i].AnimName);
        can_attack = false;
        st = state.attack;
        attackTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("hit player w/" + i);
            collision.GetComponent<take_damage>()?.takedamage(attackData[i].dmg);
        }
    }
    async void block() //also check block ignore which is animation event in block anim + also need to -0.5 second because the block animation
    {
        isblocking = true;
        selfAnim.Play("block");
        await Task.Delay(2000);
        Physics2D.IgnoreLayerCollision(7, 11, false);
        selfAnim.speed = 1;
        selfAnim.Play("unblock");

    }

    //}what can knight do
    //
    //anim-function{
    void already_dead()
    {
        Physics2D.IgnoreCollision(pcc, kcc, true);
        alreadyDead = true;
    }

    public void vincible()
    {
        invincible = true;
    }

    public void vinciblenot()
    {
        invincible = false;
    }

    public void blockIgnore()
    {
        selfAnim.speed = 0;
        Physics2D.IgnoreLayerCollision(7, 11, true);
    }
    public void blockFin()
    {
        isblocking = false;
    }
    void playandcheck(string s)//control anim
    {
        selfAnim.Play(s);
    }
    //}anim-function

    public void kangoidle()
    {
        selfAnim.Play(idleAnim);
        st = state.idle;
        can_attack = true;
    }
    void died()
    {
        dead = true;
        selfAnim.Play(dieAnim);
        st = state.dead;
        kn_takeD.enabled = false;
        StartCoroutine(extendDeath());
    }

    IEnumerator extendDeath()
    {
        rb.velocity = new Vector2(0, -9.8f);
        while (!is_ground)//test when jump added
        {
            yield return new WaitForSeconds(0.3f);
        }
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(3f);
        if (dead && alreadyDead && is_ground)
        {
            foreach (Transform child in tr.transform.GetChild(0))
                Destroy(child.gameObject);
            Destroy(kn_takeD);
            Destroy(this);
        }
    }// wrap things up after knight dead

}

