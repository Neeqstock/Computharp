<h1 align="center">
  Computharp
</h1>

*An hackable interface to play music using standard laptop equipment.*

Please refer to the [Computharp page](https://neeqstock.notion.site/Computharp-3a6cfb04dec64d9084beaab417344956) on my personal website for more informations, instrument description and usage guide.

</br><div align="center">
    <img src="ComputharpLogo.png" width="150px" alt="Computharp logo">    

</div></br>

Computharp is a software MIDI controller made to play music with computer keyboards. Plural, because Computharp distinguishes between multiple keyboards, and assign controls to them independently. In this way, you can use both hands to play on different octaves, on different keyboards.

The layout is isomorphic: every letter, number and function key row becomes a "string" of the instrument. Every string is tuned three half-tones up from the previous one, but the rule can be changed (e.g. to five half-tones, like a normal guitar), with a single keypress.

Touchpad and/or mouse can be used like violin bows to control channel pressure.

Every keyboard can be tuned in a different way, and the interface can highlight different scales.

Scores can be written as words. A major scale, for example, could be written as "cbfgry56". Theoretically, I expect some writing skills can be transfered into music playing skills.

## Requirements
- A laptop with a working touchpad, a working keyboard, a working screen
- Possibly an extra keyboard, and please clean your grandma's keyboard from dust and crap, and buy a PS/2 to USB adapter
- Windows. Yeah, the OS. Yeah, sorry, this is not cross platform :( for now.

## Installation and running

1. Download the latest release from the [Releases](https://github.com/Neeqstock/Computharp/releases)
 page
2. Unzip the archive
3. Run `Computharp.exe`

## Contribution

Computharp is licensed through a GNU GPL-v3 Free Open-Source software license. Feel free to fork this repository and contribute!
### Dependencies
- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), which SDK and development tools can be automatically downloaded within the [Visual Studio](https://visualstudio.microsoft.com/it/downloads/) installer
- Netychords depends on [NITHlibrary](https://github.com/LIMUNIMI/NITHlibrary) and [NITHdmis](https://github.com/LIMUNIMI/NITHdmis). Please clone both of them, and place them in an adjacent folder to your Netychords folder. Visual Studio should automatically locate them after opening `Computharp.sln`

## Issues

If you have any issue and/or proposal, please open a GitHub issue on the [Issues page](https://github.com/LIMUNIMI/Computharp/issues) of this repository.
