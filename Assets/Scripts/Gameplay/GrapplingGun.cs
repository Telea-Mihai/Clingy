using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{

    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    public float maxDistance = 100f;
    public float aimassist=1f;
    private SpringJoint joint;
    public AudioSource grapplesource;
    public AudioClip grappleshoot;
    public GameObject grapplepointindicator;
    private GameObject temp;

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            grapplesource.pitch = Random.Range(0.8f, 1.2f);
            grapplesource.clip = grappleshoot;
            grapplesource.Play();
            StartGrapple();
        }
        else if (context.canceled)
        {
            StopGrapple();
        }
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.SphereCast(camera.position,aimassist, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
    }


    void StopGrapple()
    {
        Destroy(joint);
    }



    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
