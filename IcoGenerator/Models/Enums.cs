namespace IcoGenerator.Models;

public enum BackgroundType
{
    Solid,
    LinearGradient,
    RadialGradient,
    Image
}

/// <summary>Nine-point anchor for positioning the text, matching common design-tool "anchor" pickers.</summary>
public enum AnchorPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum ShapePreset
{
    Square,
    Rounded,
    Squircle,
    Circle
}
