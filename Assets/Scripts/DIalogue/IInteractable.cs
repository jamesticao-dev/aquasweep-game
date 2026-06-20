public interface IInteractable
{
    void Interact();

    bool CanInteract();

    bool TouchInteract { get; }
}