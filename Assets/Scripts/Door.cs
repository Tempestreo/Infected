using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    HingeJoint hinge;
    JointMotor motor;
    float angle;
    // Start is called before the first frame update
    void Start()
    {
        hinge = this.gameObject.GetComponent<HingeJoint>();
        motor = hinge.motor;
    }

    // Update is called once per frame
    void Update()
    {
        angle = hinge.angle;
        motor.targetVelocity = -angle;
        hinge.motor = motor;
    }
}
