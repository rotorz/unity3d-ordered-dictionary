// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Rotorz.Extras.Collections {

	public partial class OrderedDictionaryAsset<TKey, TValue> {

		/// <summary>
		/// A read-only ordered collection of values from the associated <see cref="OrderedDictionaryAsset{TKey, TValue}"/> instance.
		/// </summary>
		public sealed class ValueCollection : IList<TValue>, ICollection {

			private readonly OrderedDictionaryAsset<TKey, TValue> _dictionary;

			/// <summary>
			/// Initializes a new instance of the <see cref="OrderedDictionaryAsset{TKey, TValue}.ValueCollection"/> class.
			/// </summary>
			/// <param name="dictionary">The associated dictionary.</param>
			public ValueCollection(OrderedDictionaryAsset<TKey, TValue> dictionary) {
				_dictionary = dictionary;
			}

			/// <inheritdoc/>
			public int Count {
				get { return _dictionary._values.Count; }
			}

			/// <inheritdoc/>
			bool ICollection<TValue>.IsReadOnly {
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
			/// Gets value of entry at a specific index in the ordered dictionary.
			/// </summary>
			/// <param name="index">Zero-based index of entry.</param>
			/// <returns>
			/// The <see cref="TValue"/>.
			/// </returns>
			/// <exception cref="System.ArgumentOutOfRangeException">
			/// If <paramref name="index"/> is out of range of the collection.
			/// </exception>
			public TValue this[int index] {
				get {
					_dictionary.CheckIndexArgument(index);
					return _dictionary._values[index];
				}
			}

			/// <inheritdoc/>
			TValue IList<TValue>.this[int index] {
				get { return this[index]; }
				set { ThrowReadOnlyException(); }
			}

			/// <inheritdoc/>
			public bool Contains(TValue item) {
				return _dictionary._values.Contains(item);
			}

			/// <inheritdoc/>
			void ICollection.CopyTo(Array array, int index) {
				(_dictionary._values as ICollection).CopyTo(array, index);
			}

			/// <inheritdoc/>
			public void CopyTo(TValue[] array, int arrayIndex) {
				_dictionary._values.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Gets an object for enumerating over the ordered collection of values.
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
			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() {
				return GetEnumerator();
			}

			/// <inheritdoc/>
			void ICollection<TValue>.Add(TValue item) {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			bool ICollection<TValue>.Remove(TValue item) {
				ThrowReadOnlyException();
				return false;
			}

			/// <inheritdoc/>
			void ICollection<TValue>.Clear() {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			public int IndexOf(TValue item) {
				return _dictionary._values.IndexOf(item);
			}

			/// <inheritdoc/>
			void IList<TValue>.Insert(int index, TValue item) {
				ThrowReadOnlyException();
			}

			/// <inheritdoc/>
			void IList<TValue>.RemoveAt(int index) {
				ThrowReadOnlyException();
			}

			/// <summary>
			/// Enumerator for enumerating through the values of an ordered dictionary.
			/// </summary>
			public struct Enumerator : IEnumerator<TValue> {

				private readonly OrderedDictionaryAsset<TKey, TValue> _dictionary;
				private readonly int _version;

				private int _index;
				private TValue _current;

				/// <summary>
				/// Initializes a new instance of the <see cref="OrderedDictionaryAsset{TKey, TValue}.KeyCollection.Enumerator"/> structure.
				/// </summary>
				/// <param name="dictionary">The associated dictionary.</param>
				public Enumerator(OrderedDictionaryAsset<TKey, TValue> dictionary) {
					_dictionary = dictionary;
					_version = dictionary._version;

					_index = 0;
					_current = default(TValue);
				}

				/// <inheritdoc/>
				void IDisposable.Dispose() {
				}

				/// <inheritdoc/>
				public TValue Current {
					get { return _current; }
				}

				/// <inheritdoc/>
				object IEnumerator.Current {
					get { return _current; }
				}

				/// <inheritdoc/>
				public bool MoveNext() {
					_dictionary.CheckVersion(_version);

					if (_index < _dictionary._values.Count) {
						_current = _dictionary._values[_index];
						++_index;
						return true;
					}
					return false;
				}

				/// <inheritdoc/>
				public void Reset() {
					_dictionary.CheckVersion(_version);

					_current = default(TValue);
					_index = 0;
				}
			}

		}

	}

}
