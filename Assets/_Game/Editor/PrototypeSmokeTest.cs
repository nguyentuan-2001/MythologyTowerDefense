using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using MythologyTowerDefense.Core;
using MythologyTowerDefense.Gameplay.Economy;
using MythologyTowerDefense.Gameplay.Enemies;
using MythologyTowerDefense.Gameplay.Path;
using MythologyTowerDefense.Gameplay.Towers;
using MythologyTowerDefense.Gameplay.Waves;

namespace MythologyTowerDefense.EditorTools
{
    /// <summary>
    /// Headless Edit-Mode logic check. Exercises GameManager/CurrencyManager/BaseHealth/
    /// WaveManager/EnemyController/TowerController/TowerPad wiring without requiring Play Mode.
    /// Coroutine-driven wave timing (spawn cadence, wave completion wait) is NOT exercised here
    /// and must be verified manually in Play Mode.
    /// </summary>
    public static class PrototypeSmokeTest
    {
        [MenuItem("Tools/Mythology TD/Run Headless Smoke Test")]
        public static void RunSmokeTest()
        {
            bool pass = true;
            string failReason = string.Empty;

            try
            {
                PrototypeSceneBuilder.BuildScene();

                GameObject currencyGO = GameObject.Find("TDRoot/Systems/CurrencyManager");
                GameObject baseHealthGO = GameObject.Find("TDRoot/Systems/BaseHealth");
                GameObject waveManagerGO = GameObject.Find("TDRoot/Systems/WaveManager");
                GameObject gameManagerGO = GameObject.Find("TDRoot/Systems/GameManager");
                GameObject pathGO = GameObject.Find("TDRoot/Path");

                if (currencyGO == null || baseHealthGO == null || waveManagerGO == null || gameManagerGO == null || pathGO == null)
                {
                    throw new Exception("One or more core system GameObjects were not found after build.");
                }

                currencyGO.SendMessage("Awake");
                currencyGO.SendMessage("Start");
                baseHealthGO.SendMessage("Awake");
                baseHealthGO.SendMessage("Start");
                gameManagerGO.SendMessage("OnEnable");

                CurrencyManager currency = currencyGO.GetComponent<CurrencyManager>();
                BaseHealth baseHealth = baseHealthGO.GetComponent<BaseHealth>();
                WaveManager waveManager = waveManagerGO.GetComponent<WaveManager>();
                GameManager gameManager = gameManagerGO.GetComponent<GameManager>();
                PathDefinition path = pathGO.GetComponent<PathDefinition>();

                if (currency.Currency != 60)
                {
                    throw new Exception($"Expected starting currency 60, got {currency.Currency}.");
                }

                if (baseHealth.Health != 10)
                {
                    throw new Exception($"Expected starting base health 10, got {baseHealth.Health}.");
                }

                if (path.WaypointCount != 7)
                {
                    throw new Exception($"Expected 7 waypoints, got {path.WaypointCount}.");
                }

                // --- Enemy defeated -> reward granted exactly once, unregistered ---
                waveManagerGO.SendMessage("SpawnEnemy");

                if (waveManager.ActiveEnemies.Count != 1)
                {
                    throw new Exception($"Expected 1 active enemy after spawn, found {waveManager.ActiveEnemies.Count}.");
                }

                EnemyController enemy = waveManager.ActiveEnemies[0];
                int currencyBeforeKill = currency.Currency;
                enemy.TakeDamage(9999);

                if (currency.Currency != currencyBeforeKill + 8)
                {
                    throw new Exception($"Expected currency to increase by 8 on kill, went from {currencyBeforeKill} to {currency.Currency}.");
                }

                if (waveManager.ActiveEnemies.Count != 0)
                {
                    throw new Exception("Enemy was not unregistered from WaveManager after death.");
                }

                // --- Enemy reaches base -> base takes damage exactly once, unregistered ---
                waveManagerGO.SendMessage("SpawnEnemy");
                EnemyController arrivingEnemy = waveManager.ActiveEnemies[0];

                FieldInfo waypointIndexField = typeof(EnemyController).GetField("waypointIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                waypointIndexField.SetValue(arrivingEnemy, path.WaypointCount);

                int healthBeforeArrival = baseHealth.Health;
                arrivingEnemy.gameObject.SendMessage("Update");

                if (baseHealth.Health != healthBeforeArrival - 1)
                {
                    throw new Exception($"Expected base health to drop by 1 on enemy arrival, went from {healthBeforeArrival} to {baseHealth.Health}.");
                }

                if (waveManager.ActiveEnemies.Count != 0)
                {
                    throw new Exception("Enemy was not unregistered from WaveManager after reaching base.");
                }

                // --- Tower placement via pad: cost, occupancy, duplicate-block ---
                GameObject archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Towers/CelestialArcher.prefab");
                GameObject panguPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Towers/PanguTitan.prefab");
                if (archerPrefab == null || panguPrefab == null)
                {
                    throw new Exception("CelestialArcher.prefab or PanguTitan.prefab not found.");
                }

                GameObject padGO = GameObject.Find("TDRoot/TowerPads/TowerPad_00");
                if (padGO == null)
                {
                    throw new Exception("TowerPad_00 not found.");
                }

                TowerPad pad = padGO.GetComponent<TowerPad>();
                int currencyBeforePlacement = currency.Currency;
                bool placed = pad.TryPlaceTower(archerPrefab, 30);

                if (!placed)
                {
                    throw new Exception("Failed to place a tower on an unoccupied, affordable pad.");
                }

                if (currency.Currency != currencyBeforePlacement - 30)
                {
                    throw new Exception($"Expected currency to drop by 30 (tower cost), went from {currencyBeforePlacement} to {currency.Currency}.");
                }

                if (!pad.IsOccupied)
                {
                    throw new Exception("Pad did not mark itself occupied after placement.");
                }

                if (pad.TryPlaceTower(archerPrefab, 30))
                {
                    throw new Exception("Pad allowed placing a second tower on an already-occupied pad.");
                }

                // --- Tower targeting + firing ---
                TowerController placedTower = pad.PlacedTower;
                if (placedTower == null)
                {
                    throw new Exception("Placed tower component not found.");
                }

                // Position the test enemy at the actual midpoint of the path segment TowerPad_00
                // guards (not an arbitrary point next to the tower). This exercises the real
                // pad-offset-vs-tower-range geometry instead of an artificial shortcut — a prior
                // version of this test placed the enemy right next to the tower and missed a real
                // bug where the pad offset (5.5) was larger than the tower range (4.5), so towers
                // never actually reached the road.
                waveManagerGO.SendMessage("SpawnEnemy");
                EnemyController targetEnemy = waveManager.ActiveEnemies[0];
                Vector3 segmentMidpoint = Vector3.Lerp(path.GetWaypoint(0), path.GetWaypoint(1), 0.5f);
                targetEnemy.transform.position = segmentMidpoint;

                int projectilesBefore = UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsInactive.Exclude).Length;
                placedTower.gameObject.SendMessage("Update");
                int projectilesAfter = UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsInactive.Exclude).Length;

                if (projectilesAfter <= projectilesBefore)
                {
                    throw new Exception("Tower on TowerPad_00 did not fire at an enemy standing on the path segment it guards (pad-offset-vs-range geometry bug).");
                }

                // --- Pangu Titan splash damage: one shot should hit both nearby enemies ---
                GameObject padGO1 = GameObject.Find("TDRoot/TowerPads/TowerPad_01");
                if (padGO1 == null)
                {
                    throw new Exception("TowerPad_01 not found.");
                }

                // Earlier steps already spent currency on the Archer purchase; top up so this
                // section tests Pangu's splash mechanic, not an incidental affordability edge case.
                currency.Add(50);

                TowerPad pad1 = padGO1.GetComponent<TowerPad>();
                if (!pad1.TryPlaceTower(panguPrefab, 50))
                {
                    throw new Exception("Failed to place Pangu Titan on TowerPad_01.");
                }

                TowerController panguTower = pad1.PlacedTower;
                if (panguTower == null)
                {
                    throw new Exception("Placed Pangu Titan component not found.");
                }

                Vector3 segmentMidpoint1 = Vector3.Lerp(path.GetWaypoint(1), path.GetWaypoint(2), 0.5f);

                // Index by "most recently spawned" (Count - 1), not a fixed index — earlier tests
                // leave their enemies alive and registered (projectiles never actually tick their
                // Update in this headless test, so they never land), so the list isn't empty here.
                waveManagerGO.SendMessage("SpawnEnemy");
                EnemyController splashTargetA = waveManager.ActiveEnemies[waveManager.ActiveEnemies.Count - 1];
                splashTargetA.transform.position = segmentMidpoint1;

                waveManagerGO.SendMessage("SpawnEnemy");
                EnemyController splashTargetB = waveManager.ActiveEnemies[waveManager.ActiveEnemies.Count - 1];
                splashTargetB.transform.position = segmentMidpoint1 + Vector3.forward * 1.0f;

                FieldInfo currentHealthField = typeof(EnemyController).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);
                int healthABefore = (int)currentHealthField.GetValue(splashTargetA);
                int healthBBefore = (int)currentHealthField.GetValue(splashTargetB);

                // A projectile's own Update() never ticks here (edit mode doesn't advance
                // Time.deltaTime across SendMessage calls), so it would never actually "arrive".
                // Fire the tower, find the freshly spawned projectile, then teleport it onto the
                // target and tick its Update once to force Impact() deterministically.
                ProjectileController[] projectilesBeforePangu = UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsInactive.Exclude);
                panguTower.gameObject.SendMessage("Update");
                ProjectileController[] projectilesAfterPangu = UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsInactive.Exclude);

                ProjectileController newProjectile = null;
                foreach (ProjectileController candidate in projectilesAfterPangu)
                {
                    bool existedBefore = false;
                    foreach (ProjectileController old in projectilesBeforePangu)
                    {
                        if (old == candidate)
                        {
                            existedBefore = true;
                            break;
                        }
                    }

                    if (!existedBefore)
                    {
                        newProjectile = candidate;
                        break;
                    }
                }

                if (newProjectile == null)
                {
                    throw new Exception("Pangu Titan did not fire at enemies within range.");
                }

                // Teleport onto whichever enemy the tower actually locked (not an assumed fixed
                // point) so this test doesn't depend on tie-break specifics between A and B.
                FieldInfo projTargetField = typeof(ProjectileController).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
                EnemyController lockedTarget = (EnemyController)projTargetField.GetValue(newProjectile);
                if (lockedTarget == null)
                {
                    throw new Exception("Pangu Titan's projectile has no locked target.");
                }

                newProjectile.transform.position = lockedTarget.transform.position;
                newProjectile.gameObject.SendMessage("Update");

                int healthAAfter = (int)currentHealthField.GetValue(splashTargetA);
                int healthBAfter = (int)currentHealthField.GetValue(splashTargetB);

                if (healthAAfter >= healthABefore || healthBAfter >= healthBBefore)
                {
                    throw new Exception($"Expected Pangu Titan's splash to damage both nearby enemies. A: {healthABefore}->{healthAAfter}, B: {healthBBefore}->{healthBAfter}.");
                }

                // --- Lose state ---
                baseHealth.TakeDamage(baseHealth.Health);

                if (gameManager.State != GameState.Lost)
                {
                    throw new Exception($"Expected GameState.Lost after base health reached zero, got {gameManager.State}.");
                }

                // --- Win state (direct check; wave-coroutine timing is not exercised here) ---
                GameObject winCheckGO = GameObject.Find("TDRoot/Systems/GameManager");
                MethodInfo handleAllWavesCompleted = typeof(GameManager).GetMethod("HandleAllWavesCompleted", BindingFlags.NonPublic | BindingFlags.Instance);
                // GameManager is already Lost; re-create a fresh one to verify the Won transition in isolation.
                GameObject freshGmGO = new GameObject("TempGameManagerForWinCheck");
                GameManager freshGm = freshGmGO.AddComponent<GameManager>();
                handleAllWavesCompleted.Invoke(freshGm, null);

                if (freshGm.State != GameState.Won)
                {
                    throw new Exception($"Expected GameState.Won after HandleAllWavesCompleted, got {freshGm.State}.");
                }

                UnityEngine.Object.DestroyImmediate(freshGmGO);
            }
            catch (Exception ex)
            {
                pass = false;
                failReason = ex.Message;
            }
            finally
            {
                PrototypeSceneBuilder.BuildScene();
            }

            if (pass)
            {
                Debug.Log("SMOKE_TEST_RESULT: PASS - GameManager/CurrencyManager/BaseHealth/WaveManager/EnemyController/TowerController/TowerPad logic verified headlessly.");
            }
            else
            {
                Debug.LogError("SMOKE_TEST_RESULT: FAIL - " + failReason);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }
        }
    }
}
