[//]: # "******************************************************************************"
[//]: # "THIS DOCUMENTATION IS BEST VIEWED ONLINE AT https://github.com/meniku/UnityVoxelTools"
[//]: # "******************************************************************************"

# Unity Voxel Tools (Highly Experimental)

**WARNING: It's really experimental, code and documentation is partially buggy, undocumented, unperformant, shitty, etc)**

So if you're not scared yet: This project is a collection of various tools for working with Voxel Models in Unity. It currently only supports Magicka Voxel Format.

Altough there could be serveral use cases, the main reason for developing it was to use Unity to allow easy setup of scenes that mimic the look like pixelart but allow realtime lightning ( and thus making it much easier for non-artists like we are to get some good looking scenes :) 

**Play a game based on the tools here: [Ludum Dare 40: Xmas Fair Drinking Simulator](https://ldjam.com/events/ludum-dare/40/xmas-fair-drinking-simulator)**

![Xmas](https://static.jam.vg/content/58f/d/z/cb14.gif)

![Science](http://labs.nkuebler.de/UnityVoxelTools/images/Science.png)

*[Animated Version](https://twitter.com/twitter/statuses/724237971050893312)*

![Pyramid](http://labs.nkuebler.de/UnityVoxelTools/images/Pyramid.png)

![Industrial](http://labs.nkuebler.de/UnityVoxelTools/images/Industrial.png)


## Flexible Import & Manipulation Pipline (NPipeline & NPVox)

![Industrial](http://labs.nkuebler.de/UnityVoxelTools/images/Pipeline.png)

![Industrial](http://labs.nkuebler.de/UnityVoxelTools/images/PipelineEditor.png)

## Animation Editor (NPVoxAnim)

![Character](http://labs.nkuebler.de/UnityVoxelTools/images/Character.gif)

![Character](http://labs.nkuebler.de/UnityVoxelTools/images/AnimationEditor.png)

## Blockmap Editor (GNBlockMap)

![BlockmapEditor](http://labs.nkuebler.de/UnityVoxelTools/images/BlockmapEditor.png)

## Simplifiers To Reduce Draw Calls (NPVoxSimpifiers)

![Simplifiers](http://labs.nkuebler.de/UnityVoxelTools/images/Simplifiers.png)


## The Packages

### NPipeline

Is the framework for the Import Pipeline:

* Pipes have 3 storage modes: 
	* `Attached`: Meshes etc will be stored together with the NPipeline Containers
	* `Resource`: Use for data that doesn't need to show in the editor (like animations) to reduce size of your SCM
	* `Memory`: Use for transformation pipes that only hold temporary data
* Pipelines automatic trigger reimpoorts: if you save your .vox file, the pipeline for the voxelfile will be imported again automatically (only if you cross reference pipes in different pipe-containers, you will need to manually `invalidate and reimport`)



### NPVox 

Voxel Processing Pipeline. Has a lot of different pipe processors available like:

* `NPVoxMagickaSource` (import models from magicka voxel)
* `Combiner` (combine multiple vox models)
* `Flipper` (flip along axes)
* `Slicer` (slice a model into multiple pieces)
* `Socket Attacher` (place a socket to your model that can be used as reference point for combining later, useful for example if you want to animate models with the ability replace the current weapon)
* `Socket Combiner` (combine two models at two sockets)

And more Features:

* converting between vox and unity spaces via tha `NPVoxToUnity` tool + Runtime Raycasts (slow)
* A lot of different settings on the way to generate your Meshes: Different Normal Modes, Optimization Settings, Cutout/Repeat Settings
* Simple Prefab generator to generate prefabs to be used in your GNBlockMapEditor
* there's a ton of smaller things and features

### NPVoxSimplifiers

* Swap between Mesh & Simplified version within editor.
* Only implemented currently is the `NPVoxCubeSimplifier` that works for simplifying cube-like models. Works by snappshotting every side of the cube and generates a tile in TextureAtlasses for Albedo and Normal maps. 
* Heavily recommended to improve performance when possible to use it.
* We also got a tool that's not included yet to to postprocess GNBlockMaps and merge together static simplified cubes into one mesh. It works quite well but it's not yet generic enough. If you're interested poke me.

### NPVoxAnim

**This is the most flacky and unoptimized part of the whole tools. Use with care!** There is no simplifiers for any animations available yet, animations work by swapping out meshes, which is obviously not optimal. Again: use with care!

Extends the NPVox pipe rocessors by:

* Transformer (allow Matrix transformations of selected bounds within models)
* Skeleton Builder (allow to define bones for you model)
* Skeleton Transformer (allow transformations if your bones)
* Socket Transformer ( allow to transform sockets within models)
* Trail Attacher / Generator (allow to add trails to your models)

Adds special "NPVoxAnimation" Container, that has it's own editor extensions to make editing the processors more convenient:

* Add, Delete, Copy, Shift Frames
* Add, Delete, Copy, Shift Pipe Processors within frames
* Edit parameters of the processors by Handles in the scene
* Preview your animation in the editor
* Preview with attached models to the sockets
* Setup Looping & Per Frame Duration
* `NPVoxAnimationPlayer` to play animations

### GNBlockMap (+Editor)

* Make your Cube-Based games simple to edit by painting prefabs on a grid
* Allow to switch between Multiple Layers
* Randomization of various things like Axes, Flipping, Tiles within a folder etc
* Easy Prefab Navigation
* Box Selection & Transformation
* etc etc etc


## Important Known Bugs

### Always change the palette of your magicka voxel models, the default palette is messed up

## Credits

### Developers
* Nils Kübler (Main Developer [http://twitter.com/nkuebler](@nkuebler))
* Vitali Maurer (helped with GNBlockMap editor; most of the example images are based on his voxel models and shaders )

### Used External Code
* [NPVoxReader is based on Magicka Voxel Implementation by Giawa](https://www.giawa.com/magicavoxel-c-importer/)
* [Enum Flag Property Drawer](http://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer)
* [TangentSolver](http://www.terathon.com/code/tangent.html)
* if I missed someone please ping me
