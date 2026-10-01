## Studio tour 1.9 — in-place inspection, phones and technical shelf

Object inspection keeps the authored object at its original position and scale. Orbit, tilt, pan, zoom and reset move the camera around it; there is no enlarged floating copy. Cover opening reveals a centered spread immediately and hinges over the right-hand page. Narrow screens retain a readable single-page layout.

Both phones now use a dedicated portrait interface with tap/swipe unlock, a home screen, locally saved Notes, the five-drawing Photos gallery, a clock, calculator, lock and power controls. “Pick up phone” and “Put down” replace the computer seating flow. The previous game-branded phone screen is replaced in the scene and device card. Computers and tablets retain their workspace.

Six publisher-sourced technical titles join the existing books: AI Agents and Applications, AI Agents in Action, AI Engineering, Learning TypeScript, Learn Docker in a Month of Lunches (second edition), and Designing Data-Intensive Applications (second edition). A few face-out covers and a low stack occupy existing shelf tops. The library has a Tech & tools filter. Sources and image URLs are recorded in `assets/books/technology-sources.json` (checked October 1, 2026). These are book companions and source links; full commercial book texts are not bundled. Run `StudioTechnologyBuilder.Apply()` after importing these covers and `Brand/phone-home.png`.

The displayed drawing PNGs were edited with the built-in image generator to remove handwritten signatures/dates (and the still-life competition heading), retaining the drawings as closely as possible. Prompts are recorded in `assets/art/generation.json`; untouched source photos remain available through Original.

Validation scripts include `verify-phone.cjs`, opening-center assertions in `verify-reader-fit.cjs`, and the existing room/art/reader checks.

## Studio tour 1.8 — drawing collections (layout foundation)

Five studio-supplied pencil drawings are arranged as loose sheets across the working desks and a slim oak drawing table. The original desk papers and supplies are restored and clear of the drawings. Each new sheet has a brighter neutral paper material, a thin cotton-paper edge, slight curl, and its own pickup interaction. A still life sits beside the software desk’s book, a pair overlaps beside the creative desk’s tablet, and two full studies sit with drafting stacks, pencils and an eraser on the matching oak table. A flight note rests on the lab’s existing storage box. The papers use clear surfaces without replacing the original props. The clean paper PNGs were regenerated from the supplied photographs using the built-in image generator, preserving their subjects and compositions as closely as possible. They are reconstructions, not pixel-identical scans. The exact source JPEGs, including signatures, remain in `assets/art` and accessible through Original. The PNGs and full generation prompts (`assets/art/generation.json`) are saved there for reuse; the room uses compressed texture imports. The collection credit is **Studio drawing collections**, as requested by the studio.

Pick up any sheet or choose **Objects → Still life · 5 pencil studies**. The large viewer supports side arrows, keyboard arrows, swipe navigation, wheel/pinch zoom, drag to pan, rotation, Fit, Save PNG, and an Original link. Moving through the collection returns the previous sheet to the desk; closing restores the current one. The book reader also uses side navigation and a larger paper area, with bounded folding and responsive pagination.

Copy `assets/art/*-paper.png` to `Assets/Backbenchers/Art` before running `StudioArtBuilder.Apply` (also called by the immersion builder). This adds dynamic paper meshes; bake lighting and save before building if restoring previously hidden props or removing prior scene additions. Run `scripts/verify-art.cjs` alongside the reader and room regression scripts below.

Validation: Unity Web build succeeded with zero errors. Browser checks cover all five scene paper pickups/returns, gallery and object controls, device power/sit/stand, room lights and door, SFX events, book replacement, the 211-page archival PDF, touch page turns, art swipe/pinch, keyboard navigation, PNG links, reduced motion, 320–1280 px layouts, short windows, resizing mid-turn, and native Chrome fullscreen. No page runtime errors occurred in the checked room flows. Physical iOS/Android hardware remains untested.

## Studio tour 1.7 — room, objects, and paper

Objects remain at their authored positions while the camera provides horizontal orbit, vertical tilt, pan, zoom, reset, and optional auto rotation. Drag or use the buttons; Shift-drag pans, scroll/pinch zooms, arrows tilt/rotate, and R resets. Wall artwork has controlled angled views, pan and zoom. Inspection panels size to the available window height, and the camera accounts for the panel’s footprint.

The portrait, jasmine and bird studies return to the original wall grouping, and both original small desk frames are present again. The added aircraft and drone wall frames have been removed at the studio’s request. The Irrawaddy study stays beside the reading shelf. A sailplane drawing follows the plotter’s existing curved paper mesh. `scripts/create-flight-art.py` preserves the drawing source (Pillow; Courier New on macOS).

The physical wall switch and Lights button toggle the room’s illumination. Existing baked surfaces fade to an evening level while device screens retain their independent power and glow. The door swings on its hinge with its handle; the tour button guides the view to the entrance. Original procedural footsteps, switches, door, pickup, return, and paper sounds have an independent SFX mute control. Reduced motion disables camera easing, automatic rotation, and door/light transitions.

The reader has a hinged cover, immediate pointer-driven paper folds, forward/back touch swipes, and animated pickup/return. Choosing another book restores the previous room model and loads only the selected jacket; stale asynchronous selections and PDF loads cannot replace it. The paper stage reserves room for the toolbars and footer during resize and fullscreen changes. Short windows collapse extra controls under Tools. Published books retain their clearly labeled reading companions; the 211-page archival novel remains available in full.

To reproduce: copy the authored scripts from `Source/Backbenchers` to the local project, copy `assets/flight-*.png` to `Assets/Backbenchers/Brand` and `assets/art/*-paper.png` to `Assets/Backbenchers/Art`, run **Upgrade immersive room**, bake the lighting, save, and run `StudioImmersionBuilder.Build`. Run `python3 scripts/build-manifest.py` after a successful build. The commercial environment source remains local.

Browser regression scripts are in `scripts/verify-reader.cjs`, `verify-reader-touch.cjs`, `verify-reader-fit.cjs`, and `verify-room.cjs`. They use Playwright and default to `http://localhost:8765`; set `STUDIO_URL` to check another deployment. `PLAYWRIGHT_MODULE` and `CHROMIUM_EXECUTABLE` optionally select existing installations. Screenshots and results go to `/tmp`. The room script deliberately checks the WebGL fallback; WebGPU is checked separately in native Chrome. Mobile layouts and gestures are emulated; physical mobile devices are not covered.

## Studio tour 1.6

- Contextual game-style prompts for 144 authored display and everyday objects, plus seven working devices. Hover or aim, then tap/click or press E. Only the object under the pointer is labeled. Structural walls and floor remain scenery.
- Immersive in-room inspection with preserved return position, close/far controls, and controlled left/right views for solid objects. The three bounty posters have individual camera framing, next/previous navigation, and portfolio links. An Objects menu provides keyboard/touch access to the main collection.
- Auto details starts with a sharper, pixel-budgeted image and adapts from Unity’s measured frame pacing with smoothing and cooldowns. It does not promise a fixed frame rate on every device. Light and Detailed remain available.
- StPageFlip paper folds, front/back page surfaces, book-sized sheets, measured text pagination without inner text scrollbars, Myanmar serif type, a distraction-free reading mode, and bounded nearby PDF-page rendering. Full archival Myanmar fiction: မောင်ရင်မောင် မမယ်မ by James Hla Kyaw, 211 scan leaves, loaded only when selected. Source/edition notes are in `assets/books/SOURCES.md`.

The reading-room builder now runs **Apply object interactions** automatically; it can also be run independently before exporting Web. This adds colliders and metadata without moving furniture or invalidating the lighting bake.

# Backbenchers Studio

An interactive Unity studio visit by Thomas D. Lynn. The welcome page and project stories load before the 3D engine; choose **Step inside** to explore.

- Drag to look; WASD or arrows to walk.
- Five compact camera controls provide a guided visit: room, desk, crew, drone lab, and reading shelf.
- Touch movement controls and a light quality preset support smaller screens.
- Selected work, crew posters, profile, and portfolio links remain available without starting Unity.

## Release 1.6 validation

Unity Web build succeeded. All four output hashes match `unity/build.json`. Browser checks covered the in-scene book prompt and E-key handler, book pickup/return, individual bounty navigation and all three portfolio destinations, Mac mini and drone inspection, viewing-angle controls, return to room, and the EGUnion phone workspace. The actual 211-page archival PDF rendered and turned in desktop and narrow layouts; a physical corner fold was visually checked. Text pagination at 320 px had no horizontal overflow or clipped page copy; reading controls retain 44 px touch targets. The final poster framing was corrected after a screenshot revealed overlap with its information panel. No browser runtime errors were reported in the checked flows. The room reported about 58–60 fps on this laptop. Physical mobile hardware and real multitouch remain unverified; adaptive detail is not an all-device frame-rate guarantee.

## A working shelf, a reading desk · 1.5

The original binders, storage boxes, plants, and ornaments remain in their Demo positions. Books occupy small stacks within the original book footprints, with an outward-facing jacket above each stack and two featured books on the west cabinet. Twenty-four distinct titles appear once each. **05 Read** provides a closer shelf view. The wall gallery combines the faceless Aung San Suu Kyi portrait with original jasmine, Irrawaddy, and bird studies in warm oak, brass, and cotton-paper frames. The Mac mini has an Apple logo inlay.

Tap a physical book to pick it up: its room model is hidden until **Put back** returns it. The paper reader uses two-page spreads on laptops and a single page on phones, animated turns, keyboard/swipe navigation, contents, search, bookmarks, text sizing, and private notes. The room stays visible behind the pages. Published titles contain labeled companions and source links, not full copyrighted texts. The six-page studio journal is original writing. **Open file** reads local PDF, TXT, and Markdown files without uploading them. PDFs render through a lazily loaded, vendored PDF.js worker with bounded canvas resolution, fit-to-page zoom, and text search. Scanned PDFs need embedded text for search. Text imports are limited to 2 MB; PDFs to 40 MB and 2,000 pages. Password-protected PDFs need an unlocked copy. File contents are not persisted; bookmarks and notes remain in browser storage.

A quiet original 72 BPM lo-fi sketch starts after **Step inside**. It is synthesized locally using Web Audio, with no streamed recording. The tour bar provides mute, Comfort settings provides volume, and music suspends when the tab is hidden or the visitor exits. The preference is remembered.

`StudioGalleryBuilder.Apply` runs at the end of the repeatable workstation upgrade. Copy `studio-nature-triptych.png` and `apple-logo.png` into the Unity Brand folder along with the 1.4 assets. Rebuild, rebake lighting, save, and export as before. Original publisher covers are retained; the nature artwork was generated with the built-in image tool, with its full prompt saved under `assets`.

The reading interactions were inspired by [md2book](https://github.com/thixpin/md2book); the reading UI is custom and does not bundle that package. Version 1.6 uses StPageFlip 2.0.7 (MIT) for physical paper folds; see `desktop/pageflip/README.md` for its small lifecycle and PDF-canvas fixes. PDF.js 6.3.289 is from Mozilla's official `pdfjs-dist` npm package under Apache-2.0. Its license and font/CMap/wasm notices are preserved in `desktop/pdfjs`.

## Release 1.5 validation

The final Unity Web build succeeded and every fingerprinted runtime file matches its manifest. Browser checks verified shelf-book pickup and return, cover/spread transitions, bookmarks, saved position and notes, text size, and text search. A real 14-page PDF was imported, rendered, searched, zoomed, turned, and closed; a PDF.js lifecycle cleanup issue found during testing was corrected. The reading desk and tour controls were checked at 1280 px, 375 px, and 320 px widths. Music mute/resume suspended and resumed its AudioContext, and the room reported about 60 fps on this machine with music playing. PDF rendering is capped at 2.4 million pixels per visible page, and the PDF library is not part of the initial 3D load. Physical mobile performance and real multitouch remain unverified.

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
