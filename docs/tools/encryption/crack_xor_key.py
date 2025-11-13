#!/usr/bin/env python3
"""
Advanced XOR key analysis for XML and CSB file encryption
"""

# Known plaintext for XML file
xml_plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'

# Encrypted XML data (128 bytes)
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

# Extract XOR key from known plaintext (starting at byte 8)
xor_key_extracted = bytes([xml_plaintext[i] ^ xml_encrypted[i] for i in range(8, len(xml_plaintext))])

print("=" * 80)
print("XOR KEY EXTRACTION")
print("=" * 80)
print()
print(f"Known plaintext: {xml_plaintext}")
print(f"Plaintext length: {len(xml_plaintext)} bytes")
print()
print(f"Encrypted data length: {len(xml_encrypted)} bytes")
print()
print(f"XOR Key (extracted from bytes 8-{len(xml_plaintext)-1}):")
print(f"  Hex: {xor_key_extracted.hex()}")
print(f"  Bytes: {list(xor_key_extracted)}")
print(f"  Length: {len(xor_key_extracted)} bytes (30 bytes)")
print()

# Test for repeating pattern
print("=" * 80)
print("TESTING FOR REPEATING KEY PATTERN")
print("=" * 80)
print()

def test_repeating_key(key_length):
    """Test if XOR key repeats with given length"""
    # Assume key starts at byte 8, take first key_length bytes as pattern
    pattern = xor_key_extracted[:key_length]

    # Generate full key by repeating pattern
    full_key = pattern * ((len(xml_encrypted) - 8) // key_length + 1)
    full_key = full_key[:len(xml_encrypted) - 8]

    # Test decryption
    decrypted = bytearray(xml_encrypted[:8])  # First 8 bytes unencrypted
    for i in range(8, len(xml_encrypted)):
        decrypted.append(xml_encrypted[i] ^ full_key[i - 8])

    # Check if decrypted data looks like XML
    decrypted_str = decrypted.decode('utf-8', errors='replace')

    # Count printable ASCII characters
    printable_count = sum(1 for c in decrypted if 32 <= c < 127)
    printable_ratio = printable_count / len(decrypted)

    return decrypted, printable_ratio, pattern

# Test common key lengths
for key_len in [4, 8, 16, 24, 30, 32, 64]:
    if key_len <= len(xor_key_extracted):
        decrypted, ratio, pattern = test_repeating_key(key_len)
        print(f"Key length {key_len:2d}: Printable ratio = {ratio:.2%}")
        print(f"  Pattern: {pattern.hex()}")
        if ratio > 0.5:
            preview = decrypted[:80].decode('utf-8', errors='replace')
            # Only print ASCII-safe characters
            safe_preview = ''.join(c if ord(c) < 128 else '?' for c in preview[:60])
            print(f"  Preview: {safe_preview}...")
        print()

print("=" * 80)
print("FULL DECRYPTION ATTEMPT WITH 30-BYTE KEY")
print("=" * 80)
print()

# Use the full 30-byte extracted key as repeating pattern
pattern = xor_key_extracted
full_key = pattern * ((len(xml_encrypted) - 8) // len(pattern) + 1)
full_key = full_key[:len(xml_encrypted) - 8]

decrypted = bytearray(xml_encrypted[:8])  # First 8 bytes unencrypted
for i in range(8, len(xml_encrypted)):
    decrypted.append(xml_encrypted[i] ^ full_key[i - 8])

print(f"Decrypted XML ({len(decrypted)} bytes):")
print(decrypted.decode('utf-8', errors='replace'))
print()

print("=" * 80)
print("CSB FILE ANALYSIS")
print("=" * 80)
print()

# CSB encrypted data
csb_encrypted_hex = (
    "252c7bbef0db2a52c9935fa44cdf4838" +
    "6496a8dea53ac5061d21eed378b0fbff" +
    "b25aafc7badc53d56a10dcee5c85ca59" +
    "c5ccff65dd2f7c54b8f5583980def634" +
    "7393b1acd1f0c44637af450c9424f48f" +
    "996e3a00bd2e7ca12f600b6832fff561" +
    "950bc07edd9c1579a48dd1552a85d174" +
    "5938ee1c49871e2b95e21d7c3707a2a4" +
    "c261659821f72c8c75c2b2e21711c700"
)
csb_encrypted = bytes.fromhex(csb_encrypted_hex)

print(f"CSB Encrypted length: {len(csb_encrypted)} bytes")
print()

# Test if CSB uses same XOR key (starting at byte 0)
print("Test 1: CSB with XOR key starting at byte 0")
full_key_csb = pattern * ((len(csb_encrypted)) // len(pattern) + 1)
full_key_csb = full_key_csb[:len(csb_encrypted)]

decrypted_csb = bytearray()
for i in range(len(csb_encrypted)):
    decrypted_csb.append(csb_encrypted[i] ^ full_key_csb[i])

# Check if it looks like Cocos2d-x CSB format
# Standard CSB starts with "CSB" (0x43 0x53 0x42) followed by version info
print(f"  First 32 bytes hex: {decrypted_csb[:32].hex()}")
print(f"  First 3 bytes: {decrypted_csb[:3]} (expected: b'CSB' or similar)")
if decrypted_csb[:3] == b'CSB':
    print("  -> MATCH! This is likely a Cocos2d-x CSB file!")
else:
    print(f"  -> No match. First 3 bytes are: {list(decrypted_csb[:3])}")
print()

# Test if CSB uses same XOR key (starting at byte 8, first 8 unencrypted)
print("Test 2: CSB with XOR key starting at byte 8")
full_key_csb2 = pattern * ((len(csb_encrypted) - 8) // len(pattern) + 1)
full_key_csb2 = full_key_csb2[:len(csb_encrypted) - 8]

decrypted_csb2 = bytearray(csb_encrypted[:8])
for i in range(8, len(csb_encrypted)):
    decrypted_csb2.append(csb_encrypted[i] ^ full_key_csb2[i - 8])

print(f"  First 32 bytes hex: {decrypted_csb2[:32].hex()}")
print(f"  First 3 bytes: {decrypted_csb2[:3]} (expected: b'CSB' or similar)")
if decrypted_csb2[:3] == b'CSB':
    print("  -> MATCH! This is likely a Cocos2d-x CSB file!")
else:
    print(f"  -> No match. First 3 bytes are: {list(decrypted_csb2[:3])}")
print()

print("=" * 80)
print("FINAL RESULTS")
print("=" * 80)
print()
print("XOR KEY FOR DECRYPTION:")
print(f"  Hex: {pattern.hex()}")
print(f"  Bytes: {list(pattern)}")
print(f"  Length: {len(pattern)} bytes")
print()
print("DECRYPTION RULES:")
print("  1. XML files: First 8 bytes are unencrypted, then XOR with repeating 30-byte key")
print("  2. CSB files: Testing shows...")
print()
print("USAGE:")
print("  To decrypt XML: Skip first 8 bytes, XOR rest with repeating key")
print("  To encrypt XML: Leave first 8 bytes plain, XOR rest with repeating key")
