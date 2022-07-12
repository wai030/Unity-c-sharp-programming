using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;


[RequireComponent(typeof(create_text_array))]

public class showcanvas : MonoBehaviour
{
    /*there are only 1 panel is allowed in scene
     please implement create_text_array.cs in an empty gameobject in scene before using canvas
    if there are problem of showing the optionpanel, it is probably because the Panel with tag problem
    when placing optionpanel in inspector, please arrange optionpanel in ascending order
    */
    [SerializeField] List<GameObject> optionPanel;
    [SerializeField] ai AI;
    public event Action sendpause;
    public static int i = 0;// i is the line location
    public int txtspeed;
    GameObject Panel;
    Text t;
    create_text_array ta;
    CancellationTokenSource cancel;
    string txtn= "start", z;
    bool txtfinish = true, _pasue = false;
    bool pause
    {
        get
        {
            return _pasue;
        }
        set
        {
            if (value == _pasue)
                return;
            _pasue = value;
            Time.timeScale = pause ? 0f : 1f;
            sendpause?.Invoke();
        }
    }


    void Awake()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        ta = GetComponent<create_text_array>();
        t = gameObject.transform.GetComponentInChildren<Text>();
        Panel=GameObject.FindGameObjectWithTag("Panel");
        if (ta == null||t == null|| Panel==null)
            gameObject.SetActive(false);
    }

    private void Start()
    {
        t.text = "";
        cancel = new CancellationTokenSource();
        pause = true;
        talk();
    }
    void Update()
    {
        if (Input.GetButtonDown("interact"))
            talk();
    }

    void talk()
    {
        t.text = "";
        if (i  == ta.getlength(txtn))
        {
            i = 0;
            Panel.SetActive(false);
        }
        else if (txtfinish)// txtfinish mean the line of word is finished or not
        {
            text(ta.gettext(txtn, i), cancel.Token);
        }
        else if (!txtfinish)
        {
            cancel.Cancel();
            cancel.Dispose();
            cancel = new CancellationTokenSource();
        }
    }
    async void text(string s, CancellationToken token)
    {
        try
        {
            txtfinish = false;
            switch (s)
            {
                case string a when a.Contains("!?!option"):
                    Panel.SetActive(false);
                    GameObject op = Instantiate(optionPanel[(int)char.GetNumericValue(a[9]) - 1], gameObject.transform);
                    lp(op);
                    break;

                case string a when a.Contains("###End"):
                    Panel.SetActive(false);
                    pause = false;
                    t.text = "";
                    AI?.hitalert();
                    break;
                default:
                    foreach (var k in ta.gettext(txtn, i))
                    {
                        t.text += k;
                        await Task.Delay(txtspeed, token);
                    }
                    i += 1;
                    txtfinish = true;
                    break;
            }
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)
        {
            t.text = ta.gettext(txtn, i);
            i += 1;
            txtfinish = true;
        }
        
    }
   
    void lp(GameObject op)// k= how many option, op = obj instantiated
    {
        i += 1;
        foreach (Transform G in op.transform)
        {
            z = ta.gettext(txtn, i).Split('(', ')')[3];
            string fil = ta.gettext(txtn, i).Split('(', ')')[1];
            i += 1;
            G.gameObject.GetComponent<Button>().onClick.AddListener(() => TaskOnClick(fil, op));// get button text
            G.GetChild(0).GetComponent<Text>().text = z;
        }
    }
    void TaskOnClick(string q, GameObject opt)
    {
        txtn = q;
        i = 0;
        txtfinish = true;
        Panel.SetActive(true);
        opt.SetActive(false);
        talk();
    }
    public bool returnpause()
    {
        return pause;
    }

    public void another_speech(string i)
    {
        txtn = i;
        talk();
        Panel.SetActive(true);
    }
}
