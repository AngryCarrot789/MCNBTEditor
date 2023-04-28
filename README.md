# MCNBTEditor
An NBT and Region File editor for minecraft (still WIP), made using MVVM

This is a remake of an app I previously made called MCNBTViewer, but there was so much code to convert and fix that I just gave up and restarted the entire project

## Building/Compiling

To build this project, you need to download and compile a stream utility class I wrote called REghZy.Streams (found at https://github.com/AngryCarrot789/REghZyUtilsCS/tree/master/REghZy.Streams). I'm using this
because AFAIK, C#'s build in binary reader and writer don't support little-endian data... and I also don't like the API for those classes so I just converted 
java's DataInputStream and DataOutputStream into C# code will a few modifications too (e.g. using unsafe code to do pointer magic for performance reasons)

Once you have REghZy.Streams.dll references to both the core and WPF project, it should build completely fine (I may include the DLL in the project in the future)
