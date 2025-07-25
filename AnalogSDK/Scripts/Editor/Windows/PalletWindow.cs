using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System;
using UnityEditor.AddressableAssets;

namespace AnalogSDK.Editor
{
    public class PallletWindow : EditorWindow
    {
        public string palletTitle = "New Pallet";
        public string palletBarcode = "Barcode";
        public string palletAuthor = "Author";
        public string palletVersion = "1.0";
        public static Pallet selectedPallet;
        [MenuItem("AnalogSDK/Editor/Pallet/Editor")]
        public static void OpenWindow()
        {
            PallletWindow window = GetWindow<PallletWindow>("Pallet Editor");
            window.Show();
        }
        public void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Pallet Settings", EditorStyles.boldLabel);
            palletTitle = EditorGUILayout.TextField("Pallet Title", palletTitle);
            palletAuthor = EditorGUILayout.TextField("Author", palletAuthor);
            palletVersion = EditorGUILayout.TextField("Version", palletVersion);

            if (GUILayout.Button("Create Pallet"))
            {
                CreatePallet();
            }
        }

        public void CreatePallet()
        {
            string path = "Assets/SDK/pallets";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            palletBarcode = $"{palletTitle}.{palletAuthor}.{palletVersion}";

            Pallet newPallet = CreateInstance<Pallet>();
            newPallet.Title = palletTitle;
            newPallet.Barcode = palletBarcode;
            newPallet.Author = palletAuthor;
            newPallet.Version = palletVersion;

            AssetDatabase.CreateAsset(newPallet, path + "/" + palletTitle + ".pallet.asset");
            AddPalletToAddressables(newPallet);
            AssetDatabase.SaveAssets();
            selectedPallet = newPallet;
        }

        private void AddPalletToAddressables(Pallet newPallet)
        {
            //Use this object to manipulate addressables
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            string group_name = "pallets";
            string label_name = "pallet";
            string path_to_object = AssetDatabase.GetAssetPath(newPallet);
            string custom_address = newPallet.Barcode;

            //Create a group with no schemas
            //settings.CreateGroup(group_name, false, false, false, new List<AddressableAssetGroupSchema> { settings.DefaultGroup.Schemas[0] });
            //Create a group with the default schemas
            if (settings.FindGroup(group_name) == null) settings.CreateGroup(group_name, false, false, false, settings.DefaultGroup.Schemas);

            //Create a Label
            if(!settings.GetLabels().Contains(label_name)) settings.AddLabel(label_name, false);

            //Make a gameobject an addressable
            AddressableAssetGroup obj = settings.FindGroup(group_name);
            var guid = AssetDatabase.AssetPathToGUID(path_to_object);

            //This is the function that actually makes the object addressable
            var entry = settings.CreateOrMoveEntry(guid, obj);
            entry.labels.Add(label_name);
            entry.address = custom_address;

            //You'll need these to run to save the changes!
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
        }

        public void SelectPallet()
        {
            string path = "Assets/SDK/pallets";
            string[] palletGuids = AssetDatabase.FindAssets("t:Pallet", new[] { path });

            GenericMenu menu = new GenericMenu();

            foreach (string guid in palletGuids)
            {
                string palletPath = AssetDatabase.GUIDToAssetPath(guid);
                Pallet pallet = AssetDatabase.LoadAssetAtPath<Pallet>(palletPath);
                menu.AddItem(new GUIContent(pallet.Title), false, () => SelectPalletFromMenu(pallet));
            }

            menu.ShowAsContext();
        }

        public void SelectPalletFromMenu(Pallet pallet)
        {
            selectedPallet = pallet;
        }
    }
}