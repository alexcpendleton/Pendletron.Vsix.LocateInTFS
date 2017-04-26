# Locate in TFS - Visual Studio Extension (2010, 2012, 2013, 2015)
Opens up the Source Control Explorer window to the location of the currently selected file. This works from the Solution Explorer and from the active document. It works in Community editions and better.

Latest version: 1.2.1 (August 9, 2015)

## What it looks like:

![Screenshot](/src/Pendletron.Vsix.LocateInTFS/Resources/LocateInTFS_Screenshot.png)

----------

## Changelog

### v1.2.1 (August 9, 2015)
- Updated for Visual Studio 2015. Thanks to [holm0563](https://github.com/holm0563) for the pull request.

### v1.1.1 (January 25, 2015)

**Bug fixes**

- On a system that uses a European culture info the version string "12.0" is converted to a double with the value 120. This should fix some of the null reference exceptions that some of you have reported. Thanks to **sitofabi** for the diagnosis and pull request.

### v1.1.0 (May 16, 2014)

**Bug fixes**

- In VS2012 and VS2013, when the Source Control Explorer was not already open the "Locate in TFS" command would only open to the root TFS path. This has been fixed, it will now open to the selected file.

**Features**

- The Source Control Explorer's file browser will now scroll to the selected file, if necessary.  

----------

Icon attribution: [http://freecns.yanlu.de/](http://freecns.yanlu.de/)

Open source, MIT license.

----------

Download it on the Visual Studio Gallery
[http://visualstudiogallery.msdn.microsoft.com/5cc44f63-4ea8-4c17-8aa4-95037a2d32ef](http://visualstudiogallery.msdn.microsoft.com/5cc44f63-4ea8-4c17-8aa4-95037a2d32ef)
