using System.Numerics;

namespace BoneSmith.Models;

public sealed class RuntimeBoneTransform
{
    public string BoneName { get; init; } = string.Empty;

    public Vector3 Translation { get; init; } = Vector3.Zero;
    public Vector3 Rotation { get; init; } = Vector3.Zero;
    public Vector3 Scaling { get; init; } = Vector3.One;
    public Vector3 ChildScaling { get; init; } = Vector3.One;

    public bool PropagateTranslation { get; init; }
    public bool PropagateRotation { get; init; }
    public bool PropagateScale { get; init; }
    public bool ChildScalingIndependent { get; init; }

    public bool IsEdited =>
        !Approximately(Translation, Vector3.Zero, 0.00001f) ||
        !Approximately(Rotation, Vector3.Zero, 0.1f) ||
        !Approximately(Scaling, Vector3.One, 0.00001f) ||
        (ChildScalingIndependent && !Approximately(ChildScaling, Vector3.One, 0.00001f)) ||
        PropagateTranslation ||
        PropagateRotation ||
        PropagateScale;

    private static bool Approximately(Vector3 a, Vector3 b, float margin)
        => MathF.Abs(a.X - b.X) < margin &&
           MathF.Abs(a.Y - b.Y) < margin &&
           MathF.Abs(a.Z - b.Z) < margin;
}
