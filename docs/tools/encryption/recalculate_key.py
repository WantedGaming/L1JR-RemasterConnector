#!/usr/bin/env python3
"""
Recalculate XOR key using actual PAK file data
"""

# Known plaintext
xml_plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'

# ACTUAL encrypted data from PAK file (ui.pak, first file)
pak_encrypted_hex = (
    "583f786db1246ac4aaeabfd57a44b631" +
    "3426e3b654bf462607dc23b198dab7c4"
)
pak_encrypted = bytes.fromhex(pak_encrypted_hex)

print("Known plaintext:", xml_plaintext[:32])
print("PAK encrypted:", pak_encrypted)
print()

# Calculate XOR key
min_len = min(len(xml_plaintext), len(pak_encrypted))
xor_key = bytes([xml_plaintext[i] ^ pak_encrypted[i] for i in range(min_len)])

print("XOR Key (from PAK):")
print(f"  Hex: {xor_key.hex()}")
print(f"  Bytes: {list(xor_key)}")
print(f"  Length: {len(xor_key)} bytes")
print()

# Compare with old key from your hex dump
old_key_hex = "0000000000000000bc99d6bac97d88a17e16c196ecd539e807b54dd678fcfe31"
old_key = bytes.fromhex(old_key_hex)

print("Old key (from hex dump):")
print(f"  Hex: {old_key_hex}")
print()

print("Comparison:")
print("Position | Plaintext | PAK Encrypted | Old Encrypted | PAK Key | Old Key")
print("-" * 75)
for i in range(min(len(xml_plaintext), len(pak_encrypted))):
    p = xml_plaintext[i]
    e_pak = pak_encrypted[i]
    k_pak = xor_key[i]
    k_old = old_key[i] if i < len(old_key) else 0

    # Reconstruct what old encrypted would be
    e_old = p ^ k_old if k_old > 0 else p

    print(f"{i:8d} | {p:9d} ({chr(p) if 32 <= p < 127 else '?'}) | {e_pak:13d} | {e_old:13d} | {k_pak:7d} | {k_old:7d}")

print()
print("=" * 80)
print("CONCLUSION:")
print("=" * 80)

if xor_key[:8] == bytes(8):
    print("First 8 bytes: NO ENCRYPTION (all zeros)")
    print("Encryption starts at byte 8")
    print()
    print("XOR Key (bytes 8+):")
    print(f"  Hex: {xor_key[8:].hex()}")
    print(f"  Bytes: {list(xor_key[8:])}")
else:
    print("ENTIRE FILE IS ENCRYPTED (including first 8 bytes)")
    print()
    print("XOR Key:")
    print(f"  Hex: {xor_key.hex()}")
    print(f"  Bytes: {list(xor_key)}")
