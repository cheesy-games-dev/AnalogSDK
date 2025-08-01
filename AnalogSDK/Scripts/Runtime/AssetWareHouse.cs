using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace AnalogSDK
{
    public class AssetWarehouse : MonoBehaviour
    {
        public const string SavedMeshesPath = "Assets/SDK/meshes";
        public static bool ready
        {
            get
            {
                bool ready = Instance != null && Instance.Pallets.Count > 0;
                return ready;
            }
        }
        public static AssetWarehouse Instance { get; private set; }
        public static Action OnReady;
        public static Action<Pallet> OnLoadedPallet;
        public static Action<Crate> OnLoadedCrate;
        public static AssetLabelReference palletLabel = new()
        {
            labelString = "pallet"
        };
        public List<Pallet> Pallets = new();
        public List<Crate> Crates = new();
        public List<SpawnableCrate> SpawnableCrates = new();
        public List<LevelCrate> LevelCrates = new();
        private void OnValidate()
        {
            Instance = this;
        }
        private void Start()
        {
            if (Instance)
            {
                Debug.LogWarning("An instance of AssetWareHouse already exists. Destroying the new instance.");
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(Instance);
            LoadMods();
        }

        public static Crate GetCrateByBarcode(string barcode)
        {
            Crate crate = Instance.Crates.FirstOrDefault(c => c.Barcode.ToLower() == barcode.ToLower());
            return crate;
        }

        public virtual void LoadMods()
        {
            Debug.Log("Loading Mods");
            Pallets.Clear();
            SpawnableCrates.Clear();
            LevelCrates.Clear();
            Addressables.LoadAssetsAsync<Pallet>(palletLabel, LoadPallet);
            if (ready) OnReady?.Invoke();
        }
        public virtual void LoadLevel(LevelCrate level, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(level.CrateReference.name, mode);
            Debug.Log($"Loading Level: {level.CrateReference.name}");
        }

        protected virtual void LoadPallet(Pallet pallet)
        {
            Debug.Log($"Loaded: {pallet}");
            Debug.Log($"Loaded: {pallet.Crates}");
            Pallets.Add(pallet);
            Crates.AddRange(pallet.Crates);
            try
            {
                SpawnableCrates.AddRange(pallet.Crates.Cast<SpawnableCrate>());
            }
            catch (Exception e)
            {
                Debug.Log($"No Spawnable Crates in {pallet}. Exception: {e.Message}");
            }
            finally
            {
                try
                {
                    LevelCrates.AddRange(pallet.Crates.Cast<LevelCrate>());
                }
                catch (Exception e)
                {
                    Debug.Log($"No Level Crates in {pallet}. Exception: {e.Message}");
                }
            }
            OnLoadedPallet?.Invoke(pallet);
            foreach (Crate crate in pallet.Crates)
            {
                OnLoadedCrate?.Invoke(crate);
            }
            Pallets.Sort();
            SpawnableCrates.Sort();
            LevelCrates.Sort();
        }
    }
}