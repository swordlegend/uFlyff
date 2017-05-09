# uFlyff
This project is an attempt at porting a game called Flyff (Fly For Fun) to Unity. The idea is to have a common set of files which are configured as close to the real game as possible. This will create the platform for Flyff community members to fork off of in order to create their own versions of the game. 

## Current Progress & Todo's
The project is currently in it's infancy. Right now work is being done to interperate Flyff's proprietary file formats and convert them into assets that Unity knows how to handle. 
- [x] Custom AssetPostprocessor implmentation that imports Flyff's proprietary .o3d file format and translates the data into Mesh objects.
- [ ] Custom AssetPostprocessor implmentation that imports Flyff's proprietary .chr file format and translates the data into Avatar objects.
- [ ] Custom AssetPostprocessor implmentation that imports Flyff's proprietary .ani file format and translates the data into AnimationClip objects.

## Related Projects 
- [ATools](https://github.com/Aishiro/ATools) by @Aishiro