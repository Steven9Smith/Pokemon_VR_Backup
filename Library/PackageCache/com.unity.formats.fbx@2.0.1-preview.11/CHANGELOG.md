# Changes in Fbx Exporter

## [2.0.1-preview.11] - 2019-04-10
CHANGES
* Fix ExportTimelineClipTest failing in unit tests

## [2.0.1-preview.10] - 2019-04-01
FIXES
* Fix integration files still missing. They were ignored by .npmignore created when running Package Validation

## [2.0.1-preview.9] - 2019-04-01
FIXES
* Fix missing integration files in previous release

## [2.0.1-preview.8] - 2019-03-29
FIXES
* Fix FbxExportSettings compile error on 2018.3

## [2.0.1-preview.7] - 2019-03-22
FIXES
* Fix Max 2020 integration installation hanging
* Fix duplicate DCCs showing in integration install dropdown if MAYA_LOCATION set

## [2.0.1-preview.6] - 2019-02-08
CHANGES
* Updated package.json
* Reverted change to Runtime asmdef
* Fixed non-deterministic behaviour in tests
* Updated com.autodesk.fbx to 2.0.0-preview.7

## [2.0.1-preview.5] - 2019-02-01
CHANGES
* Updated to com.autodesk.fbx 2.0.0-preview.6
* Updated asmdefs to only include Editor platform

## [2.0.1-preview.4] - 2019-01-31
CHANGES
* Updated unityRelease version in package.json

## [2.0.1-preview.3] - 2019-01-24
CHANGES
* Moved tests to separate .tests package
* Added Timeline dependency
* Export of blendshapes is experimental, YMMV

## [2.0.1-preview.2] - 2018-12-05
CHANGES
* Updated dependency to com.autodesk.fbx@2.0.0-preview.4

## [2.0.1-preview.1] - 2018-12-04
CHANGES
* Updated dependency to com.autodesk.fbx@2.0.0-preview.3

## [2.0.1-preview] - 2018-11-13
NEW FEATURES
* In Unity 2018.3 Prefab Variants replaces Linked Prefabs. The "Convert To Linked Prefab" menu items have been removed.
* Updated documentation

FIXES
* Fix error when exporting SkinnedMesh with bones that are not descendants of the root bone
* Fix animation only export not exporting animation in 2.0.0
* Fix calculating center of root objects when exporting "Local Pivot"/"Local Centered"

KNOWN ISSUES
* In Unity 2018.3 exported blendshape normals may not match original blendshape normals

## [2.0.0] - 2018-06-22
NEW FEATURES
* FBX Exporter is now distributed via the Package Manager
* Added support for Physical Cameras
* Now compatible with Unity 2018.2
* The roundtrip of assets can now be started from Maya from assets that have not been exported from Unity
* DCC integration plug-in sources have been removed from the packages folder. You can still find them in the zip file
* Now using FBX SDK version 2018.1
* Conformed to Unity's API guidelines
* Added support for exporting constraints (Rotation, Aim, Position, Scale and Parent)
* ConvertToPrefab: Add ability to convert an FBX or prefab asset from the Project view
  * Right click on an FBX file in the project view then select "Convert to Linked Prefab" to create a Linked Prefab asset for the FBX file. It will not create an instance in the scene.
  * Right click on a Prefab in the project view and select "Convert to Linked Prefab" to export the Prefab to an FBX file and link the existing Prefab to the newly created FBX file.

FIXES
* Fixed skinned mesh bone update when the number of bones change between updates
* Keyframes were sometimes missing when exporting animation curves
* The file name fields in the FBX export dialog were hard to read in Unity Pro's dark theme

KNOWN ISSUES
* ConvertToPrefab: UI doesn't provide feedback about whether it will be converting an existing file or creating new files.
  * When converting an existing FBX file, the FBX filename and FBX export options are ignored (but not greyed out).
  * When converting an existing Prefab, the Prefab filename is ignored (but not greyed out)

## [1.7.0] - 2018-06-01

FIXES

* Fixed violations of the C# Framework Design Guidelines (FDG)
* Fixed errors reported while running the Package Validation Suite

## [1.6.0] - 2018-05-29

NEW FEATURES

* Added support for physical cameras

FIXES

* Fixed skinned mesh bone update

## [1.5.0]

NEW FEATURES

* The roundtrip of assets can now be started from Maya from assets that have not been exported from Unity
* DCC integration plug-in sources have been moved away from the package
* Windows version is now using the FBX SDK version 2018.1.1
* Streamlined public interface for the ModelExporter class

FIXES:

* DCC integration plug-ins now work with packman

## [1.4.0]

NEW FEATURES

* FBX Exporter is now distributed via the Package Manager
* Constraints: we now export Unity constraints to FBX
* ConvertToPrefab: Add ability to convert an fbx or prefab asset from the Project view

Right click on an fbx in the project view then select Convert to Linked Prefab to create
a linked prefab asset for the fbx. It will not create an instance in the scene.

Right click on a prefab in the project view and select Convert to Linked Prefab to export the prefab to an fbx file
and link the existing prefab to the newly created fbx.

FIXES:

* Now compatible with Unity 2018.2.0b3
* Last frame was sometimes not exported
* FBX export dialog hard to read in Unity Pro's dark theme

KNOWN ISSUES

* ConvertToPrefab: UI doesn't provide feedback about whether it will be converting an existing file or creating new files.
When converting an existing FBX file, the fbx filename and fbx export options are ignored (but not greyed out).
When converting an existing prefab, the prefab filename is ignored (but not greyed out)

## [1.3.0f1] - 2018-04-17
NEW FEATURES
* Unity3dsMaxIntegration: Allow multi file import
* Unity3dsMaxIntegration: Allow multi file export by scene selection
* FbxExporter: Export animation clips from Timeline
* FbxExportSettings: Added new UI to set export settings
* FbxExportSettings: Added option to transfer transform animation on export
* FbxExporterSettings: Added option to export model only
* FbxExporterSettings: Added option to export animation only
* FbxExporterSettings: Added option not to export animation on skinned meshes
* FbxExportSettings: Added option to export meshes without renderers
* FbxExportSettings: Added LOD export option
* UnityMayaIntegration: Allow multi file import
* UnityMayaIntegration: Allow multi file export by scene selection
* FbxPrefabAutoUpdater: new UI to help manage name changes
* FbxExporter: Added support for exporting Blendshapes
* FbxExporter: Added support for exporting SkinnedMeshes with legacy and generic animation
* FbxExporter: Added support for exporting Lights with animatable properties (Intensity, Spot Angle, Color)
* FbxExporter: Added support for exporting Cameras with animatable properties (Field of View)
* FbxExporter: added ability to export animation on transforms
* Added Maya LT one button import/export
* Added Camera export support 
* Added 3ds Max one button import/export
* Ability to export FBX files from Unity
* Convert to linked prefab to create a prefab that auto-updates with the linked FBX
* Maya one button import/export

FIXES
* ConvertToPrefab: fix Mesh Collider not pointing to exported mesh after converting
* FbxExporter: fix so "Compatible Naming" doesn't modify scene on export
* FbxExporter: link materials to objects connected to mesh instances
* FbxExporter: export meshes in model prefab instances as mesh instances in FBX
* ConvertToPrefab: Don't re-export FBX model instances
* FbxExportSettings: fix console error on Mac when clicking "Install Unity Integration"
* FbxExporter: fix so animating spot angle in Unity animates cone angle in Maya (not penumbra)
* FbxExporter: export correct rotation order (xyz) for euler rotation animations (previously would export as zxy)
* Fix Universal Windows Platform build errors caused by UnityFbxSdk.dll being set as compatible with any platform instead of editor only.
* Fix so Object references aren't lost when using Convert to Linked Prefab Instance
* Fix Maya Integration dropdown not appearing in the Export Settings

KNOWN ISSUES
* Cannot export animation only from 3ds Max
* FbxExporter: animated skinned meshes must be in the bind pose on export (i.e. not being previewed in the Animation or Timeline windows, and the original rig's fbx must not contain animation)
* FbxExporter: animated meshes in bone hierarchy are not supported
* FbxExporter: for skinned meshes all bones must be descendants of the root bone
* 3DIntegration: FBX containing rig must have file units in cm in order for animation exported from Unity to be properly applied
* ConvertToPrefab: converting model instance that has been modified in the scene won't re-export FBX
* Requires Unity 2018.1.0
* When exporting with an animated transform for a Camera or a Light, the resulting rotation does not take the forward direction into account and is off by 90 degrees
* Key tangents are not exported and the default key tangent setting is different between Unity, FBXSDK and Maya. This cause the curve shape to change between Unity and Maya.
* Animated continuous rotations are not maintained
* Animated rotations with Euler Angles (Quaternion) or Quaternion interpolation are not converted to the correct Euler equivalent.
