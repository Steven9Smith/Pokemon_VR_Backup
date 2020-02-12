# Change log


## [0.3.3] - 2020-01-28

### Changed

* Updated dependencies for this package.


## [0.3.2] - 2020-01-16

### Changed

* Updated dependencies of this package.


## [0.3.1] - 2019-12-16

**This version requires Unity 2019.3.0f1+**

### Changes

* Updated dependencies of this package.


## [0.3.0] - 2019-12-03

### Changes

* Updated dependencies of this package.


## [0.2.0] - 2019-11-22

**This version requires Unity 2019.3 0b11+**

### New Features

* Added support for vertex skinning.

### Fixes

* Fixed an issue where disabled UnityEngine Components were not getting ignored when converted via `ConvertToEntity` (it only was working for subscenes).

### Changes

* Removed LightSystem and light conversion.
* Updated dependencies for this package.

### Upgrade guide

* LightSystem was removed
  * Lightsystem was not performance by default and the concept of driving a game object from a component turned out to be not performance by default. It was also not maintainable because every property added to lights has to be reflected in this package.
  * LightSystem will be replaced with hybrid entities in the future. This will be a more clean uniform API for graphics related functionalities. 


## [0.1.1] - 2019-08-06 

### Fixes

* Adding a disabled tag component, now correctly disables the light.

### Changes

* Updated dependencies for this package.


## [0.1.0] - 2019-07-30

### New Features

* New `GameObjectConversionSettings` class that we are using to help manage the various and growing settings that can tune a GameObject conversion.
* New ability to convert and export Assets, which is initially needed for Tiny.
  * Assets are discovered via `DeclareReferencedAsset` in the `GameObjectConversionDeclareObjectsGroup` phase and can then be converted by a System during normal conversion phases.
  * Assets can be marked for export and assigned a guid via `GameObjectConversionSystem.GetGuidForAssetExport`. During the System `GameObjectExportGroup` phase, the converted assets can be exported via `TryCreateAssetExportWriter`.
* `GetPrimaryEntity`, `HasPrimaryEntity`, and the new `TryGetPrimaryEntity` all now work on `UnityEngine.Object` instead of `GameObject` so that they can also query against Unity Assets.

### Upgrade guide

* Various GameObject conversion-related methods now receive a `GameObjectConversionSettings` object rather than a set of misc config params.
  * `GameObjectConversionSettings` has implicit constructors for common parameters such as `World`, so much existing code will likely just work.
  * Otherwise construct a `GameObjectConversionSettings`, configure it with the parameters you used previously, and send it in.
* `GameObjectConversionSystem`: `AddLinkedEntityGroup` is now `DeclareLinkedEntityGroup` (should auto-upgrade).
* The System group `GameObjectConversionDeclarePrefabsGroup` is now `GameObjectConversionDeclareObjectsGroup`. This cannot auto-upgrade but a global find&replace will fix it.
* `GameObjectConversionUtility.ConversionFlags.None` is gone, use 0 instead.

### Changes

* Changing `entities` dependency to latest version (`0.1.0-preview`).


## [0.0.1-preview.13] - 2019-05-24

### Changes

* Changing `entities` dependency to latest version (`0.0.12-preview.33`). 


## [0.0.1-preview.12] - 2019-05-16

### Fixes

* Adding/fixing `Equals` and `GetHashCode` for proxy components. 


## [0.0.1-preview.11] - 2019-05-01

Change tracking started with this version.
