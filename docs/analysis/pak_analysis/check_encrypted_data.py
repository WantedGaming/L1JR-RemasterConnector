import struct

# Read raw PAK file and check encrypted data for our sample files
files = [
    {"filename": "2k_ChatUI.xml", "offset": 0x27D, "size": 4803,
     "key": [0xB6, 0x18, 0xC5, 0x65, 0xB0, 0xF3, 0x2F, 0x8F, 0x8A, 0xF0, 0x1B, 0xA4, 0xEB, 0x91, 0xC0, 0xFF,
             0x9F, 0x96, 0x90, 0x33, 0x19, 0xBF, 0xE2, 0x13, 0xB6, 0xFA, 0xC5, 0xE5, 0x69, 0x6F, 0x9C, 0x16,
             0xFE, 0x7B, 0x9F, 0x83, 0xCC, 0x5D]},
    {"filename": "2k_MainButtonUI.xml", "offset": 0x1540, "size": 4864,
     "key": [0x59, 0xCA, 0x5F, 0xE0, 0xFA, 0x15, 0x6A, 0x58, 0xF5, 0x52, 0xE6, 0x48, 0xF9, 0x16, 0x6A, 0x78,
             0xDE, 0x0D, 0xF0, 0x16, 0x24, 0x2E, 0xBF, 0xC1, 0x8B, 0xA1, 0x3A, 0xF5, 0x35, 0x2C, 0x73, 0xB1,
             0x4B, 0x8F, 0xFB, 0xF4, 0x4A, 0x40]},
]

pak_file = r"D:\L1R Project\LineageWarriorClient\ui.pak"

# Known plaintext
plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'

print("Checking if encrypted data itself contains key pattern...\n")

with open(pak_file, 'rb') as f:
    for file_info in files:
        print(f"File: {file_info['filename']}")
        print(f"  Offset: 0x{file_info['offset']:X}")

        # Read encrypted data
        f.seek(file_info['offset'])
        encrypted = f.read(min(100, file_info['size']))

        print(f"  Encrypted (first 38 bytes): {' '.join(f'{b:02X}' for b in encrypted[:38])}")
        print(f"  Derived key (first 38 bytes): {' '.join(f'{b:02X}' for b in file_info['key'][:38])}")

        # Verify decryption
        decrypted = bytes([encrypted[i] ^ file_info['key'][i] for i in range(min(len(encrypted), len(file_info['key'])))])
        print(f"  Decrypted (first 38 bytes): {decrypted[:38]}")
        print(f"  Matches plaintext: {decrypted[:38] == plaintext[:38]}")

        # Check if key appears anywhere in the PAK before this file
        print(f"  Searching for key in PAK before offset...")
        f.seek(0)
        pak_before = f.read(file_info['offset'])
        key_bytes = bytes(file_info['key'])

        # Search for exact key match
        if key_bytes in pak_before:
            pos = pak_before.find(key_bytes)
            print(f"  FOUND: Key appears at offset 0x{pos:X}!")
        else:
            # Search for partial matches (at least 10 consecutive bytes)
            best_match = 0
            best_pos = -1
            for i in range(len(pak_before) - 10):
                match_count = 0
                for j in range(min(38, len(pak_before) - i)):
                    if pak_before[i + j] == key_bytes[j]:
                        match_count += 1
                    else:
                        break
                if match_count > best_match:
                    best_match = match_count
                    best_pos = i

            if best_match >= 10:
                print(f"  PARTIAL MATCH: {best_match}/38 bytes at offset 0x{best_pos:X}")
            else:
                print(f"  NOT FOUND: Key not embedded in PAK file before this entry")

        print()

# Additional hypothesis: keys might be in a separate file
import os
print("\nSearching for potential key files...")
search_dirs = [
    r"D:\L1R Project\LineageWarriorClient",
    r"D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"
]

for search_dir in search_dirs:
    for root, dirs, filenames in os.walk(search_dir):
        for filename in filenames:
            if filename.endswith(('.key', '.dat', '.bin', '.cfg')):
                filepath = os.path.join(root, filename)
                size = os.path.getsize(filepath)
                print(f"  Found: {filepath} ({size} bytes)")
