using System;
using UnityEditor;
using UnityEngine;

public static class PrototypeSmokeTest
{
    [MenuItem("Tower Defense/Run Headless Smoke Test")]
    public static void RunSmokeTest()
    {
        bool pass = true;
        string failReason = string.Empty;

        try
        {
            PrototypeSceneBuilder.BuildScene();

            GameObject gmGO = GameObject.Find("TDRoot/Systems/GameManager");
            if (gmGO == null)
            {
                throw new Exception("GameManager GameObject not found after build.");
            }

            GameObject pathGO = GameObject.Find("TDRoot/Path");
            if (pathGO == null)
            {
                throw new Exception("Path GameObject not found after build.");
            }

            pathGO.SendMessage("Awake");
            gmGO.SendMessage("Awake");
            gmGO.SendMessage("Start");

            GameManager gm = gmGO.GetComponent<GameManager>();
            if (gm == null)
            {
                throw new Exception("GameManager component missing.");
            }

            if (gm.Currency <= 0)
            {
                throw new Exception("Starting currency was not initialized.");
            }

            if (gm.BaseHealth <= 0)
            {
                throw new Exception("Starting base health was not initialized.");
            }

            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            if (enemyPrefab == null)
            {
                throw new Exception("Enemy.prefab not found.");
            }

            GameObject enemyInstance = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            enemyInstance.transform.position = new Vector3(-22f, 1f, -16f);
            enemyInstance.SendMessage("Start");

            Enemy enemyComp = enemyInstance.GetComponent<Enemy>();
            int currencyBeforeKill = gm.Currency;
            int countBeforeKill = gm.ActiveEnemies.Count;

            if (countBeforeKill != 1)
            {
                throw new Exception($"Expected 1 registered enemy after spawn, found {countBeforeKill}.");
            }

            enemyComp.TakeDamage(9999);

            if (gm.Currency <= currencyBeforeKill)
            {
                throw new Exception("Currency did not increase after enemy death.");
            }

            if (gm.ActiveEnemies.Count != 0)
            {
                throw new Exception("Enemy was not unregistered from GameManager after death.");
            }

            UnityEngine.Object.DestroyImmediate(enemyInstance);

            int healthBefore = gm.BaseHealth;
            gm.DamageBase(healthBefore);

            if (!gm.IsGameOver)
            {
                throw new Exception("GameOver was not triggered when base health reached zero.");
            }

            if (gm.BaseHealth != 0)
            {
                throw new Exception("BaseHealth did not clamp to zero.");
            }

            GameObject padGO = GameObject.Find("TDRoot/TowerPads/TowerPad_00");
            if (padGO == null)
            {
                throw new Exception("TowerPad_00 not found.");
            }

            TowerPlacementPad pad = padGO.GetComponent<TowerPlacementPad>();
            int currencyBeforePlacement = gm.Currency;
            bool placed = pad.TryPlaceTower();

            if (!placed)
            {
                throw new Exception("Failed to place a tower on an unoccupied, affordable pad.");
            }

            if (gm.Currency >= currencyBeforePlacement)
            {
                throw new Exception("Currency was not spent when placing a tower.");
            }

            if (!pad.IsOccupied)
            {
                throw new Exception("Pad did not mark itself occupied after placement.");
            }

            if (pad.TryPlaceTower())
            {
                throw new Exception("Pad allowed placing a second tower on an already-occupied pad.");
            }

            Tower placedTower = UnityEngine.Object.FindAnyObjectByType<Tower>();
            if (placedTower == null)
            {
                throw new Exception("Placed tower component not found in scene.");
            }

            GameObject targetEnemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            targetEnemy.transform.position = placedTower.transform.position + Vector3.forward * 2f;
            targetEnemy.SendMessage("Start");

            int projectilesBefore = UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude).Length;
            placedTower.SendMessage("Update");
            int projectilesAfter = UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude).Length;

            if (projectilesAfter <= projectilesBefore)
            {
                throw new Exception("Tower did not fire a projectile at an enemy within range.");
            }

            UnityEngine.Object.DestroyImmediate(targetEnemy);
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
            Debug.Log("SMOKE_TEST_RESULT: PASS - GameManager/Enemy/Tower/Projectile/TowerPlacementPad logic verified headlessly.");
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
