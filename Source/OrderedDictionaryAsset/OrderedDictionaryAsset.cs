// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rotorz.Extras.Collections {

	/// <summary>
	/// Base class for a serializable ordered dictionary asset. Custom dictionary asset
	/// classes should inherit from the <see cref="OrderedDictionaryAsset{TKey, TValue}"/>
	/// generic class instead.
	/// </summary>
	public abstract class OrderedDictionaryAsset : ScriptableObject {

		/// <exclude/>
		[SerializeField, HideInInspector]
		public bool _suppressErrors;

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionaryAsset"/> class.
		/// </summary>
		/// <param name="keyType">Data type of a key.</param>
		/// <param name="valueType">Data type of a value.</param>
		protected OrderedDictionaryAsset(Type keyType, Type valueType) {
			KeyType = keyType;
			ValueType = valueType;
		}

		#region Error Feedback

		/// <summary>
		/// Gets the collection of keys that somehow have two or more associated values.
		/// </summary>
		public abstract IEnumerable<object> KeysWithDuplicateValues { get; }

		#endregion

		#region Ordered Lookup

		/// <summary>
		/// Gets the total count of entries in the dictionary.
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// Implements the public interface <see cref="GetKeyFromIndex(int)"/>.
		/// </summary>
		/// <param name="index">Zero-based index of entry in ordered dictionary.</param>
		/// <returns>
		/// The key.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="index"/> is out of range.
		/// </exception>
		protected abstract object GetKeyFromIndexInternal(int index);

		/// <summary>
		/// Implements the public interface <see cref="GetValueFromIndex(int)"/>.
		/// </summary>
		/// <param name="index">Zero-based index of entry in ordered dictionary.</param>
		/// <returns>
		/// The key.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="index"/> is out of range.
		/// </exception>
		protected abstract object GetValueFromIndexInternal(int index);

		/// <summary>
		/// Gets the key of the entry at the specified index.
		/// </summary>
		/// <param name="index">Zero-based index of entry in ordered dictionary.</param>
		/// <returns>
		/// The key.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="index"/> is out of range.
		/// </exception>
		/// <see cref="GetValueFromIndex(int)"/>
		/// <see cref="Count"/>
		public object GetKeyFromIndex(int index) {
			return GetKeyFromIndexInternal(index);
		}

		/// <summary>
		/// Gets the value of the entry at the specified index.
		/// </summary>
		/// <param name="index">Zero-based index of entry in ordered dictionary.</param>
		/// <returns>
		/// The key.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="index"/> is out of range.
		/// </exception>
		/// <see cref="GetKeyFromIndex(int)"/>
		/// <see cref="Count"/>
		public object GetValueFromIndex(int index) {
			return GetValueFromIndexInternal(index);
		}

		/// <summary>
		/// Determines whether the dictionary contains the untyped key.
		/// </summary>
		/// <param name="key">Key to lookup.</param>
		/// <returns>
		/// A value of <c>true</c> if dictionary contains an entry with the specified key.
		/// Always returns a value of <c>false</c> if the specified key is not of the type
		/// <see cref="KeyType"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="key"/> is <c>null</c>.
		/// </exception>
		public abstract bool ContainsKey(object key);

		#endregion

		/// <summary>
		/// Gets the data type of a key.
		/// </summary>
		/// <see cref="ValueType"/>
		public Type KeyType { get; private set; }

		/// <summary>
		/// Gets the data type of a value.
		/// </summary>
		/// <see cref="KeyType"/>
		public Type ValueType { get; private set; }

	}

}
