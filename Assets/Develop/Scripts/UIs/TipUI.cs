using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;
using DNHper;
using Michsky.MUIP;
using Cysharp.Threading.Tasks;

public class TipUI : UIBase
{
    public async void ShowInfo(string title, string content)
    {
        var _notify = this.Get<NotificationManager>("notify_info");
        _notify.title = title;
        _notify.description = content;
        _notify.UpdateUI();
        _notify.OpenNotification();
        await UniTask.Delay(3000);
        _notify.CloseNotification();
    }

    // Start is called before the first frame update
    private void Start() { }

    // Update is called once per frame
    private void Update() { }

    // Called when this ui is loaded
    protected override void OnLoaded() { }

    // Called when this ui is shown
    protected override void OnShown() { }

    // Called when this ui is hidden
    protected override void OnHidden() { }
}
