using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.MUIP;
using UniRx;
using Cysharp.Threading.Tasks;
using UNIHper;

public class LoadingTips : MonoBehaviour
{
    [SerializeField]
    private ModalWindowManager splashWindow;

    // Start is called before the first frame update
    async void Awake()
    {
        splashWindow.gameObject.SetActive(true);
        splashWindow.Open();
        await UniTask.Delay(2500);
        splashWindow.Close();
    }

    // Update is called once per frame
    void Update() { }
}
