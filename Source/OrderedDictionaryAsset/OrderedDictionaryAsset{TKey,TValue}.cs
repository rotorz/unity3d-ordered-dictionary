// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotorz.Extras.Collections {

	/// <summary>
	/// Base class for a serializable ordered dictionary asset. Custom dictionary asset
	/// classes should inherit from this class since Unity is currently unable to
	/// serialize instances of generic types.
	/// </summary>
	/// <typeparam name="TKey">Type of key.</typeparam>
	/// <typeparam name="TValue">Type of value.</typeparam>
	public abstract partial class OrderedDictionaryAsset<TKey, TValue> : OrderedDictionaryAsset, ISerializationCallbackReceiver, IDictionary<TKey, TValue>, IDictionary, IList<KeyValuePair<TKey, TValue>> {

		protected readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
		protected readonly HashSet<object> _keysWithDuplicateValues = new HashSet<object>();

		[SerializeField, HideInInspector]
		private int _version;
		[SerializeField]
		protected List<TKey> _keys = new List<TKey>();
		[SerializeField]
		protected List<TValue> _values = new List<TValue>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionaryAsset"/> class.
		/// </summary>
		public OrderedDictionaryAsset() : base(typeof(TKey), typeof(TValue)) {
			Keys = new KeyCollection(this);
			Values = new ValueCollection(this);
		}

		#region Unity Serialization

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			OnAfterDeserialize();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {
			OnBeforeSerialize();
		}

		protected virtual void OnAfterDeserialize() {
			if (_keys.Count != _values.Count)
				Debug.LogError("Inconsistent quantity of keys and values.");

			_dictionary.Clear();
			_keysWithDuplicateValues.Clear();

			int count = Mathf.Min(_keys.Count, _values.Count);
			for (int i = 0; i < count; ++i) {
				var key = _keys[i];

				if (key == null) {
					if (!_suppressErrors) {
						// Only show these warning messages when playing.
						Debug.LogError("Encountered invalid null key.");
					}
					continue;
				}

				if (!_dictionary.ContainsKey(key))
					_dictionary.Add(key, _values[i]);
				else
					_keysWithDuplicateValues.Add(key);
			}

			// Only show these warning messages when playing.
			foreach (var key in _keysWithDuplicateValues)
				Debug.Log(string.Format("Has multiple values for the key '{0}'.", key));
		}

		protected virtual void OnBeforeSerialize() {
		}

		#endregion

		#region Error Feedback

		/// <inheritdoc/>
		public override IEnumerable<object> KeysWithDuplicateValues {
			get { return _keysWithDuplicateValues; }
		}

		#endregion

		#region Ordered Lookup

		/// <inheritdoc/>
		public sealed override int Count {
			get { return _keys.Count; }
		}

		/// <inheritdoc/>
		protected override object GetKeyFromIndexInternal(int index) {
			CheckIndexArgument(index);
			return _keys[index];
		}

		/// <inheritdoc/>
		protected override object GetValueFromIndexInternal(int index) {
			CheckIndexArgument(index);
			return _values[index];
		}

		/// <inheritdoc/>
		public sealed override bool ContainsKey(object key) {
			CheckKeyArgument(key);

			if (!KeyType.IsAssignableFrom(key.GetType()))
				return false;

			return _dictionary.ContainsKey((TKey)key);
		}

		#endregion

		#region Integrity Checks

		private void CheckVersion(int version) {
			if (version != _version)
				throw new InvalidOperationException("Cannot make changes to the collection whilst the collection is being enumerated.");
		}

		private void CheckIndexArgument(int index) {
			if ((uint)index >= Count)
				throw new ArgumentOutOfRangeException("index", index, null);
		}

		private void CheckInsertIndexArgument(int index) {
			if ((uint)index > _keys.Count)
				throw new ArgumentOutOfRangeException("index", index, null);
		}

		private void CheckKeyArgument(object key) {
			if (key == null)
				throw new ArgumentNullException("key");
			if (!(key is TKey))
				throw new ArgumentException("Incompatible type.", "key");
		}

		private void CheckKeyArgument(TKey key) {
			if (key == null)
				throw new ArgumentNullException("key");
		}

		private void CheckValueArgument(object value) {
			if (value is TValue || (value == null && !typeof(TValue).IsValueType))
				return;

			throw new ArgumentException("Incompatible type.", "value");
		}

		private void CheckUniqueKeyArgument(TKey key) {
			if (ContainsKey(key))
				throw new ArgumentException(string.Format("Already contains key '{0}'.", key), "key");
		}

		private static void ThrowReadOnlyException() {
			throw new InvalidOperationException("Collection is read-only.");
		}

		#endregion

		#region Non-Applicable Interfaces

		/// <inheritdoc/>
		bool IDictionary.IsReadOnly {
			get { return false; }
		}

		/// <inheritdoc/>
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}

		/// <inheritdoc/>
		bool IDictionary.IsFixedSize {
			get { return false; }
		}

		/// <inheritdoc/>
		object ICollection.SyncRoot {
			get { return (_dictionary as ICollection).SyncRoot; }
		}

		/// <inheritdoc/>
		bool ICollection.IsSynchronized {
			get { return (_dictionary as ICollection).IsSynchronized; }
		}

		#endregion

		/// <summary>
		/// Gets the read-only ordered collection of keys.
		/// </summary>
		/// <see cref="Values"/>
		public KeyCollection Keys { get; private set; }

		/// <summary>
		/// Gets the read-only ordered collection of values.
		/// </summary>
		/// <see cref="Keys"/>
		public ValueCollection Values { get; private set; }

		/// <inheritdoc/>
		ICollection<TKey> IDictionary<TKey, TValue>.Keys {
			get { return Keys; }
		}

		/// <inheritdoc/>
		ICollection<TValue> IDictionary<TKey, TValue>.Values {
			get { return Values; }
		}

		/// <inheritdoc/>
		ICollection IDictionary.Keys {
			get { return Keys; }
		}

		/// <inheritdoc/>
		ICollection IDictionary.Values {
			get { return Values; }
		}

		/// <inheritdoc/>
		public TValue this[TKey key] {
			get { return _dictionary[key]; }
			set {
				if (_dictionary.ContainsKey(key)) {
					_dictionary[key] = value;

					int index = IndexOf(key);
					if (index == -1)
						throw new InvalidOperationException();
					_values[index] = value;
				}
				else {
					_dictionary.Add(key, value);
					_keys.Add(key);
					_values.Add(value);
				}
			}
		}

		/// <inheritdoc/>
		object IDictionary.this[object key] {
			get {
				CheckKeyArgument(key);

				return this[(TKey)key];
			}
			set {
				CheckKeyArgument(key);
				CheckValueArgument(value);

				this[(TKey)key] = (TValue)value;
			}
		}

		/// <inheritdoc/>
		KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index] {
			get { return new KeyValuePair<TKey, TValue>(_keys[index], _values[index]); }
			set {
				var existingKey = _keys[index];

				if (!EqualityComparer<TKey>.Default.Equals(existingKey, value.Key)) {
					if (ContainsKey(value.Key))
						throw new InvalidOperationException();

					_dictionary.Remove(existingKey);
					_dictionary[value.Key] = value.Value;

					_keys[index] = value.Key;
				}

				_values[index] = value.Value;
			}
		}

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
		new public TKey GetKeyFromIndex(int index) {
			CheckIndexArgument(index);
			return _keys[index];
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
		new public TValue GetValueFromIndex(int index) {
			CheckIndexArgument(index);
			return _values[index];
		}

		/// <inheritdoc/>
		void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item) {
			Insert(index, item.Key, item.Value);
		}

		/// <summary>
		/// Insert new entry into the ordered dictionary.
		/// </summary>
		/// <param name="index">Zero-based index at which to insert the new entry.</param>
		/// <param name="key">Unique key for the new entry.</param>
		/// <param name="value">Value for the new entry.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="index"/> is out of the range of the ordered dictionary.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If dictionary already contains an entry with the specified key.
		/// </exception>
		public void Insert(int index, TKey key, TValue value) {
			CheckInsertIndexArgument(index);
			CheckUniqueKeyArgument(key);

			++_version;

			_dictionary.Add(key, value);
			_keys.Insert(index, key);
			_values.Insert(index, value);
		}

		/// <inheritdoc/>
		public void Add(TKey key, TValue value) {
			Insert(Count, key, value);
		}

		/// <inheritdoc/>
		void IDictionary.Add(object key, object value) {
			CheckKeyArgument(key);
			CheckValueArgument(value);

			var k = (TKey)key;

			if (ContainsKey(k))
				throw new ArgumentException(string.Format("Already contains key '{0}'", k), "key");

			Add(k, (TValue)value);
		}

		/// <inheritdoc/>
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
			Add(item.Key, item.Value);
		}

		/// <inheritdoc/>
		public void Clear() {
			++_version;

			_keys.Clear();
			_values.Clear();
			_dictionary.Clear();
		}

		/// <inheritdoc/>
		public bool Remove(TKey key) {
			CheckKeyArgument(key);

			if (ContainsKey(key)) {
				RemoveAt(IndexOf(key));
				return true;
			}
			return false;
		}

		/// <inheritdoc/>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
			return Remove(item.Key);
		}

		/// <inheritdoc/>
		void IDictionary.Remove(object key) {
			CheckKeyArgument(key);

			Remove((TKey)key);
		}

		/// <inheritdoc/>
		public void RemoveAt(int index) {
			CheckIndexArgument(index);

			++_version;

			_dictionary.Remove(_keys[index]);
			_keys.RemoveAt(index);
			_values.RemoveAt(index);
		}

		/// <summary>
		/// Gets an object for enumerating over the ordered collection of keys and values.
		/// </summary>
		/// <returns>
		/// The new <see cref="Enumerator"/>.
		/// </returns>
		public Enumerator GetEnumerator() {
			return new Enumerator(this, false);
		}

		/// <inheritdoc/>
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
			return GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <inheritdoc/>
		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return new Enumerator(this, true);
		}

		/// <inheritdoc/>
		public bool TryGetValue(TKey key, out TValue value) {
			return _dictionary.TryGetValue(key, out value);
		}

		/// <inheritdoc/>
		public bool ContainsKey(TKey key) {
			return _dictionary.ContainsKey(key);
		}

		/// <inheritdoc/>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
			TValue value;
			return TryGetValue(item.Key, out value) && EqualityComparer<TValue>.Default.Equals(item.Value, value);
		}

		/// <inheritdoc/>
		bool IDictionary.Contains(object key) {
			if (key == null)
				throw new ArgumentNullException("key");

			if (key is TKey)
				return _dictionary.ContainsKey((TKey)key);
			else
				return false;
		}

		/// <summary>
		/// Determines the index of an item with a aspecific key in the <see cref="OrderedDictionaryAsset{TKey, TValue}"/>.
		/// </summary>
		/// <param name="key">The key of the entry to locate in the <see cref="OrderedDictionaryAsset{TKey, TValue}"/>.</param>
		/// <returns>
		/// The zero-based index of the entry when found; otherwise, a value of <c>-1</c>.
		/// </returns>
		public int IndexOf(TKey key) {
			return _keys.IndexOf(key);
		}

		/// <inheritdoc/>
		int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item) {
			int index = IndexOf(item.Key);

			if (index != -1 && !EqualityComparer<TValue>.Default.Equals(item.Value, _values[index]))
				return -1;

			return index;
		}

		/// <inheritdoc/>
		void ICollection.CopyTo(Array array, int index) {
			(_dictionary as ICollection).CopyTo(array, index);
		}

		/// <inheritdoc/>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			(_dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
		}

	}

}
