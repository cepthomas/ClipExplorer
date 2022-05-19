# ClipExplorer TODOX update

A windows tool for playing audio and midi file clips and Yamaha style files.
This is primarily intended to be used for auditioning parts for use in compositions created in a real DAW.
To that end, and because the windows multimedia timer has inadequate accuracy for midi notes, resolution is 
limited to 32nd notes.
Likewise, minimal attention has been paid to aesthetics over functionality. This explains the poor color choices.
Audio and midi play devices are limited to the ones available on your box. (Hint- VirtualMidiSynth).

Requires VS2019 and .NET5.

Uses:
- [NBagOfTricks](https://github.com/cepthomas/NBagOfTricks/blob/main/README.md)
- [NBagOfUis](https://github.com/cepthomas/NBagOfUis/blob/main/README.md)
- [MidiLib](https://github.com/cepthomas/MidiLib/blob/main/README.md).


# Usage

- The simple UI shows a tree directory navigator on the left and standard audio transport family of controls on the right.
  Depending on file choice, audio or midi specific controls will be shown.
- If midi file type is `1`, all tracks are combined. Because.
- Files can have tags applied for the purpose of filtering.
- Click on the settings icon to edit your options.
- Some midi files with single instruments are sloppy with channel numbers so there are a couple of options for simple remapping.
- In the log view: C for clear, W for word wrap toggle.

# Usage2
- Opens style files and plays the individual sections.
- Export style files as their component parts.
- Export current selection(s) and channel(s) to a new midi file. Useful for snipping style patterns.
- Click on the settings icon to edit your options.
- Some midi files with single instruments are sloppy with channel numbers so there are a couple of options for simple remapping.
- In the log view: C for clear, W for word wrap toggle.

# Notes2
- Since midi files and NAudio use 1-based channel numbers, so does this application, except when used as an array index.
- Because the windows multimedia timer has inadequate accuracy for midi notes, resolution is limited to 32nd notes.
- If midi file type is `1`, all tracks are combined. Because.
- Tons of styles and info at https://psrtutorial.com/.

# Third Party

This application uses these FOSS components:
- NAudio DLL including modified controls and midi file utilities: [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
- Main icon: [Charlotte Schmidt](http://pattedemouche.free.fr/) (Copyright © 2009 of Charlotte Schmidt).
- Button icons: [Glyphicons Free](http://glyphicons.com/) (CC BY 3.0).
