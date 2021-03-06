using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Utility;
using System.Collections;
using UnityEngine;


namespace ThePenwickPapers
{


    public class Utility
    {


        /// <summary>
        /// Shows HUD text with specified linger time
        /// </summary>
        public static void AddHUDText(string msg)
        {
            //display time depends on length of text
            float delay = 0.8f + msg.Length * 0.03f;
            DaggerfallUI.AddHUDText(msg, delay);
        }


        /// <summary>
        /// Only shows HUD text after the delay time has passed
        /// </summary>
        public static void AddDelayedHUDText(string msg, float delay)
        {
            IEnumerator coroutine = DelayedHUDMsgCoroutine(msg, delay);
            ThePenwickPapersMod.Instance.StartCoroutine(coroutine);
        }

        static IEnumerator DelayedHUDMsgCoroutine(string msg, float delay)
        {
            yield return new WaitForSeconds(delay);
            AddHUDText(msg);
        }


        /// <summary>
        /// Creates a target GameObject that provides an invisible 'enemy' for an entity motor to move towards.
        /// </summary>
        public static DaggerfallEntityBehaviour CreateTarget(Vector3 location)
        {
            MobileTypes mobileType = MobileTypes.GiantBat;

            string displayName = "Penwick Target";
            Transform parent = GameObjectHelper.GetBestParent();

            GameObject go = GameObjectHelper.InstantiatePrefab(DaggerfallUnity.Instance.Option_EnemyPrefab.gameObject, displayName, parent, location);
            SetupDemoEnemy setupEnemy = go.GetComponent<SetupDemoEnemy>();

            setupEnemy.ApplyEnemySettings(mobileType, MobileReactions.Hostile, MobileGender.Male, 0, false);

            go.SetActive(false);

            return go.GetComponent<DaggerfallEntityBehaviour>();
        }


        public static List<DaggerfallEntityBehaviour> GetNearbyEntities(float range = 14)
        {
            List<DaggerfallEntityBehaviour> entities = new List<DaggerfallEntityBehaviour>();

            List<PlayerGPS.NearbyObject> nearby = GameManager.Instance.PlayerGPS.GetNearbyObjects(PlayerGPS.NearbyObjectFlags.Enemy, range);
            foreach (PlayerGPS.NearbyObject obj in nearby)
            {
                if (obj.gameObject && obj.gameObject.activeInHierarchy)
                {
                    DaggerfallEntityBehaviour entity = obj.gameObject.GetComponent<DaggerfallEntityBehaviour>();
                    if (entity)
                        entities.Add(entity);
                }
            }

            return entities;
        }


        public static List<DaggerfallEntityBehaviour> GetNearbyEntities(Vector3 location, float range)
        {
            List<DaggerfallEntityBehaviour> entities = GetNearbyEntities(40);

            List<DaggerfallEntityBehaviour> near = new List<DaggerfallEntityBehaviour>();

            foreach (DaggerfallEntityBehaviour entity in entities)
            {
                float distance = Vector3.Distance(location, entity.transform.position);
                if (distance <= range)
                    near.Add(entity);
            }

            return near;
        }


        public static bool IsPlayerThreatened()
        {
            List<DaggerfallEntityBehaviour> creatures = GetNearbyEntities();

            foreach (DaggerfallEntityBehaviour creature in creatures)
            {
                EnemySenses senses = creature.GetComponent<EnemySenses>();
                EnemyMotor motor = creature.GetComponent<EnemyMotor>();

                if (!senses)
                    continue;
                else if (!motor)
                    continue;
                else if (senses.Target != GameManager.Instance.PlayerEntityBehaviour)
                    continue;
                else if (creature.Entity.Team == MobileTeams.PlayerAlly)
                    continue;
                else if (!motor.IsHostile)
                    continue;
                else if (CanSeePlayer(creature))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Check if any terrain is between creature and player
        /// </summary>
        public static bool CanSeePlayer(DaggerfallEntityBehaviour creature)
        {
            Vector3 creaturePos = creature.transform.position;
            Vector3 playerPos = GameManager.Instance.PlayerObject.transform.position;

            float distance = Vector3.Distance(playerPos, creaturePos);
            Vector3 direction = (playerPos - creaturePos).normalized;

            int layerMask = 1; //just looking for terrain hits
            RaycastHit hit; //for debugging purposes
            return !Physics.SphereCast(creature.transform.position, 0.2f, direction, out hit, distance, layerMask);
        }


        public static bool IsBlind(DaggerfallEntityBehaviour behaviour)
        {
            EntityEffectManager effectManager = behaviour.GetComponent<EntityEffectManager>();
            if (!effectManager)
                return false;

            LiveEffectBundle[] bundles = effectManager.EffectBundles;
            foreach (LiveEffectBundle bundle in bundles)
            {
                foreach (IEntityEffect effect in bundle.liveEffects)
                {
                    if (effect is Blind)
                        return true;
                }
            }

            return false;
        }


    } //class Utility



} //namespace
