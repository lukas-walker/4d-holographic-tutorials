# 4d-holographic-tutorials

## Environment setup

1. install Unity Hub and current Unity version (2020.3.30f1)

https://unity3d.com/de/get-unity/download

It will tell you if you're opening a project with the wrong Unity version and will also let you install the correct version automatically. 
(For this you need a Unity account. dk if you can get around this with a direct installation.)

When choosing the Editor to install, add the UWP build support and the IL2CPP build support. You can also add these modules later.

Attention: This may take FOREVER. Don't stop the installation even though it looks like it's stuck. If you do it might show the version as installed, but it might still be broken in some way. (Happened to me before.)

2. Install .NET

Desktop App

https://dotnet.microsoft.com/en-us/download/dotnet/5.0/runtime

3. Install Visual Studio Community edition 

I'm not sure how restrictive the choice of version is. I had the 2022 version installed which was enough for the Unity2020 installation, but the Unity2019 installation that is used to open isaaks repo triggered the VS install of 2019. Don't know if this doesn't matter..

4. Download the MRTK Feature Tool. 

It lets you organize your Unity Project, install all MRTK packages and additional stuff that we might need. (e.g. for Azure)

Here's how it works: https://docs.microsoft.com/en-gb/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool

Added packages: 

```
Microsoft Azure Object Anchors
MRTK Examples
MRTK Extensions
MRTK Foundation
MRTK Standard Assets
MRTK TestUtilities
MRTK Tools
Mixed Reality OpenXR Plugin
```

5. Open the Project

Maybe an error occurs: 

```An error occurred while resolving packages:
  Project has invalid dependencies:
    com.microsoft.mixedreality.openxr: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.openxr-1.0.0.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.examples: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.examples-2.7.2.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.extensions: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.extensions-2.7.2.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.foundation: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.foundation-2.7.2.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.standardassets: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.standardassets-2.7.2.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.testutilities: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.testutilities-2.7.2.tgz\package.json] cannot be found
    com.microsoft.mixedreality.toolkit.tools: The file [D:\Unity\goji\Packages\MixedReality\com.microsoft.mixedreality.toolkit.tools-2.7.2.tgz\package.json] cannot be found
```

One way to resolve it is to install the packages again using the feature tool (it will say that the package is installed already, but you can just "install" it again anyway). Make sure you use the correct version. The tool will tell you which version is currently (supposedly) installed.

Importing everything will take very long. (10-15min on my low end computer)

6. Change the build settings to UWP

Go to File > Build settings.

Choose Platform Universal Windows Platform. If not yet installed it will tell you to install UWP the Unity Hub. (You can also do this directly in the Hub before opening the project.)



## Resources

MRTK Documentation https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/

MRTK GitHub https://github.com/microsoft/MixedRealityToolkit-Unity/releases

MRTK Feature Tool https://docs.microsoft.com/en-gb/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool

Project proposal https://www.overleaf.com/project/62272be78a4c7f8ecc8eb89b
