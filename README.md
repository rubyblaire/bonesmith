# BoneSmith

**BoneSmith** is a Dalamud plugin for **Final Fantasy XIV** that provides a BoneSmith-owned profile, template, and Studio editing workflow for character bone shaping.

BoneSmith began as a lightweight companion for Customize+, but as of **v0.40.0** it should be understood as a **Customize+ alternative / native BoneSmith profile system**. Customize+ data can still be imported, but BoneSmith works from its own local profile and template files after import.

BoneSmith is designed for players who want safer profile handling, reusable template layers, visual editing tools, cleaner library organization, Penumbra Collection auto-apply, and built-in diagnostics for troubleshooting.

## Safety Promise

BoneSmith keeps imported data in **BoneSmith-owned files** and does not edit, rewrite, or replace your original Customize+ profiles or templates.

Importing from Customize+ creates local BoneSmith copies. Deleting or editing a BoneSmith copy does **not** delete or modify the original Customize+ file.

## What BoneSmith Does

BoneSmith helps you:

- Create and manage BoneSmith-native profiles
- Create and manage reusable BoneSmith templates
- Import Customize+ profiles and templates into BoneSmith-owned files
- Import BoneSmith share strings, Customize+ clipboard/share strings, and raw compatible JSON
- Apply and release BoneSmith profiles locally
- Link templates to profiles and enable or disable them per profile
- Edit profiles in the dedicated **BoneSmith Studio**
- Use Before/After previewing while editing
- Use Safe Mode warnings for risky or extreme edits
- Mirror, average, and compare left/right bone pairs with the Symmetry Toolkit
- Organize profiles and templates with tags, search, sorting, favorites, and archive filtering
- Auto-apply profiles by Penumbra Collection rules
- Copy support/debug reports from the Diagnostics Clinic
- Publish BoneSmith sync-compatible payloads for testing with supported external sync workflows

## What BoneSmith Is Not

BoneSmith is **not** intended to modify your original Customize+ library.

BoneSmith is also not a promise that every external posing, skeleton, sync, or redraw issue is caused by BoneSmith. If another plugin changes skeleton behavior, posing behavior, or sync behavior, test carefully before assuming BoneSmith is the source.

Sync compatibility is included for testing and may vary depending on external tools, user setup, and whether another client requests and applies the published payload correctly.

## Features

### BoneSmith-Native Profiles

Profiles are the main BoneSmith character-shaping files. A profile contains bone edits and can optionally reference one or more templates.

Profiles can be created, renamed, applied, released, exported, duplicated through import flows, and opened in Studio.

### BoneSmith-Native Templates

Templates are reusable edit layers. A template can be linked to multiple profiles and enabled or disabled per profile.

Templates are useful for reusable body, face, hand, foot, hair, GPose, personal, debug, favorite, archived, or imported edits.

### Library Organization

The Profiles and Templates tabs include cleaner library tools:

- Search profiles/templates
- Search by name, filename, source type, or tags
- Tag filters
- Sorting by name, modified date, bone count, or favorites
- Hide Archived toggle
- Clear content separation with **Your Profiles** and **Your Templates** sections

### Tags

Profiles and templates support library tags, including:

- Body
- Face
- Hands
- Feet
- Hair
- GPose
- Personal
- Debug
- Favorite
- Archived
- Imported

New blank profiles/templates default to **Personal**. Imported profiles/templates receive **Imported**.

### Unified Import

The Import tab combines file-based imports and clipboard/share-string imports into one cleaner workflow.

BoneSmith can import:

- Customize+ profiles
- Customize+ templates
- Customize+ clipboard/share strings
- BoneSmith profile exports
- BoneSmith template exports
- Raw compatible JSON

After import, BoneSmith shows an import result summary with jump actions such as opening the imported profile/template, making a profile active, or opening a profile in Studio.

### Duplicate Import Handling

If BoneSmith detects that an imported profile or template may already exist, it shows a confirmation popup instead of silently cluttering the library.

Duplicate options include:

- Replace Existing
- Import as Copy
- Cancel

Copy imports receive unique names such as `Name (Imported Copy)`.

### BoneSmith Studio

BoneSmith Studio is the dedicated editing workspace for shaping bones.

Studio includes:

- Category/page navigation
- Bone search
- Two-column editing layout
- Compact bone list and inspector
- Scale, Position, and Rotation editing
- Decimal precision controls
- Save, discard, undo, redo, and reset actions
- Session-only Undo History
- Optional Skeleton Map pop-out
- Before/After preview controls
- Safe Mode warnings
- Symmetry Toolkit
- Advanced Editing / propagation tools

Studio edits are non-destructive until saved.

### Before/After Preview

Studio can capture an original snapshot and let you hold/preview the original shape without marking the profile dirty or creating save side effects.

### Safe Mode

Studio Safe Mode provides advisory warnings for:

- risky/root/high-impact bones
- extreme scale values
- extreme position or rotation values
- propagation-heavy edits

Safe Mode is there to help guide safer editing. It does not replace your judgment.

### Symmetry Toolkit

The Symmetry Toolkit helps work with left/right bone pairs.

Tools include:

- Mirror Left → Right
- Mirror Right → Left
- Average Both Sides
- Show/Hide Difference
- Open Right in Symmetry from diagnostics

### Advanced Editing and Propagation

Propagation controls are grouped under Advanced Editing and hidden by default.

Propagation can be powerful, but it can also make connected body regions inflate, move, or rotate unexpectedly. Use it carefully, especially with scale.

### Penumbra Collection Auto-Apply

BoneSmith can auto-apply profiles based on detected Penumbra Collections.

Collection rules include:

- detected collection status
- active BoneSmith profile
- loaded rule count
- last auto-apply status
- per-rule status badges
- Run Rule Now
- Set Profile
- Remove

Collection debugging has moved into the Diagnostics Clinic.

### Diagnostics Clinic

The Diagnostics Clinic is BoneSmith’s centralized support and troubleshooting area.

It includes:

- Profile Diagnostics
- Collections Debug Report
- Sync Provider Activity Report
- Full Clinic Report
- Space for future repair/dev tools

The Clinic keeps support and dev tools out of normal user workflows while still making them easy to access when something needs investigating.

### Profile Diagnostics

Profile Diagnostics includes tabs for:

- Audit
- Conflicts
- Template Stack
- Symmetry
- Compare

It can help inspect profile health, template stack behavior, conflicts, symmetry differences, and profile comparisons.

### Reports

The Diagnostics Clinic can copy several reports useful for support and tester feedback:

- Collections Debug Report
- Sync Provider Activity Report
- Full Clinic Report

The Full Clinic Report bundles key state, active profile information, collection status, sync provider activity, and other diagnostic details into one copyable report.

### Sync Compatibility

BoneSmith includes passive sync compatibility support for testing.

The goal is to publish the currently active BoneSmith profile as a sync-compatible payload that supported external sync workflows may request and apply.

BoneSmith does not require Customize+ to be installed for profiles, templates, Studio, local application, imports, or normal use.

Sync compatibility should still be treated as experimental and may vary depending on external tools and setup.

## Installation

Dalamud custom plugin repository:

```text
https://raw.githubusercontent.com/rubyblaire/bonesmith/refs/heads/main/pluginmaster.json
```

1. Add the repository URL to Dalamud custom plugin repositories.
2. Install BoneSmith.
3. Open BoneSmith in-game with `/bonesmith`, `/bsm`, or `/bs`.
4. Create or import a profile.
5. Apply the profile or open it in Studio.

## Basic Use

### Create a Profile

1. Open BoneSmith.
2. Go to **Profiles**.
3. Click **Create Profile**.
4. Select the new profile.
5. Rename it if desired.
6. Open it in Studio or apply it.

### Import a Profile

1. Go to **Import**.
2. Choose a Customize+ profile or paste a supported clipboard/share string.
3. Import into BoneSmith.
4. Use the import result card to open the profile, make it active, or open it in Studio.

### Create or Import a Template

1. Go to **Templates** to create a blank template.
2. Or go to **Import** to import a Customize+ template or template share string.
3. Link the template to a profile from Profiles or Templates.

### Open Studio

1. Select a profile.
2. Click **Open in Studio**.
3. Edit bones using Scale, Position, and Rotation.
4. Use Safe Mode, Undo History, Before/After, and Symmetry tools as needed.
5. Save when finished.

### Use Penumbra Collection Auto-Apply

1. Go to the Collections area.
2. Create a rule connecting a Penumbra Collection to a BoneSmith profile.
3. Enable auto-apply.
4. BoneSmith will detect the current collection and apply matching rules when appropriate.

## Recommended Use

BoneSmith is best used when you want a safer, BoneSmith-owned workflow for profile editing and application.

Good use cases:

- Building native BoneSmith profiles
- Creating reusable template layers
- Importing old Customize+ data into a separate BoneSmith library
- Testing profile variations without rewriting originals
- Editing body/face/hand/advanced bones in Studio
- Organizing multiple profiles with tags and favorites
- Auto-applying profiles by Penumbra Collection
- Collecting diagnostic reports for troubleshooting

## Important Notes

- BoneSmith does not edit your original Customize+ files.
- Imported profiles/templates become BoneSmith-owned copies.
- Studio edits are saved only when you choose to save them.
- Propagation can produce large visual changes. Use it carefully.
- Sync compatibility is experimental and depends on external tools.
- If another plugin changes skeleton behavior, test with that plugin disabled before assuming BoneSmith is responsible.

## Troubleshooting

### I do not see any profiles

Create a profile in Profiles or import one from the Import tab.

### I imported a profile but do not see it applied

Go to Profiles, select the imported BoneSmith copy, and click Apply.

### A template did not affect my profile

Check the selected profile’s linked templates and make sure the template is linked and enabled.

### My profile/template disappeared from the list

Check whether **Hide Archived** is enabled. Archived items are hidden by default when Hide Archived is on.

### Search shows no results

Clear the search box, change the tag filter, or show archived items.

### Scale changes make areas inflate

This can be normal, especially with Propagate Scale enabled. Try disabling propagation or editing a smaller bone chain.

### Studio edits feel too fast or too slow

Adjust the decimal/step controls in Studio. Smaller values allow finer edits.

### I want to compare my edit against the original

Use Studio’s Before/After preview tools.

### I want to undo a Studio edit

Use Undo History or the Undo button. If the session feels wrong, discard the session before saving.

### Collections auto-apply did not trigger

Check your collection rule, make sure the target profile still exists, and copy the Collections Debug Report from the Diagnostics Clinic.

### Sync is not showing on another player

Sync compatibility is still experimental. Use the Sync Provider Activity Report or Full Clinic Report from the Diagnostics Clinic to check whether BoneSmith has a payload, whether the provider is registered, and whether anything has requested or applied sync data.

## Support

For bug reports, support, or feedback:

User Manual: https://drive.google.com/file/d/1VjVScWIlzvqCylqqeXst34vpIl_4lseY/view?usp=sharing

Discord: https://discord.gg/Dr836dmbqh

Ko-fi: https://ko-fi.com/rubyblaire

When submitting a bug report, please include:

- What you were doing
- What you expected to happen
- What happened instead
- BoneSmith version
- Whether you were using Studio, Import, Collections, or Sync Compatibility
- A **Full Clinic Report** from the Diagnostics Clinic when possible

## Credits

Created by **Ruby Blaire**.

BoneSmith is a Ruby Blaire plugin made for players who want safer, cleaner, more flexible control over character profile shaping through a BoneSmith-owned workflow.

## Disclaimer

BoneSmith is an independent third-party Dalamud plugin and is not affiliated with, endorsed by, or maintained by Square Enix, goatcorp, XIVLauncher, Dalamud, Customize+, Penumbra, Mare, PlayerSync, or any other third-party tool.

Use plugins responsibly and in accordance with Dalamud, Final Fantasy XIV, and third-party tool community guidelines.

FINAL FANTASY XIV © SQUARE ENIX CO., LTD. All rights reserved.

## License

**BoneSmith is licensed under the GNU Affero General Public License v3.0.**

BoneSmith is a third-party Dalamud plugin and is not affiliated with, endorsed by, or maintained by goatcorp, XIVLauncher, Dalamud, Customize+, or Square Enix.
