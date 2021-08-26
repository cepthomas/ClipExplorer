# ClipExplorer
A windows tool for playing audio and midi file clips. This is intended to be used for auditioning parts for use in compositions created in a real DAW.
To that end, and because the windows multimedia timer has inadequate accuracy for midi notes, resolution is limited to 32nd notes. Likewise,
minimal attention has been paid to aesthetics over functionality. This explains the poor color choices. Audio and midi play devices are limited to the
ones available on your box. (Hint- VirtualMidiSynth).

# Usage
- The simple UI shows a tree directory navigator on the left and standard audio transport family of controls on the right.
  Depending on file choice, audio or midi specific controls will be shown.
- If midi file type is one, all tracks are combined. Because.
- Opens Yamaha style (.sty) files and plays the individual sections.
- Export current selection to a new midi file. Useful for snipping style patterns.
- Files can have tags applied for the purpose of filtering.
- Click on the settings icon to edit your options.
- Some midi files with single instruments are sloppy with channel numbers so there are a couple of options for simple remapping.
- In the log view: C for clear, W for word wrap toggle.


# Third Party
This application uses these FOSS components:
- NAudio DLL including modified controls and midi file utilities: [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
- Json processor: [Newtonsoft](https://github.com/JamesNK/Newtonsoft.Json) (MIT).
- Main icon: [Charlotte Schmidt](http://pattedemouche.free.fr/) (Copyright © 2009 of Charlotte Schmidt).
- Button icons: [Glyphicons Free](http://glyphicons.com/) (CC BY 3.0).
