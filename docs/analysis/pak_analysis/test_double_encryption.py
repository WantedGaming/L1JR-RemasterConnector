#!/usr/bin/env python3

# Hypothesis: Two-layer XOR encryption
# Layer 1: Master key (same for all files)
# Layer 2: File-specific key (different for each file)

master_key = bytes([0x64, 0x00, 0x00, 0x00, 0xDD, 0x04, 0x1C, 0xA1,
                    0xD8, 0x99, 0xD6, 0xBA, 0x14, 0x79, 0x94, 0x00,
                    0x1A, 0x16, 0xC1, 0x96, 0x31, 0xD1, 0x25, 0x49,
                    0x63, 0xB5, 0x4D, 0xD6, 0xA5, 0xF8, 0xE2, 0x90,
                    0xE9, 0x4B, 0xD2, 0x9F, 0x99, 0xEB])

plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'

files = [
    {"filename": "2k_ChatUI.xml", "offset": 0x27D,
     "derived_key": [0xB6, 0x18, 0xC5, 0x65, 0xB0, 0xF3, 0x2F, 0x8F, 0x8A, 0xF0, 0x1B, 0xA4, 0xEB, 0x91, 0xC0, 0xFF,
                     0x9F, 0x96, 0x90, 0x33, 0x19, 0xBF, 0xE2, 0x13, 0xB6, 0xFA, 0xC5, 0xE5, 0x69, 0x6F, 0x9C, 0x16,
                     0xFE, 0x7B, 0x9F, 0x83, 0xCC, 0x5D]},
    {"filename": "2k_MainButtonUI.xml", "offset": 0x1540,
     "derived_key": [0x59, 0xCA, 0x5F, 0xE0, 0xFA, 0x15, 0x6A, 0x58, 0xF5, 0x52, 0xE6, 0x48, 0xF9, 0x16, 0x6A, 0x78,
                     0xDE, 0x0D, 0xF0, 0x16, 0x24, 0x2E, 0xBF, 0xC1, 0x8B, 0xA1, 0x3A, 0xF5, 0x35, 0x2C, 0x73, 0xB1,
                     0x4B, 0x8F, 0xFB, 0xF4, 0x4A, 0x40]},
    {"filename": "2k_MainCharInfoUI.xml", "offset": 0x2840,
     "derived_key": [0xD9, 0x2A, 0xD2, 0xA7, 0xFC, 0xA7, 0x19, 0x54, 0xA6, 0xA0, 0xC3, 0x53, 0x15, 0x5C, 0x9F, 0x55,
                     0xB9, 0x40, 0x15, 0xCB, 0x40, 0x23, 0x70, 0x9B, 0x70, 0x1B, 0x0C, 0x70, 0xD7, 0x32, 0xBD, 0x64,
                     0xF6, 0x4B, 0x91, 0x90, 0x82, 0x5A]},
]

# For each file, calculate file-specific key
for file_info in files:
    derived = bytes(file_info['derived_key'])
    file_specific = bytes([derived[i] ^ master_key[i] for i in range(len(derived))])

    file_info['file_specific_key'] = file_specific
    print(f"{file_info['filename']}:")
    print(f"  File-specific key: {' '.join(f'{b:02X}' for b in file_specific)}")

pak_file = r"D:\L1R Project\LineageWarriorClient\ui.pak"

print("\n" + "=" * 80)
print("Testing two-layer decryption")
print("=" * 80)

with open(pak_file, 'rb') as f:
    for file_info in files:
        print(f"\n{file_info['filename']}:")

        # Read encrypted data
        f.seek(file_info['offset'])
        encrypted = f.read(60)

        # Try: encrypted XOR file_specific XOR master
        layer1 = bytes([encrypted[i] ^ file_info['file_specific_key'][i % len(file_info['file_specific_key'])] for i in range(len(encrypted))])
        decrypted1 = bytes([layer1[i] ^ master_key[i % len(master_key)] for i in range(len(layer1))])

        print(f"  Method 1 (file_specific, then master): {decrypted1[:38]}")
        print(f"  Match: {decrypted1[:38] == plaintext}")

        # Try: encrypted XOR master XOR file_specific
        layer2 = bytes([encrypted[i] ^ master_key[i % len(master_key)] for i in range(len(encrypted))])
        decrypted2 = bytes([layer2[i] ^ file_info['file_specific_key'][i % len(file_info['file_specific_key'])] for i in range(len(layer2))])

        print(f"  Method 2 (master, then file_specific): {decrypted2[:38]}")
        print(f"  Match: {decrypted2[:38] == plaintext}")

        # Try: encrypted XOR derived_key (original approach)
        derived_key = bytes(file_info['derived_key'])
        decrypted3 = bytes([encrypted[i] ^ derived_key[i % len(derived_key)] for i in range(len(encrypted))])

        print(f"  Method 3 (derived_key only):            {decrypted3[:38]}")
        print(f"  Match: {decrypted3[:38] == plaintext}")
