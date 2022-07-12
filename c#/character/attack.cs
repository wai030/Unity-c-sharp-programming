using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
 
    public class attack : MonoBehaviour
    {

        internal class atInt
        {
        attack AT;
        public atInt(attack at)
        {
            this.AT = at;
        }
        
        int _value=0;
        internal int value
        {
            get { return _value; }
            set
            {
                _value = value;
                AT.cal();
            }
        }
            

        }
        //require movement.cs implemented
        [SerializeField] movement move;
        [SerializeField] ai AI;
        [Serializable] struct attacktype
        {
            public string AnimName;
            public string Button;
            public int damage;
            public int timegap;
            public int combotime;
            internal attacktype(attacktype a)
            {
                this = a;
            }
        }
        [SerializeField] List<attacktype> ATdata;
        public Animator an;
        internal float hitAcc;
        internal atInt hit, press;
        attacktype lastAttack;
        CancellationTokenSource cancel;
        showcanvas show;
        
        bool pause, can_attack = true, can_combo_attack = true;
        
        private void Awake()
        {
            show = GameObject.FindObjectOfType<showcanvas>();
            if (show != null)
                show.sendpause += checkPause;
        }

        private void OnEnable()
        {
            if (!ATdata.Any())
                can_attack = false;

        }
        void Start()
        {
            cancel = new CancellationTokenSource();
            hit = new atInt(this);
            press = new atInt(this);
        }

        void Update()
        {
            if (move.dead || pause || !move.can_jump)
                return;
            if (move.is_ground)
                attack1();
        }

        void attack1()
        {
            if (!can_attack && !can_combo_attack)
                return;
            else if (can_combo_attack)
            {
                foreach (var i in ATdata)
                {
                    if (Input.GetButtonDown(i.Button))
                    {
                        atfunc(i);
                        break;
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
            if (AI.isblocking)//blocking at
                if ((AI.xdtr < 0 && AI.tr.localScale.x > 0) || (AI.xdtr > 0 && AI.tr.localScale.x < 0))
                    return;
                collision.GetComponent<take_damage>()?.takedamage(lastAttack.damage);
                addAndminus(hit, 1, 10);
            }
        } //attack damage trigger
        void atfunc(attacktype attackIndex)
        {
            if (lastAttack.AnimName == attackIndex.AnimName && !can_attack)
                return;
            else if (lastAttack.AnimName != attackIndex.AnimName && !can_attack)
            {
                cancel.Cancel();
                cancel.Dispose();
                cancel = null;
                cancel = new CancellationTokenSource();
            }
            lastAttack = new attacktype(attackIndex);
            an.Play(attackIndex.AnimName);
            addAndminus(press, 1, 10);
            timed(attackIndex.timegap, attackIndex.combotime, cancel.Token);
        }

        async void timed(int time, int comboTime, CancellationToken token)
        {
            try
            {
                can_combo_attack = false;
                can_attack = false;
                await Task.Delay(comboTime);
                can_combo_attack = true;
                await Task.Delay(time - comboTime);
                can_attack = true;
            }
            catch (System.OperationCanceledException) when (token.IsCancellationRequested)
            {
                can_attack = true;
            }
        }
        //about attack
        public void isAttacking()
        {
            move.attacking = true;
        }//must be implemented on attack animation at start and end (animation function)

        public void NotAttacking()
        {
            move.attacking = false;
        }//must be implemented on jumpup animation at start
        void checkPause()
        {
            pause = Time.timeScale == 0 ? true : false;
        }

        async void addAndminus(atInt input,int addnum, int time)
        {
            input.value += addnum;
            await Task.Delay(time * 1000);
            input.value -= addnum;
        }

        public void cal()
        {
        if (press.value != 0)
            hitAcc = (float)hit.value / press.value;
        else
            hitAcc = 0;
        }
    }




