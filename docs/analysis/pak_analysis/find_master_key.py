#!/usr/bin/env python3

# The decrypted output that all files produce
common_decrypted = bytes([0x58, 0x3F, 0x78, 0x6D, 0xB1, 0x24, 0x6A, 0xC4, 0xAA, 0xEA, 0xBF, 0xD5, 0x7A, 0x44, 0xB6, 0x31,
                          0x34, 0x26, 0xE3, 0xB6, 0x54, 0xBF, 0x46, 0x26, 0x07, 0xDC, 0x23, 0xB1, 0x98, 0xDA, 0xB7, 0xC4,
                          0xAF, 0x66, 0xEA, 0xBD, 0xA6, 0xD5])

# The expected plaintext
plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'

# Calculate the MASTER KEY
master_key = bytes([common_decrypted[i] ^ plaintext[i] for i in range(len(plaintext))])

print("MASTER KEY DISCOVERED!")
print("=" * 80)
print(f"\nMaster Key (38 bytes in hex):")
for i in range(0, len(master_key), 16):
    hex_str = ' '.join(f'{b:02X}' for b in master_key[i:i+16])
    print(f"  {hex_str}")

print(f"\nC# byte array:")
print("private static readonly byte[] MASTER_KEY = {")
for i in range(len(master_key)):
    if i > 0:
        print(", ", end="")
    if i > 0 and i % 8 == 0:
        print()
        print("    ", end="")
    print(f"0x{master_key[i]:02X}", end="")
print()
print("};")

# Now verify this works for all our files
print(f"\n{'=' * 80}")
print("VERIFICATION - Testing master key on all sample files")
print("=" * 80)

files = [
    {"filename": "2k_ChatUI.xml", "offset": 0x27D,
     "key": [0xB6, 0x18, 0xC5, 0x65, 0xB0, 0xF3, 0x2F, 0x8F, 0x8A, 0xF0, 0x1B, 0xA4, 0xEB, 0x91, 0xC0, 0xFF,
             0x9F, 0x96, 0x90, 0x33, 0x19, 0xBF, 0xE2, 0x13, 0xB6, 0xFA, 0xC5, 0xE5, 0x69, 0x6F, 0x9C, 0x16,
             0xFE, 0x7B, 0x9F, 0x83, 0xCC, 0x5D]},
    {"filename": "2k_MainButtonUI.xml", "offset": 0x1540,
     "key": [0x59, 0xCA, 0x5F, 0xE0, 0xFA, 0x15, 0x6A, 0x58, 0xF5, 0x52, 0xE6, 0x48, 0xF9, 0x16, 0x6A, 0x78,
             0xDE, 0x0D, 0xF0, 0x16, 0x24, 0x2E, 0xBF, 0xC1, 0x8B, 0xA1, 0x3A, 0xF5, 0x35, 0x2C, 0x73, 0xB1,
             0x4B, 0x8F, 0xFB, 0xF4, 0x4A, 0x40]},
    {"filename": "2k_MainCharInfoUI.xml", "offset": 0x2840,
     "key": [0xD9, 0x2A, 0xD2, 0xA7, 0xFC, 0xA7, 0x19, 0x54, 0xA6, 0xA0, 0xC3, 0x53, 0x15, 0x5C, 0x9F, 0x55,
             0xB9, 0x40, 0x15, 0xCB, 0x40, 0x23, 0x70, 0x9B, 0x70, 0x1B, 0x0C, 0x70, 0xD7, 0x32, 0xBD, 0x64,
             0xF6, 0x4B, 0x91, 0x90, 0x82, 0x5A]},
]

pak_file = r"D:\L1R Project\LineageWarriorClient\ui.pak"

with open(pak_file, 'rb') as f:
    for file_info in files:
        print(f"\n{file_info['filename']}:")

        # Read encrypted data
        f.seek(file_info['offset'])
        encrypted = f.read(60)

        # Decrypt using ONLY the master key
        decrypted_with_master = bytes([encrypted[i] ^ master_key[i % len(master_key)] for i in range(len(encrypted))])

        print(f"  Decrypted with master key: {decrypted_with_master[:38]}")
        print(f"  Expected plaintext:        {plaintext}")
        print(f"  Match: {decrypted_with_master[:38] == plaintext}")

        # Now check what the derived "key" actually represents
        derived_key = bytes(file_info['key'])
        file_specific_key = bytes([derived_key[i] ^ master_key[i] for i in range(len(derived_key))])

        print(f"  File-specific key (derived XOR master): {' '.join(f'{b:02X}' for b in file_specific_key[:16])}")
