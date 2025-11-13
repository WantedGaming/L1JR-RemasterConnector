# L1R Project Documentation

This folder contains organized documentation and analysis files for the L1R (Lineage Remastered) private server project.

## Documentation Structure

### 📁 /project - Main Project Documentation
- **README.md** - Main project overview and roadmap
- **CLAUDE.md** - Claude Code AI assistant instructions
- **ENCRYPTION_CRACKED.md** - Encryption cracking documentation and findings

### 📁 /database - L1RDatabase (Laravel Web App)
- **README.md** - Database web application documentation
- **CHANGELOG.md** - Database application changelog
- **implementation.md** - Implementation details and setup guide
- **ICONS_TODO.md** - Icon implementation tasks

### 📁 /pak-editor - L1R PAK Editor (.NET Tools)
- **README.md** - PAK Editor overview
- **ARCHITECTURE.md** - Software architecture details
- **CHANGELOG.md** - Editor changelog
- **QUICK-START.md** - Quick start guide
- **README_EDITORS.md** - Editor-specific documentation
- **MEMORY-EXTRACTION-BREAKTHROUGH-README.md** - Memory extraction techniques
- **NUGET-PACKAGES.md** - NuGet package dependencies

### 📁 /pak-tools - PAK File Tools
- **README.md** - PAK tools documentation
- **MEMORY-EXTRACTION-BREAKTHROUGH-README.md** - Memory extraction breakthrough

### 📁 /launcher - L1R Custom Launcher
- **README.md** - Launcher documentation
- **CHANGELOG.md** - Launcher changelog
- **PROJECT-STATUS.md** - Current project status

### 📁 /analysis - Analysis Reports and Results
- **xor_analysis_output.txt** - XOR encryption analysis results
- **pak_file_list.txt** - Complete PAK file listing
- **pak_analysisfile_metadata.txt** - PAK file metadata
- **pak_analysis/** - Detailed PAK analysis
  - ANALYSIS_REPORT.md - Comprehensive analysis report
  - analysis_results.txt - Raw analysis results
  - Python scripts for key analysis and verification
  - Key extraction text files (key_*.txt)

### 📁 /tools - Development Tools and Guides
- **encryption/** - Encryption-related Python scripts
  - analyze_encryption.py
  - crack_xor_key.py
  - decrypt_files.py
  - recalculate_key.py
  - test_actual_files.py
- **guides/** - Development guides
  - memory_intercept_guide.md
  - ui_editing_guide.md
  - csb_text_extractor.py
  - find_xor_key_in_exe.py

## Quick Links

### Getting Started
1. [Project Overview](project/README.md)
2. [Database Setup](database/README.md)
3. [PAK Editor Quick Start](pak-editor/QUICK-START.md)
4. [Launcher Status](launcher/PROJECT-STATUS.md)

### Technical Documentation
- [PAK Editor Architecture](pak-editor/ARCHITECTURE.md)
- [Database Implementation](database/implementation.md)
- [Encryption Analysis](project/ENCRYPTION_CRACKED.md)
- [Memory Extraction Techniques](pak-editor/MEMORY-EXTRACTION-BREAKTHROUGH-README.md)

### Analysis & Reports
- [PAK Analysis Report](analysis/pak_analysis/ANALYSIS_REPORT.md)
- [XOR Analysis Results](analysis/xor_analysis_output.txt)

## Project Components

The L1R project consists of:

1. **L1J-WantedServer** - Java-based game server (located in root, excluded from docs)
2. **LineageWarriorClient** - Game client with assets (located in root, excluded from docs)
3. **L1RDatabase** - Laravel web application for server management
4. **L1R-PAK-Editor** - .NET tools for editing game assets
5. **L1R-PAK-Tools** - Python tools for PAK file manipulation
6. **L1R-CustomLauncher** - Custom game launcher
7. **L1MapEditor** - Map editing tools

## Notes

- All files in this folder are **copies** of the original files for organizational purposes
- Original files remain in their respective project directories
- This structure is designed for easy navigation and reference
- The L1J-WantedServer and LineageWarriorClient folders are excluded as they contain the main codebase

## Last Updated

Generated: 2025-11-09
