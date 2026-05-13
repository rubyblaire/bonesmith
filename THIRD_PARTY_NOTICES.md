Third-Party Notices

Customize+ Runtime / IPC Compatibility Reference

BoneSmith v0.36.0 begins replacing the previous homegrown sync workaround layer with a direct compatibility port modeled on Customize+ by Aether-Tools.

Customize+ source: https://github.com/Aether-Tools/CustomizePlus
License: Apache License 2.0

The relevant BoneSmith compatibility work references and/or adapts the following Customize+ concepts:

- `CustomizePlus.Api.CustomizePlusIpc` profile IPC surface
- `IPCCharacterProfile` / `IPCBoneTransform` minimal sync payload shape
- `ProfileManager.AddTemporaryProfile` temporary-profile ownership behavior
- temporary profile apply/delete semantics used by sync clients
- active profile update events used by Mare/Lightless/PlayerSync-style integrations

Apache-2.0 notice preservation: portions of this compatibility layer are derived from or structurally based on Customize+ and retain this attribution. BoneSmith remains a separate project by Ruby Blaire and uses its own native profile/template UI and Studio.
