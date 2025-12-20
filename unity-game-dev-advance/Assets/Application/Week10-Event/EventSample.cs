using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class SampleSerialize : UnityEvent{}

public class EventSample : MonoBehaviour
{
    public event Action sampleAction;
    public event Func<int, int, int> sampleFunc;
    
    [SerializeField] Button  sampleButton;
    [SerializeField] SampleSerialize sampleSerialize;

    void Start()
    {
        sampleButton.onClick.AddListener(Hoge);
        sampleButton.onClick.AddListener(Hog2);
        
        sampleSerialize.Invoke();
        
        sampleAction += Hoge;
        sampleAction += Hog2;
        sampleFunc += Add;

        // if (sampleAction != null)
        // {
        //     sampleAction.Invoke();
        // }
        sampleAction?.Invoke();
        sampleFunc?.Invoke(10, 10);
    }

    private void OnDestroy()
    {
        sampleAction -= Hoge;
        sampleAction -= Hog2;
        sampleFunc -= Add;
    }

    private void Hoge()
    {
        Debug.Log("Hoge");
    }

    private void Hog2()
    {
        Debug.Log("Hoge2");
    }

    private int Add(int a, int b)
    {
        var result = a + b;
        Debug.Log($"{result} = {a} + {b}");
        return result;
    }
}
