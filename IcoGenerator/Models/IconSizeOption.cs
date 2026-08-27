using IcoGenerator.ViewModels;

namespace IcoGenerator.Models;

public sealed class IconSizeOption : ObservableObject
{
    public IconSizeOption(int size, bool isSelected)
    {
        Size = size;
        _isSelected = isSelected;
    }

    public int Size { get; }

    public string Label => $"{Size}×{Size}";

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
