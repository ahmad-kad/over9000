# Assets Copyright and Licensing Guide

## ⚠️ CRITICAL NOTICE

**This project contains copyrighted materials that cannot be distributed commercially or publicly.**

## Audio Assets Status

### ❌ Copyrighted Dragon Ball Z Content (DO NOT DISTRIBUTE)

The following audio files contain copyrighted Dragon Ball Z material owned by Toei Animation, Funimation, and Bandai Namco:

#### JAYUZUMI.COM Library Files (All Copyrighted)
- `9000 - AUDIO FROM JAYUZUMI.COM.mp3`
- `ARGHHHH 1 - AUDIO FROM JAYUZUMI.COM.mp3`
- `ARGHHHH 2 - AUDIO FROM JAYUZUMI.COM.mp3`
- `HEY COME ON - AUDIO FROM JAYUZUMI.COM.mp3`
- `HIS POWER LEVEL IS 1200 - AUDIO FROM JAYUZUMI.COM.mp3`
- `I GET WHAT YOU'RE GOING FOR - AUDIO FROM JAYUZUMI.COM.mp3`
- `NOOOOOO - AUDIO FROM JAYUZUMI.COM.mp3`
- `SCOUTER - AUDIO FROM JAYUZUMI.COM.mp3`
- `WHAT DID YOU SAY HIS POWER LEVEL WAS - AUDIO FROM JAYUZUMI.COM.mp3`

#### Dragon Ball Z Theme/SFX Files (All Copyrighted)
- `all-dragon-ball-eyecatch-intermission.mp3`
- `dragon-ball-z-body-falling-down-sound.mp3`
- `dragonball_battle.mp3`
- `kamehameha-wave-sound-effect.mp3`
- `surprised-dbz-sound-effect.mp3`
- `tfs-krillin-scream-1.mp3`
- `dbz-next-time-on-dragon-ball-z.mp3`
- `title-card-i.mp3`
- `perfect-cell-theme-made-with-Voicemod.mp3`

### ✅ Potentially Safe Assets

#### Original/Voicemod Creations
- `db-scouter-made-with-Voicemod.mp3` - *Verify Voicemod licensing terms*
- `over9000.swf.mp3` - *Check source licensing*

#### Generic Sound Effects
- `glass_shatter.mp3` - *Likely from free sound libraries, verify source*

## Legal Usage Guidelines

### ✅ ALLOWED (Personal Use Only)
- Private development and testing
- Educational/learning projects
- Local device installation
- Non-commercial demonstrations
- Hackathon presentations (if not publicly distributed)

### ❌ NOT ALLOWED
- Commercial distribution or sales
- Upload to app stores (Apple App Store, Google Play, etc.)
- Public hosting or sharing
- Monetization of any kind
- Distribution to third parties

## Replacement Strategy for Production

### Step 1: Identify Required Audio Types
Based on `SFX/sfx_use_cases.txt`, you need:

1. **Voice Lines** (9 files) - Character reactions, power readings
2. **Combat Effects** (4 files) - Battle sounds, energy attacks
3. **Music/Themes** (3 files) - Background music, transitions
4. **Environmental Effects** (1 file) - Glass shatter

### Step 2: Source Replacement Assets

#### Free/Legal Audio Sources:
- **Freesound.org**: CC0 licensed sounds
- **ZapSplat.com**: Some free sounds with attribution
- **YouTube Audio Library**: Free with attribution required
- **OpenGameArt.org**: Game-ready audio assets
- **Free Music Archive**: Creative Commons music

#### Original Creation:
- **Record your own voice** for custom voice lines
- **Use synthesis tools** (ElevenLabs, Murf.ai) with proper licensing
- **Create sound effects** using free tools like Audacity + free samples

### Step 3: Implementation Plan

```
Phase 1: Audit & Document (Current)
├── ✅ Audio files inventoried
├── ✅ Copyright status documented
└── ✅ Usage guidelines established

Phase 2: Replace Copyrighted Assets
├── Voice Lines → Custom recordings or licensed alternatives
├── Combat SFX → Free sound libraries
├── Music → Original compositions or CC-licensed
└── Environmental → Verified free sources

Phase 3: Legal Compliance
├── Remove all DBZ copyrighted content
├── Add proper attributions for licensed assets
├── Update documentation
└── Test with clean asset set
```

## Recommended Free Audio Alternatives

### Voice Lines Replacement:
- **ElevenLabs** or **Murf.ai**: Generate custom voice lines
- **Respeecher**: Voice synthesis with commercial licensing
- **Record original**: Use friends/family for custom voice work

### Sound Effects:
- **Freesound.org**: Search "sci-fi beep", "electronic", "explosion"
- **ZapSplat**: Free with watermark removal on purchase
- **YouTube Audio Library**: "Technology", "Sci-fi", "Interface"

### Music:
- **YouTube Audio Library**: "Electronic", "Sci-fi", "Adventure"
- **Free Music Archive**: Creative Commons licensed tracks
- **Bensound.com**: Free for personal use

## Technical Implementation Notes

When replacing assets:
1. Maintain consistent audio specifications (44.1kHz, mono, 128kbps MP3)
2. Update `SFX/sfx_use_cases.txt` with new file mappings
3. Test audio timing and volume levels in Unity
4. Update any hardcoded audio references in C# scripts

## Risk Mitigation

### For Personal Projects:
- Keep all copyrighted assets local only
- Never commit copyrighted files to public repositories
- Use `.gitignore` to exclude copyrighted content
- Document all asset sources and licensing

### For Public Distribution:
- Complete asset replacement before any public release
- Obtain proper licensing for all commercial assets
- Include attribution credits for licensed content
- Consult legal counsel for commercial applications

## Contact Information

If you need help finding replacement assets or have questions about licensing:
- Review Freesound.org and YouTube Audio Library terms
- Consult with legal professionals for commercial use
- Consider working with audio professionals for original content

---

**Last Updated**: November 15, 2025
**Status**: 🚨 REQUIRES IMMEDIATE ATTENTION - Copyrighted content present
