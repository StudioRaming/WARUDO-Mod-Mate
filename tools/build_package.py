from pathlib import Path
import argparse
import hashlib
import io
import json
import os
import re
import struct
import tarfile
import time
import zlib


def build(root, output):
    timestamp = int(os.environ.get('SOURCE_DATE_EPOCH', str(int(time.time()))))
    product = root / 'Assets/StudioRaming/WarudoBatchBuilder'
    assets = [product.parent, product] + sorted(
        (p for p in product.rglob('*') if p.suffix != '.meta'),
        key=lambda p: p.relative_to(root).as_posix())
    entries, guids = [], set()
    for p in assets:
        metadata = Path(str(p) + '.meta').read_bytes()
        match = re.search(rb'^guid:\s*([0-9a-f]{32})\s*$', metadata, re.M)
        if not match:
            raise ValueError('Missing GUID: ' + p.relative_to(root).as_posix())
        guid = match.group(1).decode('ascii')
        if guid in guids:
            raise ValueError('Duplicate GUID: ' + guid)
        guids.add(guid)
        entries.append((p, metadata, guid))
    buffer = io.BytesIO()
    with tarfile.open(fileobj=buffer, mode='w', format=tarfile.GNU_FORMAT) as archive:
        for p, metadata, guid in entries:
            directory = tarfile.TarInfo(guid + '/')
            directory.type, directory.mode, directory.mtime = tarfile.DIRTYPE, 0o777, timestamp
            archive.addfile(directory)
            files = [('asset.meta', metadata), ('pathname', p.relative_to(root).as_posix().encode())]
            if p.is_file():
                files.insert(0, ('asset', p.read_bytes()))
            for name, payload in files:
                entry = tarfile.TarInfo(guid + '/' + name)
                entry.mode, entry.mtime, entry.size = 0o777, timestamp, len(payload)
                archive.addfile(entry, io.BytesIO(payload))
    raw = buffer.getvalue()
    compressor = zlib.compressobj(6, zlib.DEFLATED, -15)
    body = compressor.compress(raw) + compressor.flush()
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(b'\x1f\x8b\x08\x08' + struct.pack('<I', timestamp)
                      + b'\x04\x00archtemp.tar\x00' + body
                      + struct.pack('<II', zlib.crc32(raw) & 0xffffffff, len(raw) & 0xffffffff))
    with tarfile.open(output, 'r:gz') as archive:
        actual = [archive.extractfile(m).read().decode() for m in archive.getmembers() if m.name.endswith('/pathname')]
    expected = [p.relative_to(root).as_posix() for p, _, _ in entries]
    if actual != expected:
        raise ValueError('Package inventory mismatch')
    receipt = {'file': output.name, 'sha256': hashlib.sha256(output.read_bytes()).hexdigest(),
               'bytes': output.stat().st_size, 'assets': expected}
    output.with_suffix('.manifest.json').write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: v for k, v in receipt.items() if k != 'assets'}))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Build a Unity 2021.3 package with update-safe timestamps; set SOURCE_DATE_EPOCH for reproducible builds.')
    parser.add_argument('--output', type=Path, default=Path('dist/WARUDO_Mod_Mate_0.1.1.unitypackage'))
    args = parser.parse_args()
    build(Path(__file__).resolve().parent.parent, args.output.resolve())
