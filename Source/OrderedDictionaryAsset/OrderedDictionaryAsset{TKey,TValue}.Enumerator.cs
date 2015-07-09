// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Rotorz.Extras.Collections {

	public partial class OrderedDictionaryAsset<TKey, TValue> {

		/// <summary>
		/// Enumerator for enumerating through the key/value pairs of an ordered dictionary.
		/// </summary>
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator {

			private readonly OrderedDictionaryAsset<TKey, TValue> _dictionary;
			private readonly int _version;
			private readonly bool _returnDictionaryEntry;

			private int _index;
			private KeyValuePair<TKey, TValue> _current;

			/// <summary>
			/// Initializes a new instance of the <see cref="OrderedDictionaryAsset{TKey, TValue}.Enumerator"/> structure.
			/// </summary>
			/// <param name="dictionary">The associated dictionary.</param>
			public Enumerator(OrderedDictionaryAsset<TKey, TValue> dictionary, bool returnDictionaryEntry) {
				_dictionary = dictionary;
				_version = dictionary._version;
				_returnDictionaryEntry = returnDictionaryEntry;

                _index = 0;
				_current = default(KeyValuePair<TKey, TValue>);
            }

			/// <inheritdoc/>
			void IDisposable.Dispose() {
			}

			/// <inheritdoc/>
			public KeyValuePair<TKey, TValue> Current {
				get { return _current; }
			}

			/// <inheritdoc/>
			object IEnumerator.Current {
				get {
					if (_returnDictionaryEntry)
						return new DictionaryEntry(_current.Key, _current.Value);
					else
						return _current;
				}
			}

			/// <summary>
			/// Gets the key of the current entry.
			/// </summary>
			public TKey Key {
				get { return _current.Key; }
			}

			/// <summary>
			/// Gets the value of the current entry.
			/// </summary>
			public TValue Value {
				get { return _current.Value; }
			}

			/// <inheritdoc/>
			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return new DictionaryEntry(_current.Key, _current.Value); }
			}

			/// <inheritdoc/>
			object IDictionaryEnumerator.Key {
				get { return _current.Key; }
			}

			/// <inheritdoc/>
			object IDictionaryEnumerator.Value {
				get { return _current.Value; }
			}

			/// <inheritdoc/>
			public bool MoveNext() {
				_dictionary.CheckVersion(_version);

				if (_index < _dictionary.Count) {
					_current = new KeyValuePair<TKey, TValue>(_dictionary._keys[_index], _dictionary._values[_index]);
					++_index;
					return true;
				}

				_current = default(KeyValuePair<TKey, TValue>);
				return false;
			}

			/// <inheritdoc/>
			void IEnumerator.Reset() {
				_dictionary.CheckVersion(_version);

				_index = 0;
				_current = default(KeyValuePair<TKey, TValue>);
			}
		}

	}

}
