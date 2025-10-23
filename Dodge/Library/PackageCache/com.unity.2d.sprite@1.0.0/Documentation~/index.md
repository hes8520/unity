# About Sprite Editor

Use Unity’s Sprite Editor to create and edit Sprite assets. Sprite Editor provides user extensibility to add custom behaviour for editing various Sprite related data.

# Installing Sprite Editor

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

# Using Sprite Editor

The Sprite Editor Manual can be found [here] (https://docs.unity3d.com/Manual/SpriteEditor.html).

# Using the Sprite Editor Data Provider APIs
The Sprite Editor Data Provider is an API which enables the user to add, change and remove Sprite data in a custom importer or editor tool. Refer to the [Sprite Editor Data Provider API](DataProvider.md) page for examples showing how you can implement the API.


# Technical details
## Requirements

This version of Sprite Editor is compatible with the following versions of the Unity Editor:

* 2019.4 and later (recommended)

## Package contents

The following table indicates the folder structure of the Sprite package:

|Location|Description|
|---|---|
|`<Editor>`|Root folder containing the source for the Sprite Editor.|
|`<Tests>`|Root folder containing the source for the tests for Sprite Editpr used the Unity Editor Test Runner.|

## Document revision history

|Date|Reason|
|---|---|
|April 13, 2022|Added Sprite Editor Data Provider API samples|
|January 25, 2019|Document created. Matches package version 1.0.0|
