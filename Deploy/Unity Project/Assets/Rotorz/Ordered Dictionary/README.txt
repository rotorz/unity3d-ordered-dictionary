README
======

Library for adding ordered dictionaries to custom `ScriptableObject` and `MonoBehaviour`
classes in a way that can be serialized by Unity provided that the key and value types
are serializable.

Licensed under the MIT license. See LICENSE file in the project root for full license
information. DO NOT contribute to this project unless you accept the terms of the
contribution agreement.

![screenshot](https://bitbucket.org/rotorz/ordered-dictionary-for-unity/raw/master/screenshot.png)

Features
--------

- Default inspector with intuitive interface.
- Drop insertion for UnityEngine.Object's where name is used for key.
- Have multiple dictionaries per `ScriptableObject` or `MonoBehaviour`!
- Serializable dictionary asset files.
- Supports any serializable key and value types.
- Ordered dictionary.

Dependencies
------------

Requires Rotorz Reorderable List Control which is open source and can be downloaded
from the following BitBucket repository:

https://bitbucket.org/rotorz/reorderable-list-editor-field-for-unity

Installing scripts
------------------

This control can be added to your project by importing the Unity package which
contains a compiled class library (DLL). This can be used by C# and UnityScript
developers.

- [Download RotorzOrderedDictionary_v0.3.0 Package (requires Unity 5.4.0+)](<https://bitbucket.org/rotorz/ordered-dictionary-for-unity/downloads/RotorzOrderedDictionary_v0.3.0.unitypackage>)

If you would prefer to use the non-compiled source code version in your project,
copy the contents of this repository somewhere into your project.

**Note to UnityScript (*.js) developers:**

UnityScript will not work with the source code version of this project unless
the contents of this repository is placed at the path "Assets/Plugins/OrderedDictionary"
due to compilation ordering.

Example 1: Sprite Library
-------------------------

    :::csharp
    // StringSpriteDictionaryEditable.cs
    using Rotorz.Extras.Collections;
    using System;
    using UnityEngine;

	// Script filename must match this class.
    public sealed class StringSpriteDictionaryEditable : EditableEntry<StringSpriteDictionary>
    {
    }

	// This class can exist in same file with any name.
    [Serializable, EditableEntry(typeof(StringSpriteDictionaryEditable))]
    public sealed class StringSpriteDictionary : OrderedDictionary<string, Sprite>
    {
    }


    // SpriteLibrary.cs
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu]
    public class SpriteLibrary : ScriptableObject
	{
		public StringSpriteDictionary sprites;
    }

Example 2: String Lookup Table
------------------------------

![screenshot2](https://bitbucket.org/rotorz/ordered-dictionary-for-unity/raw/master/screenshot2.png)

    :::csharp
    // StringStringDictionaryEditable.cs
    using Rotorz.Extras.Collections;
    using System;
    using UnityEngine;

	// Script filename must match this class.
    public sealed class StringStringDictionaryEditable : EditableEntry<StringStringDictionary>
    {
    }

	// This class can exist in same file with any name.
    [Serializable, EditableEntry(typeof(StringStringDictionaryEditable))]
    public sealed class StringStringDictionary : OrderedDictionary<string, string>
    {
    }


    // AchievementNamesBehaviour.cs
    using UnityEditor;
    using UnityEngine;

    public class AchievementNamesBehaviour : MonoBehaviour
	{
		public StringStringDictionary names;
    }

Submission to the Unity Asset Store
-----------------------------------

If you wish to include this asset as part of a package for the asset store, please
include the latest package version as-is to avoid conflict issues in user projects.
It is important that license and documentation files are included and remain intact.

**To include a modified version within your package:**

- Ensure that license and documentation files are included and remain intact. It should
  be clear that these relate to the ordered dictionary asset library.

- Copyright and license information must remain intact in source files.

- Change the namespace `Rotorz` to something unique and DO NOT use the
  name "Rotorz". For example, `YourName.Collections` or `YourName.Internal.Collections`.

- Place files somewhere within your own asset folder to avoid causing conflicts with
  other assets which make use of this project.

Useful links
------------

- [Rotorz Website](<http://rotorz.com>)

Contribution Agreement
----------------------

This project is licensed under the MIT license (see LICENSE). To be in the best
position to enforce these licenses the copyright status of this project needs to
be as simple as possible. To achieve this the following terms and conditions
must be met:

- All contributed content (including but not limited to source code, text,
  image, videos, bug reports, suggestions, ideas, etc.) must be the
  contributors own work.

- The contributor disclaims all copyright and accepts that their contributed
  content will be released to the public domain.

- The act of submitting a contribution indicates that the contributor agrees
  with this agreement. This includes (but is not limited to) pull requests, issues,
  tickets, e-mails, newsgroups, blogs, forums, etc.