// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Rotorz.Extras.Collections {

	public partial class OrderedDictionaryAsset<TKey, TValue> {

		/// <summary>
		/// A read-only ordered collection of keys from the associated <see cref="OrderedDictionaryAsset{TKey, TValue}"/> instance.
		/// </summary>
		public sealed class KeyCollection : IList<TKey>, ICollection {

			private readonly OrderedDictionaryAsset<TKey, TValue> _dictionary;

			/// <summary>
			/// Initializes a new instance of the <see cref="OrderedDictionaryAsset{TKey, TValue}.KeyCollection"/> class.
			/// </summary>
			/// <param name="dictionary">The associated dictionary.</param>
			public KeyCollection(OrderedDictionaryAsset<TKey, TValue> dictionary) {
				_dictionary = dictionary;
			}

			/// <inheritdoc/>
			public int Count {
				get { return _dictionary._keys.Count; }
			}

			/// <inheritdoc/>
			bool ICollection<TKey>.IsReadOnly {
				get { return true; }
			}

			/// <inheritdoc/>
			bool ICollection.IsSynchronized {
				get { return (_dictionary._dictionary as ICollection).IsSynchronized; }
			}

			/// <inheritdoc/>
			object ICollection.SyncRoot {
				get { return (_dictionary._dictionary as ICollection).SyncRoot; }
			}

			/// <summary>
			/// Gets key of entry at a specific index in the ordered dictionary.
			/// </summary>
			/// <param name="index">Zero-based index of entry.</param>
			/// <returns>
			/// The <see cref="TKey"/>.
			/// </returns>
			/// <exception cref="System.ArgumentOutOfRangeException">
			/// If <paramref name="index"/> is out of range of the collection.
			/// </exception>
			public TKey this[int index] {
				get {
					_dictionary.CheckIndexArgument(index);
					return _dictionary._keys[index];
				}
			}

			/// <inheritdoc/>
			TKey IList<TKey>.this[int index] {
				get { return this[index]; }
				set { ThrowReadOnlyException(); }
			}

			/// <inheritdoc/>
			public bool Contains(TKey item) {
				return _dictionary.ContainsKey(item);
			}

			/// <inheritdoc/>
			void ICollection.CopyTo(Array array, int index) {
				(_dictionary._keys as ICollection).CopyTo(array, index);
			}

			/// <inheritdoc/>
			public void CopyTo(TKey[] array, int arrayIndex) {
				_dictionary._keys.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Gets an object for enumerating over the ordered collection of keys.
			/// </summary>
			/// <returns>
			/// The new <see cref="Enumerator"/>.
			/// </returns>
			public Enumerator GetEnumerator() {
				return new Enumerator(_dictionary);
			}

			/// <inheritdoc/>
			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			/// <inheritdoc/>
			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() {
				return GetEnumerator();
			}

			/// <inheritdoc/>
			void ICollection<TKey>.Add(TKey item) {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			bool ICollection<TKey>.Remove(TKey item) {
				ThrowReadOnlyException();
				return false;
			}

			/// <inheritdoc/>
			void ICollection<TKey>.Clear() {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			public int IndexOf(TKey item) {
				return _dictionary._keys.IndexOf(item);
			}

			/// <inheritdoc/>
			void IList<TKey>.Insert(int index, TKey item) {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			void IList<TKey>.RemoveAt(int index) {
				ThrowReadOnlyException();
			}

			/// <summary>
			/// Enumerator for enumerating through the keys of an ordered dictionary.
			/// </summary>
			public struct Enumerator : IEnumerator<TKey> {

				private readonly OrderedDictionaryAsset<TKey, TValue> _dictionary;
				private readonly int _version;

				private int _index;
				private TKey _current;

				/// <summary>
				/// Initializes a new instance of the <see cref="OrderedDictionaryAsset{TKey, TValue}.KeyCollection.Enumerator"/> structure.
				/// </summary>
				/// <param name="dictionary">The associated dictionary.</param>
				public Enumerator(OrderedDictionaryAsset<TKey, TValue> dictionary) {
					_dictionary = dictionary;
					_version = dictionary._version;

					_index = 0;
					_current = default(TKey);
				}

				/// <inheritdoc/>
				void IDisposable.Dispose() {
				}

				/// <inheritdoc/>
				public TKey Current {
					get { return _current; }
				}

				/// <inheritdoc/>
				object IEnumerator.Current {
					get { return _current; }
				}

				/// <inheritdoc/>
				public bool MoveNext() {
					_dictionary.CheckVersion(_version);

					if (_index < _dictionary._keys.Count) {
						_current = _dictionary._keys[_index];
						++_index;
						return true;
					}
					return false;
				}

				/// <inheritdoc/>
				public void Reset() {
					_dictionary.CheckVersion(_version);

					_current = default(TKey);
					_index = 0;
				}
			}

		}

	}

}
