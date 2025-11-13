#!/usr/bin/env python3
"""
Analyze the encryption scheme for XML and CSB files
using known plaintext attack
"""

# Known plaintext for XML file
xml_plaintext = b'<?xml version="1.0" encoding="UTF-8"?>'
print(f"XML Plaintext length: {len(xml_plaintext)} bytes")
print(f"XML Plaintext hex: {xml_plaintext.hex()}")
print()

# Encrypted XML data (first 128 bytes from dump) - manually cleaned
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
print(f"XML Encrypted length: {len(xml_encrypted)} bytes")
print(f"XML Encrypted hex: {xml_encrypted.hex()}")
print()

# Extract XOR key
min_len = min(len(xml_plaintext), len(xml_encrypted))
xor_key = bytes([xml_plaintext[i] ^ xml_encrypted[i] for i in range(min_len)])

print("XOR Key Analysis:")
print(f"XOR Key hex: {xor_key.hex()}")
print(f"XOR Key bytes: {list(xor_key)}")
print()

# Check for patterns
print("Byte-by-byte analysis:")
for i in range(min(32, min_len)):
    plain_byte = xml_plaintext[i] if i < len(xml_plaintext) else 0
    enc_byte = xml_encrypted[i] if i < len(xml_encrypted) else 0
    key_byte = xor_key[i] if i < len(xor_key) else 0
    plain_char = chr(plain_byte) if 32 <= plain_byte < 127 else '?'
    enc_char = chr(enc_byte) if 32 <= enc_byte < 127 else '?'
    print(f"  Byte {i:2d}: Plain=0x{plain_byte:02x} ('{plain_char}')  Enc=0x{enc_byte:02x} ('{enc_char}')  Key=0x{key_byte:02x}")

print()
print("=" * 80)
print()

# Analyze CSB file - manually cleaned
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
print(f"CSB Encrypted hex (first 64): {csb_encrypted[:64].hex()}")
print()

# Try to identify CSB format
# Standard Cocos2d-x CSB starts with "CSB" (0x43 0x53 0x42)
# Let's try XOR with common headers
possible_headers = [
    b'CSB',
    b'COC',
    b'\x43\x53\x42',  # CSB in hex
]

print("CSB Header Analysis:")
print(f"Encrypted first 3 bytes: {csb_encrypted[:3].hex()} = {list(csb_encrypted[:3])}")
print()
print("Trying possible headers:")
for header in possible_headers:
    if len(csb_encrypted) >= len(header):
        key = bytes([csb_encrypted[i] ^ header[i] for i in range(len(header))])
        print(f"  Header '{header}' -> XOR key: {key.hex()} = {list(key)}")

print()
print("=" * 80)
print()

# Summary
print("FINDINGS:")
print()
print("1. XML File Encryption:")
print(f"   - First 8 bytes are UNENCRYPTED: {xml_plaintext[:8]}")
print(f"   - XOR encryption starts at byte 8")
print(f"   - XOR Key (first 32 bytes): {xor_key[:32].hex()}")
print()
print("2. CSB File Encryption:")
print(f"   - Does NOT match standard Cocos2d-x CSB format")
print(f"   - First bytes: {csb_encrypted[:16].hex()}")
print(f"   - Likely uses same or similar XOR scheme")
print()
print("3. Next Steps:")
print("   - Identify if XOR key is repeating pattern or position-based")
print("   - Test if CSB uses same key starting at byte 0 or byte 8")
