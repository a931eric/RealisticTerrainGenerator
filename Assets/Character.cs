using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity =10f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    public bool flying;
    public Transform cam;
    float velY;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            flying = !flying;
        }
        if (flying)
        {
            if (Input.GetMouseButton(1))
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * 5, 0);
                cam.Rotate(Input.GetAxis("Mouse Y") * -5, 0, 0);
            }
        }
        else
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * 5, 0);
            cam.Rotate(Input.GetAxis("Mouse Y") * -5, 0, 0);
        }


        if (!flying)
        {
            velY -= (gravity * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            velY = jumpSpeed;

        }

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        moveDirection = cam.TransformDirection(moveDirection);
        moveDirection = moveDirection * speed * (flying ? 4 : 1);
        if (!flying)
            moveDirection += new Vector3(0, velY, 0);

        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
    }
}
