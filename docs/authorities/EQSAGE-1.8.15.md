# EQ Sage 1.8.15 authority

DDVWF's initial Sage parity target is commit `e85bb85a19670eb377d876521bb1adf130b4e83a`.

Verified source seams used by the native provider:

- `src/lib/s3d/wld/wld.js`: WLD header, string table, fragment dispatch and WLD classification.
- `src/lib/s3d/s3d-decoder.js`: PFS/WLD/image producer, material mapping, GLB producer and object/animation export.
- `src/lib/s3d/animation/actor.js`: ActorDefinition and ActorInstance flags, references, locations and scale.
- `src/lib/s3d/materials/material.js`: material type to shader semantics.
- `src/viewer/controllers/ZoneController.js`: Babylon consumer lifecycle, environment, zone GLB, reusable object instantiation, regions and texture animation.

A source seam is not considered implemented merely because it is listed here. Production paths fail closed until their transitive producer and consumer dependencies are represented and tested.
