namespace DemoGame
{
    // E キーで働きかけられるもの
    // 「選ばれているかどうか」はプレイヤー側（PlayerInteractor）だけが管理する。対象は範囲の出入りを知らなくてよい
    public interface IInteractable
    {
        // 今このプレイヤーが働きかけられるか（点灯済みの灯などは false）
        bool CanInteract(PlayerInteractor interactor);

        void Interact(PlayerInteractor interactor);

        // HUD に出す文言（「Light」「Push」など。UI のフォントが英字のみなので英語で書く）
        string Prompt { get; }
    }
}
