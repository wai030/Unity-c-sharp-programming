using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
public class testobj : MonoBehaviour
{
    // Start is called before the first frame update
    protected CancellationTokenSource cancel;
    void Start()
    {
        cancel = new CancellationTokenSource();
        JJJ(cancel.Token);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("test"))
        {
            if (cancel != null)
            {
                cancel.Cancel();
                cancel.Dispose();
                cancel = null;
                cancel = new CancellationTokenSource();
            }
            
        }
        if (Input.GetButtonDown("interact"))
        {

            
            JJJ(cancel.Token);
        }
    }
    async void JJJ(CancellationToken token)
    {

        try
        {
            
        }
        catch (System.OperationCanceledException) when (token.IsCancellationRequested)
        {
            Debug.Log("2");
        }
    }
}
