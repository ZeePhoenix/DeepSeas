using UnityEngine;
using System.Collections;

public class VRSwimmer : MonoBehaviour {

    public bool LeftController
    {
        get { return leftController; }
        set
        {
            leftController = value;
            SetControllerListeners(controllerLeftHand);
        }
    }

    public bool RightController
    {
        get { return rightController; }
        set
        {
            rightController = value;
            SetControllerListeners(controllerRightHand);
        }
    }

    [SerializeField]
    private bool leftController = true;
    [SerializeField]
    private bool rightController = true;

    public float maxSpeed = 3f;
    public float maxAccel = 0.5f;
    public float deceleration = 0.1f;
    public float rotationAmplify = 2.5f;
    public VRTK.VRTK_DeviceFinder.Devices directionDevice = VRTK.VRTK_DeviceFinder.Devices.Headset;

    private float triggerFloat = 0f;
    private float acceleration = 0f;
    private float moveSpeed = 0f;
    private float strafeSpeed = 0f;
    private GameObject controllerLeftHand;
    private GameObject controllerRightHand;
    private bool leftSubscribed;
    private bool rightSubscribed;
    private VRTK.ControllerInteractionEventHandler triggerAxisChanged;
    private VRTK.ControllerInteractionEventHandler triggerUntouched;


    private void Awake()
    {
        triggerAxisChanged = new VRTK.ControllerInteractionEventHandler(DoTriggerChange);
        triggerUntouched = new VRTK.ControllerInteractionEventHandler(DoTriggerChangeEnd);

        controllerLeftHand = VRTK.VRTK_DeviceFinder.GetControllerLeftHand();
        controllerRightHand = VRTK.VRTK_DeviceFinder.GetControllerRightHand();
    }

    // Use this for initialization
    void Start () {
        VRTK.Utilities.SetPlayerObject(gameObject, VRTK.VRTK_PlayerObject.ObjectTypes.CameraRig);
        SetControllerListeners(controllerLeftHand);
        SetControllerListeners(controllerRightHand);
    }
	
	// Update is called once per frame
	private void FixedUpdate () {
        acceleration = 0f;
        CalcSpeed(ref acceleration, triggerFloat);
        if (acceleration > maxAccel)
        {
            acceleration = maxAccel;
        }
        moveSpeed += acceleration;
        Decelerate(ref moveSpeed);
        Move();
	}

    private void DoTriggerChange(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        var controllerEvents = (VRTK.VRTK_ControllerEvents)sender;
        triggerFloat = controllerEvents.GetTriggerAxis();
    }

    private void DoTriggerChangeEnd(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        triggerFloat = 0f;
    }

    private void CalcSpeed(ref float speed, float input)
    {
        if (input != 0f)
            speed = maxAccel * input;
    }

    private void Decelerate(ref float speed)
    {
        if (speed > 0)
            speed -= Mathf.Lerp(deceleration, maxSpeed, 0f);
        else if (speed < 0)
            speed += Mathf.Lerp(deceleration, -maxSpeed, 0f);
        else speed = 0;

        float deadzone = 0.15f;
        if (speed < deadzone && speed > -deadzone)
            speed = 0;
    }

    private void Move()
    {
        var deviceDirector = VRTK.VRTK_DeviceFinder.DeviceTransform(directionDevice);

        Vector3 leftPos = controllerLeftHand.transform.position;
        Vector3 rightPos = controllerRightHand.transform.position;

        float dist = Vector3.Distance(leftPos, rightPos) / 2.0f;
        Vector3 midPoint = Vector3.MoveTowards(leftPos, rightPos, dist);
        Vector3 chest = new Vector3(deviceDirector.transform.position.x, deviceDirector.transform.position.y - 0.25f, deviceDirector.transform.position.z);
        Vector3 direction = Vector3.Normalize(midPoint - chest);
        var movement = direction * moveSpeed * Time.deltaTime;

        transform.position += (movement);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }


    private void SetControllerListeners(GameObject controller)
    {
        if (controller && VRTK.VRTK_SDK_Bridge.IsControllerLeftHand(controller))
        {
            ToggleControllerListeners(controller, leftController, ref leftSubscribed);
        }
        else if (controller && VRTK.VRTK_SDK_Bridge.IsControllerRightHand(controller))
        {
            ToggleControllerListeners(controller, rightController, ref rightSubscribed);
        }
    }

    private void ToggleControllerListeners(GameObject controller, bool toggle, ref bool subscribed)
    {
        var controllerEvent = controller.GetComponent<VRTK.VRTK_ControllerEvents>();
        if (controllerEvent && toggle && !subscribed)
        {
            controllerEvent.TriggerAxisChanged += triggerAxisChanged;
            controllerEvent.TriggerReleased += triggerUntouched;
            subscribed = true;
        }
        else if (controllerEvent && !toggle && subscribed)
        {
            controllerEvent.TriggerAxisChanged -= triggerAxisChanged;
            controllerEvent.TriggerReleased -= triggerUntouched;
            triggerFloat = 0f;
            subscribed = false;
        }
    }
}
