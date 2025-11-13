#!/usr/bin/env python3
"""
Test decryption on actual extracted files
"""
import os
import sys

# XOR KEY (30 bytes)
XOR_KEY = bytes([
    0xbc, 0x99, 0xd6, 0xba, 0xc9, 0x7d, 0x88, 0xa1,
    0x7e, 0x16, 0xc1, 0x96, 0xec, 0xd5, 0x39, 0xe8,
    0x07, 0xb5, 0x4d, 0xd6, 0x78, 0xfc, 0xfe, 0x31,
    0x8d, 0x4b, 0xd2, 0x9f, 0x44, 0xef
])

def decrypt_xml(encrypted_data):
    """Decrypt XML: First 8 bytes plain, rest XOR'd with key"""
    if len(encrypted_data) < 8:
        return encrypted_data

    decrypted = bytearray(encrypted_data[:8])
    for i in range(8, len(encrypted_data)):
        key_index = (i - 8) % len(XOR_KEY)
        decrypted.append(encrypted_data[i] ^ XOR_KEY[key_index])

    return bytes(decrypted)

def encrypt_xml(plaintext_data):
    """Encrypt XML: First 8 bytes plain, rest XOR'd with key"""
    if len(plaintext_data) < 8:
        return plaintext_data

    encrypted = bytearray(plaintext_data[:8])
    for i in range(8, len(plaintext_data)):
        key_index = (i - 8) % len(XOR_KEY)
        encrypted.append(plaintext_data[i] ^ XOR_KEY[key_index])

    return bytes(encrypted)

# Find and test actual XML files
xml_test_dir = r"D:\L1R Project\LineageWarriorClient\extracted_data\ui\2k_ChatUI"

print("=" * 80)
print("TESTING ACTUAL EXTRACTED FILES")
print("=" * 80)
print()

if os.path.exists(xml_test_dir):
    xml_files = [f for f in os.listdir(xml_test_dir) if f.endswith('.xml')]
    print(f"Found {len(xml_files)} XML files in {xml_test_dir}")
    print()

    for xml_file in xml_files[:3]:  # Test first 3 files
        file_path = os.path.join(xml_test_dir, xml_file)
        print(f"Testing: {xml_file}")

        try:
            with open(file_path, 'rb') as f:
                encrypted = f.read()

            print(f"  Size: {len(encrypted)} bytes")
            print(f"  First 16 bytes (hex): {encrypted[:16].hex()}")
            print(f"  First 8 bytes (text): {encrypted[:8]}")

            # Try decryption
            decrypted = decrypt_xml(encrypted)

            # Check if it's valid XML
            if decrypted.startswith(b'<?xml'):
                print(f"  DECRYPTION SUCCESS! Valid XML header detected")
                # Try to parse first 200 bytes
                try:
                    text = decrypted[:200].decode('utf-8', errors='replace')
                    print(f"  Preview: {text[:100]}...")
                except:
                    print(f"  Preview (hex): {decrypted[:100].hex()}")
            else:
                print(f"  First 16 decrypted bytes: {decrypted[:16]}")
                print(f"  Not valid XML header")

            print()
        except Exception as e:
            print(f"  Error: {e}")
            print()
else:
    print(f"Directory not found: {xml_test_dir}")
    print()

# Try to find any actual .xml file to test
print("=" * 80)
print("SEARCHING FOR XML FILES")
print("=" * 80)
print()

search_base = r"D:\L1R Project\LineageWarriorClient\extracted_data"
if os.path.exists(search_base):
    for root, dirs, files in os.walk(search_base):
        xml_files = [f for f in files if f.endswith('.xml')]
        if xml_files:
            print(f"Found {len(xml_files)} XML files in: {root}")
            # Test first file
            test_file = os.path.join(root, xml_files[0])
            print(f"Testing: {xml_files[0]}")

            try:
                with open(test_file, 'rb') as f:
                    encrypted = f.read()

                print(f"  Size: {len(encrypted)} bytes")
                print(f"  First 32 bytes (hex): {encrypted[:32].hex()}")

                decrypted = decrypt_xml(encrypted)
                print(f"  First 32 decrypted (hex): {decrypted[:32].hex()}")

                if decrypted.startswith(b'<?xml'):
                    print(f"  SUCCESS! Valid XML!")
                    print(f"  Content preview:")
                    try:
                        text = decrypted[:300].decode('utf-8', errors='replace')
                        print(f"  {text}")
                    except:
                        pass
                print()
                break  # Only test one
            except Exception as e:
                print(f"  Error: {e}")
                print()

print()
print("=" * 80)
print("DECRYPTION FUNCTIONS READY")
print("=" * 80)
print()
print("Use these functions in your code:")
print()
print("  decrypt_xml(encrypted_bytes) -> decrypted_bytes")
print("  encrypt_xml(plaintext_bytes) -> encrypted_bytes")
print()
print("XOR Key (30 bytes):")
print(f"  {XOR_KEY.hex()}")
