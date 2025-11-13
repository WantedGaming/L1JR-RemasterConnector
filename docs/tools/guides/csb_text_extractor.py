#!/usr/bin/env python3
"""
Extract readable text strings from Lineage CSB files.
Helps identify Korean text that needs translation without decrypting XML.
"""

import sys
import struct
from pathlib import Path

def extract_strings(csb_path, min_length=3):
    """
    Extract all readable text strings from CSB binary.

    Args:
        csb_path: Path to .csb file
        min_length: Minimum string length to report
    """
    with open(csb_path, 'rb') as f:
        data = f.read()

    results = {
        'utf8_strings': [],
        'ascii_strings': [],
        'length_prefixed': []
    }

    print(f"\n[*] Analyzing: {csb_path}")
    print(f"[*] File size: {len(data)} bytes")

    # Method 1: Find null-terminated ASCII strings
    print("\n[*] Scanning for ASCII strings...")
    current = b''
    offset = 0

    for i, byte in enumerate(data):
        if 32 <= byte <= 126:  # Printable ASCII
            if len(current) == 0:
                offset = i
            current += bytes([byte])
        else:
            if len(current) >= min_length:
                results['ascii_strings'].append({
                    'offset': offset,
                    'text': current.decode('ascii'),
                    'length': len(current)
                })
            current = b''

    # Method 2: Find UTF-8 strings (Korean text)
    print("[*] Scanning for UTF-8 strings (Korean)...")
    i = 0
    while i < len(data) - 3:
        try:
            # Try to decode 2-100 bytes as UTF-8
            for length in range(3, min(100, len(data) - i)):
                try:
                    text = data[i:i+length].decode('utf-8')
                    # Check if it contains Korean characters (U+AC00 to U+D7A3)
                    if any('\uac00' <= c <= '\ud7a3' for c in text):
                        # Found Korean text
                        results['utf8_strings'].append({
                            'offset': i,
                            'text': text,
                            'length': length,
                            'type': 'Korean'
                        })
                        i += length - 1
                        break
                except UnicodeDecodeError:
                    continue
        except:
            pass
        i += 1

    # Method 3: Find length-prefixed strings (FlatBuffers format)
    print("[*] Scanning for length-prefixed strings...")
    i = 0
    while i < len(data) - 8:
        # Read potential length field (4 bytes, little-endian)
        str_len = struct.unpack('<I', data[i:i+4])[0]

        # Sanity check: reasonable string length
        if 3 <= str_len <= 500:
            if i + 4 + str_len <= len(data):
                try:
                    # Try to decode as UTF-8
                    text = data[i+4:i+4+str_len].decode('utf-8')

                    # Check if mostly printable
                    if sum(c.isprintable() or c in '\n\r\t' for c in text) / len(text) > 0.8:
                        string_type = 'Korean' if any('\uac00' <= c <= '\ud7a3' for c in text) else 'ASCII'

                        results['length_prefixed'].append({
                            'offset': i,
                            'length_field': str_len,
                            'text': text,
                            'type': string_type
                        })

                        i += 4 + str_len
                        continue
                except UnicodeDecodeError:
                    pass

        i += 1

    return results

def print_results(results, show_ascii=False):
    """Print extracted strings"""

    # Print length-prefixed strings (most reliable)
    if results['length_prefixed']:
        print(f"\n{'='*80}")
        print(f"LENGTH-PREFIXED STRINGS (Most reliable - FlatBuffers format)")
        print(f"{'='*80}")

        korean_count = sum(1 for s in results['length_prefixed'] if s['type'] == 'Korean')
        ascii_count = len(results['length_prefixed']) - korean_count

        print(f"Found {len(results['length_prefixed'])} strings ({korean_count} Korean, {ascii_count} ASCII)")

        for s in results['length_prefixed']:
            marker = "🇰🇷" if s['type'] == 'Korean' else "  "
            print(f"\n{marker} Offset: 0x{s['offset']:08X} | Length: {s['length_field']} bytes")
            print(f"   Text: {s['text'][:100]}")  # Limit to 100 chars

    # Print UTF-8 strings (Korean)
    if results['utf8_strings']:
        print(f"\n{'='*80}")
        print(f"UTF-8 STRINGS (Korean text)")
        print(f"{'='*80}")
        print(f"Found {len(results['utf8_strings'])} Korean strings")

        for s in results['utf8_strings'][:50]:  # Limit to first 50
            print(f"\n🇰🇷 Offset: 0x{s['offset']:08X} | Length: {s['length']} bytes")
            print(f"   Text: {s['text'][:100]}")

    # Print ASCII strings (optional)
    if show_ascii and results['ascii_strings']:
        print(f"\n{'='*80}")
        print(f"ASCII STRINGS")
        print(f"{'='*80}")
        print(f"Found {len(results['ascii_strings'])} ASCII strings")

        for s in results['ascii_strings'][:30]:  # Limit to first 30
            if len(s['text']) >= 5:  # Only show longer strings
                print(f"   0x{s['offset']:08X}: {s['text'][:80]}")

def save_translation_list(results, output_path):
    """Save Korean strings to translation file"""
    output = Path(output_path)

    with open(output, 'w', encoding='utf-8') as f:
        f.write("# Korean Strings Found in CSB File\n")
        f.write("# Format: Offset | Length | Korean Text | English Translation\n\n")

        # Combine all Korean strings
        korean_strings = []

        for s in results['length_prefixed']:
            if s['type'] == 'Korean':
                korean_strings.append(s)

        for s in results['utf8_strings']:
            korean_strings.append(s)

        # Sort by offset
        korean_strings.sort(key=lambda x: x['offset'])

        # Remove duplicates
        seen = set()
        for s in korean_strings:
            if s['text'] not in seen:
                seen.add(s['text'])
                f.write(f"0x{s['offset']:08X} | {s.get('length_field', s['length'])} | {s['text']} | \n")

    print(f"\n[+] Saved {len(seen)} unique Korean strings to: {output}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python csb_text_extractor.py <file.csb> [--show-ascii]")
        print("\nExample:")
        print("  python csb_text_extractor.py inventory.csb")
        print("  python csb_text_extractor.py inventory.csb --show-ascii")
        sys.exit(1)

    csb_file = Path(sys.argv[1])
    show_ascii = '--show-ascii' in sys.argv

    if not csb_file.exists():
        print(f"[!] File not found: {csb_file}")
        sys.exit(1)

    # Extract strings
    results = extract_strings(csb_file)

    # Print results
    print_results(results, show_ascii=show_ascii)

    # Save translation list
    output_file = csb_file.with_suffix('.translation.txt')
    save_translation_list(results, output_file)

    # Summary
    total_korean = sum(1 for s in results['length_prefixed'] if s['type'] == 'Korean')
    total_korean += len(results['utf8_strings'])

    print(f"\n{'='*80}")
    print(f"SUMMARY")
    print(f"{'='*80}")
    print(f"Total Korean strings found: {total_korean}")
    print(f"Total ASCII strings found: {len(results['ascii_strings'])}")
    print(f"Length-prefixed strings: {len(results['length_prefixed'])}")
    print(f"\nNext steps:")
    print(f"1. Review translation file: {output_file}")
    print(f"2. Add English translations in the rightmost column")
    print(f"3. Use hex editor to replace Korean text at specified offsets")
    print(f"   (Keep same byte length or adjust length field)")
