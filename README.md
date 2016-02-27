# Discord Soundboard

A soundboard bot for Discord using the unofficial [Discord.NET](https://github.com/RogueException/Discord.Net) API. The bot responds to mentions or direct messages and plays sound effects in a specific voice channel.

## License

Apache License 2.0

## Caveats

 This bot was written for a small circle of friends and suffers from a lack of documentation, no warranty of any kind, and is not representative of quality work.

## Dependencies
 - Discord.Net
 - Discord.Net.Audio
 - Discord.Net.Modules
 - Discord.Net.Commands
 - NAudio
 - Opus

## Features

- File Formats
 - RIFF Wave (PCM)
- Plays sound effects with simple commands
- Resamples sound effects during playback so users don't have to ensure files have a specific sample rate, channel count, or bit-depth
- Accepts attachments in messages and downloads them for use as a sound effect

## Configuration

 The default filename for the configuration file is **soundbot.cfg**

| Key             | Description                                                        |
| --------------- | ------------------------------------------------------------------ |
| email           | Bot's account e-maiil address                                      |
| password        | Bot's account password                                             |
| path.effects    | Path for sound effects                                             |
| status          | The 'playing game' status message to display                       |
| voice.channel   | The voice channel for the bot to connect to                        |
| voice.bitrate   | The voice bitrate to use (in bits)                                 |

## Configuration Sample

    email = example@example.com
    password = example
    path.effects = Effects
    status = example
    voice.channel = example
    voice.bitrate = 128

## Commands

  Assuming the bot's handle is **soundbot**:

### list

  Lists all sound effects registered by the bot.

      @soundbot list

### play

  Plays a registered sound effect. This is the default command.

    @soundbot <effect>
    @soundbot boop
