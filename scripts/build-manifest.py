from pathlib import Path
import json
root = Path(__file__).resolve().parents[1] / "unity"
patterns = {"loader": "*.loader.js", "data": "*.data.unityweb", "framework": "*.framework.js.unityweb", "wasm": "*.wasm.unityweb"}
manifest = {}
for key, pattern in patterns.items():
    matches = list((root / "Build").glob(pattern))
    if len(matches) != 1:
        raise SystemExit(f"Expected exactly one {pattern}, found {len(matches)}")
    manifest[key] = matches[0].relative_to(root).as_posix()
(root / "build.json").write_text(json.dumps(manifest, indent=2) + "\n")
print(json.dumps(manifest, indent=2))
