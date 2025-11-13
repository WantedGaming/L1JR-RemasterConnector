import hashlib
import struct

# Collected data from analysis
files = [
    {
        "index": 12,
        "filename": "2k_ChatUI.xml",
        "offset": 0x27D,
        "size": 4803,
        "key": [0xB6, 0x18, 0xC5, 0x65, 0xB0, 0xF3, 0x2F, 0x8F, 0x8A, 0xF0, 0x1B, 0xA4, 0xEB, 0x91, 0xC0, 0xFF,
               0x9F, 0x96, 0x90, 0x33, 0x19, 0xBF, 0xE2, 0x13, 0xB6, 0xFA, 0xC5, 0xE5, 0x69, 0x6F, 0x9C, 0x16,
               0xFE, 0x7B, 0x9F, 0x83, 0xCC, 0x5D]
    },
    {
        "index": 13,
        "filename": "2k_MainButtonUI.xml",
        "offset": 0x1540,
        "size": 4864,
        "key": [0x59, 0xCA, 0x5F, 0xE0, 0xFA, 0x15, 0x6A, 0x58, 0xF5, 0x52, 0xE6, 0x48, 0xF9, 0x16, 0x6A, 0x78,
               0xDE, 0x0D, 0xF0, 0x16, 0x24, 0x2E, 0xBF, 0xC1, 0x8B, 0xA1, 0x3A, 0xF5, 0x35, 0x2C, 0x73, 0xB1,
               0x4B, 0x8F, 0xFB, 0xF4, 0x4A, 0x40]
    },
    {
        "index": 14,
        "filename": "2k_MainCharInfoUI.xml",
        "offset": 0x2840,
        "size": 297,
        "key": [0xD9, 0x2A, 0xD2, 0xA7, 0xFC, 0xA7, 0x19, 0x54, 0xA6, 0xA0, 0xC3, 0x53, 0x15, 0x5C, 0x9F, 0x55,
               0xB9, 0x40, 0x15, 0xCB, 0x40, 0x23, 0x70, 0x9B, 0x70, 0x1B, 0x0C, 0x70, 0xD7, 0x32, 0xBD, 0x64,
               0xF6, 0x4B, 0x91, 0x90, 0x82, 0x5A]
    },
    {
        "index": 32,
        "filename": "AdenTelUI.xml",
        "offset": 0x10AFF,
        "size": 26096,
        "key": [0x2B, 0xC0, 0x7D, 0x66, 0xBB, 0x89, 0xD1, 0x22, 0x23, 0xE4, 0x78, 0x8B, 0xFB, 0x92, 0x40, 0x57,
               0x04, 0x97, 0x12, 0xC0, 0xA9, 0x12, 0xA2, 0x65, 0x1D, 0x95, 0xA2, 0xC7, 0x38, 0x94, 0x48, 0x2A,
               0x7F, 0xD1, 0x7D, 0x6A, 0x1C, 0x0C]
    },
    {
        "index": 80,
        "filename": "bookmarkmemui.xml",
        "offset": 0x1E32D7F,
        "size": 18296,
        "key": [0x48, 0xEB, 0x4D, 0x69, 0xD8, 0x14, 0x69, 0x0E, 0xD9, 0x90, 0x8E, 0xE1, 0xE9, 0xEF, 0x4D, 0x62,
               0x0B, 0xC8, 0xA1, 0x9D, 0x9E, 0x19, 0xD8, 0x1F, 0x89, 0x45, 0x4E, 0xA3, 0x56, 0x6A, 0x32, 0x25,
               0x3D, 0x3C, 0x56, 0xF5, 0x1F, 0x11]
    },
    {
        "index": 111,
        "filename": "chatcenterui.xml",
        "offset": 0x1E7AB88,
        "size": 4449,
        "key": [0x3E, 0x00, 0x9A, 0xFA, 0x81, 0xDB, 0xBD, 0xBE, 0x67, 0x41, 0xC8, 0xFB, 0x8D, 0x60, 0x37, 0xD4,
               0xE4, 0xC2, 0xB6, 0x02, 0x90, 0x73, 0x20, 0x7C, 0xD1, 0x98, 0xE2, 0xB8, 0xDC, 0x48, 0xB8, 0x4C,
               0x45, 0x1E, 0xFF, 0xA7, 0x4D, 0x5C]
    },
    {
        "index": 224,
        "filename": "CreateUI-c.xml",
        "offset": 0x1ECD82F,
        "size": 19923,
        "key": [0x52, 0x8B, 0x05, 0x65, 0xE2, 0x02, 0xE6, 0x5D, 0x10, 0x61, 0xF5, 0x5C, 0xD6, 0x35, 0xD4, 0x79,
               0x97, 0x6E, 0xD1, 0x52, 0x50, 0xE0, 0xC4, 0xC6, 0x4F, 0xA6, 0x89, 0x04, 0xD6, 0xCF, 0x92, 0x10,
               0x20, 0x05, 0x91, 0x5A, 0x80, 0x4D]
    },
    {
        "index": 293,
        "filename": "equipcheckui.xml",
        "offset": 0x1F08C41,
        "size": 30963,
        "key": [0x16, 0xAE, 0xE6, 0x88, 0x35, 0x81, 0x3F, 0x9E, 0x97, 0x84, 0x28, 0xAF, 0x15, 0xFB, 0x7E, 0x19,
               0x05, 0xCD, 0x46, 0x5C, 0x78, 0x16, 0x5A, 0x74, 0xB6, 0xFB, 0x4E, 0xF4, 0xB3, 0xD3, 0x57, 0xA6,
               0x74, 0xFE, 0x2F, 0x0A, 0x62, 0x0E]
    },
    {
        "index": 329,
        "filename": "friendui.xml",
        "offset": 0x22401C8,
        "size": 36858,
        "key": [0x0C, 0x1D, 0x7C, 0xD9, 0x07, 0xE0, 0xA5, 0x33, 0xDD, 0xE6, 0xDE, 0xC0, 0x67, 0xB4, 0xE9, 0x54,
               0x99, 0x96, 0x78, 0xA3, 0x25, 0xA2, 0x8E, 0x94, 0x60, 0x4C, 0x5E, 0x5A, 0xAF, 0xA1, 0x3D, 0x0F,
               0x59, 0x4B, 0x66, 0x4D, 0xF1, 0x22]
    },
    {
        "index": 474,
        "filename": "LinHelperUI.xml",
        "offset": 0x22F75D2,
        "size": 982,
        "key": [0xA1, 0x85, 0x7B, 0x4C, 0x75, 0xC3, 0xBF, 0xE7, 0xDF, 0x5C, 0x51, 0xAD, 0x5A, 0xAE, 0x61, 0xE8,
               0xC6, 0x45, 0x7A, 0xAD, 0x7B, 0xE9, 0xE7, 0xF7, 0x2D, 0xD9, 0xF9, 0x9B, 0x2B, 0xA8, 0xC2, 0x23,
               0x6F, 0x94, 0xE8, 0x19, 0x66, 0xE4]
    },
    {
        "index": 522,
        "filename": "MainMenuUI.xml",
        "offset": 0x2345608,
        "size": 5826,
        "key": [0x09, 0xCC, 0x10, 0xD0, 0x98, 0xD1, 0xB4, 0x4F, 0x90, 0xC2, 0x3C, 0xE4, 0xF0, 0x03, 0x50, 0x47,
               0x2C, 0xEB, 0x86, 0x01, 0x0C, 0x80, 0x3E, 0x4B, 0xB6, 0xF8, 0x49, 0x75, 0x32, 0x30, 0x08, 0x11,
               0xD5, 0x22, 0x5C, 0x8C, 0x92, 0xF3]
    }
]

def test_filename_hash(file_data):
    """Test if key is derived from filename hash"""
    filename = file_data["filename"]
    key = bytes(file_data["key"])

    # Test various hash functions
    hashes = {
        "MD5": hashlib.md5(filename.encode()).digest(),
        "SHA1": hashlib.sha1(filename.encode()).digest(),
        "SHA256": hashlib.sha256(filename.encode()).digest(),
    }

    # Test CRC32
    import zlib
    crc = zlib.crc32(filename.encode()) & 0xFFFFFFFF
    crc_bytes = struct.pack('<I', crc)

    results = {}
    for name, hash_val in hashes.items():
        match_count = sum(1 for i in range(min(len(key), len(hash_val))) if key[i] == hash_val[i])
        results[name] = match_count

    results["CRC32"] = sum(1 for i in range(min(len(key), 4)) if key[i] == crc_bytes[i])

    return results

def test_filename_variations(file_data):
    """Test variations of filename encoding"""
    filename = file_data["filename"]
    key = bytes(file_data["key"])

    tests = {
        "lowercase": filename.lower().encode(),
        "uppercase": filename.upper().encode(),
        "no_extension": filename.rsplit('.', 1)[0].encode(),
        "extension_only": filename.split('.')[-1].encode(),
    }

    results = {}
    for name, test_bytes in tests.items():
        md5 = hashlib.md5(test_bytes).digest()
        match_count = sum(1 for i in range(min(len(key), len(md5))) if key[i] == md5[i])
        results[f"MD5_{name}"] = match_count

    return results

def test_offset_correlation(file_data):
    """Test if key is derived from file offset"""
    offset = file_data["offset"]
    key = bytes(file_data["key"])

    # Test offset as seed for various encodings
    tests = {}

    # Direct offset bytes (little endian)
    offset_le = struct.pack('<I', offset)
    tests["offset_LE_4bytes"] = sum(1 for i in range(min(len(key), 4)) if key[i] == offset_le[i])

    # Offset as big endian
    offset_be = struct.pack('>I', offset)
    tests["offset_BE_4bytes"] = sum(1 for i in range(min(len(key), 4)) if key[i] == offset_be[i])

    # MD5 of offset
    offset_md5 = hashlib.md5(offset_le).digest()
    tests["MD5_offset"] = sum(1 for i in range(min(len(key), len(offset_md5))) if key[i] == offset_md5[i])

    return tests

def test_index_correlation(file_data):
    """Test if key is derived from file index"""
    index = file_data["index"]
    key = bytes(file_data["key"])

    tests = {}

    # Direct index bytes
    index_le = struct.pack('<I', index)
    tests["index_LE_4bytes"] = sum(1 for i in range(min(len(key), 4)) if key[i] == index_le[i])

    # MD5 of index
    index_md5 = hashlib.md5(index_le).digest()
    tests["MD5_index"] = sum(1 for i in range(min(len(key), len(index_md5))) if key[i] == index_md5[i])

    return tests

def test_size_correlation(file_data):
    """Test if key is derived from file size"""
    size = file_data["size"]
    key = bytes(file_data["key"])

    tests = {}

    # Direct size bytes
    size_le = struct.pack('<I', size)
    tests["size_LE_4bytes"] = sum(1 for i in range(min(len(key), 4)) if key[i] == size_le[i])

    # MD5 of size
    size_md5 = hashlib.md5(size_le).digest()
    tests["MD5_size"] = sum(1 for i in range(min(len(key), len(size_md5))) if key[i] == size_md5[i])

    return tests

def test_combined_metadata(file_data):
    """Test if key is derived from combined metadata"""
    filename = file_data["filename"]
    offset = file_data["offset"]
    index = file_data["index"]
    size = file_data["size"]
    key = bytes(file_data["key"])

    tests = {}

    # Filename + offset
    combo1 = filename.encode() + struct.pack('<I', offset)
    md5_1 = hashlib.md5(combo1).digest()
    tests["MD5_filename+offset"] = sum(1 for i in range(min(len(key), len(md5_1))) if key[i] == md5_1[i])

    # Filename + index
    combo2 = filename.encode() + struct.pack('<I', index)
    md5_2 = hashlib.md5(combo2).digest()
    tests["MD5_filename+index"] = sum(1 for i in range(min(len(key), len(md5_2))) if key[i] == md5_2[i])

    # Filename + size
    combo3 = filename.encode() + struct.pack('<I', size)
    md5_3 = hashlib.md5(combo3).digest()
    tests["MD5_filename+size"] = sum(1 for i in range(min(len(key), len(md5_3))) if key[i] == md5_3[i])

    # All metadata combined
    combo4 = filename.encode() + struct.pack('<III', offset, index, size)
    md5_4 = hashlib.md5(combo4).digest()
    tests["MD5_all_metadata"] = sum(1 for i in range(min(len(key), len(md5_4))) if key[i] == md5_4[i])

    return tests

# Run all tests
print("=" * 80)
print("ENCRYPTION KEY PATTERN ANALYSIS")
print("=" * 80)
print()

# Test each file
all_results = {}

for file_data in files:
    print(f"\n{'=' * 80}")
    print(f"File: {file_data['filename']}")
    print(f"Index: {file_data['index']}, Offset: 0x{file_data['offset']:X}, Size: {file_data['size']}")
    print(f"Key (first 16 bytes): {' '.join(f'{b:02X}' for b in file_data['key'][:16])}")
    print("-" * 80)

    # Collect all test results
    results = {}
    results.update(test_filename_hash(file_data))
    results.update(test_filename_variations(file_data))
    results.update(test_offset_correlation(file_data))
    results.update(test_index_correlation(file_data))
    results.update(test_size_correlation(file_data))
    results.update(test_combined_metadata(file_data))

    # Show best matches
    sorted_results = sorted(results.items(), key=lambda x: x[1], reverse=True)
    print("\nTop 10 best matches:")
    for test_name, match_count in sorted_results[:10]:
        if match_count > 0:
            print(f"  {test_name:30s}: {match_count:2d}/38 bytes matched ({match_count*100/38:.1f}%)")

    all_results[file_data['filename']] = results

# Aggregate analysis
print(f"\n\n{'=' * 80}")
print("AGGREGATE ANALYSIS - Finding consistent patterns")
print("=" * 80)

# Find tests that consistently produce high matches
test_names = set()
for results in all_results.values():
    test_names.update(results.keys())

test_scores = {}
for test_name in test_names:
    scores = [results.get(test_name, 0) for results in all_results.values()]
    avg_score = sum(scores) / len(scores)
    max_score = max(scores)
    min_score = min(scores)
    test_scores[test_name] = {
        'avg': avg_score,
        'max': max_score,
        'min': min_score,
        'scores': scores
    }

# Sort by average score
sorted_tests = sorted(test_scores.items(), key=lambda x: x[1]['avg'], reverse=True)

print("\nTests sorted by average match rate across all files:")
for test_name, stats in sorted_tests[:15]:
    print(f"  {test_name:30s}: avg={stats['avg']:5.2f}, max={stats['max']:2d}, min={stats['min']:2d}")

# Look for patterns in the keys themselves
print(f"\n\n{'=' * 80}")
print("KEY PATTERN ANALYSIS")
print("=" * 80)

print("\nKey entropy analysis:")
for file_data in files:
    key = file_data['key']
    unique_bytes = len(set(key))
    print(f"  {file_data['filename']:25s}: {unique_bytes}/38 unique bytes ({unique_bytes*100/38:.1f}%)")

print("\nKey length analysis:")
for file_data in files:
    key = file_data['key']
    # Find actual key length (before trailing zeros)
    actual_length = len(key)
    for i in range(len(key) - 1, -1, -1):
        if key[i] != 0:
            actual_length = i + 1
            break
    print(f"  {file_data['filename']:25s}: {actual_length} bytes (38 bytes total)")

# Statistical analysis
print(f"\n\n{'=' * 80}")
print("STATISTICAL CORRELATION ANALYSIS")
print("=" * 80)

import statistics

# Check if any metadata field correlates with key bytes
print("\nChecking correlation between metadata and key bytes...")

for byte_pos in [0, 1, 2, 3]:  # Check first 4 bytes of keys
    print(f"\nByte position {byte_pos}:")

    key_bytes = [f['key'][byte_pos] for f in files]

    # Correlate with index
    indices = [f['index'] for f in files]
    print(f"  Index range: {min(indices)} - {max(indices)}")
    print(f"  Key byte range: {min(key_bytes)} - {max(key_bytes)}")

    # Correlate with offset
    offsets = [f['offset'] for f in files]
    print(f"  Offset range: 0x{min(offsets):X} - 0x{max(offsets):X}")

    # Correlate with size
    sizes = [f['size'] for f in files]
    print(f"  Size range: {min(sizes)} - {max(sizes)}")
