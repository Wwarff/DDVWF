# Dragon's Den Visual WorldForge (DDVWF)

DDVWF is a renderer-agnostic EverQuest world editor. This repository is the clean development and CI authority for the rebuilt application.

## Architecture

Native EQ client assets -> Sage-derived decode/export semantics -> Complete Zone IR <- EQEmu/Spire server data -> editor command system -> renderer adapter.

DDVWF owns zone lifecycle, entity identity, selection, transforms, Inspector state, undo/redo, persistence and save semantics. Renderers visualize canonical state and provide rendering/picking services.

## Authority rules

- Do not copy or reconstruct the obsolete DragonDen-WorldForge tree as implementation authority.
- Reproduce functions implemented by EQ Sage from the actual Sage source/data flow, including producer and consumer dependencies.
- Reproduce EQEmu server semantics from EQEmu schema/source and Spire APIs rather than representing server entities as anonymous meshes.
- Keep server writes disabled until write semantics are proven.
- No guessed fallbacks, silent no-ops or placeholder production paths.
- Runtime evidence outranks source-only claims.
- The next human visual gate is an integrated application gate, not an incremental rendering experiment.

## UI contract

The application uses 12-point Verdana throughout. The workspace contains Scene/Zone and EQ Asset Browser panes, a central renderer viewport, Inspector and Model/Object/Texture/Material Viewer, diagnostics/change/native-data panes, movable persistent tool windows, and persistent workspace/camera/zone settings.

## Development

The repository begins clean intentionally. CI and implementation are built here without importing the obsolete DDVWF source tree.
