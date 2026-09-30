# Backbenchers Studio

An interactive Unity studio visit by Thomas D. Lynn. The welcome page and project stories load before the 3D engine; choose **Step inside** to explore.

- Drag to look; WASD or arrows to walk.
- Four compact camera controls provide a guided visit: room, desk, crew, and drone lab.
- Touch movement controls and a light quality preset support smaller screens.
- Selected work, crew posters, profile, and portfolio links remain available without starting Unity.

## Reading room and motion update · 1.4

Exploration now follows display cadence, with time-based acceleration and damped turning. Auto quality adjusts render resolution from Unity frame samples; it targets 60 fps without assuming every GPU or browser can sustain it. Background pages and non-scene dialogs throttle rendering. Touch devices use a left-thumb movement stick and independent drag-to-look, and Comfort settings expose sensitivity and reduced motion.

Seating moves the camera to each device's actual screen. The curved monitor geometry is larger, and the HTML workspace follows its projected bounds on desktop and landscape tablets. Small screens use a readable focus view. Stand up restores the previous room position. Local and remote screen previews only encode while the computer is in use; the last preview remains on the physical monitor afterward.

Merlin's original procedural Mac mini study has a rounded metal shell, recessed base, ports and cable routing. The Phantom, keyboard, computer and research mat have separate worktop zones. Original Demo shelf footprints now hold upright books and low stacks, with each of the 24 titles placed exactly once. A faceless graphite portrait occupies an existing desk frame. A new interactive phone uses the actual EGUnion Android launcher artwork; its lightweight handset geometry is original because no handset mesh was found in the local EGUnion project. Twenty-four distinct sourced titles include nine verified Khet Zaw books, Kyar Pauk, memoir, poetry and Spring Revolution writing; this is a selected collection, not a claim of an exhaustive bibliography. Open **Books** or tap a physical book for cover details and sources. See [cover provenance](assets/books/SOURCES.md).

To reproduce the additive scene upgrade, copy `assets/books` image files into `Assets/Backbenchers/Books`, copy the studio screen JPGs, `aung-san-suu-kyi-pencil.png`, and `egunion-phone-screen.png` into `Assets/Backbenchers/Brand`, and copy the authored scripts from `Source/Backbenchers`. Run **Upgrade reading room and workstations**, then build Web. It preserves the original room, is repeatable, and is also called by **Rebuild research studio**. The procedural meshes are generated locally under `DisplayMeshes`. Rebake warm studio lighting after changing book or desk placement, save the scene, then build.

## Release 1.4 validation

The final Unity Web build completed with zero errors. Browser checks covered all seven devices (power off/on, seat, stand up), screen alignment at 1280 × 800, rotation to 820 × 1180, and a 375 px mobile layout with no horizontal overflow. The library contains 24 unique titles and links to full-size covers. The local Python terminal returned 42 for `print(6 * 7)`. Desktop exploration reported around 60 fps on this machine. Touch layouts were checked in browser emulation; real multitouch gestures and physical iOS/Android performance still require device testing.

## Interactive desktop update

Version 1.3 removes the large desk nameplates and the logo board surround, leaving the original Backbenchers mark directly on the wall. An FPVStrike static model display uses the Military_Drone_02 airframe referenced by EGUnion's FPVStrike prefab. Its visible mesh is baked into a static display for lighting and runtime efficiency; no gameplay or flight systems are imported.

Every device opens a curved monitor interface while the real room remains visible. **Ubuntu** connects to a password-protected Ubuntu 24.04 LTS desktop on the studio's existing server. It uses lightweight Xfce and Ubuntu Yaru styling, with a real Bash shell, Python, Node.js, Git, browser, editor, and persistent Linux files. It is one shared crew workstation; the three monitors do not create separate accounts or sessions. Screen power controls the display, not the server. Closing the monitor returns to the room and retains the most recent preview without background image encoding.

Public visitors can also use the separate **local** Files, Editor, and Terminal apps without server credentials. These run supported file commands, quoted arguments, redirection, text pipelines, and real Python through [Pyodide](https://pyodide.org/en/stable/usage/webworker.html) 314.0.7, loaded lazily from jsDelivr. The local workspace is not Ubuntu: native binaries and apt are available only in the authenticated Ubuntu desktop. Completed local text files persist in browser storage (2 MB total, 500 KB per file); clearing browser storage removes them. Download exports a file. Stop terminates a long-running worker and restores the last completed snapshot.

Ubuntu credentials are never bundled in the website or repository. The desktop connection uses noVNC over authenticated VNC inside TLS WebSocket transport. See [server operations](server/README.md) for deployment, isolation, maintenance, and recovery. Local browser workspace files are separate from Ubuntu files and are not uploaded automatically.

## Research studio update

Version 1.2 adds a brighter warm-white lighting bake, a rear-wall logo board, the framed Backbenchers Jolly Roger, and three crew workstations. Racer and Phantom drone display models and a Glock game-art study are imported from the local EGUnion 1.10.1 project. Source model files and commercial assets remain outside this public repository.

Six devices (three monitors, two tablets, and a phone) support power toggling, screen changes, and realtime light spill. Tap a device, aim and press E, or use the Devices menu. Desktop Walk mode requests mouse capture where the browser supports it; Escape releases it. Drag-to-look and touch movement remain available. The four guided views now include the drone lab.

`StudioLabBuilder` extends the Demo-based `StudioBuilder` scene. Use **Prepare lab models**, **Rebuild research studio**, **Apply Web Release Settings**, then **Bake warm studio lighting**. Rebuild after source changes before baking. `screen-art.html` preserves the custom screen artwork source.

## Warm studio update

The room retains the original Demo furniture and prop placement, wood finishes, and red lamps. Three small oak-framed bounty posters occupy the original three-picture gallery footprint; the Backbenchers identity is now displayed above the rear bookcase. Static lighting is baked with soft pendant light and cool window fill, using the Demo sun direction. Realtime shadows are disabled, with one or two per-pixel lights reserved for device glow; rendering slows while the page is hidden or behind a non-scene dialog.

Original bounty artwork links back to [Thomas D. Lynn](https://thomasdlynn.dev), [Hlaing Gyi](https://mfu-hlaing.github.io/), and [Trafalgar D. Merlin](https://afk-merlin.github.io/). Open Graph and Twitter metadata reference a dedicated 1200 × 630 social preview. Its HTML source is `social-preview.html`.

## Release

Built with Unity 6000.7.0b2, with WebGPU preferred and WebGL2 fallback. Brotli builds use Unity's decompression fallback for GitHub Pages. `unity/build.json` maps the generated build files to the custom loader. GitHub Actions publishes only the site assets.

The editable Unity project is maintained locally as `BackbenchersStudio`. Its branded scene is `Assets/Backbenchers/Scenes/BackbenchersStudio.unity`. Use the **Backbenchers** editor menu to prepare model lightmap UVs, rebuild the scene, apply web settings, and bake lighting. Wait for the bake to finish and save the scene, then build Web to this repository's `unity` directory. Run `python3 scripts/build-manifest.py` after a build to fingerprint all runtime artifacts and avoid mixing cached releases. Authored Unity scripts are preserved in `Source/Backbenchers`; copy them into `Assets/Backbenchers` in a project containing your licensed original package and studio branding assets.

## Credits and rights

The vendored noVNC client is version 1.7.0, commit `63107bd06d9e1f6136ff21aeda8cd62cbf0d433e`, from [the official noVNC repository](https://github.com/novnc/noVNC). Its MPL-2.0 and bundled dependency notices are preserved in `desktop/novnc/LICENSE.txt` and the source headers.

Original Backbenchers logo and project imagery supplied by the studio. Environment based on **Design Studio Interior v1.3** by **3D Everything**. The commercial environment source package is not distributed in this repository. Compiled runtime content is included as part of the studio experience; no license to reuse the underlying commercial assets is granted. Project collaborators and original creators are credited in the linked project stories at https://thomasdlynn.dev.

Unity 6.7 is a beta release. Responsive layout checks do not substitute for testing on physical iOS and Android devices.
