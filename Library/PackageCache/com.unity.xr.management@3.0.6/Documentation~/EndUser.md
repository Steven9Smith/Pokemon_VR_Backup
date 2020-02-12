# End-user documentation

## Installing an XR plug-in using XR Plugin Management

To install an XR plug-in, follow these steps:

1. Install the XR Plugin Management package from [Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).
2. Once the XR Plugin Management package is installed, open the **Project Settings** window. Select the **XR Plugin Management** tab on the left.
4. In the **XR Plugin Management** tab, click the **+** button to add a Loader for your plug-in.
    If the Loader you're looking for isn't in the list, you might need to install the provider for it. Select the **Install** button from the set of known providers, or use the **Package Manager** window (menu: **Window &gt; Package Manager**) to install the provider you need.


## Adding default Loader and Settings instances

When you install a package via XR Plugin Management, Unity might prompt you to create Loader and Settings instances for the package. This is an optional step to help you get things up and running. If you don't want to create these instances on package install, the Editor prompts you to create them when you access the components that require them (**XRManager Settings** and **Project Settings**).

**Note:** After an XR Provider Loader has been initialized, you can access the `XRGeneralSettings.Instance.Manager.activeLoader` field to manually control the XR Provider's subsystems.

## Adding plug-in Loaders

To add plug-in Loaders, follow these steps:

1. Access the **Project Settings** window (menu: **Edit** &gt; **Project Settings**).
2. Select the **XR Plugin Management** tab on the left.
3. Modify Loaders for each platform your application targets. You can configure the set of Loaders, and their default order.

## Automatic XR loading

By default, XR Plugin Management intializes automatically and starts your XR environment when the application loads. At runtime, this happens immediately before the first Scene loads. In Play mode, this happens immediately after the first Scene loads, but before `Start` is called on your GameObjects. In both scenarios, XR should be set up before calling the MonoBehaviour [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) method, so you should be able to query the state of XR in the `Start` method of your GameObjects.

If you want to start XR on a per-Scene basis (for example, to start in 2D and transition into VR), follow these steps:

1. Access the **Project Settings** window (menu: **Edit** &gt; **Project Settings**).
2. Select the **XR Plugin Management** tab on the left.
3. Disable the **Initialize on Startup** option for each platform you support.
4. At runtime, call the following methods on `XRGeneralSettings.Instance.Manager` to add/create, remove, and reorder the Loaders from your scripts:

|Method|Description|
|---|---|
|`InitializeLoader(Async)`|Sets up the XR environment to run manually.|
|`StartSubsystems`|Starts XR and puts your application into XR mode.|
|`StopSubsystems`|Stops XR and takes your application out of XR mode. You can call `StartSubsystems` again to go back into XR mode.|
|`DeinitializeLoader`|Shuts down XR and removes it entirely. You must call `InitializeLoader(Async)` before you can run XR again.|

To handle pause state changes in the Editor, subscribe to the [`EditorApplication.pauseStateChanged`](https://docs.unity3d.com/ScriptReference/EditorApplication-pauseStateChanged.html) API, then stop and start the subsystems according to the new pause state that the `pauseStateChange` delegate method returns.

## Customizing build and runtime settings

Any package that needs build or runtime settings should provide a settings data type for use. This data type appears in the **Project Settings** window, underneath a top level **XR** node.

By default, Unity doesn't create a custom settings data instance. If you want to modify build or runtime settings for the package, you must go to the package author’s entry in **Project Settings** and select **Create**. This creates an instance of the settings that you can then modify inside **Project Settings**.

## Installing the XR Plugin Management package

Most XR Plugin provider packages typically include XR Plugin Management, so you shouldn't need to install it. If you do need to install it, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

## Installing the Legacy Input Helpers package

Unity requires the Legacy Input Helpers package to operate XR devices correctly. To check if the Legacy Input Helpers package is installed, open the **Project Settings** window and navigate to **XR Plugin Management** &gt; **Input Helpers**. If Unity can't locate the package, click the **Install Legacy Helpers Package** button to install it.
