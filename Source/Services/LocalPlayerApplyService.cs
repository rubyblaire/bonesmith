using System.Numerics;
using BoneSmith.Models;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using static FFXIVClientStructs.Havok.Animation.Rig.hkaPose;

namespace BoneSmith.Services;

public unsafe sealed class LocalPlayerApplyService
{
    private readonly Configuration configuration;
    private readonly IObjectTable objectTable;
    private readonly IPluginLog log;
    private readonly RuntimePayloadService payloadService;

    public bool IsEnabled { get; private set; }
    public long ApplyTicks { get; private set; }
    public string Status { get; private set; } = "Local player apply prototype stopped.";

    public int LastMatchedBones { get; private set; }
    public int LastAppliedBones { get; private set; }
    public int LastPartialSkeletons { get; private set; }
    public int LastScannedBones { get; private set; }

    public int LastNonRootScaleEdits { get; private set; }
    public int LastRootScaleEdits { get; private set; }
    public int LastRotationEdits { get; private set; }
    public int LastTranslationEdits { get; private set; }
    public int LastSkippedRootBones { get; private set; }
    public int LastPropagatedChildren { get; private set; }
    public int LastPropagationSourceBones { get; private set; }
    public Vector3 LastRootScaleValue { get; private set; } = Vector3.One;
    public int ConsecutiveExceptions { get; private set; }
    public DateTimeOffset? LastExceptionAt { get; private set; }

    public LocalPlayerApplyService(
        Configuration configuration,
        IObjectTable objectTable,
        IPluginLog log,
        RuntimePayloadService payloadService)
    {
        this.configuration = configuration;
        this.objectTable = objectTable;
        this.log = log;
        this.payloadService = payloadService;
    }

    public void Start()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        ApplyTicks = 0;
        ConsecutiveExceptions = 0;
        LastExceptionAt = null;
        Status = "Local player full-transform prototype enabled.";
        log.Information("BoneSmith local player full-transform prototype enabled.");
    }

    public void Stop()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        Status = "Local player apply prototype stopped.";
        log.Information("BoneSmith local player apply prototype stopped.");
    }

    public void StopAndResetLocalScale()
    {
        Stop();
        ResetLocalPlayerScale();
    }

    public void Tick()
    {
        if (!IsEnabled)
            return;

        ApplyTicks++;

        var payload = payloadService.LastPayload ?? payloadService.BuildPayload();

        if (payload is null)
        {
            Status = "No runtime payload available. Build/stage a profile or template first.";
            return;
        }

        ProbeOrApply(payload, ApplyMode.FullTransformNoRoot);
    }

    public void ProbeLocalPlayer()
    {
        var payload = payloadService.LastPayload ?? payloadService.BuildPayload();

        if (payload is null)
        {
            Status = "Probe failed: no runtime payload available.";
            return;
        }

        ProbeOrApply(payload, ApplyMode.ProbeOnly);
    }

    public void ApplyOnce()
    {
        var payload = payloadService.LastPayload ?? payloadService.BuildPayload();

        if (payload is null)
        {
            Status = "Apply once failed: no runtime payload available.";
            return;
        }

        ProbeOrApply(payload, ApplyMode.FullTransformNoRoot);
    }

    public void ApplyRootScaleFromPayload()
    {
        var payload = payloadService.LastPayload ?? payloadService.BuildPayload();

        if (payload is null)
        {
            Status = "Root scale failed: no runtime payload available.";
            return;
        }

        if (!payload.BoneBindings.TryGetValue("n_root", out var rootTransform))
        {
            Status = "Root scale failed: active payload does not contain n_root.";
            return;
        }

        if (IsVectorApproximately(rootTransform.Scaling, Vector3.One, 0.00001f))
        {
            Status = $"Root scale failed: n_root scaling is identity ({rootTransform.Scaling}).";
            return;
        }

        if (!TryGetLocalCharacterBase(out var cBase, out var failure))
        {
            Status = $"Root scale failed: {failure}";
            return;
        }

        try
        {
            cBase->DrawObject.Object.Scale = rootTransform.Scaling;
            LastRootScaleValue = rootTransform.Scaling;
            Status = $"Applied n_root draw-object scale from payload: {rootTransform.Scaling}.";
        }
        catch (Exception ex)
        {
            Status = $"Root scale exception: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith root scale apply failed.");
        }
    }

    public void ApplyDebugRootScale()
    {
        if (!TryGetLocalCharacterBase(out var cBase, out var failure))
        {
            Status = $"Debug root scale failed: {failure}";
            return;
        }

        try
        {
            var scale = new Vector3(1.08f, 1.08f, 1.08f);
            cBase->DrawObject.Object.Scale = scale;
            LastRootScaleValue = scale;
            Status = "Applied debug draw-object scale 1.08 to local player.";
        }
        catch (Exception ex)
        {
            Status = $"Debug root scale exception: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith debug root scale failed.");
        }
    }

    public void ResetLocalPlayerScale()
    {
        if (!TryGetLocalCharacterBase(out var cBase, out var failure))
        {
            Status = $"Reset local player scale failed: {failure}";
            return;
        }

        try
        {
            cBase->DrawObject.Object.Scale = Vector3.One;
            LastRootScaleValue = Vector3.One;
            Status = "Reset local player draw-object scale to 1.0.";
        }
        catch (Exception ex)
        {
            Status = $"Reset scale exception: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith reset local player scale failed.");
        }
    }

    private void ProbeOrApply(RuntimePayload payload, ApplyMode mode)
    {
        try
        {
            LastMatchedBones = 0;
            LastAppliedBones = 0;
            LastPartialSkeletons = 0;
            LastScannedBones = 0;
            LastNonRootScaleEdits = 0;
            LastRootScaleEdits = 0;
            LastRotationEdits = 0;
            LastTranslationEdits = 0;
            LastSkippedRootBones = 0;
            LastPropagatedChildren = 0;
            LastPropagationSourceBones = 0;

            CountPayloadEditTypes(payload);

            if (!TryGetLocalCharacterBase(out var cBase, out var failure))
            {
                Status = failure;
                return;
            }

            if (cBase->Skeleton == null)
            {
                Status = "Local player skeleton is not available yet.";
                return;
            }

            LastPartialSkeletons = cBase->Skeleton->PartialSkeletonCount;

            for (var partialIndex = 0; partialIndex < cBase->Skeleton->PartialSkeletonCount; partialIndex++)
            {
                var partial = cBase->Skeleton->PartialSkeletons[partialIndex];
                var pose = partial.GetHavokPose(0);

                if (pose == null)
                    continue;

                var boneCount = pose->Skeleton->Bones.Length;
                LastScannedBones += boneCount;

                for (var boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    var boneName = pose->Skeleton->Bones[boneIndex].Name.String;

                    if (string.IsNullOrWhiteSpace(boneName))
                        continue;

                    if (!payload.BoneBindings.TryGetValue(boneName, out var transform))
                        continue;

                    if (!transform.IsEdited)
                        continue;

                    LastMatchedBones++;

                    if (boneName.Equals("n_root", StringComparison.OrdinalIgnoreCase))
                    {
                        LastSkippedRootBones++;
                        continue;
                    }

                    if (mode == ApplyMode.ProbeOnly)
                        continue;

                    var access = pose->AccessBoneModelSpace(boneIndex, PropagateOrNot.DontPropagate);

                    if (access == null)
                        continue;

                    // C+ order: scale, then rotation, then translation adjusted by current rotation.
                    var beforeTranslation = new Vector3(access->Translation.X, access->Translation.Y, access->Translation.Z);
                    var beforeRotation = new Quaternion(access->Rotation.X, access->Rotation.Y, access->Rotation.Z, access->Rotation.W);
                    var beforeScale = new Vector3(access->Scale.X, access->Scale.Y, access->Scale.Z);

                    ApplyScale(access, transform);
                    ApplyRotation(access, transform);
                    ApplyTranslationWithRotation(access, transform);

                    LastAppliedBones++;

                    if (configuration.EnableExperimentalChildPropagation &&
                        (transform.PropagateTranslation || transform.PropagateRotation || transform.PropagateScale))
                    {
                        LastPropagationSourceBones++;
                        LastPropagatedChildren += PropagateChildrenSimplified(
                            pose,
                            boneIndex,
                            transform,
                            beforeTranslation,
                            beforeRotation,
                            beforeScale);
                    }
                }
            }

            Status = mode switch
            {
                ApplyMode.ProbeOnly =>
                    $"Probe complete. Partials: {LastPartialSkeletons}, scanned: {LastScannedBones}, matched: {LastMatchedBones}. Non-root scale edits: {LastNonRootScaleEdits}, root scale edits: {LastRootScaleEdits}, rotation edits: {LastRotationEdits}, translation edits: {LastTranslationEdits}.",
                _ =>
                    $"Applied full-transform prototype. Partials: {LastPartialSkeletons}, scanned: {LastScannedBones}, matched: {LastMatchedBones}, applied: {LastAppliedBones}, skipped root: {LastSkippedRootBones}, propagated children: {LastPropagatedChildren}."
            };
        }
        catch (Exception ex)
        {
            ConsecutiveExceptions++;
            LastExceptionAt = DateTimeOffset.Now;
            Stop();
            Status = $"Apply prototype exception, runtime stopped: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith local player apply prototype failed.");
        }
    }

    private static void ApplyScale(FFXIVClientStructs.Havok.Common.Base.Math.QsTransform.hkQsTransformf* access, RuntimeBoneTransform transform)
    {
        if (IsVectorApproximately(transform.Scaling, Vector3.One, 0.00001f))
            return;

        access->Scale.X *= transform.Scaling.X;
        access->Scale.Y *= transform.Scaling.Y;
        access->Scale.Z *= transform.Scaling.Z;
    }

    private static void ApplyRotation(FFXIVClientStructs.Havok.Common.Base.Math.QsTransform.hkQsTransformf* access, RuntimeBoneTransform transform)
    {
        if (IsVectorApproximately(transform.Rotation, Vector3.Zero, 0.1f))
            return;

        var current = new Quaternion(access->Rotation.X, access->Rotation.Y, access->Rotation.Z, access->Rotation.W);
        var delta = ToQuaternion(transform.Rotation);
        var result = Quaternion.Multiply(current, delta);

        access->Rotation.X = result.X;
        access->Rotation.Y = result.Y;
        access->Rotation.Z = result.Z;
        access->Rotation.W = result.W;
    }

    private static void ApplyTranslationWithRotation(FFXIVClientStructs.Havok.Common.Base.Math.QsTransform.hkQsTransformf* access, RuntimeBoneTransform transform)
    {
        if (IsVectorApproximately(transform.Translation, Vector3.Zero, 0.00001f))
            return;

        var currentRotation = new Quaternion(access->Rotation.X, access->Rotation.Y, access->Rotation.Z, access->Rotation.W);
        var adjusted = Vector3.Transform(transform.Translation, currentRotation);

        access->Translation.X += adjusted.X;
        access->Translation.Y += adjusted.Y;
        access->Translation.Z += adjusted.Z;
    }


    private int PropagateChildrenSimplified(
        hkaPose* pose,
        int parentBoneIndex,
        RuntimeBoneTransform transform,
        Vector3 beforeTranslation,
        Quaternion beforeRotation,
        Vector3 beforeScale)
    {
        if (pose == null)
            return 0;

        var parentAccess = pose->AccessBoneModelSpace(parentBoneIndex, PropagateOrNot.DontPropagate);

        if (parentAccess == null)
            return 0;

        var afterTranslation = new Vector3(parentAccess->Translation.X, parentAccess->Translation.Y, parentAccess->Translation.Z);
        var afterRotation = new Quaternion(parentAccess->Rotation.X, parentAccess->Rotation.Y, parentAccess->Rotation.Z, parentAccess->Rotation.W);
        var afterScale = new Vector3(parentAccess->Scale.X, parentAccess->Scale.Y, parentAccess->Scale.Z);

        var deltaTranslation = afterTranslation - beforeTranslation;
        var deltaRotation = afterRotation * Quaternion.Inverse(beforeRotation);

        var propagated = 0;
        var boneCount = pose->Skeleton->Bones.Length;

        for (var childIndex = 0; childIndex < boneCount; childIndex++)
        {
            if (childIndex == parentBoneIndex)
                continue;

            if (!IsDescendantOf(pose, childIndex, parentBoneIndex))
                continue;

            var childAccess = pose->AccessBoneModelSpace(childIndex, PropagateOrNot.DontPropagate);

            if (childAccess == null)
                continue;

            if (configuration.EnablePropagationTranslation &&
                transform.PropagateTranslation &&
                !IsVectorApproximately(deltaTranslation, Vector3.Zero, 0.00001f))
            {
                childAccess->Translation.X += deltaTranslation.X;
                childAccess->Translation.Y += deltaTranslation.Y;
                childAccess->Translation.Z += deltaTranslation.Z;
            }

            if (configuration.EnablePropagationRotation &&
                transform.PropagateRotation &&
                !IsVectorApproximately(transform.Rotation, Vector3.Zero, 0.1f))
            {
                var childRotation = new Quaternion(childAccess->Rotation.X, childAccess->Rotation.Y, childAccess->Rotation.Z, childAccess->Rotation.W);
                var result = Quaternion.Multiply(childRotation, deltaRotation);

                childAccess->Rotation.X = result.X;
                childAccess->Rotation.Y = result.Y;
                childAccess->Rotation.Z = result.Z;
                childAccess->Rotation.W = result.W;
            }

            if (configuration.EnablePropagationScale &&
                transform.PropagateScale)
            {
                var scaleToUse = transform.ChildScalingIndependent ? transform.ChildScaling : transform.Scaling;

                if (!IsVectorApproximately(scaleToUse, Vector3.One, 0.00001f))
                {
                    childAccess->Scale.X *= scaleToUse.X;
                    childAccess->Scale.Y *= scaleToUse.Y;
                    childAccess->Scale.Z *= scaleToUse.Z;
                }
            }

            propagated++;
        }

        return propagated;
    }

    private static bool IsDescendantOf(hkaPose* pose, int childIndex, int ancestorIndex)
    {
        if (pose == null || childIndex < 0 || ancestorIndex < 0)
            return false;

        var current = childIndex;
        var guard = 0;

        while (current >= 0 && guard++ < 512)
        {
            var parent = pose->Skeleton->ParentIndices[current];

            if (parent == ancestorIndex)
                return true;

            current = parent;
        }

        return false;
    }


    private void CountPayloadEditTypes(RuntimePayload payload)
    {
        foreach (var (boneName, transform) in payload.BoneBindings)
        {
            var hasScale = !IsVectorApproximately(transform.Scaling, Vector3.One, 0.00001f);
            var hasRotation = !IsVectorApproximately(transform.Rotation, Vector3.Zero, 0.1f);
            var hasTranslation = !IsVectorApproximately(transform.Translation, Vector3.Zero, 0.00001f);

            if (hasScale)
            {
                if (boneName.Equals("n_root", StringComparison.OrdinalIgnoreCase))
                    LastRootScaleEdits++;
                else
                    LastNonRootScaleEdits++;
            }

            if (hasRotation)
                LastRotationEdits++;

            if (hasTranslation)
                LastTranslationEdits++;
        }
    }

    private bool TryGetLocalCharacterBase(out CharacterBase* cBase, out string failure)
    {
        cBase = null;
        failure = string.Empty;

        var localPlayer = objectTable[0];

        if (localPlayer is null)
        {
            failure = "Object table slot 0/local player is not available.";
            return false;
        }

        if (localPlayer.Address == nint.Zero)
        {
            failure = "Object table slot 0/local player address is zero.";
            return false;
        }

        var gameObject = (GameObject*)localPlayer.Address;
        cBase = (CharacterBase*)gameObject->DrawObject;

        if (cBase == null)
        {
            failure = "Local player CharacterBase/DrawObject is not available yet.";
            return false;
        }

        return true;
    }

    private static Quaternion ToQuaternion(Vector3 rotation)
    {
        return Quaternion.CreateFromYawPitchRoll(
            rotation.X * MathF.PI / 180f,
            rotation.Y * MathF.PI / 180f,
            rotation.Z * MathF.PI / 180f);
    }

    private static bool IsVectorApproximately(Vector3 a, Vector3 b, float margin)
        => MathF.Abs(a.X - b.X) < margin &&
           MathF.Abs(a.Y - b.Y) < margin &&
           MathF.Abs(a.Z - b.Z) < margin;

    private enum ApplyMode
    {
        ProbeOnly,
        FullTransformNoRoot
    }
}
