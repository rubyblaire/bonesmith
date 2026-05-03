# BoneSmith

**BoneSmith** is a lightweight companion plugin for **Customize+** that lets you apply saved profiles, manage referenced templates, and safely release active changes without rewriting or replacing your Customize+ data.

BoneSmith is designed to give users more control over how their existing Customize+ profiles are applied at runtime, especially when a profile references multiple templates and you only want some of them active.

## What BoneSmith Does
BoneSmith helps you:
- Browse your existing Customize+ profiles
- Select and apply a profile through BoneSmith
- View the templates referenced by a selected profile
- Toggle referenced templates on or off per profile
- Restore profile template defaults
- Release the currently applied BoneSmith profile
- Reset BoneSmith runtime state safely
- Recover from stuck or active selections
- Keep Customize+ profile data intact

BoneSmith does **not** create, edit, rewrite, or replace Customize+ profiles. It works with the data you already have.

## Features
### Profile Selection
View your detected Customize+ profiles in a clean, user-friendly list. Select a profile to see its details and apply it through BoneSmith.

### Referenced Template Toggles
When a profile references Customize+ templates, BoneSmith shows those templates by name where possible.
Each referenced template can be toggled on or off individually.
This allows you to use a profile while excluding certain templates without modifying the original Customize+ profile.

### Runtime-Only Overrides
Template toggles are handled by BoneSmith at runtime.
Your Customize+ profiles are not rewritten.

### Restore Profile Defaults
Each profile can have its BoneSmith template overrides cleared with **Restore Profile Defaults**.
This returns that profile to its original referenced-template behavior inside BoneSmith.

### Release and Reset Tools
BoneSmith includes simple recovery tools for releasing active selections and resetting BoneSmith’s runtime state.
These tools are designed to help if something feels stuck or if you want to cleanly stop BoneSmith from managing the current profile.

### Home Status Summary
The Home tab shows a quick status overview, including:
- Selected profile
- Active templates
- Runtime state
- Render hook readiness

### Settings
The Settings tab includes plugin information, safety notes, and quick links for support.

## Commands
`/bonesmith`

Opens the BoneSmith window.

## Installation
1. Install BoneSmith through your Dalamud custom plugin repository.
2. Make sure Customize+ is installed and configured.
3. Open BoneSmith with `/bonesmith`.
4. Select a profile.
5. Toggle any referenced templates you want included or excluded.
6. Apply the profile.

## Recommended Use
BoneSmith is best used when you already have Customize+ profiles and templates set up, but want more flexible control over what actually gets applied.

For example:
- A profile references several templates, but you only want some active today
- You want to temporarily disable a referenced template without editing Customize+
- You want to test profile combinations safely
- You want a simple release/reset flow when changing setups

## Important Notes
BoneSmith works with Customize+ data, but it is not a replacement for Customize+.
BoneSmith does not permanently alter your Customize+ profiles.
If you want to permanently edit a profile, do that inside Customize+.
BoneSmith’s template toggles only affect how BoneSmith applies a selected profile.

## Troubleshooting
### My profile did not apply
Check that:
- Customize+ is installed
- The profile still exists
- Any referenced templates still exist
- BoneSmith shows the render hook as ready
- The selected profile has not been deleted or renamed externally

### A template name is missing
If BoneSmith cannot resolve a template name, it may show fallback information instead.
This usually means the referenced template could not be found in the detected Customize+ data.

### Something feels stuck
Use the Recovery tab.

Recommended order:
1. Release active selection
2. Reset BoneSmith runtime state
3. Reopen the plugin window
4. Reapply the profile if needed

## Support
For bug reports, support, or feedback:

Discord: https://discord.gg/Dr836dmbqh

Ko-fi: https://ko-fi.com/rubyblaire

## Credits
Created by **Ruby Blaire**.
BoneSmith is a Ruby Blaire plugin made for players who want cleaner, safer, and more flexible control over their Customize+ profile workflow while Customize+ is down.

## Disclaimer
BoneSmith is an independent plugin and is not affiliated with Square Enix.
Use plugins responsibly and in accordance with Dalamud, Final Fantasy XIV, and third-party tool community guidelines.

## License

BoneSmith is licensed under the MIT License.

See the `LICENSE` file for details.
