using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEditor;

[System.Serializable]
public class movement : MonoBehaviour
{
    /*ai script is for calculating score only for ai usage
     AnimName is only telling the string under it is the animation name*/
    [SerializeField] Animator an;
    [SerializeField] Rigidbody2D rig;
    [SerializeField] float Pspeed = 250;
    [HideInInspector] public bool dead = false, attacking = false, can_jump = true;
    [HideInInspector] [SerializeField] internal ai AI_ ,AI;
    [HideInInspector] [SerializeField] internal string walkAnim, idleAnim, jumpUpAnim, fallAnim, deadAnim;
    [HideInInspector] [SerializeField] internal bool enemyAI_,walkA, idleA, jumpA, fallA, deadA;
    public int jumppforce;
    public event Action sendpause;
    LayerMask mask;
    CancellationTokenSource cancel;
    showcanvas show;
    Vector2 ve,//inherit Y velocity
            tr;//raycast location
    float move;
    bool _direction, _isground=true, _pause=false;
    RaycastHit2D ra;
    bool direction 
    {
        get 
        { return _direction; }
        set
        {
            
            if (_direction == value)
                return;
            _direction = value;
            AI?.updatefacing();
        }
    }
    public bool is_ground
    {
        get
        {
            return _isground;
        }
        set
        {
            if (_isground == value)
                return;
            _isground = value;
            if (_isground)
            {
                cancel.Cancel();
                cancel.Dispose();
                attacking = false;
            }
        }
    }
    public bool pause
    { get
        {
            return _pause;
        }
        set
        {
            if (_pause == value)
                return;
            _pause = value;
            sendpause?.Invoke();
        }
    }
    private void Awake()
    {
        show = GameObject.FindObjectOfType<showcanvas>();
        if(show!=null)
            show.sendpause += checkPause;
    }
    void Start()
    {
        mask = LayerMask.GetMask("Platforms"); // determine ground touch
        cancel = new CancellationTokenSource();
    }
    void Update()
    {
        if (dead || attacking || pause)
        {
            move = 0;
            return;
        }
            tr = new Vector2(rig.position.x, rig.position.y);//for raycast ground check
            isground();
                if (Input.GetButtonDown("Jump") &&jumpA &&fallA)
                {
                    cancel = new CancellationTokenSource();
                    if(is_ground && can_jump)
                        jump(cancel.Token);
                }
                if (Input.GetAxis("Horizontal") == 0 &&idleA)
                {
                    move = 0;
                    if (is_ground&&can_jump)
                        an.Play(idleAnim);
                
                }
                else if (!attacking && Mathf.Abs(Input.GetAxis("Horizontal")) > 0&&walkA )
                {
                    if(is_ground&&move!=0)
                        an.Play(walkAnim);
                    walk();
                }
                else
                    move = 0;
            ve = rig.velocity;
    }
    void FixedUpdate()
    {
        if (dead)
            rig.velocity = new Vector2(0, -50);
        else
        {
            Physics2D.gravity = new Vector2(0, -9.8f);
            rig.velocity = new Vector2(move * Time.fixedDeltaTime, ve.y);
        }
    }
    public int choose(int min, int max, float i)
    {
        if (i == 0)
            return 0;
        else if (i < 0)
            return min;
        else
            return max;
    }//self defined math.clamp for localscale "direction"
    public void isground()
    {
        ra = Physics2D.CircleCast(tr, 0.1f, Vector2.down * .1f, 1f, mask);
        is_ground= ra.collider != null;
    }   //raycast groundcheck

    void walk()
    {
        move = Input.GetAxis("Horizontal") * Pspeed;
        transform.localScale = new Vector3(choose(-1, 1, move) * 5, 5, 1);
        if (move > 0)
            direction = true;
        else if (move < 0)
            direction = false;
    }//its called walk what do u expect
    async void jump(CancellationToken token)
    {
        try
        {
            can_jump = false;
            rig.AddForce(Vector2.up * jumppforce, ForceMode2D.Impulse);
            while (true)
            {
                if (rig.velocity.y > 0)
                    an.Play(jumpUpAnim);
                else
                {
                    an.Play(fallAnim);
                    if (is_ground)
                        break;  
                }
                await Task.Delay(100, token);
            }
            can_jump = true;
            return;
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)
        {
            if(idleA)
                an.Play("idle");
            can_jump = true;
            return;
        }
    }//its called jump what do u expect
    void alreadydead()
    {
        dead = true;
        Physics2D.IgnoreLayerCollision(8, 11, true);
        if (!can_jump)
        {
            cancel.Cancel();
            cancel.Dispose();
        }

        if(deadA)
            an.Play(deadAnim);

    }//play dead animation when takedamage.cs hp<=0

    void checkPause()
    {
        pause = Time.timeScale == 0 ? true : false;
    }
}

[CustomEditor(typeof(movement))]
[CanEditMultipleObjects]// custom editor
public class movementeditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        movement move = target as movement;
        move.enemyAI_ = EditorGUILayout.Toggle("AI_", move.enemyAI_);
        if (move.enemyAI_)
        {
            move.AI_=EditorGUILayout.ObjectField("AIscript", move.AI_, typeof(ai)) as ai;
            move.AI = move.AI_;
        }
        else
            move.AI = null;
        move.walkA = EditorGUILayout.Toggle("walkA", move.walkA);
        if (move.walkA)
            move.walkAnim = EditorGUILayout.TextField("walkAnim", move.walkAnim);
        move.idleA = EditorGUILayout.Toggle("idleA", move.idleA);
        if (move.idleA)
            move.idleAnim = EditorGUILayout.TextField("idleAnim", move.idleAnim);
        move.jumpA = EditorGUILayout.Toggle("jumpA", move.jumpA);
        if (move.jumpA)
            move.jumpUpAnim = EditorGUILayout.TextField("jumpUpAnim", move.jumpUpAnim);
        move.fallA = EditorGUILayout.Toggle("fallA", move.fallA);
        if (move.fallA)
            move.fallAnim = EditorGUILayout.TextField("fallAnim", move.fallAnim);
        move.deadA = EditorGUILayout.Toggle("deadA", move.deadA);
        if (move.deadA)
            move.deadAnim = EditorGUILayout.TextField("deadAnim", move.deadAnim);
        serializedObject.ApplyModifiedProperties();
    }

}

