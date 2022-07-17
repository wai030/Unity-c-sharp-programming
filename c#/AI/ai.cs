using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;

public class ai : MonoBehaviour
{
    private class aiattInt
    {
        ai AI;
        public aiattInt(ai ai)
        {
            this.AI = ai;
        }

        int _value = 0;
        internal int value
        {
            get { return _value; }
            set
            {
                _value = value;
                AI.cal();
            }
        }
    }
    [SerializeField] internal Transform tr;
    [SerializeField] Transform ptr,  texttr;
    [SerializeField] CapsuleCollider2D kcc, pcc;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator selfAnim;
    [SerializeField] take_damage ad_takeD, kn_takeD;
    [SerializeField] score Score;
    [SerializeField] SpriteRenderer SR;
    [SerializeField] string idleAnim, parryAnim, walkAnim, dieAnim;
    [SerializeField] List<attackType> attackData;
    [Serializable] struct attackType
    {
        public string AnimName;
        public int dmg;
    }
    [SerializeField] GameObject dialogue, self; 
    internal float aiAccuracy, xdtr,ok;
    internal int direction = 0;
    internal bool inrange, wallcrashed, isblocking;
    float dasht = -3, attackTime = -3, speed = 0, determineTime;
    int score = 0, RandomAtIndex = 0, attack_last_i = 4, midAtIndex = 0, walkforward = 0, walkis1 = 0, ranAtpercent=0;
    bool is_dashing = false, dead = false, pause = false, can_attack = true, is_facing, _is_left = false, is_ground, alreadyDead = false, waitismock, mockFin, haveMock = false, stop;

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
    CancellationTokenSource cancel, mockaction, blockAct;
    showcanvas show;
    detectplayer detect;
    event Action DOit;
    RaycastHit2D ra;
    LayerMask mask;
    aiattInt hit, press;

    private void Awake()
    {
        show = GameObject.FindObjectOfType<showcanvas>();
        detect = gameObject.GetComponentInChildren<detectplayer>();
        if (show != null)
            show.sendpause += checkPause;
        mask = LayerMask.GetMask("Platforms");
        cancel = new CancellationTokenSource();
        mockaction = new CancellationTokenSource();
        blockAct = new CancellationTokenSource();

    }// return pause

    void Start()
    {
        DOit = aggressive;
        DOit += isground;
        mockFin = true;
        hit = new aiattInt(this);
        press = new aiattInt(this);
    }
    void Update()
    {
        if (!dead)
        {
            if (!pause)
            {
                if (stop || isblocking)
                {
                    rb.velocity = new Vector2(0, 0);
                    facing_dir();
                    return;
                }
                facing_dir();
                determine();
                DOit?.Invoke();
            }
            else
            {
                selfAnim.Play(idleAnim);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isblocking)
            rb.velocity = new Vector2(0, -9.81f);  
        if (is_dashing)
            return;
            rb.velocity = new Vector2(walkforward * direction * Time.fixedDeltaTime * walkis1 * speed * 50, -9.81f);//walk 
    }

    //ai{
    public void determine()
    {
        if (!can_attack || isblocking)
            return;
        if (!mockFin && Mathf.Abs(xdtr) < 5)//if mock not yet finish and player come close 
        {
            cancelAndCreate(ref mockaction);//mockaction will cancel the mocking in mock timer
        }
        if (kn_takeD.currentH <= 0.3 * kn_takeD.maxH&& haveMock==true)
        {
            md = mind.passive;
            return;
        }

        if (Time.time - determineTime < 3)
            return;
        stateCal(Score.calculate());
        determineTime = Time.time;
        if (UnityEngine.Random.Range(0, 100) < score)
            md = mind.counter;
        else
            md = mind.aggressive;
    }

    void stateCal(int score)
    {
        if (score > 50 || score < -50)
            return;
        this.score = 50 + score;
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
                if (Mathf.Abs(xdtr) > 5)
                {
                    md = mind.mock;
                    mockFin = false;//set this in determine can focus on mocking on them when not in other state
                    dialogue.SetActive(true);
                    await Task.Delay(30000, mockaction.Token);
                    mockFin = true;
                    dialogue.SetActive(false);
                    cancelAndCreate(ref cancel);
                    waitismock = false;
                    haveMock = true;//mocked player
                }
                await Task.Delay(1000);//if player is in range and ai want to mock, it will wait until he move away first
            }
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)//cancel when got hitted
        {
            dialogue.SetActive(false);
            waitismock = false;
        }
        catch (System.OperationCanceledException) when (mockaction.IsCancellationRequested)//cancel if closer than 5 unit
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
            if (can_attack && Mathf.Abs(xdtr) > 10 && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(1, 0, 15));//dash 
            }
            if(Mathf.Abs(xdtr) > 3&& Mathf.Abs(xdtr) <3.5 && Time.time - dasht > 3)//dash attack
            {
                dasht = Time.time;
                StartCoroutine(dash(1, -1, 10f));
            }
            else if (can_attack)
                walk(2f, 1f, 1, 1);
            return;
        }
        else if (can_attack && inrange)
        {
            if (Time.time - attackTime > 2.5)
            {
                attackTime = Time.time;
                ranAtpercent = UnityEngine.Random.Range(0, 100);
                if (ranAtpercent < 50)
                    attack(true);
                else if(ranAtpercent<75)
                    StartCoroutine(dash(1, -1, 10f, 1));
                else
                    StartCoroutine(dash(1, -1, 10f, 2));
            }
                
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
                texttr.localScale = new Vector3(tr.localScale.x/5 * -1, 1, 1);
                md = mind.counter;
                haveMock = false;
            }
            if (can_attack && Mathf.Abs(xdtr) >= 13)
            {
                kangoidle();
                heal();
            }
            else if (can_attack && Mathf.Abs(xdtr) < 4 && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(-1, -1,15));
            }
            else if (can_attack && Mathf.Abs(xdtr) < 13)
                walk(2, 1, -1, -1);


        }
        else if (can_attack && inrange)
        {
            if (can_attack && Time.time - dasht > 3)
            {
                dasht = Time.time;
                StartCoroutine(dash(-1, -1,15));//dash away asap
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
                ranAtpercent = UnityEngine.Random.Range(0, 100);
                attackTime = Time.time;
                if (ranAtpercent < 30)
                {
                    StartCoroutine(dash(-1, 0, 10f));
                }
                else if (ranAtpercent < 60)
                    StartCoroutine(dash(-1, 1, 15f, useForce:false));
                else if (ranAtpercent < 90)
                    StartCoroutine(dash(-1, 1, 15f));
                else
                {
                    block(blockAct);
                }
                    
            }

            else
                kangoidle();
        }
    }

    //}ai

    void walk(float speed, float playback, int walkforward, int lookToward) // 1 is true -1 is false
    {
        if (!inrange)
        {
            selfAnim.SetFloat("speed", 1 * playback);
            selfAnim.Play(walkAnim);
            this.walkforward=walkforward;
            this.speed = speed;
            walkis1 = 1;
            /*rb.velocity = new Vector2(walkforward * direction * Time.deltaTime * speed, 0);
            tr.Translate(walkforward * direction * Time.deltaTime * speed, 0, 0);*/
            tr.localScale = is_left ? new Vector3(lookToward * -1 * 5, 1 * 5, 1) : new Vector3(lookToward * 1 * 5, 1 * 5, 1);
            texttr.localScale = new Vector3(tr.localScale.x/5 * -1, 1, 1);
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
            case ai.mind.aggressive:
                DOit = aggressive;
                break;
            case ai.mind.counter:
                DOit = counter;
                break;
            case ai.mind.passive:
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
        if (isblocking)
            if ((xdtr < 0 && tr.localScale.x < 0) || (xdtr > 0 && tr.localScale.x > 0))
            {
                suddenEndBlock();
            }
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
    IEnumerator dash(int tow, int attack, float force, int atI = 1, float dashT = 0.2f, bool useForce = true) //tow mean to where, attack =0 is break, force 15 is cool, atI= atindex
    {
        can_attack = false;
        is_dashing = true;
        Physics2D.IgnoreLayerCollision(7, 11, true);
        Physics2D.IgnoreCollision(pcc, kcc, true);
        rb.AddForce(new Vector2(force * direction * tow, 0f), ForceMode2D.Impulse);
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(dashT);
        rb.velocity = new Vector2(0f, 0f);
        rb.gravityScale = gravity;
        can_attack = true;
        is_dashing = false;
        Physics2D.IgnoreLayerCollision(7, 11, false);
        Physics2D.IgnoreCollision(pcc, kcc, false);
        if (attack == 0)
        {
            kangoidle();
            yield break;
        }

        if(attack == 1)//dash back and forward
        {
            stop = true;
            selfAnim.Play("idle");
            walkis1 = 0;
            yield return new WaitForSeconds(0.5f);
            walkis1 = 1;
            dasht = Time.time;
            attackTime = Time.time;
            stop = false;
            if(useForce)
                StartCoroutine(dash(1, -1, force,2, dashT:0.1f));
            else
                StartCoroutine(dash(1, -1, force*2, 2, dashT: 0.1f));
            yield return new WaitForSeconds(0.3f);
            
            
        }
        else if (attack == -1)//back at
        {
            tr.localScale = new Vector3(direction * 5, 1 * 5, 1);
            this.attack(false, atI);   
        }
    }
    //perhaps need remake

    IEnumerator roll()
    {
        selfAnim.Play("roll");
        Physics2D.IgnoreCollision(pcc, kcc, true);
        can_attack = false;
        if (is_left)
            tr.localScale = new Vector3(-1, 1, 1);
        else
            tr.localScale = new Vector3(1, 1, 1);
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

    //attack functions{
    void attack(bool RandomAttack, int AtIndex=0)
    {
        if (RandomAttack)
        {
            RandomAtIndex = UnityEngine.Random.Range(0, 3);
            if (attack_last_i != RandomAtIndex)
                attack_last_i = RandomAtIndex;
            else
            {
                switch (RandomAtIndex)
                {
                    case 0:
                        RandomAtIndex = UnityEngine.Random.Range(1, 3);
                        break;
                    case 1:
                        midAtIndex = UnityEngine.Random.Range(0, 2);
                        RandomAtIndex = midAtIndex == 0 ? 0 : 2;
                        break;
                    case 2:
                        RandomAtIndex = UnityEngine.Random.Range(0, 2);
                        break;
                }
                attack_last_i = RandomAtIndex;
            }
        }
        else
        {
            RandomAtIndex = AtIndex;
            attack_last_i = AtIndex;
        }
        selfAnim.Play(attackData[RandomAtIndex].AnimName);
        addAndminus(press, 1, 10);
        can_attack = false;
        attackTime = Time.time;
        walkis1 = 0;
    }

    async void addAndminus(aiattInt input, int addnum, int time)
    {
        input.value += addnum;
        await Task.Delay(time * 1000);
        input.value -= addnum;
    }

    public void cal()
    {
        if (press.value != 0)
            aiAccuracy = (float)hit.value / press.value;
        else
            aiAccuracy = 0;
    }//for the other class use

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<take_damage>()?.takedamage(attackData[RandomAtIndex].dmg);
            addAndminus(hit, 1, 10);
        }
    }
    //}end of at func

    async void block(CancellationTokenSource token) //also check block ignore which is animation event in block anim + also need to -0.5 second because the block animation
    {
        try
        {
            isblocking = true;
            selfAnim.Play("block");
            await Task.Delay(20000, token.Token);/*
            Physics2D.IgnoreLayerCollision(7, 11, false);*/
            selfAnim.speed = 1;
            selfAnim.Play("unblock");
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)
        {
            selfAnim.speed = 1;
            selfAnim.Play("unblock");
            isblocking = false;
        }
    }

    void suddenEndBlock()
    {
        cancelAndCreate(ref blockAct);
    }

    async void heal()
    {
        stop = true;
        SR.color = new Color(0.92f, 1, 0);
        await Task.Delay(2000);
        kn_takeD.takedamage(-500, false);
        stop = false;
        SR.color = new Color(1, 1, 1);

    }

    //}what can knight do
    //
    //anim-function{
    void already_dead()
    {
        Physics2D.IgnoreCollision(pcc, kcc, true);
        alreadyDead = true;
    }
    public void blockIgnore()
    {
        selfAnim.speed = 0;/*
        Physics2D.IgnoreLayerCollision(7, 11, true);*/
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
        can_attack = true;
        walkis1 = 0;
    }

    void died()
    {
        dead = true;
        selfAnim.Play(dieAnim);
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
            Destroy(self);
        }
    }// wrap things up after knight dead

}