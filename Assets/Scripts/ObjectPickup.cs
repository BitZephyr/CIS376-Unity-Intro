using System.Collections;
using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    public Transform weaponHoldPoint;  /* The point where the weapon will be held */
    public Camera playerCamera;  /* Camera used for raycasting (MUST be assigned in the Inspector) */
    private GameObject heldWeapon = null;  /* Currently held weapon */
    public float pickupRange = 3f;  /* Adjusted pickup range */
    private bool canPickUp = true;  /* Prevent spamming the 'E' key */
    
    /* Swinging Variables */
    public float swingDuration = 0.3f;  /* Time taken for a full swing */
    public float swingAngle = 90f;  /* The angle of the swing */
    private bool isSwinging = false;  /* Prevents multiple swings */
    private Collider weaponCollider;  /* Used for hit detection */

    void Update()
    {
        if (weaponHoldPoint == null)
        {
            Debug.LogError("WeaponHoldPoint is not assigned in the Inspector!");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned! Raycast will not work.");
            return;
        }

        /* Handle Weapon Pickup */
        HandleWeaponPickup();

        /* Handle Weapon Swing */
        if (heldWeapon != null && Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartCoroutine(SwingWeapon());
        }
    }

    /* Raycast to detect and pick up a weapon */
    void HandleWeaponPickup()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * hit.distance, Color.yellow);
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);

            if (hit.collider.CompareTag("Weapon") && heldWeapon == null)
            {
                Debug.Log("Aiming at Weapon. Press E to pick it up.");

                if (canPickUp && Input.GetKeyDown(KeyCode.E))
                {
                    PickupWeapon(hit.collider.gameObject);
                    canPickUp = false;
                    StartCoroutine(ResetPickUpCooldown());
                }
            }
        }
        else
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * pickupRange, Color.white);
        }
    }

    /* Pick up a weapon */
    void PickupWeapon(GameObject weapon)
    {
        if (heldWeapon == null)
        {
            heldWeapon = weapon;
            heldWeapon.transform.SetParent(weaponHoldPoint);
            heldWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            Debug.Log("Weapon picked up: " + heldWeapon.name);
            
            Rigidbody weaponRigidbody = heldWeapon.GetComponent<Rigidbody>();
            weaponCollider = heldWeapon.GetComponent<Collider>();

            if (weaponRigidbody != null)
            {
                weaponRigidbody.isKinematic = true;
            }

            if (weaponCollider != null)
            {
                weaponCollider.enabled = false; /* Disable collider until swinging */
            }

            heldWeapon.SetActive(true);
        }
    }

    /* Coroutine for weapon swinging */
    IEnumerator SwingWeapon()
    {
        isSwinging = true;
        float elapsedTime = 0f;

        Quaternion initialRotation = heldWeapon.transform.localRotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(-swingAngle, 0f, 0f);

        /* Enable weapon collider for hit detection */
        if (weaponCollider != null) 
        {
            weaponCollider.enabled = true;
        }

        /* Swing forward */
        while (elapsedTime < swingDuration / 2)
        {
            heldWeapon.transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / (swingDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        /* Swing back */
        elapsedTime = 0f;
        while (elapsedTime < swingDuration / 2)
        {
            heldWeapon.transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, elapsedTime / (swingDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        /* Disable collider after swinging */
        if (weaponCollider != null) 
        {
            weaponCollider.enabled = false;
        }

        heldWeapon.transform.localRotation = initialRotation;
        isSwinging = false;
    }

    /* Cooldown to prevent spamming weapon pickup */
    IEnumerator ResetPickUpCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        canPickUp = true;
    }
}