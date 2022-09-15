# Locate in TFS - Visual Studio Extension (2010, 2012, 2013, 2015, 2017, 2019)
2022 - This project is archived, it will not be updated. There is a pull request to upgrade this to VS2022. Please feel free to fork this open source repository and make your VS2022+ version. I'll link it here and on the VS Marketplace for alternatives. Take care. :)

Opens up the Source Control Explorer window to the location of the currently selected file. This works from the Solution Explorer and from the active document. It works in Community editions and better. Please note that you must be connected to a TFS instance in your Team Explorer for this extension to work. After installation, you might receive a message that this extension will stop working in a future version. 

Latest version (2017+): 3.0.0 (May 13, 2019)
Latest version (2010-2015): 1.2.1 (August 9, 2015)

## What it looks like:

![Screenshot](/src/Pendletron.Vsix.LocateInTFS/Resources/LocateInTFS_Screenshot.png)

----------

Download it on the Visual Studio Gallery

2010-2015:
 [http://visualstudiogallery.msdn.microsoft.com/5cc44f63-4ea8-4c17-8aa4-95037a2d32ef](http://visualstudiogallery.msdn.microsoft.com/5cc44f63-4ea8-4c17-8aa4-95037a2d32ef)

2017+: [https://marketplace.visualstudio.com/items?itemName=AlexPendleton.LocateinTFS2017](https://marketplace.visualstudio.com/items?itemName=AlexPendleton.LocateinTFS2017)

----------

## Changelog
### v3.0.0 (May 13, 2019)
- Updated for Visual Studio 2019. Thanks to [BoredTweak](https://github.com/BoredTweak) for the pull request. Thank you for your assistance and patience. :)


### v2.0.0 (June 21, 2017)
- Updated for Visual Studio 2017. Thanks to [o-farooq](https://github.com/o-farooq) for the pull request. This required an entirely new package, so the 2010-2015 extension and the 2017+ extension are at different URIs.


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
