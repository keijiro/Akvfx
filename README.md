Akvfx
=====

![gif](https://i.imgur.com/h0kApp4.gif)
![gif](https://i.imgur.com/lXRiwU3.gif)

**Akvfx** is a Unity plugin that captures color/depth data from an [Azure
Kinect] device and converts them into attribute maps (textures) handy for using
with [Visual Effect Graph].

[Azure Kinect]: https://azure.com/kinect
[Visual Effect Graph]: https://unity.com/visual-effect-graph

System Requirements
-------------------

- Unity 2019.3
- Azure Kinect DK

See also the [System Requirements] page of Azure Kinect DK. Note that Akvfx
doesn't support Linux at the moment.

[System Requirements]:
    https://docs.microsoft.com/en-us/azure/kinect-dk/system-requirements

How To Install
--------------

This package uses the [scoped registry] feature to resolve package
dependencies. Please add the following sections to the manifest file
(Packages/manifest.json).

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

To the `scopedRegistries` section:

```
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.akvfx": "1.0.4"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.akvfx": "1.0.4",
    ...
```
