using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rigidBody;
    [SerializeField] float _speed;
    [SerializeField] float _gravity;
    public float maxDistance = 3.0f;
    public float gameSpeed = 1.0f;
    Direction _moveDirection = Direction.Left;
    public Player Player { get; set; }
    public bool CanMove { get; set; } = false;
    readonly float minPosY = -12.0f;

    void Start()
    {
        if (transform.localScale.x < 0)
        {
            _moveDirection = Direction.Right;
        }

        else
        {
            _moveDirection = Direction.Left;
        }
    }
    bool IsMove()
    {
        if (Player == null)
        {
            return false;
        }

        if (Player.IsDead || Player.IsGoal)
        {
            return false;
        }

        if (Vector3.Distance(Player.transform.localPosition, transform.localPosition) > maxDistance)
        {
            if (_moveDirection == Direction.Right)
            {
                if (transform.localPosition.x < Player.transform.localPosition.x)
                {
                    return false;
                }
            }

            else
            {
                if (transform.localPosition.x > Player.transform.localPosition.x)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void FixedUpdate()
    {
        if (!CanMove)
        {
            return;
        }

        if (transform.localPosition.y < minPosY)
        {
            _rigidBody.Sleep();
            CanMove = false;
            return;
        }

        if (IsMove())
        {
            int xVector = -1;
            if (_moveDirection == Direction.Right)
            {
                xVector = 1;
                transform.localScale = new Vector3(-1, 1, 1);
            }

            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            _rigidBody.velocity = new Vector2(xVector * _speed, -_gravity) * gameSpeed;
        }

        else
        {
            _rigidBody.velocity = new Vector2(0, -_gravity) * gameSpeed;
        }
    }
}

enum Direction
{
    Right,
    Left,
}