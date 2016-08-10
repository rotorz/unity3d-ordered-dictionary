// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Extras.Collections {

	/// <summary>
	/// Ordered dictionary asset adaptor for a <see cref="ReorderableListControl"/> can
	/// be subclassed to override its behavior.
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="OrderedDictionaryAssetInspector"/> class can also be
	/// subclassed allowing you to initialize a custom <see cref="OrderedDictionaryAssetListAdaptor"/>
	/// subclass to be instantiated.</para>
	/// </remarks>
	public class OrderedDictionaryAssetListAdaptor : IReorderableListAdaptor, IReorderableListDropTarget {

		protected readonly OrderedDictionaryAsset Target;
		protected readonly SerializedPropertyAdaptor KeysPropertyAdaptor;
		protected readonly SerializedPropertyAdaptor ValuesPropertyAdaptor;

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionaryAssetListAdaptor"/> class.
		/// </summary>
		/// <param name="target">The target object.</param>
		/// <param name="keysPropertyAdaptor">The adaptor for the ordered dictionary's keys.</param>
		/// <param name="valuesPropertyAdaptor">The adaptor for the ordered dictionary's values.</param>
		public OrderedDictionaryAssetListAdaptor(OrderedDictionaryAsset target, SerializedPropertyAdaptor keysPropertyAdaptor, SerializedPropertyAdaptor valuesPropertyAdaptor) {
			Target = target;
			KeysPropertyAdaptor = keysPropertyAdaptor;
			ValuesPropertyAdaptor = valuesPropertyAdaptor;
		}

		#region Manipulation

		/// <inheritdoc/>
		public virtual bool CanDrag(int index) {
			return KeysPropertyAdaptor.CanDrag(index) && ValuesPropertyAdaptor.CanDrag(index);
		}

		/// <inheritdoc/>
		public virtual bool CanRemove(int index) {
			return KeysPropertyAdaptor.CanRemove(index) && ValuesPropertyAdaptor.CanRemove(index);
		}

		/// <inheritdoc/>
		public int Count {
			get { return KeysPropertyAdaptor.Count; }
		}

		/// <inheritdoc/>
		public virtual void Clear() {
			KeysPropertyAdaptor.Clear();
			ValuesPropertyAdaptor.Clear();
		}

		/// <inheritdoc/>
		void IReorderableListAdaptor.Add() {
			throw new NotImplementedException();
		}

		public virtual void Add(SerializedProperty inputKeyProperty, SerializedProperty inputValueProperty) {
			if (KeysPropertyAdaptor.arrayProperty.arraySize != ValuesPropertyAdaptor.arrayProperty.arraySize)
				throw new InvalidOperationException("Cannot add entry because of inconsistent count of keys and values.");

			int count = KeysPropertyAdaptor.arrayProperty.arraySize + 1;
			KeysPropertyAdaptor.arrayProperty.arraySize = count;
			ValuesPropertyAdaptor.arrayProperty.arraySize = count;

			var addedKeyProperty = KeysPropertyAdaptor.arrayProperty.GetArrayElementAtIndex(count - 1);
			var addedValueProperty = ValuesPropertyAdaptor.arrayProperty.GetArrayElementAtIndex(count - 1);
			SerializedPropertyUtility.CopyPropertyValue(addedKeyProperty, inputKeyProperty);
			SerializedPropertyUtility.CopyPropertyValue(addedValueProperty, inputValueProperty);
		}

		/// <inheritdoc/>
		void IReorderableListAdaptor.Duplicate(int index) {
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		void IReorderableListAdaptor.Insert(int index) {
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public virtual void Move(int sourceIndex, int destIndex) {
			KeysPropertyAdaptor.Move(sourceIndex, destIndex);
			ValuesPropertyAdaptor.Move(sourceIndex, destIndex);
		}

		/// <inheritdoc/>
		public virtual void Remove(int index) {
			KeysPropertyAdaptor.Remove(index);
			ValuesPropertyAdaptor.Remove(index);
		}

		#endregion

		#region Drawing

		/// <summary>
		/// Indicates whether a null key error was encountered.
		/// </summary>
		public bool HadNullKeyErrorOnLastRepaint { get; private set; }

		/// <inheritdoc/>
		public virtual float GetItemHeight(int index) {
			float keyHeight = KeysPropertyAdaptor.GetItemHeight(index);
			float valueHeight = ValuesPropertyAdaptor.GetItemHeight(index);
			return Mathf.Max(keyHeight, valueHeight);
		}

		/// <inheritdoc/>
		public virtual void BeginGUI() {
			if (Event.current.type == EventType.Repaint)
				HadNullKeyErrorOnLastRepaint = false;
		}

		/// <inheritdoc/>
		public virtual void EndGUI() {
		}

		/// <inheritdoc/>
		public virtual void DrawItemBackground(Rect position, int index) {
		}

		/// <inheritdoc/>
		public virtual void DrawItem(Rect position, int index) {
			// Intercept context click before sub-controls have a chance to avoid
			// revealing undesirable commands such as item insertion/removal.
			if (Event.current.type == EventType.ContextClick && position.Contains(Event.current.mousePosition)) {
				OnContextClickItem(index);
				Event.current.Use();
			}

			Color restoreColor = GUI.color;
			if (!Target._suppressErrors) {
				var key = Target.GetKeyFromIndex(index);

				if (Target.KeysWithDuplicateValues.Contains(key))
					GUI.color = Color.red;

				if (key == null) {
					HadNullKeyErrorOnLastRepaint = true;
					GUI.color = new Color(1f, 0f, 1f);
				}
			}

			Rect keyPosition = position;
			keyPosition.width /= 3f;
			keyPosition.height = KeysPropertyAdaptor.GetItemHeight(index);
			KeysPropertyAdaptor.DrawItem(keyPosition, index);

			GUI.color = restoreColor;

			Rect valuePosition = position;
			valuePosition.xMin = keyPosition.xMax + 5f;
			valuePosition.height = ValuesPropertyAdaptor.GetItemHeight(index);
			ValuesPropertyAdaptor.DrawItem(valuePosition, index);
		}

		/// <summary>
		/// Occurs allowing a list item to respond to a context click.
		/// </summary>
		/// <param name="index">Zero-based index of the list item.</param>
		protected virtual void OnContextClickItem(int index) {
		}

		#endregion

		#region Drop Insertion

		private Rect DropTargetPosition {
			get {
				// Expand size of drop target slightly so that it is easier to drop.
				Rect dropPosition = ReorderableListGUI.CurrentListPosition;
				dropPosition.y -= 10;
				dropPosition.height += 15;
				return dropPosition;
			}
		}

		/// <inheritdoc/>
		public bool CanDropInsert(int insertionIndex) {
			if (!typeof(string).IsAssignableFrom(Target.KeyType))
				return false;
			if (!typeof(Object).IsAssignableFrom(Target.ValueType))
				return false;
			if (!DropTargetPosition.Contains(Event.current.mousePosition))
				return false;

			var valueType = Target.ValueType;

			foreach (var obj in DragAndDrop.objectReferences) {
				if (!EditorUtility.IsPersistent(obj))
					continue;

				if (valueType.IsAssignableFrom(obj.GetType())) {
					return true;
				}
				else {
					string assetPath = AssetDatabase.GetAssetPath(obj);
					if (AssetDatabase.LoadAllAssetsAtPath(assetPath).Any(o => valueType.IsAssignableFrom(o.GetType())))
						return true;
				}
			}

			return false;
		}

		/// <inheritdoc/>
		public void ProcessDropInsertion(int insertionIndex) {
			if (Event.current.type == EventType.DragPerform) {
				foreach (var objectReference in GetDraggedObjectReferences()) {
					if (Target.ContainsKey(objectReference.name))
						continue;
					InsertObjectReferenceEntry(insertionIndex++, objectReference);
				}
			}
		}

		private IEnumerable<Object> GetDraggedObjectReferences() {
			var objectReferences = new HashSet<Object>();
			var valueType = Target.ValueType;

			foreach (var obj in DragAndDrop.objectReferences) {
				if (!EditorUtility.IsPersistent(obj))
					continue;

				if (valueType.IsAssignableFrom(obj.GetType())) {
					objectReferences.Add(obj);
				}
				else {
					string assetPath = AssetDatabase.GetAssetPath(obj);
					objectReferences.UnionWith(AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(o => valueType.IsAssignableFrom(o.GetType())));
				}
			}

			return objectReferences.OrderBy(sprite => sprite.name);
		}

		private void InsertObjectReferenceEntry(int insertionIndex, Object objectReference) {
			KeysPropertyAdaptor.Insert(insertionIndex);
			ValuesPropertyAdaptor.Insert(insertionIndex);

			var keyProperty = KeysPropertyAdaptor.arrayProperty.GetArrayElementAtIndex(insertionIndex);
			var valueProperty = ValuesPropertyAdaptor.arrayProperty.GetArrayElementAtIndex(insertionIndex);

			keyProperty.stringValue = objectReference.name;
			valueProperty.objectReferenceValue = objectReference;
		}

		#endregion

	}

}
