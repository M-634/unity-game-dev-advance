using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UniTaskSample : MonoBehaviour
{
    
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _cube;
    
    private CancellationTokenSource cancellationTokenSource;
    
    //reference :r
    void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        
        _button.onClick.AddListener(cancellationTokenSource.Cancel);
        
         // Hoge().Forget();
          LoopDemo().Forget();
         // Demo().Forget();
    }

    private async UniTask Demo()
    {
        Debug.Log("待機開始");
        await UniTask.WaitUntil(() => _cube.activeSelf, cancellationToken: cancellationTokenSource.Token, cancelImmediately:true);
        Debug.Log("Cubeが表示された。");
    }
    
    
    private async UniTask Hoge()
    {
        //1frame 待機
        await UniTask.Yield();
        
        Debug.Log("1frame");
        
        //10秒待機
        await UniTask.Delay(TimeSpan.FromSeconds(10));
        
        Debug.Log("delay 10秒");
    }
    
    private async UniTask LoopDemo()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            Debug.Log("1秒");
            
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }
        }
        
        Debug.Log("止まった！！！");
    }
    
    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}
