# ArisenTools
Collection of tools for Dragon's Dogma: Dark Arisen (PC version). Currently heavily under construction, but feel free to take a look.

##DDDAarc
Unpacks and repacks .arc archive files. Should support 99% of file extensions, please post an issue if you spot one that is just a hash.

Unpacking syntax:
```
DDDAarc -x input.arc output_folder
```

Repacking syntax:
```
DDDAarc -p input_folder output.arc
```

I have also included drag'n'drop batch files for unpacking and repacking. Just drag input.arc / input_folder on the matching batch file.

##DDDApck
Unpacks .pck package files. Repacking is not planned. Only tested on message.pck.

It is currently unknown how to pull the right file names and extensions, hence the hex name and generic extension.

Unpacking syntax:
```
DDDApck input.pck
```

##Requirements
* Microsoft .NET Framework 4.5
* [Zlib.net (supplied)](http://www.componentace.com/zlib_.NET.htm) (for building only)