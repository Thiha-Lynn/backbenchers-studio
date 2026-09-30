# Backbenchers Studio

An interactive Unity studio visit by Thomas D. Lynn. The welcome page and project stories load before the 3D engine; choose **Step inside** to explore.

- Drag to look; WASD or arrows to walk.
- Four compact camera controls provide a guided visit: room, desk, crew, and quiet corner.
- Touch movement controls and a light quality preset support smaller screens.
- Selected work, crew posters, profile, and portfolio links remain available without starting Unity.

## Warm studio update

The room retains the original Demo furniture and prop placement, wood finishes, and red lamps. Three small oak-framed bounty posters occupy the original three-picture gallery footprint; the Backbenchers identity sits in the original wall-text location. Static lighting is baked with soft pendant light and cool window fill, using the Demo sun direction. Realtime shadows and per-pixel lights are disabled; rendering slows while idle or behind a dialog.

Original bounty artwork links back to [Thomas D. Lynn](https://thomasdlynn.dev), [Hlaing Gyi](https://mfu-hlaing.github.io/), and [Trafalgar D. Merlin](https://afk-merlin.github.io/). Open Graph and Twitter metadata reference a dedicated 1200 × 630 social preview. Its HTML source is `social-preview.html`.

## Release

Built with Unity 6000.7.0b2, with WebGPU preferred and WebGL2 fallback. Brotli builds use Unity's decompression fallback for GitHub Pages. `unity/build.json` maps the generated build files to the custom loader. GitHub Actions publishes only the site assets.

The editable Unity project is maintained locally as `BackbenchersStudio`. Its branded scene is `Assets/Backbenchers/Scenes/BackbenchersStudio.unity`. Use the **Backbenchers** editor menu to prepare model lightmap UVs, rebuild the scene, apply web settings, and bake lighting. Wait for the bake to finish and save the scene, then build Web to this repository's `unity` directory. Run `python3 scripts/build-manifest.py` after a build to fingerprint all runtime artifacts and avoid mixing cached releases. Authored Unity scripts are preserved in `Source/Backbenchers`; copy them into `Assets/Backbenchers` in a project containing your licensed original package and studio branding assets.

## Credits and rights

Original Backbenchers logo and project imagery supplied by the studio. Environment based on **Design Studio Interior v1.3** by **3D Everything**. The commercial environment source package is not distributed in this repository. Compiled runtime content is included as part of the studio experience; no license to reuse the underlying commercial assets is granted. Project collaborators and original creators are credited in the linked project stories at https://thomasdlynn.dev.

Unity 6.7 is a beta release. Responsive layout checks do not substitute for testing on physical iOS and Android devices.
