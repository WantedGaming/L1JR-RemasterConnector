#!/usr/bin/env python3
"""
Final XOR decryption tool for XML and CSB files
Based on known plaintext attack analysis
"""
import sys

# EXTRACTED XOR KEY (30 bytes) from known plaintext attack
XOR_KEY = bytes([
    0xbc, 0x99, 0xd6, 0xba, 0xc9, 0x7d, 0x88, 0xa1,
    0x7e, 0x16, 0xc1, 0x96, 0xec, 0xd5, 0x39, 0xe8,
    0x07, 0xb5, 0x4d, 0xd6, 0x78, 0xfc, 0xfe, 0x31,
    0x8d, 0x4b, 0xd2, 0x9f, 0x44, 0xef
])

def decrypt_xml(encrypted_data):
    """
    Decrypt XML file:
    - First 8 bytes are unencrypted (should be '<?xml ve')
    - Remaining bytes XOR'd with repeating 30-byte key
    """
    if len(encrypted_data) < 8:
        return encrypted_data

    # First 8 bytes are plain
    decrypted = bytearray(encrypted_data[:8])

    # Decrypt remaining bytes
    for i in range(8, len(encrypted_data)):
        key_index = (i - 8) % len(XOR_KEY)
        decrypted.append(encrypted_data[i] ^ XOR_KEY[key_index])

    return bytes(decrypted)

def decrypt_csb_method1(encrypted_data):
    """
    Decrypt CSB file - Method 1:
    XOR entire file with key starting at position 0
    """
    decrypted = bytearray()
    for i in range(len(encrypted_data)):
        key_index = i % len(XOR_KEY)
        decrypted.append(encrypted_data[i] ^ XOR_KEY[key_index])
    return bytes(decrypted)

def decrypt_csb_method2(encrypted_data):
    """
    Decrypt CSB file - Method 2:
    First 8 bytes plain, then XOR with key
    """
    if len(encrypted_data) < 8:
        return encrypted_data

    decrypted = bytearray(encrypted_data[:8])
    for i in range(8, len(encrypted_data)):
        key_index = (i - 8) % len(XOR_KEY)
        decrypted.append(encrypted_data[i] ^ XOR_KEY[key_index])
    return bytes(decrypted)

def encrypt_xml(plaintext_data):
    """
    Encrypt XML file:
    - First 8 bytes stay plain
    - Remaining bytes XOR'd with repeating 30-byte key
    """
    if len(plaintext_data) < 8:
        return plaintext_data

    encrypted = bytearray(plaintext_data[:8])
    for i in range(8, len(plaintext_data)):
        key_index = (i - 8) % len(XOR_KEY)
        encrypted.append(plaintext_data[i] ^ XOR_KEY[key_index])
    return bytes(encrypted)

# Test with provided samples
print("=" * 80)
print("XOR KEY INFORMATION")
print("=" * 80)
print(f"Key hex: {XOR_KEY.hex()}")
print(f"Key length: {len(XOR_KEY)} bytes")
print()

# Test XML decryption
xml_encrypted_hex = (
    "3c3f786d6c207665ceeabfd5a740aa90" +
    "5026e3b689bb5a8763dc23b145deab65" +
    "cb66eabd7bd18780bb4a32f1847a7153" +
    "a77423320ba125e84cd05ae4adbee497" +
    "b74e5d3b63082f667a112c30314426b7" +
    "f8b157e4026d3da40647420a2bba4942" +
    "697bc674300b3d9704d764b258d701d4" +
    "a0fcdcf9322837df9d0af40ba763f0f9"
)
xml_encrypted = bytes.fromhex(xml_encrypted_hex)

print("=" * 80)
print("XML DECRYPTION TEST")
print("=" * 80)
xml_decrypted = decrypt_xml(xml_encrypted)
print(f"Encrypted length: {len(xml_encrypted)} bytes")
print(f"Decrypted length: {len(xml_decrypted)} bytes")
print()
print("Decrypted content (first 100 bytes, hex):")
print(xml_decrypted[:100].hex())
print()
print("Decrypted content (as text, with replacements):")
# Print byte by byte to avoid encoding issues
for i, byte in enumerate(xml_decrypted[:100]):
    if 32 <= byte < 127:
        sys.stdout.write(chr(byte))
    else:
        sys.stdout.write(f'[{byte:02x}]')
print()
print()

# Test CSB decryption
csb_encrypted_hex = (
    "252c7bbef0db2a52c9935fa44cdf4838" +
    "6496a8dea53ac5061d21eed378b0fbff" +
    "b25aafc7badc53d56a10dcee5c85ca59" +
    "c5ccff65dd2f7c54b8f5583980def634"
)
csb_encrypted = bytes.fromhex(csb_encrypted_hex)

print("=" * 80)
print("CSB DECRYPTION TEST - Method 1 (XOR from byte 0)")
print("=" * 80)
csb_decrypted1 = decrypt_csb_method1(csb_encrypted)
print(f"First 16 bytes hex: {csb_decrypted1[:16].hex()}")
print(f"First 16 bytes: {list(csb_decrypted1[:16])}")
print(f"First 3 bytes as string: {csb_decrypted1[:3]}")
if csb_decrypted1[:3] == b'CSB':
    print("SUCCESS! Matches Cocos2d-x CSB header!")
else:
    print("No CSB header match")
print()

print("=" * 80)
print("CSB DECRYPTION TEST - Method 2 (First 8 bytes plain)")
print("=" * 80)
csb_decrypted2 = decrypt_csb_method2(csb_encrypted)
print(f"First 16 bytes hex: {csb_decrypted2[:16].hex()}")
print(f"First 16 bytes: {list(csb_decrypted2[:16])}")
print(f"First 3 bytes as string: {csb_decrypted2[:3]}")
if csb_decrypted2[:3] == b'CSB':
    print("SUCCESS! Matches Cocos2d-x CSB header!")
else:
    print("No CSB header match")
print()

print("=" * 80)
print("SUMMARY")
print("=" * 80)
print()
print("XOR KEY (hex):")
print(XOR_KEY.hex())
print()
print("Decryption Methods:")
print("  XML files:  Skip first 8 bytes, XOR rest with 30-byte repeating key")
print("  CSB files:  Method 1 = XOR entire file from byte 0")
print("              Method 2 = Skip first 8 bytes, XOR rest")
print()
print("Functions available:")
print("  - decrypt_xml(encrypted_data) -> decrypted_data")
print("  - decrypt_csb_method1(encrypted_data) -> decrypted_data")
print("  - decrypt_csb_method2(encrypted_data) -> decrypted_data")
print("  - encrypt_xml(plaintext_data) -> encrypted_data")
