using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CallbackSystem {
    public class PlayerAttack : MonoBehaviour {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private LineRenderer aimLineRenderer;
        [SerializeField] private LayerMask enemyLayerMask, laserLayerMask;
        private Vector3 aimingDirection = Vector3.forward;
        private PlayerHealth health;
        private PlayerController controller;
        private Camera cam;
        private bool isAlive = true;
        [SerializeField] [Range(0f, 50f)] private float maxDistance = 30f;
        [SerializeField] [Range(0f, 100f)] private float laserSelfDmg = 10f;
        [SerializeField] private float damage = 75f, teamDamage = 30f;
        [SerializeField] private int bullets = 10;
        [SerializeField] private GameObject bullet, upgradedBullet;
        private ResourceUpdateEvent resourceEvent;
        private bool laserWeapon = true;
        private bool activated = false, isPlayerOne, recentlyFired;
        private bool canShootLaser, projectionWeaponUpgraded, laserWeaponUpgraded;
        private float reducedSelfDmg, weaponCooldown, currentHitDistance;

        /*
         * From where the players weapon and ammunition is instantiated, stored and managed.
         * Only call on ResourceEvents concering ammunition from this script using UpdateBulletCount(increase/decrease).
         */

        public void Die() => isAlive = false;

        public bool IsPlayerOne() { return isPlayerOne; }
        public void UpdateBulletCount(int amount)
        {
            bullets += amount;
            //resourceEvent.isPlayerOne = isPlayerOne;
            resourceEvent.ammoChange = true;
            resourceEvent.a = bullets;
            EventSystem.Current.FireEvent(resourceEvent);
        }

        private void Awake() {
            controller = GetComponent<PlayerController>();
            cam = GetComponentInChildren<Camera>();
            health = GetComponent<PlayerHealth>();
            resourceEvent = new ResourceUpdateEvent();
            isPlayerOne = health.IsPlayerOne();
            reducedSelfDmg = laserSelfDmg/2;
            weaponCooldown = 0f;
        }

        [SerializeField] private Material bulletMat;
        [SerializeField] private Material laserMat;
        
        private void Update()
        {
            canShootLaser = (health.ReturnHealth() > laserSelfDmg || health.ReturnBatteries() > 0);
            // if (health.ReturnHealth() > laserSelfDmg || health.ReturnBatteries() > 0)
            // {
            //     canShootLaser = true;
            // }
            // else
            // {
            //     canShootLaser = false;
            // }

            // TODO joystick laser 
            if (!activated)
            {
                resourceEvent.ammoChange = true;
                resourceEvent.isPlayerOne = isPlayerOne;
                resourceEvent.a = bullets;
                EventSystem.Current.FireEvent(resourceEvent);
                activated = true;
            }
            if (recentlyFired && weaponCooldown < 0.5f)
                weaponCooldown += Time.deltaTime;
            else
                recentlyFired = false;

            if (isAlive) {
                AnimateLasers();
            } else {
                aimLineRenderer.enabled = false;
            }
        }

        public void Fire(InputAction.CallbackContext context) {
            if (context.started && isAlive && !recentlyFired) {
                if (laserWeapon && canShootLaser) {
                    ShootLaser();
                    StartCoroutine(AnimateLineRenderer(aimingDirection));
                } else if (!laserWeapon) {
                    FireProjectileWeapon();
                }
                recentlyFired = true;
                weaponCooldown = 0f;
            }
        }

        public void WeaponSwap(InputAction.CallbackContext context) {
            if (context.performed) {
                laserWeapon = !laserWeapon;
                // TODO [Sound] Play weapon swap sound(s)
            }
        }

        //This method & Pass Through(Y) on Input Actions if up = laser & down = projectile.
        public void WeaponSwapWithMouseWheel(InputAction.CallbackContext context) {
            if (context.performed) {
                float scrollDelta = context.ReadValue<float>();

                if (Mathf.Abs(scrollDelta) > 100.0f) {
                    laserWeapon = scrollDelta > 0;
                    // TODO [Sound] Play weapon swap sound(s)
                }
            }
        }

        private void AimDirection()
        {
            transform.LookAt(transform.position + aimingDirection);
        }

        private void ApplyJoystickFireDirection() 
        {
            if (controller.GetRightJoystickInput().magnitude > 0.1f) {
                aimingDirection.x = controller.GetRightJoystickInput().x;
                aimingDirection.z = controller.GetRightJoystickInput().y;
                aimingDirection.Normalize();
                aimingDirection = Quaternion.Euler(0, 45, 0) * aimingDirection;

            }
        }
        
        private void ShootLaser() {
            if (canShootLaser)
            {
                if (laserWeaponUpgraded)
                    laserSelfDmg = reducedSelfDmg;

                health.TakeDamage(laserSelfDmg);

                AudioController ac = AudioController.instance; //TODO: change audio parameter to fire with channel time!
                ac.PlayOneShotAttatched(IsPlayerOne() ? ac.player1.fire1 : ac.player2.fire1, gameObject); //laser sound

                Physics.Raycast(transform.position + transform.forward + Vector3.up, aimingDirection, out RaycastHit hitInfo, 30.0f, enemyLayerMask);
                if (hitInfo.collider != null)
                {
                    if (hitInfo.transform.tag == "Enemy" || hitInfo.transform.tag == "Player") 
                    {
                        IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
                        
                        if (damageable != null) // Enemies were colliding with pickups, so moved them to enemy ( for now ) layer thus this nullcheck to avoid pickups causing issues here
                        {
                            if(hitInfo.transform.tag == "Player")
                                damageable.TakeDamage(teamDamage);
                            else
                            damageable.TakeDamage(damage); //TODO pickUp-object should not be on enemy-layer! // maybe they should have their own layer?
                        }
                    }
                    else if (hitInfo.transform.tag == "BreakableObject")
                    {
                        BreakableObject breakable = hitInfo.transform.GetComponent<BreakableObject>();
                        breakable.DropBoxLoot();
                    }
                }
            }
        }

        private IEnumerator AnimateLineRenderer(Vector3 direction) {
            Vector3[] positions = { transform.position + Vector3.up, transform.position + Vector3.up + direction * 30.0f };
            lineRenderer.SetPositions(positions);
            float t = 0.0f;
            while (t < 1.0f) {
                float e = Mathf.Lerp(Ease.EaseOutQuint(t), Ease.EaseOutBounce(t), t);
                float lineWidth = Mathf.Lerp(.5f, .0f, e);
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                Color color = Color.Lerp(Color.white, Color.red, Ease.EaseInQuart(t));
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                t += Time.deltaTime * 3.0f;
                yield return null;
            }

            lineRenderer.startWidth = 0.0f;
            lineRenderer.endWidth = 0.0f;
        }

        private void AnimateLaserSightLineRenderer(Vector3 dir) {
            Vector3[] positions = { transform.position + Vector3.up, transform.position + Vector3.up + dir * currentHitDistance };
            aimLineRenderer.SetPositions(positions);
            float lineWidth = 0.05f;
            aimLineRenderer.startWidth = lineWidth;
            aimLineRenderer.endWidth = lineWidth;
            Color color = new Color(1f, 0.2f, 0.2f);
            aimLineRenderer.startColor = color;
            aimLineRenderer.endColor = color;
        }

        private void UpdateLaserSightDistance()
        {
            Physics.Raycast(transform.position + Vector3.up, aimingDirection, out RaycastHit hit, maxDistance, laserLayerMask);
                if(hit.collider != null)
                currentHitDistance = hit.distance;
                else
                currentHitDistance = maxDistance;
        }

        public void TargetMousePos(InputAction.CallbackContext context) {
            Vector3 mousePos = context.ReadValue<Vector2>();
            mousePos.z = 15.0f;
            Plane plane = new Plane(Vector3.up, transform.position + Vector3.up);
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (plane.Raycast(ray, out float enter)) {
                Vector3 hitPoint = ray.GetPoint(enter);
                aimingDirection = hitPoint + Vector3.down - transform.position;
            }
        }
        public void Respawn()
        {
            isAlive = true;
            AnimateLasers();
        }

        private void AnimateLasers()
        {
            aimLineRenderer.enabled = true;
            aimLineRenderer.material = laserWeapon ? laserMat : bulletMat;

            AimDirection();
            ApplyJoystickFireDirection();
            UpdateLaserSightDistance();
            AnimateLaserSightLineRenderer(gameObject.transform.forward);
        }

        private void FireProjectileWeapon()
        {
            if (bullets > 0)
            {
                if (AIData.Instance.EnemyMuzzleflash != null)
                {
                    Instantiate(AIData.Instance.EnemyMuzzleflash, transform.position, Quaternion.identity);
                }
                Debug.Log("Standard projectile weapon fired!");
                AudioController ac = AudioController.instance;
                ac.PlayOneShotAttatched(IsPlayerOne() ? ac.player1.fire2 : ac.player2.fire2, gameObject); //Gun sound
                UpdateBulletCount(-1);
                if(projectionWeaponUpgraded)
                    Instantiate(upgradedBullet, transform.position + transform.forward + Vector3.up, transform.rotation, null);
                else
                    Instantiate(bullet, transform.position + transform.forward + Vector3.up, transform.rotation, null);
            }
        }

        public void UpgradeProjectileWeapon()
        {
            Debug.Log("Projectile weapon upgraded!");
            projectionWeaponUpgraded = true;
        }

        public void UpgradeLaserWeapon()
        {
            Debug.Log("Projectile weapon upgraded!");
            laserWeaponUpgraded = true;
        }
    }
}
