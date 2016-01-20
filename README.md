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

##DDDAarclist
Processes all .arc archive files from a given folder recursively. Gathered info is printed in comma-seperated format, no files are extracted.

Useful for finding duplicates or files of interest. You can force full path printing for archive names by using the flag `-full`.

Syntax:
```
DDDAarclist input_folder [flag]
```

##DDDApck
Unpacks and repacks .pck package files. Repacked files not tested in-game but should be functional.

It is currently unknown how to pull the right file names and extensions, hence the hex name and generic extension.

Syntax:
```
DDDApck input.pck|input_folder
```

##DDDAtexlist
Processes all .tex texture files from a given folder recursively. Gathered info is printed in comma-seperated format, no files are converted.

Useful for finding duplicates or files of interest. You can force full path printing for archive names by using the flag `-full`.

Syntax:
```
DDDAtexlist input_folder [flag]
```

##Requirements
* Microsoft .NET Framework 4.5
* [Zlib.net (supplied)](http://www.componentace.com/zlib_.NET.htm) (for building only)

##License info Zlib.net
Copyright (c) 2006-2007, ComponentAce

http://www.componentace.com

All rights reserved.

<sub>Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:</sub>

<sub>Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.</sub>

<sub>Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. </sub>

<sub>Neither the name of ComponentAce nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. </sub>

<sub>THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.</sub>