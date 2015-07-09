// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Extras.Collections {

	/// <summary>
	/// Inspector that is assumed by default for all <see cref="OrderedDictionaryAsset"/>
	/// subclasses but can be subclassed to override its behavior.
	/// </summary>
	/// <see cref="OrderedDictionaryAssetListAdaptor"/>
	[CustomEditor(typeof(OrderedDictionaryAsset), editorForChildClasses: true)]
	public class OrderedDictionaryAssetInspector : Editor {

		protected const string DefaultDictionaryTitleText = "Entries";

		protected SerializedProperty KeysProperty { get; private set; }
		protected SerializedProperty ValuesProperty { get; private set; }

		#region Messages and Event Handlers

		protected virtual void OnEnable() {
			InitializeNewInputInstance();

			KeysProperty = serializedObject.FindProperty("_keys");
			ValuesProperty = serializedObject.FindProperty("_values");

			InitializeListControl();
        }

		protected virtual void OnDisable() {
			CleanupNewInputInstance();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			DrawOtherPropertiesGUI();
			DrawDictionaryGUI();

			serializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region Dictionary List Control

		protected ReorderableListControl ListControl { get; private set; }
		protected OrderedDictionaryAssetListAdaptor ListAdaptor { get; private set; }

		private void InitializeListControl() {
			ListControl = CreateListControl();
			ListAdaptor = CreateListAdaptor((OrderedDictionaryAsset)target);
        }

		/// <summary>
		/// Creates the <see cref="ReorderableListControl"/> that will be used to draw
		/// and manipulate the list of ordered dictionary entries.
		/// </summary>
		/// <returns>
		/// The new <see cref="ReorderableListControl"/> instance.
		/// </returns>
		protected virtual ReorderableListControl CreateListControl() {
			var flags =
				  ReorderableListFlags.DisableDuplicateCommand
				| ReorderableListFlags.HideAddButton
				;
			return new ReorderableListControl(flags);
		}

		/// <summary>
		/// Creates a <see cref="OrderedDictionaryAssetListAdaptor"/> that will be used
		/// to draw and manipulate the list of ordered dictionary entries.
		/// </summary>
		/// <param name="target">The target ordered dictionary asset.</param>
		/// <returns>
		/// The new <see cref="OrderedDictionaryAssetListAdaptor"/> instance.
		/// </returns>
		protected virtual OrderedDictionaryAssetListAdaptor CreateListAdaptor(OrderedDictionaryAsset target) {
			var keysAdaptor = new SerializedPropertyAdaptor(KeysProperty);
			var valuesAdaptor = new SerializedPropertyAdaptor(ValuesProperty);
			return new OrderedDictionaryAssetListAdaptor(target, keysAdaptor, valuesAdaptor);
		}

		#endregion

		#region New Input Data
		
		private OrderedDictionaryAsset _newInput;

		private SerializedObject _newInputSerializedObject;
		private SerializedProperty _newInputKeysProperty;
		private SerializedProperty _newInputValuesProperty;

		/// <summary>
		/// Gets the <see cref="SerializedObject"/> that represents the new input.
		/// </summary>
		protected SerializedObject NewInputSerializedObject {
			get { return _newInputSerializedObject; }
		}

		/// <summary>
		/// Gets the <see cref="SerializedProperty"/> that represents the new input key.
		/// </summary>
		protected SerializedProperty NewInputKeyProperty {
			get { return _newInputKeysProperty.GetArrayElementAtIndex(0); }
		}

		/// <summary>
		/// Gets the <see cref="SerializedProperty"/> that represents the new input value.
		/// </summary>
		protected SerializedProperty NewInputValueProperty {
			get { return _newInputValuesProperty.GetArrayElementAtIndex(0); }
		}

		/// <summary>
		/// Gets the <see cref="OrderedDictionaryAssetListAdaptor"/> that is used to draw
		/// input controls allowing the user to insert a new entry into the ordered
		/// dictionary.
		/// </summary>
		protected OrderedDictionaryAssetListAdaptor NewInputListAdaptor { get; private set; }

		private void InitializeNewInputInstance() {
			_newInput = (OrderedDictionaryAsset)CreateInstance(target.GetType());
			_newInput.hideFlags = HideFlags.DontSave;
			_newInput._suppressErrors = true;

			_newInputSerializedObject = new SerializedObject(_newInput);
			_newInputKeysProperty = _newInputSerializedObject.FindProperty("_keys");
			_newInputValuesProperty = _newInputSerializedObject.FindProperty("_values");

			// Ensure that new input has exactly one element in its array.
			if (_newInputKeysProperty.arraySize != 1) {
				_newInputSerializedObject.Update();
				_newInputKeysProperty.arraySize = 1;
				_newInputValuesProperty.arraySize = 1;

				var applyModifiedPropertiesWithoutUndoMethod = typeof(SerializedObject).GetMethod("ApplyModifiedPropertiesWithoutUndo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (applyModifiedPropertiesWithoutUndoMethod == null) {
					// Fail-safe!
					applyModifiedPropertiesWithoutUndoMethod = typeof(SerializedObject).GetMethod("ApplyModifiedProperties", BindingFlags.Instance | BindingFlags.Public);
				}
				applyModifiedPropertiesWithoutUndoMethod.Invoke(_newInputSerializedObject, null);
			}

			NewInputListAdaptor = CreateNewItemListAdaptor(_newInput);
		}

		/// <summary>
		/// Creates a <see cref="OrderedDictionaryAssetListAdaptor"/> which will be used
		/// to draw input controls allowing the user to insert a new entry into the
		/// ordered dictionary.
		/// </summary>
		/// <param name="target">The target ordered dictionary that contains exactly one
		/// element representing the new input.</param>
		/// <returns>
		/// The new <see cref="OrderedDictionaryAssetListAdaptor"/> instance.
		/// </returns>
		protected virtual OrderedDictionaryAssetListAdaptor CreateNewItemListAdaptor(OrderedDictionaryAsset target) {
			var keysAdaptor = new SerializedPropertyAdaptor(_newInputKeysProperty);
			var valuesAdaptor = new SerializedPropertyAdaptor(_newInputValuesProperty);
			return new OrderedDictionaryAssetListAdaptor(target, keysAdaptor, valuesAdaptor);
		}

		private void CleanupNewInputInstance() {
			if (_newInput != null) {
				DestroyImmediate(_newInput);
				_newInput = null;
			}
			else if (!ReferenceEquals(_newInput, null)) {
				_newInput = null;
			}
		}

		/// <summary>
		/// Resets state of the new input controls.
		/// </summary>
		protected void ResetNewInput() {
			NewInputSerializedObject.Update();
			SerializedPropertyUtility.ResetValue(NewInputKeyProperty);
			SerializedPropertyUtility.ResetValue(NewInputValueProperty);
			NewInputSerializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region Dictionary GUI

		private static GUIStyle s_AddButtonStyle;
		
		private static GUIStyle AddButtonStyle {
			get {
				if (s_AddButtonStyle == null) {
					s_AddButtonStyle = new GUIStyle(ReorderableListStyles.FooterButton2);
					s_AddButtonStyle.fixedHeight = 0f;
                }
				return s_AddButtonStyle;
			}
		}

		[SerializeField]
		private string _dictionaryTitleText = DefaultDictionaryTitleText;

		/// <summary>
		/// Gets or sets the title text of the dictionary's <see cref="ReorderableListControl"/>.
		/// </summary>
		protected string DictionaryTitleText {
			get { return _dictionaryTitleText; }
			set { _dictionaryTitleText = value; }
        }

		/// <summary>
		/// Draws the dictionary GUI.
		/// </summary>
		protected virtual void DrawDictionaryGUI() {
			DrawDictionaryTitle();
			DrawDictionaryListControl();
			DrawDictionaryNewInput();
			GUILayout.Space(5);
			DrawDictionaryErrors();
		}

		/// <summary>
		/// Draws the title of the dictionary's <see cref="ReorderableListControl"/>.
		/// </summary>
		protected virtual void DrawDictionaryTitle() {
			ReorderableListGUI.Title(DictionaryTitleText);
		}

		/// <summary>
		/// Draws the <see cref="ReorderableListControl"/> using the <see cref="ListAdaptor"/>.
		/// </summary>
		protected virtual void DrawDictionaryListControl() {
			ListControl.Draw(ListAdaptor);
		}

		private const float AddButtonWidth = 32f;
		private const float VerticalPadding = 8f;
		private const float HalfVerticalPadding = VerticalPadding / 2f;

		/// <summary>
		/// Draws new input controls.
		/// </summary>
		protected virtual void DrawDictionaryNewInput() {
			NewInputSerializedObject.Update();

			var adaptor = NewInputListAdaptor;
			
			// Inclusive of the new input controls and the add button.
			Rect totalPosition = GUILayoutUtility.GetRect(0, adaptor.GetItemHeight(0) + VerticalPadding);

			// Intercept context click before sub-controls have a chance to avoid
			// revealing undesirable commands such as item insertion/removal.
			if (Event.current.type == EventType.ContextClick && totalPosition.Contains(Event.current.mousePosition)) {
				OnNewInputContextClick();
				Event.current.Use();
			}

			// Background behind input controls excluding add button.
			Rect backgroundPosition = totalPosition;
            backgroundPosition.width -= AddButtonWidth + 5f;
            if (Event.current.type == EventType.Repaint)
				ReorderableListStyles.Container.Draw(backgroundPosition, GUIContent.none, false, false, false, false);

			Rect newInputPosition = totalPosition;
			newInputPosition.xMin += 24f;
			newInputPosition.x -= 12f;
			newInputPosition.y += HalfVerticalPadding;
            newInputPosition.width -= AddButtonWidth;
			newInputPosition.height -= VerticalPadding;

			adaptor.BeginGUI();
			adaptor.DrawItemBackground(newInputPosition, 0);
			adaptor.DrawItem(newInputPosition, 0);
			adaptor.EndGUI();

			Rect addButtonPosition = totalPosition;
			addButtonPosition.xMin = addButtonPosition.xMax - AddButtonWidth;
			addButtonPosition.xMax -= 1f;
			DrawAddNewInputButton(addButtonPosition);

			NewInputSerializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Draws button for adding new input.
		/// </summary>
		/// <param name="position">Absolute position of button in GUI.</param>
		protected void DrawAddNewInputButton(Rect position) {
			EditorGUI.BeginDisabledGroup(!CanAddNewInput);

			var addButtonNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal);
			var addButtonActive = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Active);
			if (GUIHelper.IconButton(position, addButtonNormal, addButtonActive, AddButtonStyle))
				OnAddNewInputButtonClick();

			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Gets a value indicating whether the user can click the add new input button.
		/// </summary>
		protected virtual bool CanAddNewInput {
			get {
				var dictionary = (OrderedDictionaryAsset)target;
				var newKeyValue = _newInput.GetKeyFromIndex(0);

				bool isNullKey = newKeyValue == null;
				bool dictionaryAlreadyContainsKey = newKeyValue != null && dictionary.ContainsKey(newKeyValue);

				return !dictionaryAlreadyContainsKey && !isNullKey;
            }
		}

		/// <summary>
		/// Occurs when the add new input button is clicked.
		/// </summary>
		protected virtual void OnAddNewInputButtonClick() {
			ListAdaptor.Add(NewInputKeyProperty, NewInputValueProperty);
			ResetNewInput();
		}

		/// <summary>
		/// Occurrs when the user context clicks on the new input controls.
		/// </summary>
		/// <remarks>
		/// <para>Intercepts context click before sub-controls have a chance to display
		/// a context menu to avoid exposing undesirable commands which could otherwise
		/// cause corruption by allowing individual keys or values to be removed.</para>
		/// </remarks>
		protected virtual void OnNewInputContextClick() {
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Reset Input"), false, ResetNewInput);
			menu.ShowAsContext();
		}

		private bool _hasNullKeyErrorOnLayout;
		
		/// <summary>
		/// Draws error feedback relating to the ordered dictionary.
		/// </summary>
		protected virtual void DrawDictionaryErrors() {
			var dictionary = (OrderedDictionaryAsset)target;
			if (dictionary.KeysWithDuplicateValues.Any())
				EditorGUILayout.HelpBox("Multiple values have been assigned to the same key.", MessageType.Error);

			if (Event.current.type == EventType.Layout)
				_hasNullKeyErrorOnLayout = ListAdaptor.HadNullKeyErrorOnLastRepaint;
			if (_hasNullKeyErrorOnLayout)
                EditorGUILayout.HelpBox("One or more null keys were encountered.", MessageType.Error);
		}

		#endregion

		#region Other Properties GUI

		private static readonly HashSet<string> s_ExcludePropertyNames = new HashSet<string>() {
			"m_Script",
			"_version",
			"_keys",
			"_values"
		};

		/// <summary>
		/// The default implementation draws the GUI for each of the visible serialized
		/// properties in the target <see cref="OrderedDictionaryAsset"/>.
		/// </summary>
		/// <remarks>
		/// <para>Each property is drawn using its associated <see cref="PropertyDrawer"/>
		/// via the method <see cref="EditorGUILayout.PropertyField(SerializedProperty)"/>.</para>
		/// </remarks>
		protected virtual void DrawOtherPropertiesGUI() {
			var property = serializedObject.GetIterator();

			bool enterChildren = true;
			int visiblePropertyCount = 0;

			while (property.NextVisible(enterChildren)) {
				enterChildren = false;

				if (s_ExcludePropertyNames.Contains(property.name))
					continue;

				EditorGUILayout.PropertyField(property, true);
				++visiblePropertyCount;
            }

			if (visiblePropertyCount != 0)
				EditorGUILayout.Space();
		}
		
		#endregion

	}

}
