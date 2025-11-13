#!/usr/bin/env python3
"""
Find XOR encryption key in Lineage client executable.

Searches for the 38-byte key we already know, then extracts surrounding bytes
to find the full key.
"""

import sys
from pathlib import Path

# The 38-byte key we already derived
KNOWN_KEY_SEGMENT = bytes([
    0x74, 0x0C, 0x79, 0x66, 0x62, 0x03, 0x71, 0x66, 0x71, 0x72, 0x60, 0x62, 0x6C,
    0x03, 0x19, 0x16, 0x14, 0x17, 0x03, 0x66, 0x6C, 0x64, 0x62, 0x65, 0x60, 0x6C,
    0x68, 0x03, 0x19, 0x56, 0x53, 0x45, 0x16, 0x3C, 0x03, 0x14, 0x17, 0x19
])

def find_key_in_file(file_path, context_bytes=256):
    """
    Search for known key segment in executable and extract surrounding context.

    Args:
        file_path: Path to executable (Lin.bin, LWLauncher.exe, etc.)
        context_bytes: How many bytes before/after to extract
    """
    print(f"\n[*] Searching in: {file_path}")

    try:
        with open(file_path, 'rb') as f:
            data = f.read()
    except Exception as e:
        print(f"[!] Error reading file: {e}")
        return None

    # Search for the known key segment
    offset = data.find(KNOWN_KEY_SEGMENT)

    if offset == -1:
        print(f"[!] Known key segment NOT found in {file_path}")
        return None

    print(f"[+] Found key segment at offset: 0x{offset:08X} ({offset})")

    # Extract surrounding context
    start = max(0, offset - context_bytes)
    end = min(len(data), offset + len(KNOWN_KEY_SEGMENT) + context_bytes)

    context = data[start:end]

    # Calculate relative position of known key in context
    key_start_in_context = offset - start
    key_end_in_context = key_start_in_context + len(KNOWN_KEY_SEGMENT)

    print(f"[+] Extracted {len(context)} bytes of context")
    print(f"[+] Known key is at bytes [{key_start_in_context}:{key_end_in_context}] in context")

    # Try to find key boundaries (look for patterns)
    print("\n[*] Analyzing context for key boundaries...")

    # Strategy 1: Look for null padding before/after
    before_nulls = 0
    for i in range(key_start_in_context - 1, -1, -1):
        if context[i] == 0:
            before_nulls += 1
        else:
            break

    after_nulls = 0
    for i in range(key_end_in_context, len(context)):
        if context[i] == 0:
            after_nulls += 1
        else:
            break

    if before_nulls > 10 or after_nulls > 10:
        print(f"[+] Found null padding: {before_nulls} bytes before, {after_nulls} bytes after")
        print(f"[+] Key likely starts at context[{key_start_in_context}]")

    # Strategy 2: Check for common key lengths (64, 128, 256)
    for key_len in [64, 128, 256, 512]:
        if key_start_in_context >= key_len - len(KNOWN_KEY_SEGMENT):
            # Check if key might start earlier
            potential_start = key_start_in_context - (key_len - len(KNOWN_KEY_SEGMENT))
            if potential_start >= 0:
                potential_key = context[potential_start:potential_start + key_len]
                if len(potential_key) == key_len:
                    print(f"\n[*] Potential {key_len}-byte key found:")
                    print(f"    Offset in file: 0x{start + potential_start:08X}")
                    save_key_candidate(potential_key, key_len, file_path.stem)

    # Save full context for manual inspection
    context_file = Path("tools") / f"{file_path.stem}_key_context.bin"
    context_file.write_bytes(context)
    print(f"\n[+] Saved full context to: {context_file}")
    print(f"    Known key is at bytes 0x{key_start_in_context:X}-0x{key_end_in_context:X}")

    return context

def save_key_candidate(key_bytes, length, source_name):
    """Save potential key to file"""
    output_file = Path("tools") / f"xor_key_{length}bytes_{source_name}.bin"
    output_file.write_bytes(key_bytes)
    print(f"    Saved to: {output_file}")

    # Print hex preview
    print(f"    First 64 bytes (hex):")
    for i in range(0, min(64, len(key_bytes)), 16):
        hex_str = ' '.join(f'{b:02X}' for b in key_bytes[i:i+16])
        print(f"      {i:04X}: {hex_str}")

def search_all_executables(client_dir):
    """Search all executables in client directory"""
    client_path = Path(client_dir)

    exe_files = []
    exe_files.extend(client_path.glob("*.exe"))
    exe_files.extend(client_path.glob("*.dll"))
    exe_files.extend(client_path.glob("*.bin"))
    exe_files.extend(client_path.glob("**/*.exe"))
    exe_files.extend(client_path.glob("**/*.dll"))

    print(f"[*] Found {len(exe_files)} executable files to search")

    results = []
    for exe_file in exe_files:
        if exe_file.stat().st_size > 100 * 1024 * 1024:  # Skip files > 100MB
            print(f"[!] Skipping large file: {exe_file} ({exe_file.stat().st_size / 1024 / 1024:.1f} MB)")
            continue

        context = find_key_in_file(exe_file)
        if context:
            results.append((exe_file, context))

    return results

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python find_xor_key_in_exe.py <path_to_client_directory>")
        print("   or: python find_xor_key_in_exe.py <path_to_specific_exe>")
        sys.exit(1)

    target_path = Path(sys.argv[1])

    if not target_path.exists():
        print(f"[!] Path not found: {target_path}")
        sys.exit(1)

    if target_path.is_dir():
        results = search_all_executables(target_path)
        print(f"\n[*] Search complete. Found key in {len(results)} file(s)")
    else:
        find_key_in_file(target_path)
