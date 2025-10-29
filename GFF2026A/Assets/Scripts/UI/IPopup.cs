public interface IPopup
{
    void Open();
    void Close();
    bool IsOpen { get; }
}