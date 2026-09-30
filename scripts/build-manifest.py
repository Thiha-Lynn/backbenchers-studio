"""Fingerprint Unity's generated files so an update never mixes cached releases."""
from pathlib import Path
import hashlib
import json
root = Path(__file__).resolve().parents[1] / "unity"
manifest_path = root / "build.json"
old = json.loads(manifest_path.read_text()) if manifest_path.exists() else {}
suffixes = {"loader": ".loader.js", "data": ".data.unityweb", "framework": ".framework.js.unityweb", "wasm": ".wasm.unityweb"}
manifest = {}
replacements = {}
for key, suffix in suffixes.items():
    source = root / "Build" / ("unity" + suffix)
    if not source.exists() and key in old:
        source = root / old[key]
    if not source.is_file() or source.stat().st_size == 0:
        raise SystemExit(f"Missing or incomplete Unity artifact: {source}")
    digest = hashlib.sha256(source.read_bytes()).hexdigest()[:12]
    destination = root / "Build" / ("studio-" + digest + suffix)
    if source != destination:
        source.replace(destination)
    manifest[key] = destination.relative_to(root).as_posix()
    replacements["unity" + suffix] = destination.name
for previous in old.values():
    path = root / previous
    if previous not in manifest.values() and path.parent == root / "Build" and path.is_file():
        path.unlink()
index = root / "index.html"
if index.exists():
    html = index.read_text()
    # The default Unity template constructs filenames from its build basename.
    for original, hashed in replacements.items():
        html = html.replace(original, hashed)
    index.write_text(html)
manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
print(json.dumps(manifest, indent=2))
