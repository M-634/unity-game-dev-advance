namespace Section.Common
{
    public interface IInteractable
    {
        bool CanInteract(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
        string Prompt { get; }
    }
}
